using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_TEAM_INFO
    {
        public int idx { get; set; }
        public byte country_flg { get; set; }
        public byte league_flg { get; set; }
        public byte area_flg { get; set; }
        public int team_idx { get; set; }
        public byte isuse { get; set; }
    }
}
