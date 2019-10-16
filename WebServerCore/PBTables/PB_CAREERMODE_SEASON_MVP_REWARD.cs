using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_SEASON_MVP_REWARD
    {
        public byte difficulty { get; set; }
        public byte awards_type { get; set; }
        public byte reward_type { get; set; }
        public int reward_idx { get; set; }
        public int reward_count { get; set; }
    }
}
