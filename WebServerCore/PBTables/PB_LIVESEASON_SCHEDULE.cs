using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_LIVESEASON_SCHEDULE
    {
        public int idx { get; set; }
        public byte liveseason_type { get; set; }
        public int open_date { get; set; }
        public byte close_date_type { get; set; }
        public int close_date { get; set; }
        public byte close_type { get; set; }
        public byte use { get; set; }
    }
}
