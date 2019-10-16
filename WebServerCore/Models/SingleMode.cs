namespace ApiWebServer.Models
{
    public class TeamRankingInfo
    {
        public int Ranking;
        public int TIdx;
    }

    public class SingleAccountInfo
    {
        public int team_idx { get; set; }
        public int season_idx { get; set; }
        public byte season_type { get; set; }
        public byte season_level { get; set; }
        public int game_idx { get; set; }
        public string last_rank { get; set; }
        public byte my_normal_rank { get; set; }
        public byte my_post_rank { get; set; }
        public byte max_level { get; set; }
    }

    public class SingleAccountPostSeasonInfo
    {
        public int normal_team1 { get; set; }
        public int normal_team2 { get; set; }
        public int normal_team3 { get; set; }
        public int normal_team4 { get; set; }
        public int normal_team5 { get; set; }
        public byte type_idx { get; set; }
        public int game_idx { get; set; }
        public byte finish_flag { get; set; }
        public int team_idx1 { get; set; }
        public int team_idx2 { get; set; }
        public int team_win1 { get; set; }
        public int team_win2 { get; set; }
    }


}