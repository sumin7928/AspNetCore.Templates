using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_COACH_POSITION
    {
        public int idx { get; set; }
        public int lineup_max_value { get; set; }
        public string master_position_num { get; set; }
        public int lineup_num { get; set; }
    }
}
