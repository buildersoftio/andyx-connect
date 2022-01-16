using Andy.X.Client;
using Andy.X.Client.Extensions;
using Andy.X.Connect.Core.Abstraction.Services.Sql;
using Andy.X.Connect.Core.Configurations;
using Andy.X.Connect.Core.Utilities.Logging;
using Andy.X.Connect.IO.Locations;
using Andy.X.Connect.Model.Postgres;
using Npgsql;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        private CancellationTokenSource cancelNotification;

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

            cancelNotification = new CancellationTokenSource();
            Task.Run(() => ContinueWaitingEvents(cancelNotification.Token));

            Logger.LogInformation($"POSTGRE Adapter for {_table.Name} connected");
        }

        public void Disconnect()
        {
            pgConnection.Close();
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();
        }

        private async void ProduceInsertedEvent(object data)
        {
            if (_table.Insert == true)
            {
                await producerInsert.ProduceAsync(data);
            }
        }

        private async void ProduceUpdatedEvent(object data)
        {
            if (_table.Update == true)
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
                ServiceUrl = _xNodeConfiguration.BrokerServiceUrls[0],
                Tenant = _xNodeConfiguration.Tenant,
                Product = _xNodeConfiguration.Product
            });

            if (table.Insert == true)
            {
                producerInsert = new Producer<object>(xClient, new Client.Configurations.ProducerConfiguration<object>()
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
                producerUpdate = new Producer<object>(xClient, new Client.Configurations.ProducerConfiguration<object>()
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
                producerDelete = new Producer<object>(xClient, new Client.Configurations.ProducerConfiguration<object>()
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
            pgCommand = new NpgsqlCommand($"LISTEN andyx{_table.Name.ToLower()}datachange;", pgConnection);
        }

        private Task ContinueWaitingEvents(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                pgConnection.Wait();
            }

            return Task.CompletedTask;
        }

        private void PgConnection_Notification(object sender, NpgsqlNotificationEventArgs e)
        {
            Payload payload = e.Payload.JsonToObject<Payload>();

            if (payload.Action == "INSERT")
                ProduceInsertedEvent(payload.Data);

            if (payload.Action == "UPDATE")
                ProduceUpdatedEvent(payload.Data);

            if (payload.Action == "DELETE")
                ProduceDeletedEvent(payload.Data);
        }

        private void CreateOrReplacePostgreFunction()
        {
            string functionCommand = File.ReadAllText(AppLocations.GetCreateFunction_NotifyOnDataChangeFile());
            functionCommand = functionCommand
                .Replace("{table_name}", _table.Name.ToLower())
                .Replace("{database_name}", _dbName.ToLower());

            pgCommand = new NpgsqlCommand(functionCommand, pgConnection);
            pgCommand.ExecuteNonQuery();
        }

        private void CreatePostgreTrigger()
        {
            string triggerCommand = File.ReadAllText(AppLocations.GetCreateTrigger_OnDataChangeFile());
            triggerCommand = triggerCommand.Replace("{table_name}", _table.Name.ToLower())
                                           .Replace("{database_name}", _dbName);

            pgCommand = new NpgsqlCommand(triggerCommand, pgConnection);
            try
            {
                pgCommand.ExecuteNonQuery();

            }
            catch (Exception)
            {
                // Trigger already exists,
                // TODO Check first if trigger exists..
            }
        }
    }
}
