﻿using DBCD.Providers;
using DBCDumpHost.Utils;
using DBDefsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBCDumpHost.Services
{
    public class DBDProvider : IDBDProvider
    {
        private readonly DBDReader dbdReader;
        private Dictionary<string, (string FilePath, Structs.DBDefinition Definition)> definitionLookup;

        public DBDProvider()
        {
            dbdReader = new DBDReader();
            LoadDefinitions();
        }

        public int LoadDefinitions()
        {
            var definitionsDir = SettingManager.definitionDir;
            Logger.WriteLine("Reloading definitions from directory " + definitionsDir);

            // lookup needs both filepath and def for DBCD to work
            // also no longer case sensitive now
            var definitionFiles = Directory.EnumerateFiles(definitionsDir);
            definitionLookup = definitionFiles.ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => (x, dbdReader.Read(x)), StringComparer.OrdinalIgnoreCase);

            Logger.WriteLine("Loaded " + definitionLookup.Count + " definitions!");

            return definitionLookup.Count;
        }

        public Stream StreamForTableName(string tableName)
        {
            tableName = Path.GetFileNameWithoutExtension(tableName);

            if (definitionLookup.TryGetValue(tableName, out var lookup))
                return new FileStream(lookup.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            throw new FileNotFoundException("Definition for " + tableName);
        }

        public bool TryGetDefinition(string tableName, out Structs.DBDefinition definition)
        {
            if (definitionLookup.TryGetValue(tableName, out var lookup))
            {
                definition = lookup.Definition;
                return true;
            }

            definition = default(Structs.DBDefinition);
            return false;
        }
    }
}
