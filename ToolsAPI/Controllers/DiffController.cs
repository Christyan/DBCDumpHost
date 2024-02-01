using DBDefsLib;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ToolsAPI.Utils;

namespace ToolsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiffController : Controller
    {
        [HttpGet]
        [Route("diff_api")]
        public async Task<string> DiffApi(string from, string to, int start = 0, string cdnDir = "wow")
        {
            Console.WriteLine("Serving root diff for root " + from + " => " + to + " (" + cdnDir + ")");

            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(SettingsManager.cascToolHost + "/casc/root/diff_api/?from=" + from +"&to=" + to);
            }
        }
    }
}
