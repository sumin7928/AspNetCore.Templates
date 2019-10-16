using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_SPRINGCAMP_GROUP
    {
        public int idx { get; set; }
        public byte level { get; set; }
        public byte season { get; set; }
        public byte target_type { get; set; }
        public byte target_position { get; set; }
        public byte max_count { get; set; }
        public int bonus_ratio { get; set; }
        public byte stat1_type { get; set; }
        public byte stat1_up_value { get; set; }
        public byte stat1_up_value_bonus { get; set; }
        public byte stat2_type { get; set; }
        public byte stat2_up_value { get; set; }
        public byte stat2_up_value_bonus { get; set; }
        public byte stat3_type { get; set; }
        public byte stat3_up_value { get; set; }
        public byte stat3_up_value_bonus { get; set; }
        public byte stat4_type { get; set; }
        public byte stat4_up_value { get; set; }
        public byte stat4_up_value_bonus { get; set; }
    }
}
