using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiWebServer.Models
{
    public class ScheduleInfo
    {
        public int schedule_idx;
        public int season_idx;
        public string start_time;
        public string end_time;
        public byte use_flag;
    }

    public class CompetitionInfo
    {
        public int game_no;
        public int season_idx;
        public int rating_idx;
        public int point;
        public int winning_streak;
        public int season_reward_idx;
        public int promotion_reward_idx;
        public byte rank_modify_flag;
        public int preferred_player;
        public int preferred_coach;
        public string battle_key;
    }

    public class CompetitionRecord
    {
        public int top_rating_idx;
        public int top_ranking;
        public int previous_rating_idx;
        public int previous_ranking;
        public int win;
        public int draw;
        public int lose;
        public string match_history;
    }

}
