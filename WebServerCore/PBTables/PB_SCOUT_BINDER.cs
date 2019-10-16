using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_SCOUT_BINDER
    {
        public int idx { get; set; }
        public byte binder_type { get; set; }
        public byte rate { get; set; }
        public byte binder_slot_type { get; set; }
        public int binder_slot1 { get; set; }
        public int binder_slot2 { get; set; }
        public int binder_slot3 { get; set; }
        public int binder_slot4 { get; set; }
        public int binder_slot5 { get; set; }
        public byte reward_type { get; set; }
        public byte reward_idx { get; set; }
        public byte reward_count { get; set; }
        public byte korea { get; set; }
        public byte america { get; set; }
        public byte japan { get; set; }
        public byte taiwan { get; set; }
    }
}
