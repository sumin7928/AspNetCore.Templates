using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_SKILL_COACHING
    {
        public int idx { get; set; }
        public byte grade { get; set; }
        public int next_grade_idx { get; set; }
    }
}
