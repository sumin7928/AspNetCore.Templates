using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_MARKET_URL
    {
        public byte country_type { get; set; }
        public int market_type { get; set; }
        public string url { get; set; }
    }
}
