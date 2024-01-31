using DBCD;
using System.Threading.Tasks;

namespace ToolsAPI.Services
{
    public interface IDBCManager
    {
        Task<IDBCDStorage> GetOrLoad(string name, string build);
    }
}
