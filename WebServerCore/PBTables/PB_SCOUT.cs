using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_SCOUT
    {
        public int scout_idx { get; set; }
        public byte scout_type { get; set; }
        public byte scout_cost_type { get; set; }
        public int scout_cost_value { get; set; }
        public int scout_time { get; set; }
        public byte scout_num_min { get; set; }
        public byte scout_num_max { get; set; }
        public byte korea { get; set; }
        public byte america { get; set; }
        public byte japan { get; set; }
        public byte taiwan { get; set; }
    }
}
