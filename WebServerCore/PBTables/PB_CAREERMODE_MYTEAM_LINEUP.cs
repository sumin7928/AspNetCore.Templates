using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_MYTEAM_LINEUP
    {
        public int idx { get; set; }
        public byte difficulty { get; set; }
        public int lineup_group_idx { get; set; }
        public byte player_type { get; set; }
        public byte lineup_type { get; set; }
        public byte order { get; set; }
        public byte position { get; set; }
        public int player_idx { get; set; }
        public long serial_index { get; set; }
        public byte player_reinforce_grade { get; set; }
        public int player_potential_1 { get; set; }
        public int player_potential_2 { get; set; }
        public int player_potential_3 { get; set; }
        public byte isuse { get; set; }
    }
}
