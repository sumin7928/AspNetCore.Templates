using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_TEAM_SELECT_PLAYER
    {
        public int team_idx { get; set; }
        public int pitcher_idx_1 { get; set; }
        public int pitcher_idx_2 { get; set; }
        public int pitcher_idx_3 { get; set; }
        public int batter_idx_1 { get; set; }
        public int batter_idx_2 { get; set; }
        public int batter_idx_3 { get; set; }
        public int coach_idx_1 { get; set; }
        public int coach_idx_2 { get; set; }
        public int coach_idx_3 { get; set; }
    }
}
