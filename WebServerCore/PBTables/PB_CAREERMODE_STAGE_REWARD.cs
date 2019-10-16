using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_STAGE_REWARD
    {
        public byte difficulty { get; set; }
        public byte result_type { get; set; }
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
        public int exp_manager { get; set; }
        public byte stage_mvp_reward_type { get; set; }
        public int stage_mvp_reward_idx { get; set; }
        public int stage_mvp_reward_count { get; set; }
    }
}
