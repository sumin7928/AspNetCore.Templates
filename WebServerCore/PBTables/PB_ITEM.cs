using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_ITEM
    {
        public int item_idx { get; set; }
        public byte item_type { get; set; }
        public string memo { get; set; }
        public byte korea { get; set; }
        public byte america { get; set; }
        public byte japan { get; set; }
        public byte taiwan { get; set; }
        public byte use_flag { get; set; }
    }
}
