using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_INVENTORY_LEVEL
    {
        public byte type { get; set; }
        public byte extend_level { get; set; }
        public int max_count { get; set; }
        public byte cost_type { get; set; }
        public int cost_value { get; set; }
    }
}
