using System;
using System.Collections.Generic;

namespace ApiWebServer.PBTables
{
    public partial class PB_COACH_SKILL_RANKUP
    {
        public int idx { get; set; }
        public string name { get; set; }
        public int coachskill_rankup_rate { get; set; }
        public int coachskill_rankup_rate_failrevision { get; set; }
        public byte coachskill_rankup_cost_type { get; set; }
        public int coachskill_rankup_cost_count { get; set; }
    }
}
