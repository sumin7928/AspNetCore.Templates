using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_TEAM_CREATE_GROUP
    {
        public int idx { get; set; }
        public int team_idx { get; set; }
        public int lineup_group_idx { get; set; }
    }
}
