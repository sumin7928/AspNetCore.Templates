using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_SPRINGCAMP_ADVICE
    {
        public int idx { get; set; }
        public byte level { get; set; }
        public byte season { get; set; }
        public byte target_type { get; set; }
        public byte target_position { get; set; }
        public byte retry { get; set; }
        public byte max_count { get; set; }
        public byte advice_type { get; set; }
        public byte advice_subtype { get; set; }
        public byte advice_value1 { get; set; }
        public byte advice_value2 { get; set; }
    }
}
