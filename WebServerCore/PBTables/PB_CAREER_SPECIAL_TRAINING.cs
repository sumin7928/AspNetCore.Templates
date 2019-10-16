using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_CAREER_SPECIAL_TRAINING
    {
        public byte training_id { get; set; }
        public byte training_type { get; set; }
        public byte appear_type { get; set; }
        public byte target_type { get; set; }
        public byte max_count { get; set; }
    }
}
