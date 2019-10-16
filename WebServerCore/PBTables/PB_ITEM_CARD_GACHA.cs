using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_ITEM_CARD_GACHA
    {
        public int item_idx { get; set; }
        public byte rate_idx { get; set; }
        public byte pack_type { get; set; }
        public byte play_type { get; set; }
        public int player_idx { get; set; }
        public int importance { get; set; }
    }
}
