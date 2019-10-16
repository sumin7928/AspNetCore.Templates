using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_RANK_REWARD
    {
        public short idx { get; set; }
        public byte country { get; set; }
        public byte difficulty { get; set; }
        public byte match_type { get; set; }
        public byte ranking { get; set; }
        public byte reward_type1 { get; set; }
        public int reward_idx1 { get; set; }
        public int reward_count1 { get; set; }
        public byte reward_type2 { get; set; }
        public int reward_idx2 { get; set; }
        public int reward_count2 { get; set; }
        public byte reward_type3 { get; set; }
        public int reward_idx3 { get; set; }
        public int reward_count3 { get; set; }
        public byte reward_type4 { get; set; }
        public int reward_idx4 { get; set; }
        public int reward_count4 { get; set; }
    }
}
