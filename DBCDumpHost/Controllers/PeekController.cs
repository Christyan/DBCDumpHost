﻿using DBCD;
using DBCDumpHost.Services;
using DBCDumpHost.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBCDumpHost.Controllers
{
    [Route("api/peek")]
    [ApiController]
    public class PeekController : ControllerBase
    {
        private readonly DBCManager dbcManager;

        public PeekController(IDBCManager dbcManager)
        {
            this.dbcManager = dbcManager as DBCManager;
        }

        public class PeekResult
        {
            public Dictionary<string, string> values { get; set; }
            public int offset { get; set; }
        }

        // GET: peek/
        [HttpGet]
        public string Get()
        {
            return "No DBC selected!";
        }

        // GET: peek/name
        [HttpGet("{name}")]
        public async Task<PeekResult> Get(string name, string build, string col, int val, bool useHotfixes = false, string pushIDs = "")
        {
            Logger.WriteLine("Serving peek row for " + name + "::" + col + " (" + build + ", hotfixes: " + useHotfixes + ") value " + val);

            List<int> pushIDList = null;

            if (useHotfixes && pushIDs != "")
            {
                pushIDList = new List<int>();

                var pushIDsExploded = pushIDs.Split(',');

                if (pushIDsExploded.Length > 0)
                {
                    foreach (var pushID in pushIDs.Split(','))
                    {
                        if (int.TryParse(pushID, out int pushIDInt))
                        {
                            pushIDList.Add(pushIDInt);
                        }
                    }
                }
            }

            var storage = await dbcManager.GetOrLoad(name, build, useHotfixes, LocaleFlags.All_WoW, pushIDList);

            var result = new PeekResult {values = new Dictionary<string, string>()};

            if (!storage.Values.Any())
            {
                return result;
            }

            if (col == "ID" && storage.TryGetValue(val, out var rowByIndex))
            {
                foreach (var fieldName in storage.AvailableColumns)
                {
                    if (fieldName != col)
                        continue;

                    var field = rowByIndex[fieldName];

                    // Don't think FKs to arrays are possible, so only check regular value
                    if (field.ToString() != val.ToString()) continue;

                    foreach (var subfieldName in storage.AvailableColumns)
                    {
                        var subfield = rowByIndex[subfieldName];

                        if (subfield is Array a)
                        {
                            for (var k = 0; k < a.Length; k++)
                            {
                                result.values.Add(subfieldName + "[" + k + "]", a.GetValue(k).ToString());
                            }
                        }
                        else
                        {
                            result.values.Add(subfieldName, subfield.ToString());
                        }
                    }
                }
            }
            else
            {
                var recordFound = false;

                foreach (var row in storage.Values)
                {
                    if (recordFound)
                        continue;

                    foreach (var fieldName in storage.AvailableColumns)
                    {
                        if (fieldName != col)
                            continue;

                        var field = row[fieldName];

                        // Don't think FKs to arrays are possible, so only check regular value
                        if (field.ToString() != val.ToString()) continue;

                        foreach (var subfieldName in storage.AvailableColumns)
                        {
                            var subfield = row[subfieldName];

                            if (subfield is Array a)
                            {
                                for (var k = 0; k < a.Length; k++)
                                {
                                    result.values.Add(subfieldName + "[" + k + "]", a.GetValue(k).ToString());
                                }
                            }
                            else
                            {
                                result.values.Add(subfieldName, subfield.ToString());
                            }
                        }

                        recordFound = true;
                    }
                }
            }

            return result;
        }
    }
}
