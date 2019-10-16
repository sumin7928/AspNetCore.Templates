using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_MANAGEMENT_EVENT
    {
        public int idx { get; set; }
        public int ratio_mood_good { get; set; }
        public int ratio_mood_normal { get; set; }
        public int ratio_mood_bad { get; set; }
        public byte eventtarget { get; set; }
        public byte select_count { get; set; }
    }
}
