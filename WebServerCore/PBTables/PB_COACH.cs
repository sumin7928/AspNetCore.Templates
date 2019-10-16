using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_COACH
    {
        public int coach_idx { get; set; }
        public byte master_position { get; set; }
        public int coach_unique_idx { get; set; }
        public string coach_name { get; set; }
        public short power { get; set; }
        public int teamidx { get; set; }
        public int coaching_skill { get; set; }
        public byte isuse { get; set; }
        public int get_rate { get; set; }
    }
}
