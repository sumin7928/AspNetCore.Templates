using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_PLAYER_SKILL_POTENTIAL
    {
        public int idx { get; set; }
        public int basic_idx { get; set; }
        public byte PotenType { get; set; }
        public byte Grade { get; set; }
        public int PotenActiveRate { get; set; }
        public int career_poten_rate { get; set; }
        public int skii_effect_idx { get; set; }
    }
}
