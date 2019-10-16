namespace ApiWebServer.Models
{
    public class PBPlayer
    {
        public int player_idx;
        public byte player_type;
        public int player_unique_idx;
        public string player_name;
        //public int player_grade;
        public byte position;
        public byte second_position;
        //public int year;
        public int team_idx;
        public short player_health;
        public int get_rate;
        public short overall;
    }

    /*public class Player
    {
        public long account_player_idx;
        public byte player_type;
        public int player_idx;
        public byte is_starting;
        public byte order;
        public byte position;
        public short reinforce_grade;
        public short player_health;
        public int potential_idx1;
        public int potential_idx2;
        public int potential_idx3;
        public byte sub_pos_open;
    }*/

    /*public class CreatePlayerMaterial
    {
        public int player_idx { get; set; }
        public byte player_type { get; set; }
        public int is_starting { get; set; }
        public byte order { get; set; }
        public byte position { get; set; }
        public short grade { get; set; }
        public int exp { get; set; }
        public short reinforce_grade { get; set; }
        public short player_health { get; set; }
        public int potential_idx1 { get; set; }
        public int potential_idx2 { get; set; }
        public int potential_idx3 { get; set; }
    }*/
    
    public class AccountCoach
    {
        public long account_coach_idx;
        public int coach_idx;
        public byte player_type;
        public byte order;
        public int coaching_skill;
        public byte is_starting;
        public byte cr_position;
        public byte cr_order;
        public byte cr_is_starting;
        public int position;
        public int coach_slot_idx;
    }
    public class AccountCoachSlot
    {
        public int idx;
        public int position;
        public long account_coach_idx;
    }
    public class TeamCoach
    {
        public int coach_idx;
        public byte player_type;
        public int position;
        public byte is_starting;
        public int coaching_skill;
    }

    public class AccountPlayerTrainingInfo
    {
        /// <summary>
        /// 선수 고유키
        /// </summary>
        public long account_player_idx;
        /// <summary>
        /// 잠재력1
        /// </summary>
        public int potential_idx1;
        /// <summary>
        /// 잠재력2
        /// </summary>
        public int potential_idx2;
        /// <summary>
        /// 잠재력3
        /// </summary>
        public int potential_idx3;
        /// <summary>
        /// 서브포지션 오픈여부
        /// </summary>
        public byte sub_pos_open;
    }
    public class TeamPaidPlayerList
    {
        /// <summary>
        /// 인덱스
        /// </summary>
        public int idx;
        /// <summary>
        /// 포지션
        /// </summary>
        public byte position;
        /// <summary>
        /// 국가 코드
        /// </summary>
        public byte country;
        /// <summary>
        /// 선수 인덱스
        /// </summary>
        public int player_idx;
    }
    public class PlayerLockDeleteCheck
    {
        /// <summary>
        /// 선수 고유 인덱스
        /// </summary>
        public long account_player_idx;
        /// <summary>
        /// 잠금 여부
        /// </summary>
        public byte is_lock;
        /// <summary>
        /// 선발 여부
        /// </summary>
        public byte is_starting;
        /// <summary>
        /// 코치 커리어모드 선발 여부
        /// </summary>
        public byte cr_is_starting;
    }
}
