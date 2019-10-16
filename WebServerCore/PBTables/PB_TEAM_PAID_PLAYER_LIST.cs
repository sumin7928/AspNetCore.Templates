using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_TEAM_PAID_PLAYER_LIST
    {
        public int idx { get; set; }
        public byte position { get; set; }
        public byte country_1 { get; set; }
        public int player_idx_1 { get; set; }
        public byte country_2 { get; set; }
        public int player_idx_2 { get; set; }
        public byte country_3 { get; set; }
        public int player_idx_3 { get; set; }
        public byte country_4 { get; set; }
        public int player_idx_4 { get; set; }
    }
}
