using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_SCOUT_GACHA
    {
        public int scout_idx { get; set; }
        public byte rate_idx { get; set; }
        public byte pack_type { get; set; }
        public byte play_type { get; set; }
        public int player_idx { get; set; }
        public int importance { get; set; }
    }
}
