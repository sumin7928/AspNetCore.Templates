using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_PLAYER_PITCHER
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
        public short player_health { get; set; }
        public short control { get; set; }
        public short ballspeed { get; set; }
        public short pitching_arsenal { get; set; }
        public short tension { get; set; }
        public short b_idx1 { get; set; }
        public short stat1 { get; set; }
        public short b_idx2 { get; set; }
        public short stat2 { get; set; }
        public short b_idx3 { get; set; }
        public short stat3 { get; set; }
        public short b_idx4 { get; set; }
        public short stat4 { get; set; }
        public short b_idx5 { get; set; }
        public short stat5 { get; set; }
        public short b_idx6 { get; set; }
        public short stat6 { get; set; }
        public short era { get; set; }
        public short w { get; set; }
        public short l { get; set; }
        public short s { get; set; }
        public short overall { get; set; }
        public int get_rate { get; set; }
        public byte isuse { get; set; }
    }
}
