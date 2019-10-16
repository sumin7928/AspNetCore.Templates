using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CDN_URL
    {
        public byte country_type { get; set; }
        public byte os_type { get; set; }
        public string url { get; set; }
    }
}
