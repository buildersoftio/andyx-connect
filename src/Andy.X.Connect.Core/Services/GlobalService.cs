using Andy.X.Connect.Core.Abstraction.Services.Sql;
using Andy.X.Connect.Core.Configurations;
using Andy.X.Connect.Core.Services.Generators;
using Andy.X.Connect.Core.Utilities.Extensions.Json;
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
        private DbEngineConfiguration dbEngineConfiguration;
        private XNodeConfiguration xNodeConfiguration;

        private ConcurrentDictionary<string, ISqlDbTableService> sqlDbTableServices;
        private SqlDbServiceGenerator sqlDbServiceGenerator;

        public GlobalService()
        {
            ReadConfigurationFiles();
            CreateMSSqlServices();
            InitializeEngineDbTableServices();
        }

        private void ReadConfigurationFiles()
        {
            Console.WriteLine("ANDYX-CONNECT|[init]|importing|config_files");
            if (Directory.Exists(AppLocations.ConfigDirectory()) != true)
            {
                Console.WriteLine($"ANDYX-CONNECT|[error]|importing|config directory|does not exists|path={AppLocations.ConfigDirectory()}");
                Console.WriteLine($"ANDYX-CONNECT|[ok]|importing|config directory|created|path={AppLocations.ConfigDirectory()}");
                Directory.CreateDirectory(AppLocations.ConfigDirectory());
            }

            if (Directory.Exists(AppLocations.ServicesDirectory()) != true)
                Directory.CreateDirectory(AppLocations.ServicesDirectory());

            if (Directory.Exists(AppLocations.TemplatesDirectory()) != true)
                Directory.CreateDirectory(AppLocations.TemplatesDirectory());

            // checking if files exits
            if (File.Exists(AppLocations.GetDbEnginesConfigurationFile()) != true)
            {
                Console.WriteLine($"ANDYX-CONNECT|[error]|importing|dbengine_config.json|file not exists|path={AppLocations.GetDbEnginesConfigurationFile()}");
                throw new Exception($"ANDYX-CONNECT|[error]|importing|dbengine_config.json|file not exists|path={AppLocations.GetDbEnginesConfigurationFile()}");
            }

            // checking if files exits
            if (File.Exists(AppLocations.GetXNodeConfigurationFile()) != true)
            {
                Console.WriteLine($"ANDYX-CONNECT|[error]|importing|xnode_config.json|file not exists|path={AppLocations.GetXNodeConfigurationFile()}");
                throw new Exception($"ANDYX-CONNECT|[error]|importing|xnode_config.json|file not exists|path={AppLocations.GetXNodeConfigurationFile()}");
            }

            dbEngineConfiguration = File.ReadAllText(AppLocations.GetDbEnginesConfigurationFile()).JsonToObject<DbEngineConfiguration>();
            Console.WriteLine($"ANDYX-CONNECT|[ok]|importing|dbengine_config.json|imported");

            xNodeConfiguration = File.ReadAllText(AppLocations.GetXNodeConfigurationFile()).JsonToObject<XNodeConfiguration>();
            Console.WriteLine($"ANDYX-CONNECT|[ok]|importing|xnode_config.json|imported");
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
                if (engine.EngineType == EngineTypes.MSSQL)
                {
                    foreach (var database in engine.Databases)
                    {
                        foreach (var table in database.Tables)
                        {
                            var tableWorker = AssemblyLoadContext.Default.
                                LoadFromAssemblyPath(AppLocations.GetDbServiceAssemblyFile(engine.EngineType.ToString(), database.Name, table.Name));
                            var serviceType = tableWorker.GetType("Andy.X.Connect.MSSQL.Code.Generated.Service.SqlDbTableService");

                            ConstructorInfo ctor = serviceType.GetConstructor(new[] { typeof(string), typeof(string), typeof(Table), typeof(XNodeConfiguration) });
                            ISqlDbTableService instance = ctor.Invoke(new object[] { engine.ConnectionString, database.Name, table, xNodeConfiguration })
                                as ISqlDbTableService;
                            instance.Connect();

                            sqlDbTableServices.TryAdd($"{database.Name}-{table.Name}", instance);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"ANDYX-CONNECT|[skipped]|{engine.EngineType}|engine_not_implemented");
                }
            }
        }
    }
}
