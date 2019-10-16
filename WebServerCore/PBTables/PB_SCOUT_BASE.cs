using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_SCOUT_BASE
    {
        public int scout_idx { get; set; }
        public byte country { get; set; }
        public byte league_flg { get; set; }
        public byte area_flg { get; set; }
        public byte product_type { get; set; }
        public byte select_type { get; set; }
        public int team_condition { get; set; }
        public byte posion_condition { get; set; }
        public byte overall_condition { get; set; }
    }
}
