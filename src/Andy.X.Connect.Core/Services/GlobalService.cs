using Andy.X.Connect.Core.Abstraction.Services.Sql;
using Andy.X.Connect.Core.Configurations;
using Andy.X.Connect.Core.Services.Generators;
using Andy.X.Connect.Core.Services.Oracle;
using Andy.X.Connect.Core.Utilities.Extensions.Json;
using Andy.X.Connect.Core.Utilities.Logging;
using Andy.X.Connect.IO.Locations;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Andy.X.Connect.Core.Services
{
    public class GlobalService
    {
        private AndyXConfiguration andyXConfiguration;
        private DbEngineConfiguration dbEngineConfiguration;
        private bool isDbEngineConfigImported;

        private ConcurrentDictionary<string, ISqlDbTableService> sqlDbTableServices;
        private SqlDbServiceGenerator sqlDbServiceGenerator;

        public GlobalService()
        {
            isDbEngineConfigImported = false;

            ReadConfigurationFiles();
            CreateAndInitializeDbEngineServices();
        }

        private void ReadConfigurationFiles()
        {
            Logger.LogInformation("Importing configuration files");
            if (Directory.Exists(AppLocations.ConfigDirectory()) != true)
            {
                Logger.LogError($"Importing configuration files failed, config directory does not exists; path={AppLocations.ConfigDirectory()}");
                Directory.CreateDirectory(AppLocations.ConfigDirectory());
                Logger.LogInformation($"Importing configuration files failed, config directory created; path={AppLocations.ConfigDirectory()}");
            }

            if (Directory.Exists(AppLocations.ServicesDirectory()) != true)
                Directory.CreateDirectory(AppLocations.ServicesDirectory());

            if (Directory.Exists(AppLocations.TemplatesDirectory()) != true)
                Directory.CreateDirectory(AppLocations.TemplatesDirectory());

            // checking if files exits
            if (File.Exists(AppLocations.GetAndyXConfigurationFile()) != true)
            {
                Logger.LogError($"Importing configuration files failed, andyx_config.json file does not exists; path={AppLocations.GetAndyXConfigurationFile()}");
                throw new Exception($"ANDYX-CONNECT|[error]|importing|andyx_config.json|file not exists|path={AppLocations.GetAndyXConfigurationFile()}");
            }

            // checking if dbengines files exits
            if (File.Exists(AppLocations.GetDbEnginesConfigurationFile()) == true)
            {
                isDbEngineConfigImported = true;
                dbEngineConfiguration = File.ReadAllText(AppLocations.GetDbEnginesConfigurationFile()).JsonToObject<DbEngineConfiguration>();
                Logger.LogInformation($"Database engines are imported successfully");
            }
            else
            {
                Logger.LogWarning($"Importing database engine file configuration is skipped, dbengine_config.json file does not exists; path={AppLocations.GetDbEnginesConfigurationFile()}");
            }

            andyXConfiguration = File.ReadAllText(AppLocations.GetAndyXConfigurationFile()).JsonToObject<AndyXConfiguration>();
            Logger.LogInformation($"Andy X configuration settings are imported successfully");
        }

        private void CreateAndInitializeDbEngineServices()
        {
            if (isDbEngineConfigImported is true)
            {
                CreateMSSqlServices();
                InitializeEngineDbTableServices();
            }
        }

        private void CreateMSSqlServices()
        {
            sqlDbServiceGenerator = new SqlDbServiceGenerator(dbEngineConfiguration);
            sqlDbServiceGenerator.CreateMSSqlTableModels();
        }

        private void InitializeEngineDbTableServices()
        {
            sqlDbTableServices = new ConcurrentDictionary<string, ISqlDbTableService>();

            foreach (var engine in dbEngineConfiguration.Engines)
            {
                switch (engine.EngineType)
                {
                    case EngineTypes.MSSQL:
                        InitializeMSSQLServices(engine);
                        break;
                    case EngineTypes.Oracle:
                        InitializeOracleServers(engine);
                        break;
                    case EngineTypes.PostgreSQL:
                        Logger.LogWarning($"Engine {engine.EngineType} has not been implemented");
                        break;
                    default:
                        Logger.LogWarning($"Engine {engine.EngineType} has not been implemented");
                        break;
                }
            }
        }

        private void InitializeOracleServers(Engine engine)
        {
            foreach (var database in engine.Databases)
            {
                foreach (var table in database.Tables)
                {
                    ISqlDbTableService instance = new OracleDbService(engine.ConnectionString, database.Name, table, andyXConfiguration);
                    instance.Connect();

                    sqlDbTableServices.TryAdd($"ORACLE-{database.Name}-{table.Name}", instance);
                }
            }
        }

        private void InitializeMSSQLServices(Engine engine)
        {
            foreach (var database in engine.Databases)
            {
                foreach (var table in database.Tables)
                {
                    var tableWorker = AssemblyLoadContext.Default.
                        LoadFromAssemblyPath(AppLocations.GetDbServiceAssemblyFile(engine.EngineType.ToString(), database.Name, table.Name));
                    var serviceType = tableWorker.GetType("Andy.X.Connect.MSSQL.Code.Generated.Service.SqlDbTableService");

                    ConstructorInfo ctor = serviceType.GetConstructor(new[] { typeof(string), typeof(string), typeof(Table), typeof(AndyXConfiguration) });
                    ISqlDbTableService instance = ctor.Invoke(new object[] { engine.ConnectionString, database.Name, table, andyXConfiguration })
                        as ISqlDbTableService;
                    instance.Connect();

                    sqlDbTableServices.TryAdd($"MSSQL-{database.Name}-{table.Name}", instance);
                }
            }
        }
    }
}
