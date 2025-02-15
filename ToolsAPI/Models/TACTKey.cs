﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToolsAPI.Models
{
    /// <summary>
    /// A TACT key is used to encrypt game content. See https://wowdev.wiki/TACT#TACT_keys for more information.
    /// </summary>
    public class TACTKey
    {
        /// <summary>
        /// ID from TactKey.db2.
        /// </summary>
        public int? ID { get; set; }

        /// <summary>
        /// Hex representation of 8-byte lookup from TactKeyLookup.db2.
        /// </summary>
        public string Lookup { get; set; }

        /// <summary>
        /// Hex representation of 16-byte key from TactKey.db2 and/or hotfixes.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Manually set description of what this key encrypts.
        /// </summary>
        public string Description { get; set; }
    }
}
