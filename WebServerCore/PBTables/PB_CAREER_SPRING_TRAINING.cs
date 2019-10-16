using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREER_SPRING_TRAINING
    {
        public byte training_id { get; set; }
        public byte target_type { get; set; }
        public byte max_count { get; set; }
        public int bonus_ratio { get; set; }
        public byte stat1_type { get; set; }
        public byte stat1_value_min { get; set; }
        public byte stat1_value_max { get; set; }
    }
}
