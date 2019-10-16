using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_MANAGEMENT_INJURY
    {
        public int idx { get; set; }
        public byte injury_type { get; set; }
        public byte injury_group { get; set; }
        public int ratio { get; set; }
        public byte period_min { get; set; }
        public byte period_max { get; set; }
        public int next_injury_idx { get; set; }
        public int next_injury_prob { get; set; }
        public int next_injury_prob_add { get; set; }
        public byte effect { get; set; }
    }
}
