using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_PLAYER_BATTER
    {
        public int player_idx { get; set; }
        public int player_unique_idx { get; set; }
        public string player_name { get; set; }
        public byte position { get; set; }
        public byte second_position { get; set; }
        public byte hand1 { get; set; }
        public byte hand2 { get; set; }
        public int teamidx { get; set; }
        public byte allstar { get; set; }
        public byte form { get; set; }
        public short power { get; set; }
        public short contact { get; set; }
        public short speed { get; set; }
        public short throwing { get; set; }
        public short fielding { get; set; }
        public short batting_eye { get; set; }
        public short avg { get; set; }
        public short hr { get; set; }
        public short stl { get; set; }
        public short overall { get; set; }
        public int get_rate { get; set; }
        public byte isuse { get; set; }
    }
}
