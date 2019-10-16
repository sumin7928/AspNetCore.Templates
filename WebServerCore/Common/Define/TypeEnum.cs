namespace ApiWebServer.Common.Define
{

    #region 공통 
    /// <summary>
    /// 세션 레디스 그룹 Index
    /// </summary>
    public enum REDIS_SESSION_IDX
    {
        USER_SESSION = 0,   // 유저 세션 정보
        INDEX_MAX = 16,
    }

    public enum DB_SESSION_LOCK : byte
    {
        NONE = 0,
        SET_LOCK = 1,
        REMOVE_LOCK = 2
    }

    public enum NEXT_STEP : int
    {
        NONE = 0,
        TRY_FORCE_END = -1,
        TRY_RE_LOGIN = -2
    }

    public enum MESSAGE_KEY : int
    {
        UNKNOWN_ERROR = 1,    // 904000001	"예상하지 못한 문제가 발생하였습니다.
        DISCONNECT = 2,    // 904000002	"게임과의 연결이 끊겼습니다.
        OTHER_DEVICE_CONNECT = 3,    // 904000003	"다른 기기에서 접속하였습니다.
        NETWORK_ERROR = 4,    // 904000004	"네트워크 접속이 원할하지 않습니다.
        WRONG_ACCOUNT_LOGIN = 5,   // 904000011    잘못된 계정입니다. 확인 후 다시 시도하여 주세요. 
    }

    public enum GAME_KEY_TYPE : byte
    {
        NONE = 0,
        CAREER_MODE_GAME,
        COMPETITION_GAME,
        SESSION_KEY,
        TEMP_NICK_NAME
    }

    public enum GAME_RESULT
    {
        NONE = 0,
        WIN = 1,
        DRAW = 2,
        LOSE = 3
    }

    public enum LIVESEASON_SCHEDULE_IDX
    {
        NONE = 0,
        COMPETITION = 1,
        CYCLE = 2
    }


    ///// <summary>
    ///// 901_텍스트_테이블.xlsm - Error_Text 시트
    ///// </summary>
    //public enum MsgKey : int
    //{
    //    UNKNOWN_ERROR = 1,    // 904000001	"예상하지 못한 문제가 발생하였습니다.
    //    DISCONNECT = 2,    // 904000002	"게임과의 연결이 끊겼습니다.
    //    OTHER_DEVICE_CONNECT = 3,    // 904000003	"다른 기기에서 접속하였습니다.
    //    NETWORK_ERROR = 4,    // 904000004	"네트워크 접속이 원할하지 않습니다.
    //    NOT_ENOUGH_GOLD = 5,    // 904000005	골드가 부족합니다.	골드가 부족합니다.
    //    NOT_ENOUGH_STAMINA = 6,    // 904000006	스테미너가 부족합니다. 	스테미너가 부족합니다. 
    //    NOT_ENOUGH_ORB = 7,    // 904000007	오브가 부족합니다.	오브가 부족합니다.
    //    NOT_ENOUGH_MATERIAL = 8,    // 904000008	재료가 부족합니다.	재료가 부족합니다.
    //    STAGE_ULOCK_TERMS_REMAINS = 9,    // 904000009	잠겨있는 스테이지 입니다. 확인 후 다시 시도하여 주세요.
    //    STORY_ALEADY_FINISHED_GAME = 10,   // 904000010    이미 종료 처리된 스테이지 입니다.
    //    WRONG_ACCOUNT_LOGIN = 11,   // 904000011    잘못된 계정입니다. 확인 후 다시 시도하여 주세요. 
    //    //================================
    //    MAX,
    //}


    public enum RANDOM_TYPE : int
    {
        GLOBAL,
        CAREERMODE_INJURY
    }

    #endregion

    #region Post
    public enum POST_ADD_TYPE : byte
    {
        ONE_BY_ONE = 0,
        MULTIPLE_BY_ONE = 1
    }
    
    #endregion

    #region Const Enum
    public enum CONST_DATA : int
    {
        CONST_TIME_OUT = 0,
        CONST_RETRY_COUNT = 1,
    }

    public enum PLAYER_TYPE : byte
    {
        TYPE_BATTER = 0,
        TYPE_PITCHER = 1,
        TYPE_COACH = 2,
        TYPE_BATTER_PITCHER = 3,
        MAX
    }

    public enum COACH_MASTER_TYPE : byte
    {
        TYPE_ALL = 0,
        TYPE_PITCHER = 1,
        TYPE_BATTER = 2,
        TYPE_TRAINER = 3
    }

    #endregion

    #region PlayerIdx Range
    public enum PLAYER_IDX_RANGE : int
    {
        MIN_PITCHER_IDX = 100001,
        MAX_PITCHER_IDX = 200000,
        MIN_BATTER_IDX = 200001,
        MAX_BATTER_IDX = 300000,
        MIN_COACH_IDX = 300001,
        MAX_COACH_IDX = 400000
    }

    public enum PLAYER_POSITION : byte
    {
        INVEN = 0,
        DH = 1,
        C = 2, 
	    B1 = 3, 
	    B2 = 4, 
	    B3 = 5,
        SS = 6,
        LF = 7,
        CF = 8,
        RF = 9,

        SP = 10,
        RP = 11,
        CP = 12,

        HD = 13,
        PC = 14,
        HC = 15,
        FBC = 16,
        TBC = 17,
        BC = 18,
        BTC = 19,
        TC = 20,

        CB = 21, //후보타자

        ALL = 100
    }

    public enum PLAYER_ORDER : byte
    {
        INVEN_BATTER = 100,
        INVEN_PITCHER = 101,
        INVEN_COACH = 102

    }

    public enum GAME_MODETYPE : byte
    {
        MODE_PVP = 0,
        MODE_CAREERMODE = 1
    }
    #endregion
    public enum ITEM_TYPE : byte
    {
        TYPE_ITEM_PACKAGE = 0,
        TYPE_CARD,
        TYPE_ITEM_GACHA,
        TYPE_ITEM_SINGLE,
        TYPE_ITEM_CONTENT,
        TYPE_CARD_GACHA
    }

    public enum ITEM_CARD_SELECT_TYPE : byte
    {
        SELECT_NOT = 0,
        SELECT_TEAM,
        SELECT_POSITION,
        SELECT_TEAM_POSITION,
    }

    public enum ITEM_CARD_GACHA_PACK_TYPE : byte
    {
        PLAYER_COACH = 0,
        PLAYER,
        COACH,
    }

    public enum ITEM_CONTENTS_USE_TYPE : byte
    {
        INVEN = 1,
        CAREERMODE = 2,
        REINFORCE = 3,
        SCOUT = 4
    }

    public enum ITEM_CONTENTS_EFFECT_TYPE : byte
    {
        NONE = 0,
        SIMULRATION = 1,
        REINFORCE_RATE_UP = 2,
        SCOUT_DIRECT = 3,
        SCOUT_BINDER_RESET = 4
    }

    public enum CHARACTER_INVEN_TYPE : int
    {
        PLAYER = 0,
        COACH = 1
    }

    public enum CHARACTER_OBTAIN_TYPE
    {
        OBTAIN_ITEM = 0,
        OBTAIN_SCOUT = 1
    }

    public enum SCOUT_USE_TYPE
    {
        NONE = 0,
        PLAYER = 1,
        COACH = 2,
        MAX = 3
    }

    public enum SCOUT_TYPE : byte
    {
        TYPE_CARD = 1,
        TYPE_CARD_GACHA = 2
    }

    public enum SCOUT_BINDER_TYPE : byte
    {
        PLAYER_COACH = 0,
        PLAYER = 1,
        COACH = 2,
        MAX
    }

    public enum SCOUT_BINDER_SLOT_TYPE : byte
    {
        RATE = 1,
        IDX = 2,
        MAX
    }

    public enum SCOUT_SEARCH_FINISH_TYPE : byte
    {
        NORMAL = 0,
        GOODS = 1,
        ITEM = 2
    }
    /*
    #region ItemType
    public enum ITEM_TYPE : int
    {
        TYPE_START = 0,
        CASH = 1,
        GOLD = 2,
        POINT = 3,      // 훈련포인트
        PLAYER_CARD = 4,
        COACH_CARD = 5,
        EQUIPMENT = 6,
        USE_ITEM = 7,
        BONUS_CASH = 8,
        GOODS = 101,      // 재화 통합
        TYPE_END
    }
    #endregion

    #region ItemCode
    public enum ITEM_CODE_CATEGORY : int
	{
		CODE_START = 1000,
		//============================
		//재화
		DEFAULT_START = 1000,
		DEFAULT_CASH = 1001,
		DEFAULT_MGBALL = 1002,        // 기획서상엔 오브(ORB)라고 되어있음. 없어졌음. 추후 삭제.
		DEFAULT_GOLD = 1003,
		DEFAULT_STAMINA = 1004, // 스태미너(배트)	스태미너(배트)
		DEFAULT_TICKET = 1005,  // 티켓	티켓
		DEFAULT_FRIEND_POINT = 1006,    //	우정포인트	우정포인트
		DEFAULT_POINT2 = 1007,  //	포인트 2	포인트 2
		DEFAULT_EXP = 1008,     //	경험치	경험치
		DEFAULT_DIA = 1009,     // 다이아
		DEFAULT_HEART = 1010,     // 하트
		DEFAULT_END,

		//장비
		EQUIPMENT_START = 3000,
		EQUIPMENT_BAT = 3001,                 //
		EQUIPMENT_GLOVE = 3002,
		EQUIPMENT_SHOES = 3003,
		EQUIPMENT_END,

		//아이템
		ITEM_START = 4000,
		ITEM_EVOLUTION = 4001,         //진화
		ITEM_AWAKENING = 4002,          //각성
		ITEM_MEDAL = 4003,          //메달
		ITEM_END,
		//============================
		CODE_END,
	}
    #endregion*/

    #region Server Config

    public enum WEB_HEADER_PROPERTIES
    {
        REQUEST_NO = 0,
        INSPECT = 1
    }

    #endregion

    public enum SERVICE_NATION_TYPE
    {
        NONE = 0,
        KOREA = 1,
        AMERICA = 2,
        JAPAN = 3,
        TAIWAN = 4,
        MAX
    }

    #region Reward Process

    public enum REWARD_TYPE : byte
    {
        NULL = 0,
        DIA = 1,
        GOLD = 2,
        TRAIN_POINT = 3,
        MASTERY_POINT = 4,
        PLAYER_CARD = 101,
        COACH_CARD = 102,
        EQUIP_ITEM = 103,
        NORMAL_ITEM = 104,
        DIA_BONUS = 105
    }

    #endregion

    #region CareerMode

    public enum CONTRACT_TYPE : byte
    {
        NONE = 0,

        RECONTRACT = 1,
        REJECT = 2,
        FAIL = 3,
        DESTROY = 4,
        RECONTRACT_REWARD = 5,

        STAND_BY_CONTRACT = 11,
        STAND_BY_FAILED = 12,

        MAX
    }

    public enum OWNER_GOAL_TYPE : short
    {
        NONE = 0,

        PENNATRACE_RANK = 11,
        PENNATRACE_ATTACK = 12,
        PENNATRACE_DEFENCE = 13,
        PENNATRACE_MANAGE = 14,

        POST_RANK = 21,
        POST_ATTACK = 22,
        POST_DEFENCE = 23,

        MAX
    }

    public enum RECOMMEND_ADVENTAGE_TYPE : short
    {
        NONE = 0,

        SKILL_BUFF = 1,
        WINNER_REWARD = 2,

        MAX
    }

    public enum NATION_LEAGUE_TYPE : byte
    {
        NONE = 0,

        KBO = 1,
        MLB = 2,
        NPB = 3,
        CPB = 4,

        KBO_MLB = 5,
        CPB_MLB = 6,
        NPB_MLB = 7,

        WHATEVER = 99,
        MAX,
    }

    public enum LEAGUE_TYPE : byte
    {
        NONE = 0,

        KBO = 1,
        AMERICAN,
        NATIONAL,
        CENTRAL,
        PACIFIC,
        CPB,

        MAX,
    }

    public enum LEAGUE_AREA_TYPE : byte
    {
        NONE = 0,

        EAST = 1,
        CENTER,
        WEST,

        MAX,
    }

    public enum SEASON_HALF_YEAR_TYPE : byte
    {
        NONE = 0,
        FIRST = 1,
        SECOND,
        TOTAL,
    }

    public enum SEASON_MATCH_GROUP
    {
        NONE = 0,
        PENNANTRACE = 1,
        POST_SEASON = 2,
        FINISHED = 3,
        MAX,
    }

    public enum SEASON_MATCH_TYPE
    {
        NONE = 0,

        PENNATRACE = 1,
        PENNATRACE_TIE_BREAK,
        PENNATRACE_RANKING,

        PS_KBO_WILD_CARD,                // 3전 2선승제, 4위팀 1승부여
        PS_KBO_SEMIPLAYOFF,             // 5전 3선승제, 정규리그 3위팀 vs 와일드 카드 승리팀
        PS_KBO_PLAYOFF,                 // 5전 3선승제, 정규리그 2위팀 vs 준플레이오프 승리팀
        PS_KBO_KOREA_SERIES,             // 7전 4선승제, 정규리그 1위팀 vs 플레이오프 승리팀

        PS_MLB_WILDCARD,
        PS_MLB_DIVISION,
        PS_MLB_CHAMPIONSHIP,
        PS_MLB_WORLD_SERIES,

        PS_NPB_FIRST_STAGE,
        PS_NPB_FINAL_STAGE,
        PS_NPB_JP_SERIES,

        PS_CPB_SEMI_PLAYOFF,
        PS_CPB_PLAYOFF,

        MAX,
    }

    public enum SPRING_CAMP_STEP : byte
    {
        STEP_TRAINING = 1,
        STEP_TEAM_BONUS = 2,
        FINISH = 3
    }

    public enum SPECIAL_TRAINING_STEP : byte
    {
        NULL = 0,
        STEP_MIDDLE = 1,
        STEP_LAST = 2
    }

    public enum SPRING_CAMP_MAIN_TYPE : byte
    {
        OPEN_POTEN = 1,
        OPEN_SUB_POSITON = 2,
        STAT_UP = 3
    }

    public enum CAREER_MVP_TYPE
    {
        START_BATTER = 1,
        START_PITCHER = 6,
        TOTAL_MVP = 10
    }

    public enum AWARD_TYPE
    {
        HR = 0,
        H,
        RBI,
        SB,
        AVG,
        W,
        HLD,
        SV,
        SO,
        ERA,
        MAX = 10
    }

    public enum TEAM_RECORD_TYPE : int
    {
        G = 0,                              // 경기수
        W,                                  // 팀 승리
        L,                                  // 팀 패배
        D,                                  // 팀 무승부
        WIN_RATE,                           // 팀 승률
        AR,                                 // 팀 평득점
        TR,                                 // 팀 득점
        AB,                                 // 팀 타수
        AVG_B,                              // 팀 타율
        H,                                  // 팀 안타
        HR,                                 // 팀 홈런
        SB,                                 // 팀 도루
        GIDP,                               // 팀 병살타
        DP,                                 // 병살
        TP,                                 // 삼중살
        B_BB,                               // 팀 볼넷(타자)
        B_HB,                               // 팀 사구(타자기록)
        F_E,                                // 팀 에러

        IP,                                 // 팀 이닝수
        PH,                                 // 팀 피안타
        ER,                                 // 팀 자책점(투수)
        ERA,                                // 팀 평균자책
        AVG_P,                              // 팀 피안타율
        PAB,                                // 상대 팀 타자들의 타수
        WHIP,                               // 팀 이닝출루허용
        SO,                                 // 팀 탈삼진
        BB,                                 // 팀 볼넷(투수)
        HBP,                                // 팀 사구(투수기록)
        SV,                                 // 팀 세이브
        HLD,                                // 팀 홀드

        Max,
    }

    public enum BATTER_RECORD_TYPE : int
    {
        G = 0,                              // 경기수
        PA,                                 // 타석
        AB,                                 // 타수
        AVG,                                // 타율
        SLG,                                // 장타율
        OBP,                                // 출루율
        OPS,                                // 출루장타율
        H,                                  // 안타		
        H1B,                                // 1루타
        H2B,                                // 2루타			
        H3B,                                // 3루타
        HR,                                 // 홈런
        BB,                                 // 볼넷
        TB,                                 // 총루타
        RBI,                                // 타점
        R,                                  // 득점
        SB,                                 // 도루성공
        CB,                                 // 도루실패
        SB_RATE,                            // 도루성공률
        SO,                                 // 삼진
        HB,                                 // 4구
        GIDP,                               // 병살타
        SF,                                 // 희생 플라이
        SH,                                 // 희생 번트

        F_SB,                               // 도루 허용
        F_CS,                               // 도루 저지
        F_CS_RATE,                          // 도루 저지율(%)			
        F_PB,                               // 포일 (패스트볼)			
        F_PO,                               // 자살
        F_A,                                // 보살
        F_E,                                // 실책
        F_FA,                               // 수비 성공율(%)			

        Max,
    }


    // 순서 변경 금지
    public enum PITCHER_RECORD_TYPE : int
    {
        G = 0,                              // 경기수
        ERA,                                // 평균자책(방어율)
        W,                                  // 승리
        L,                                  // 패배
        HLD,                                // 홀드
        SV,                                 // 세이브
        IP,                                 // 이닝(아웃 카운트)
        WHIP,                               // 이닝출루허용
        SO,                                 // 탈삼진
        H,                                  // 피안타
        BAB,                                // 상대 타자 타수
        HR,                                 // 피홈런
        R,                                  // 실점
        ER,                                 // 자책점
        BB,                                 // 볼넷
        HBP,                                // 사사구
        CG,                                 // 완투승
        SHO,                                // 완봉승
        QS,                                 // 퀄리트스타트

        Max,
    }

    public enum CYCLE_EVENT_FLAG : byte
    {
        NOT_CYCLE = 0,
        NEW_CYCLE_NOT_EVENT = 1,
        NEW_CYCLE_NEW_EVENT = 2
    }

    #endregion

    #region Mission&Achievement
    public enum MISSION_ACHIEVEMENT_ACTION_TYPE
    {
        TYPE_START = 0,
        SEASONMODE_PLAY = 1,
        SEASONMODE_ENTIRE_PLAY = 2,
        SEASONMODE_SIMUL_PLAY = 3,
        FOURTH = 4,
        FIFTH = 5,
        SIXTH = 6,
        LOGIN = 7,
        DAILY_MISSION_COMPLETE = 8,
        TYPE_END
    }



    public enum MISSION_TYPE_DB : byte
    {
        DAY = 1,
        WEEK = 2
    }

    public enum MISSION_TYPE_PB
    {
        SUNDAY = 1,
        MONDAY = 2,
        TUESDAY = 3,
        WEDNESDAY = 4,
        THURSDAY = 5,
        FRIDAY = 6,
        SATURDAY = 7,
        WEEK = 8,
        NEW_USER = 9,
        COMBACK_USER = 10,
        RANDOM = 11

    }

    #endregion

    #region Item

    
    public enum CONSUME_REWARD_TYPE : byte
    {
        CONSUME = 1,
        REWARD = 2,
        CONSUMEREWARD = 3
    }
    #endregion

    #region LiveSeason

    public enum COMPETITION_RANK_FLAG : byte
    {
        NONE = 0,
        RANK_UP = 1,
        RANK_DOWN = 2
    }

    public enum COMPETITION_MATCH_TARGET : byte
    {
        BOT = 0,
        ABOVE_ALL_USER = 1,
        ONLY_USER = 2
    }

    #endregion

}
