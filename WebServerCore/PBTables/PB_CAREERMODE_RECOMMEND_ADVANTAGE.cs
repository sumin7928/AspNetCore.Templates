using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREERMODE_RECOMMEND_ADVANTAGE
    {
        public short idx { get; set; }
        public byte country { get; set; }
        public byte advantage_group { get; set; }
        public int reference_idx { get; set; }
    }
}
