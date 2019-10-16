using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_SKILL_LEADERSHIP
    {
        public int idx { get; set; }
        public int basic_idx { get; set; }
        public byte category { get; set; }
        public byte grade { get; set; }
        public int add_rate { get; set; }
    }
}
