using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_COMPETITIVE_PLAY
    {
        public short idx { get; set; }
        public string rank_name { get; set; }
        public byte matching_target { get; set; }
        public byte rankup_point { get; set; }
        public byte rankdown_lose_value { get; set; }
        public byte win_point { get; set; }
        public byte straight_win_point { get; set; }
        public byte lose_point { get; set; }
        public int season_reset_rank { get; set; }
        public byte Intervention_limit { get; set; }
        public byte win_reward_type1 { get; set; }
        public int win_reward_idx1 { get; set; }
        public int win_reward_count1 { get; set; }
        public byte win_reward_type2 { get; set; }
        public int win_reward_idx2 { get; set; }
        public int win_reward_count2 { get; set; }
        public byte win_reward_type3 { get; set; }
        public int win_reward_idx3 { get; set; }
        public int win_reward_count3 { get; set; }
        public byte lose_reward_type1 { get; set; }
        public int lose_reward_idx1 { get; set; }
        public int lose_reward_count1 { get; set; }
        public byte lose_reward_type2 { get; set; }
        public int lose_reward_idx2 { get; set; }
        public int lose_reward_count2 { get; set; }
        public byte lose_reward_type3 { get; set; }
        public int lose_reward_idx3 { get; set; }
        public int lose_reward_count3 { get; set; }
        public byte rankup_reward_type1 { get; set; }
        public int rankup_reward_idx1 { get; set; }
        public int rankup_reward_count1 { get; set; }
        public byte rankup_reward_type2 { get; set; }
        public int rankup_reward_idx2 { get; set; }
        public int rankup_reward_count2 { get; set; }
        public byte rankup_reward_type3 { get; set; }
        public int rankup_reward_idx3 { get; set; }
        public int rankup_reward_count3 { get; set; }
        public byte season_reward_type1 { get; set; }
        public int season_reward_idx1 { get; set; }
        public int season_reward_count1 { get; set; }
        public byte season_reward_type2 { get; set; }
        public int season_reward_idx2 { get; set; }
        public int season_reward_count2 { get; set; }
        public byte season_reward_type3 { get; set; }
        public int season_reward_idx3 { get; set; }
        public int season_reward_count3 { get; set; }
        public int exp_manager_win { get; set; }
        public int exp_manager_lose { get; set; }
    }
}
