using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_REPEAT_MISSION
    {
        public int idx { get; set; }
        public short type { get; set; }
        public short action_type { get; set; }
        public byte action_count { get; set; }
        public string description { get; set; }
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
