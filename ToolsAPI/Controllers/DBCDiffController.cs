﻿using ToolsAPI.Services;
using ToolsAPI.Utils;
using DBDiffer;
using DBDiffer.DiffResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Threading.Tasks;

namespace ToolsAPI.Controllers
{
    [Route("api/diff")]
    [ApiController]
    public class DBCDiffController : ControllerBase
    {
        private readonly DBCManager dbcManager;

        public DBCDiffController(IDBCManager dbcManager)
        {
            this.dbcManager = dbcManager as DBCManager;
        }

        public async Task<string> Diff(string name, string build1, string build2, bool useHotfixesFor1 = false, bool useHotfixesFor2 = false)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(build1) || string.IsNullOrEmpty(build2))
            {
                return "Invalid arguments! Require name, build1, build2";
            }

            Logger.WriteLine("Serving diff for " + name + " between " + build1 + " and " + build2);

            var dbc1 = (IDictionary) await dbcManager.GetOrLoad(name, build1, useHotfixesFor1);
            var dbc2 = (IDictionary) await dbcManager.GetOrLoad(name, build2, useHotfixesFor2);

            var comparer = new DBComparer(dbc1, dbc2);
            WoWToolsDiffResult diff = (WoWToolsDiffResult)comparer.Diff(DiffType.WoWTools);

            return diff.ToJSONString();
        }
    }
}
