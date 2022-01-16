using Andy.X.Client;
using Andy.X.Connect.Core.Configurations;
using Andy.X.Connect.Core.Abstraction.Services.Sql;
using System;
using TableDependency.SqlClient;

namespace Andy.X.Connect.Experimental.Worker
{
    public class SqlDbTableService : ISqlDbTableService
    {
        private readonly string dbName;
        private readonly Table table;

        private SqlTableDependency<Object> sqlTableDependency;
        private Producer<Object> producerInsert;
        private Producer<Object> producerUpdate;
        private Producer<Object> producerDelete;

        private bool isConnected;

        public SqlDbTableService(string connectionString, string dbName, Table table, AndyXConfiguration xNodeConfiguration)
        {
            this.dbName = dbName;
            this.table = table;

            isConnected = false;

            sqlTableDependency = new SqlTableDependency<Object>(connectionString, table.Name);

            sqlTableDependency.OnStatusChanged += SqlTableDependency_OnStatusChanged;
            sqlTableDependency.OnChanged += SqlTableDependency_OnChanged;
            sqlTableDependency.OnError += SqlTableDependency_OnError;

            XClient xClient = new XClient(new Client.Configurations.XClientConfiguration()
            {
                ServiceUrl = xNodeConfiguration.BrokerServiceUrls[0],
                Tenant = xNodeConfiguration.Tenant,
                Product = xNodeConfiguration.Product
            });

            if (table.Insert == true)
            {
                producerInsert = new Producer<Object>(xClient, new Client.Configurations.ProducerConfiguration<Object>()
                {
                    Component = xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-insert",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-insert"
                });

                producerInsert.BuildAsync().Wait();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|insertProducer|{dbName}-{table.Name}-insert|initialized");
            }

            if (table.Update == true)
            {
                producerUpdate = new Producer<Object>(xClient, new Client.Configurations.ProducerConfiguration<Object>()
                {
                    Component = xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-update",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-update"
                });
                producerUpdate.BuildAsync().Wait();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|updateProducer|{dbName}-{table.Name}-update|initialized");
            }

            if (table.Delete == true)
            {
                producerDelete = new Producer<Object>(xClient, new Client.Configurations.ProducerConfiguration<Object>()
                {
                    Component = xNodeConfiguration.Component,
                    Name = $"{dbName}-{table.Name}-delete",
                    RetryProducing = false,
                    Topic = $"{dbName}-{table.Name}-delete"
                });
                producerDelete.BuildAsync().Wait();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|deleteProducer|{dbName}-{table.Name}-delete|initialized");
            }
        }

        private void SqlTableDependency_OnStatusChanged(object sender, TableDependency.SqlClient.Base.EventArgs.StatusChangedEventArgs e)
        {
            if (isConnected == true)
            {
                if (e.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.StopDueToError ||
                    e.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.StopDueToCancellation)
                {
                    Console.WriteLine($"ANDYX-CONNECT-MSSQL|[error]|adapter|{table.Name}|disconnected");
                    Reconnect();
                }
            }
            else
            {
                if (e.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.StopDueToCancellation)
                    Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|adapter|{table.Name}|disconnected_due_cancellation");
            }

            if (e.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.Started)
            {
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|adapter|{table.Name}|connected");
            }
            Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|adapter|status={e.Status}");
        }

        private void SqlTableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"ANDYX-CONNECT-MSSQL|[error]|{e.Server}|{e.Database}|details={e.Error.Message}");
        }

        private void SqlTableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<Object> e)
        {
            switch (e.ChangeType)
            {
                case TableDependency.SqlClient.Base.Enums.ChangeType.Delete:
                    ProduceDeletedEvent(e.Entity);
                    break;
                case TableDependency.SqlClient.Base.Enums.ChangeType.Insert:
                    ProduceInsertedEvent(e.Entity);
                    break;
                case TableDependency.SqlClient.Base.Enums.ChangeType.Update:
                    ProduceUpdatedEvent(e.Entity);
                    break;
                default:
                    break;
            }
        }

        private async void ProduceDeletedEvent(object entity)
        {
            if (table.Delete == true)
            {
                await producerDelete.ProduceAsync(entity);
            }
        }

        private async void ProduceUpdatedEvent(object entity)
        {
            if (table.Update == true)
            {
                await producerUpdate.ProduceAsync(entity);
            }
        }

        private async void ProduceInsertedEvent(object entity)
        {
            if (table.Insert == true)
            {
                await producerInsert.ProduceAsync(entity);
            }
        }

        public void Connect()
        {
            isConnected = true;
            if (table.Insert == true)
            {
                producerInsert.OpenAsync().Wait();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|insertProducer|{dbName}-{table.Name}-insert|connecting");
            }
            if (table.Update == true)
            {
                producerUpdate.OpenAsync().Wait();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|updateProducer|{dbName}-{table.Name}-update|connecting");
            }
            if (table.Delete == true)
            {
                producerDelete.OpenAsync().Wait();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|deleteProducer|{dbName}-{table.Name}-delete|connecting");
            }

            sqlTableDependency.Start();
            Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|adapter|{table.Name}|connecting");
        }

        private void Reconnect()
        {
            // For now don't try to reconnect Andy X, but only SQL Connection, Andy X Client has re-connect feature.
            //if (table.Insert == true)
            //{
            //    producerInsert.OpenAsync().Wait();
            //    Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|insertProducer|{dbName}-{table.Name}-insert|reconnecting");
            //}
            //if (table.Update == true)
            //{
            //    producerUpdate.OpenAsync().Wait();
            //    Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|updateProducer|{dbName}-{table.Name}-update|reconnecting");
            //}
            //if (table.Delete == true)
            //{
            //    producerDelete.OpenAsync().Wait();
            //    Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|deleteProducer|{dbName}-{table.Name}-delete|reconnecting");
            //}

            sqlTableDependency.Start();
            Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|adapter|{table.Name}|reconnecting");
        }

        public void Disconnect()
        {
            isConnected = false;
            sqlTableDependency.Stop();
            Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|adapter|{table.Name}|disconnected");

            if (table.Insert == true)
            {
                producerInsert.CloseAsync();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|insertProducer|{dbName}-{table.Name}-insert|disconnected");
            }
            if (table.Update == true)
            {
                producerUpdate.CloseAsync();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|updatedProducer|{dbName}-{table.Name}-update|disconnected");
            }
            if (table.Delete == true)
            {
                producerDelete.CloseAsync();
                Console.WriteLine($"ANDYX-CONNECT-MSSQL|[ok]|deleteProducer|{dbName}-{table.Name}-delete|disconnected");
            }
        }
    }
    //{ datamodel}
}
