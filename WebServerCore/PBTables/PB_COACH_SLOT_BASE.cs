using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_COACH_SLOT_BASE
    {
        public int idx { get; set; }
        public byte order { get; set; }
        public byte coach_slot_type { get; set; }
        public byte coach_slot_open_lv { get; set; }
        public int coach_slot_open_cost_type { get; set; }
        public int coach_slot_open_cost_count { get; set; }
    }
}
