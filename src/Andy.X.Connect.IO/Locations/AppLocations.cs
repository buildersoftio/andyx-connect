﻿using System;
using System.IO;

namespace Andy.X.Connect.IO.Locations
{
    public static class AppLocations
    {
        #region Directories
        public static string GetRootDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string ConfigDirectory()
        {
            return Path.Combine(GetRootDirectory(), "config");
        }

        public static string ServicesDirectory()
        {
            return Path.Combine(GetRootDirectory(), "services");
        }

        public static string TemplatesDirectory()
        {
            return Path.Combine(GetRootDirectory(), "templates");
        }

        public static string MSSQLTemplatesDirectory()
        {
            return Path.Combine(TemplatesDirectory(), "ms-sql");
        }

        public static string PostgreTemplatesDirectory()
        {
            return Path.Combine(TemplatesDirectory(), "postgre-sql");
        }

        public static string LogsDirectory()
        {
            return Path.Combine(GetRootDirectory(), "logs");
        }
        #endregion

        public static string GetAndyXConfigurationFile()
        {
            return Path.Combine(ConfigDirectory(), "andyx_config.json");
        }

        public static string GetDbEnginesConfigurationFile()
        {
            return Path.Combine(ConfigDirectory(), "dbengine_config.json");
        }

        public static string GetQueueConfigurationFile()
        {
            return Path.Combine(ConfigDirectory(), "queues_config.json");
        }

        public static string GetDbServiceAssemblyFile(string engine, string database, string table)
        {
            return Path.Combine(GetRootDirectory(), $"Andy.X.{engine}.{database}.{table}.Service.dll");
        }

        public static string GetSqlModelGeneratorFile()
        {
            return Path.Combine(MSSQLTemplatesDirectory(), "sql_modelgen.sql");
        }

        public static string GetServiceCodeGeneratorFile()
        {
            return Path.Combine(MSSQLTemplatesDirectory(), "csharp_sql_dbworker.cstemp");
        }

        public static string GetCreateTrigger_OnDataChangeFile()
        {
            return Path.Combine(PostgreTemplatesDirectory(), "CreateTrigger_OnDataChange.sql");
        }

        public static string GetCreateFunction_NotifyOnDataChangeFile()
        {
            return Path.Combine(PostgreTemplatesDirectory(), "CreateFunction_NotifyOnDataChange.sql");
        }

        public static string[] GetAssemblyFiles()
        {
            return Directory.GetFiles(GetRootDirectory(), "*.dll");
        }

        public static string GetLogConfigurationFile()
        {
            return Path.Combine(LogsDirectory(), $"xconnect-{DateTime.Now:dd-MM-yyyy}.log");
        }
    }
}
