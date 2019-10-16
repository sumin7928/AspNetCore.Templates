using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_TEAM_COUNTRY_SQUAD
    {
        public int idx { get; set; }
        public byte position { get; set; }
        public byte league_flg { get; set; }
        public byte player_type { get; set; }
        public byte lineup_type { get; set; }
        public byte order { get; set; }
    }
}
