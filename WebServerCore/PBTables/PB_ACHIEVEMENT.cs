using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_ACHIEVEMENT
    {
        public int idx { get; set; }
        public byte korea { get; set; }
        public byte america { get; set; }
        public byte japan { get; set; }
        public byte taiwan { get; set; }
        public byte category { get; set; }
        public string description { get; set; }
        public short action_type { get; set; }
        public int action_count { get; set; }
        public int next_level_idx { get; set; }
        public byte flow_number { get; set; }
        public byte reward_type1 { get; set; }
        public int reward_idx1 { get; set; }
        public int reward_count1 { get; set; }
        public byte reward_type2 { get; set; }
        public int reward_idx2 { get; set; }
        public int reward_count2 { get; set; }
        public byte reward_type3 { get; set; }
        public int reward_idx3 { get; set; }
        public int reward_count3 { get; set; }
    }
}
