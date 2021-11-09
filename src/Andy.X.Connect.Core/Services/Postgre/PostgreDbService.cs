using Andy.X.Client;
using Andy.X.Connect.Core.Abstraction.Services.Sql;
using Andy.X.Connect.Core.Configurations;
using Andy.X.Connect.Core.Utilities.Logging;
using Andy.X.Connect.IO.Locations;
using Npgsql;
using System;
using System.IO;

namespace Andy.X.Connect.Core.Services.Postgre
{
    public class PostgreDbService : ISqlDbTableService
    {
        private readonly string _connectionString;
        private readonly string _dbName;
        private readonly Table _table;
        private readonly AndyXConfiguration _xNodeConfiguration;


        private Producer<object> producerInsert;
        private Producer<object> producerUpdate;
        private Producer<object> producerDelete;

        private NpgsqlConnection pgConnection;
        private NpgsqlCommand pgCommand;

        private bool isConnected;

        public PostgreDbService(string connectionString, string dbName, Table table, AndyXConfiguration xNodeConfiguration)
        {
            _connectionString = connectionString;
            _dbName = dbName;
            _table = table;
            _xNodeConfiguration = xNodeConfiguration;

            InitializePostgreConnection();
            InitializeAndyXConsumers(dbName, table);
        }

        public void Connect()
        {
            isConnected = true;
            if (_table.Insert == true)
            {
                producerInsert.OpenAsync().Wait();
                Logger.LogInformation($"POSTGRE producer {_dbName}-{_table.Name}-inserted is active");
            }
            if (_table.Update == true)
            {
                producerUpdate.OpenAsync().Wait();
                Logger.LogInformation($"POSTGRE producer {_dbName}-{_table.Name}-updated is active");

            }
            if (_table.Delete == true)
            {
                producerDelete.OpenAsync().Wait();
                Logger.LogInformation($"POSTGRE producer {_dbName}-{_table.Name}-deleted is active");
            }

            pgConnection.Open();

            pgCommand.ExecuteNonQuery();
            Logger.LogInformation($"POSTGRE Adapter for {_table.Name} connected");
        }

        public void Disconnect()
        {
            pgConnection.Close();
        }

        private async void ProduceInsertedEvent(object data)
        {
            if (_table.Delete == true)
            {
                await producerInsert.ProduceAsync(data);
            }
        }

        private async void ProduceUpdatedEvent(object data)
        {
            if (_table.Delete == true)
            {
                await producerUpdate.ProduceAsync(data);
            }
        }

        private async void ProduceDeletedEvent(object data)
        {
            if (_table.Delete == true)
            {
                await producerDelete.ProduceAsync(data);
            }
        }


        private void InitializeAndyXConsumers(string dbName, Table table)
        {
            XClient xClient = new XClient(new Client.Configurations.XClientConfiguration()
            {
                XNodeUrl = _xNodeConfiguration.BrokerServiceUrls[0],
                Tenant = _xNodeConfiguration.Tenant,
                Product = _xNodeConfiguration.Product
            });

            if (table.Insert == true)
            {
                producerInsert = new Producer<object>(xClient, new Client.Configurations.ProducerConfiguration()
                {
                    Component = _xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-inserted",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-inserted"
                });

                producerInsert.BuildAsync().Wait();
                Logger.LogInformation($"POSTGRE producer {dbName}-{table.Name}-inserted is initializing");
            }

            if (table.Update == true)
            {
                producerUpdate = new Producer<object>(xClient, new Client.Configurations.ProducerConfiguration()
                {
                    Component = _xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-updated",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-updated"
                });
                producerUpdate.BuildAsync().Wait();
                Logger.LogInformation($"POSTGRE producer {dbName}-{table.Name}-updated is initializing");
            }

            if (table.Delete == true)
            {
                producerDelete = new Producer<object>(xClient, new Client.Configurations.ProducerConfiguration()
                {
                    Component = _xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-deleted",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-deleted"
                });
                producerDelete.BuildAsync().Wait();
                Logger.LogInformation($"POSTGRE producer {dbName}-{table.Name}-deleted is initializing");
            }
        }

        private void InitializePostgreConnection()
        {
            pgConnection = new NpgsqlConnection(_connectionString);

            pgConnection.Open();
            CreateOrReplacePostgreFunction();
            CreatePostgreTrigger();
            pgConnection.Close();

            pgConnection.Notification += PgConnection_Notification;
            pgCommand = new NpgsqlCommand($"LISTEN andyx_{_table.Name.ToLower()}_datachange;", pgConnection);
        }

        private void PgConnection_Notification(object sender, NpgsqlNotificationEventArgs e)
        {
            // Test the payload of postgre in this case.
        }

        private void CreateOrReplacePostgreFunction()
        {
            string functionCommand = File.ReadAllText(AppLocations.GetCreateFunction_NotifyOnDataChangeFile());
            functionCommand = functionCommand.Replace("{table_name}", _table.Name.ToLower());

            pgCommand = new NpgsqlCommand(functionCommand, pgConnection);
            pgCommand.ExecuteNonQuery();
        }

        private void CreatePostgreTrigger()
        {
            string triggerCommand = File.ReadAllText(AppLocations.GetCreateFunction_NotifyOnDataChangeFile());
            triggerCommand = triggerCommand.Replace("{table_name}", _table.Name.ToLower())
                                           .Replace("{database_name}", _dbName);

            pgCommand = new NpgsqlCommand(triggerCommand, pgConnection);
            pgCommand.ExecuteNonQuery();
        }
    }
}
