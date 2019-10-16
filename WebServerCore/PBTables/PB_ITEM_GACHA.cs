using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_ITEM_GACHA
    {
        public int item_idx { get; set; }
        public byte rate_idx { get; set; }
        public byte reward_type { get; set; }
        public int reward_idx { get; set; }
        public int reward_count { get; set; }
        public int get_rate { get; set; }
    }
}
