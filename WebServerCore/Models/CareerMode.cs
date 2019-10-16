namespace ApiWebServer.Models
{

    public class CareerModeInfo
    {
        public int total_career_cnt;
        public byte contract_no;
        public byte career_no;
        public byte mode_level;
        public byte country_type;
        public byte league_type;
        public byte area_type;
        public byte half_type;
        public int team_idx;
        public byte match_group;
        public byte match_type;
        public int degree_no;
        public int game_no;
        public byte now_rank;
        public byte finish_match_group;
        public int recommend_buff_val;
        public byte springcamp_step;
        public string last_rank;
        public byte recontract_cnt;
        public byte previous_contract;
        public int recommend_reward_idx;
        public byte specialtraining_step;       // 0:진행할것없음 1:1차 진행 2 : 2차 진행

        public short teammood;                      //관리요소의 분위기 수치
        public byte event_flag;                //0 관리주기 아님, 1:새관리주기(이벤트 없음) 2:새관리주기(이벤트있음)
        public int injury_game_no_new;         //마지막 신규 부상 발생 게임넘버
        public int injury_game_no_chain;       //마지막 연계부상 발생 게임넘버
        public byte injury_group1;             //1그룹 부상 인원수
        public byte injury_group2;             //2그룹 부상 인원수
        public byte injury_group3;             //3그룹 부상 인원수
        public byte injury_group4;             //4그룹 부상 인원수

    }

    public class CareerModeManagementConfig
    {
        public int manage_cycle_mlb;
        public int manage_cycle_kbo;
        public int manage_cycle_npb;
        public int manage_cycle_cpbl;
        public int event_appear_new_prob;
        public int event_appear_new_max;
        public int teammood_default;
        public int teammood_good_value;
        public int teammood_bad_value;
        public int teammood_change_win;
        public int teammood_change_draw;
        public int teammood_change_lose;
        public int InstantHeal_CostType;
        public int InstantHeal_Cost;
        public int injury_appear_new_prob;
        public int injury_appear_new_max;
        public int injury_appear_new_cootime;
        public int injury_appear_chain_max;
        public int injury_appear_chain_cootime;
        public int injury_have_max_group1;
        public int injury_have_max_group2;
        public int injury_have_max_group3;
        public int injury_have_max_group4;
        public int condition_notice_best;
        public int condition_notice_worst;

    }
}