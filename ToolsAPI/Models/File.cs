using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ToolsAPI.Models
{
    public class File
    {
        /// <summary>
        /// FileDataID as given by root file or other official sources.
        /// </summary>
        public int FileDataID { get; set; }

        /// <summary>
        /// Hex representation of 8-byte lookup, if known.
        /// </summary>
        public string? Lookup { get; set; }

        /// <summary>
        /// Filename of the file, if known.
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// Type of the file, if known.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Whether or not the filename is official (true) or named by the community (false).
        /// </summary>
        public bool IsOfficialFilename { get; set; }
    }
}