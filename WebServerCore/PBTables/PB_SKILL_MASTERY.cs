using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_SKILL_MASTERY
    {
        public int idx { get; set; }
        public int next_level_idx { get; set; }
        public byte mastery_skill_level { get; set; }
        public byte category { get; set; }
        public int conditionIdx { get; set; }
        public byte group_count { get; set; }
        public byte subject { get; set; }
        public byte group { get; set; }
        public byte preference { get; set; }
        public byte precondition { get; set; }
        public int precondition_value { get; set; }
        public int skill_effect_idx { get; set; }
    }
}
