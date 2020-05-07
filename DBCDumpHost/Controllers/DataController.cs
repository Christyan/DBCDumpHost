﻿using DBCD;
using DBCD.Providers;
using DBCDumpHost.Services;
using DBCDumpHost.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBCDumpHost.Controllers
{
    [Route("api/data")]
    [ApiController]
    public class DataController : ControllerBase
    {
        public struct DataTablesResult
        {
            public int draw { get; set; }
            public int recordsFiltered { get; set; }
            public int recordsTotal { get; set; }
            public List<List<string>> data { get; set; }
            public string error { get; set; }
        }

        private readonly DBDProvider dbdProvider;
        private readonly DBCManager dbcManager;

        public DataController(IDBDProvider dbdProvider, IDBCManager dbcManager)
        {
            this.dbdProvider = dbdProvider as DBDProvider;
            this.dbcManager = dbcManager as DBCManager;
        }

        // GET: data/
        [HttpGet]
        public string Get()
        {
            return "No DBC selected!";
        }

        // GET/POST: data/name
        [HttpGet("{name}"), HttpPost("{name}")]
        public async Task<DataTablesResult> Get(string name, string build, int draw, int start, int length, bool useHotfixes = false, LocaleFlags locale = LocaleFlags.All_WoW)
        {
            var searching = false;

            var parameters = new Dictionary<string, string>();

            if (Request.Method == "POST")
            {
                // POST, what site uses
                foreach (var post in Request.Form)
                {
                    parameters.Add(post.Key, post.Value);
                }

                if (parameters.ContainsKey("draw"))
                    draw = int.Parse(parameters["draw"]);

                if (parameters.ContainsKey("start"))
                    start = int.Parse(parameters["start"]);

                if (parameters.ContainsKey("length"))
                    length = int.Parse(parameters["length"]);
            }
            else
            {
                // GET, backwards compatibility for scripts/users using this
                foreach (var get in Request.Query)
                {
                    parameters.Add(get.Key, get.Value);
                }
            }

            if (!parameters.ContainsKey("search[value]"))
            {
                parameters.Add("search[value]", "");
            }

            var searchValue = parameters["search[value]"].Trim();

            if (string.IsNullOrWhiteSpace(searchValue))
            {
                Logger.WriteLine("Serving data " + start + "," + length + " for dbc " + name + " (" + build + ") for draw " + draw);
            }
            else
            {
                searching = true;
                Logger.WriteLine("Serving data " + start + "," + length + " for dbc " + name + " (" + build + ") for draw " + draw + " with search " + searchValue);
            }

            var result = new DataTablesResult
            {
                draw = draw
            };

            try
            {
                var storage = await dbcManager.GetOrLoad(name, build, useHotfixes, locale);

                if (storage == null)
                {
                    throw new Exception("Definitions for this DB/version combo not found in definition cache!");
                }

                result.recordsTotal = storage.Values.Count();

                result.data = new List<List<string>>();

                var filtering = false;
                var filters = new Dictionary<int, Predicate<object>>();

                var siteColIndex = 0;
                if (storage.Values.Count > 0)
                {
                    DBCDRow firstItem = storage.Values.First();

                    for (var i = 0; i < storage.AvailableColumns.Length; ++i)
                    {
                        var field = firstItem[storage.AvailableColumns[i]];
                        if (field is Array a)
                        {
                            for (var j = 0; j < a.Length; j++)
                            {
                                if (parameters.ContainsKey("columns[" + siteColIndex + "][search][value]") && !string.IsNullOrWhiteSpace(parameters["columns[" + siteColIndex + "][search][value]"]))
                                {
                                    var filterVal = parameters["columns[" + siteColIndex + "][search][value]"];
                                    searching = true;
                                    filtering = true;
                                    filters.Add(siteColIndex, CreateFilterPredicate(filterVal));
                                }

                                siteColIndex++;
                            }
                        }
                        else
                        {
                            if (parameters.ContainsKey("columns[" + siteColIndex + "][search][value]") && !string.IsNullOrWhiteSpace(parameters["columns[" + siteColIndex + "][search][value]"]))
                            {
                                var filterVal = parameters["columns[" + siteColIndex + "][search][value]"];
                                searching = true;
                                filtering = true;
                                filters.Add(siteColIndex, CreateFilterPredicate(filterVal));
                            }

                            siteColIndex++;
                        }
                    }
                }

                var resultCount = 0;
                foreach (DBCDRow item in storage.Values)
                {
                    siteColIndex = 0;

                    var rowList = new List<string>();
                    var matches = false;
                    var allMatch = true;

                    for (var i = 0; i < storage.AvailableColumns.Length; ++i)
                    {
                        var field = item[storage.AvailableColumns[i]];

                        if (field is Array a)
                        {
                            for (var j = 0; j < a.Length; j++)
                            {
                                var val = a.GetValue(j).ToString();
                                if (searching)
                                {
                                    if (filtering)
                                    {
                                        if (filters.ContainsKey(siteColIndex))
                                        {
                                            if (filters[siteColIndex](a.GetValue(j)))
                                            {
                                                matches = true;
                                            }
                                            else
                                            {
                                                allMatch = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (val.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase))
                                            matches = true;
                                    }
                                }

                                val = System.Web.HttpUtility.HtmlEncode(val);
                                rowList.Add(val);
                                siteColIndex++;
                            }
                        }
                        else
                        {
                            var val = field.ToString();
                            if (searching)
                            {
                                if (filtering)
                                {
                                    if (filters.ContainsKey(siteColIndex))
                                    {
                                        if (filters[siteColIndex](field))
                                        {
                                            matches = true;
                                        }
                                        else
                                        {
                                            allMatch = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (val.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase))
                                        matches = true;
                                }
                            }

                            val = System.Web.HttpUtility.HtmlEncode(val);
                            rowList.Add(val);
                            siteColIndex++;
                        }
                    }

                    if (searching)
                    {
                        if (matches && allMatch)
                        {
                            resultCount++;
                            result.data.Add(rowList);
                        }
                    }
                    else
                    {
                        resultCount++;
                        result.data.Add(rowList);
                    }
                }

                result.recordsFiltered = resultCount;

                var takeLength = length;
                if ((start + length) > resultCount)
                {
                    takeLength = resultCount - start;
                }

                result.data = result.data.GetRange(start, takeLength);
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error occured during serving data: " + e.Message);
                result.error = e.Message.Replace(SettingManager.dbcDir, "");
            }

            return result;
        }

        private static Predicate<object> CreateFilterPredicate(String filterVal)
        {
            if (filterVal.StartsWith("exact:"))
            {
                return CreateRegexPredicate("^" + filterVal.Remove(0, 6) + "$");
            }
            else if (filterVal.StartsWith("regex:"))
            {
                return CreateRegexPredicate(filterVal.Remove(0, 6));
            }
            else if (filterVal.StartsWith("0x"))
            {
                UInt64 flags;
                if (UInt64.TryParse(filterVal.Remove(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out flags))
                {
                    return CreateFlagsPredicate(flags);
                }
            }

            // Fallback logic is kept outside of an `else` branch to permit invalid filter recovery.
            return CreateRegexPredicate(filterVal);
        }

        private static Predicate<object> CreateRegexPredicate(String pattern)
        {
            var re = new Regex(pattern);
            return (field) => re.IsMatch(field.ToString());
        }

        private static Predicate<object> CreateFlagsPredicate(UInt64 flags)
        {
            return (field) =>
            {
                var num = Convert.ToUInt64(field, CultureInfo.InvariantCulture);
                return (num & flags) == flags;
            };
        }
    }

    [Flags]
    public enum LocaleFlags : uint
    {
        All = 0xFFFFFFFF,
        None = 0,
        //Unk_1 = 0x1,
        enUS = 0x2,
        koKR = 0x4,
        //Unk_8 = 0x8,
        frFR = 0x10,
        deDE = 0x20,
        zhCN = 0x40,
        esES = 0x80,
        zhTW = 0x100,
        enGB = 0x200,
        enCN = 0x400,
        enTW = 0x800,
        esMX = 0x1000,
        ruRU = 0x2000,
        ptBR = 0x4000,
        itIT = 0x8000,
        ptPT = 0x10000,
        enSG = 0x20000000, // custom
        plPL = 0x40000000, // custom
        All_WoW = enUS | koKR | frFR | deDE | zhCN | esES | zhTW | enGB | esMX | ruRU | ptBR | itIT | ptPT
    }
}
