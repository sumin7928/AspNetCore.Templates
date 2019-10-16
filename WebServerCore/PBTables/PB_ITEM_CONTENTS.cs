using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_ITEM_CONTENTS
    {
        public int item_idx { get; set; }
        public byte item_use_type { get; set; }
        public byte effect_type { get; set; }
        public int effect_value { get; set; }
        public int effect_time { get; set; }
    }
}
