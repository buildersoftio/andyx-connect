using Andy.X.Client;
using Andy.X.Connect.Core.Abstraction.Services.Sql;
using Andy.X.Connect.Core.Configurations;
using Andy.X.Connect.Core.Utilities.Extensions.Json;
using Andy.X.Connect.Core.Utilities.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace Andy.X.Connect.Core.Services.Oracle
{
    public class OracleDbService : ISqlDbTableService
    {
        private readonly string _connectionString;
        private readonly string _dbName;
        private readonly Table _table;
        private readonly AndyXConfiguration _xNodeConfiguration;

        private Producer<List<Dictionary<string, object>>> producerInsert;
        private Producer<List<Dictionary<string, object>>> producerUpdate;
        private Producer<List<Dictionary<string, object>>> producerDelete;
        private Producer<object> producerError;

        private OracleConnection oracleConnection;
        private OracleDependency oracleDependency;
        private OracleCommand oracleCommand;

        private bool isConnected;

        public OracleDbService(string connectionString, string dbName, Table table, AndyXConfiguration xNodeConfiguration)
        {
            _connectionString = connectionString;
            _dbName = dbName;
            _table = table;
            _xNodeConfiguration = xNodeConfiguration;

            InitializeOracleConnection();
            InitializeAndyXConsumers(dbName, table);
        }

        public void Connect()
        {
            isConnected = true;
            if (_table.Insert == true)
            {
                producerInsert.OpenAsync().Wait();
                Logger.LogInformation($"ORACLE producer {_dbName}-{_table.Name}-inserted is active");
            }
            if (_table.Update == true)
            {
                producerUpdate.OpenAsync().Wait();
                Logger.LogInformation($"ORACLE producer {_dbName}-{_table.Name}-updated is active");

            }
            if (_table.Delete == true)
            {
                producerDelete.OpenAsync().Wait();
                Logger.LogInformation($"ORACLE producer {_dbName}-{_table.Name}-deleted is active");
            }

            producerError.OpenAsync().Wait();
            Logger.LogInformation($"ORACLE producer {_dbName}-{_table.Name}-error-occurred is active");

            oracleConnection.Open();

            oracleCommand.ExecuteNonQuery();
            Logger.LogInformation($"ORACLE Adapter for {_table.Name} connected");
        }

        public void Disconnect()
        {
            oracleConnection.Close();
        }

        private void InitializeOracleConnection()
        {
            oracleConnection = new OracleConnection(_connectionString);
            oracleCommand = new OracleCommand($"select * from {_table.Name}", oracleConnection);

            oracleDependency = new OracleDependency(oracleCommand);
            oracleDependency.QueryBasedNotification = false;

            oracleDependency.OnChange += OracleDependency_OnChange;
        }

        private void OracleDependency_OnChange(object sender, OracleNotificationEventArgs eventArgs)
        {
            switch (eventArgs.Info)
            {
                case OracleNotificationInfo.Insert:
                    ProduceInsertedEvent(eventArgs.Details.ToListOfDictionary());
                    break;
                case OracleNotificationInfo.Delete:
                    ProduceDeletedEvent(eventArgs.Details.ToListOfDictionary());
                    break;
                case OracleNotificationInfo.Update:
                    ProduceUpdatedEvent(eventArgs.Details.ToListOfDictionary());
                    break;
                case OracleNotificationInfo.Error:
                    producerError.ProduceAsync(new { Title = "Error Occurred", Source = eventArgs.Source.ToString(), Resources = eventArgs.ResourceNames });
                    break;
                default:
                    break;
            }

        }

        private async void ProduceInsertedEvent(List<Dictionary<string, object>> data)
        {
            if (_table.Delete == true)
            {
                await producerInsert.ProduceAsync(data);
            }
        }

        private async void ProduceUpdatedEvent(List<Dictionary<string, object>> data)
        {
            if (_table.Delete == true)
            {
                await producerUpdate.ProduceAsync(data);
            }
        }

        private async void ProduceDeletedEvent(List<Dictionary<string, object>> data)
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
                producerInsert = new Producer<List<Dictionary<string, object>>>(xClient, new Client.Configurations.ProducerConfiguration()
                {
                    Component = _xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-inserted",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-inserted"
                });

                producerInsert.BuildAsync().Wait();
                Logger.LogInformation($"ORACLE producer {dbName}-{table.Name}-insert has been initialized");
            }

            if (table.Update == true)
            {
                producerUpdate = new Producer<List<Dictionary<string, object>>>(xClient, new Client.Configurations.ProducerConfiguration()
                {
                    Component = _xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-updated",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-updated"
                });
                producerUpdate.BuildAsync().Wait();
                Logger.LogInformation($"ORACLE producer {dbName}-{table.Name}-update has been initialized");
            }

            if (table.Delete == true)
            {
                producerDelete = new Producer<List<Dictionary<string, object>>>(xClient, new Client.Configurations.ProducerConfiguration()
                {
                    Component = _xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-deleted",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-deleted"
                });
                producerDelete.BuildAsync().Wait();
                Logger.LogInformation($"ORACLE producer {dbName}-{table.Name}-delete has been initialized");
            }

            producerError = new Producer<object>(xClient, new Client.Configurations.ProducerConfiguration()
            {
                Component = _xNodeConfiguration.Component,
                Name = $"{dbName}-{table.Name}-error-occurred",
                RetryProducing = false,
                Topic = $"{dbName}-{table.Name}-error-occurred"
            });
            producerDelete.BuildAsync().Wait();
        }
    }
}
