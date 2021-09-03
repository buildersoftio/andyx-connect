using System.Collections.Generic;

namespace Andy.X.Connect.Core.Configurations
{
    public class DbEngineConfiguration
    {
        public List<Engine> Engines { get; set; }
    }

    public class Engine
    {
        public string ConnectionString { get; set; }
        public List<Database> Databases { get; set; }
    }

    public class Database
    {
        public string Name { get; set; }
        public List<Table> Tables { get; set; }

        public Database()
        {
            Tables = new List<Table>();
        }
    }

    public class Table
    {
        public string Name { get; set; }

        public bool Insert { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
    }
}
