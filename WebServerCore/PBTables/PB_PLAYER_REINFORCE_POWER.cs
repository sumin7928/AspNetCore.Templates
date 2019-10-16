using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_PLAYER_REINFORCE_POWER
    {
        public int idx { get; set; }
        public int probability { get; set; }
        public int fail_add_probability { get; set; }
        public int poten_rankup_reinforce_const { get; set; }
        public byte potential_slot_count { get; set; }
        public byte potential_slot_open { get; set; }
        public byte price_type1 { get; set; }
        public int price_count1 { get; set; }
        public byte price_type2 { get; set; }
        public int price_count2 { get; set; }
        public byte price_type3 { get; set; }
        public int price_count3 { get; set; }
        public int add_power { get; set; }
        public int add_contact { get; set; }
        public int add_speed { get; set; }
        public int add_throwing { get; set; }
        public int add_flelding { get; set; }
        public int add_health { get; set; }
        public int add_contral { get; set; }
        public int add_ballspeed { get; set; }
        public int add_tension { get; set; }
        public int add_stat1 { get; set; }
        public int add_stat2 { get; set; }
        public int add_stat3 { get; set; }
        public int add_stat4 { get; set; }
        public int add_stat5 { get; set; }
        public int add_stat6 { get; set; }
    }
}
