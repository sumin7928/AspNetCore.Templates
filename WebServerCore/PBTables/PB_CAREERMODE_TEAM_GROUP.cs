using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_TEAM_GROUP
    {
        public int team_idx { get; set; }
        public int lineup_group_idx { get; set; }
        public byte team_tier { get; set; }
    }
}
