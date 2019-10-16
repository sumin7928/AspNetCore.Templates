using System;
using System.Collections.Generic;
using System.Linq;
using ApiWebServer.Common;
using ApiWebServer.Common.Define;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Cache.PBTables
{
    public class CareerModeTable : ICommonPBTable
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private List<PB_CAREERMODE_TEAM_GROUP> _teamGroup = new List<PB_CAREERMODE_TEAM_GROUP>();
        private Dictionary<byte, List<PB_CAREERMODE_TEAM_GROUP>> _leagueTeamGroup = new Dictionary<byte, List<PB_CAREERMODE_TEAM_GROUP>>();

        private Dictionary<byte, List<PB_CAREERMODE_CHAINCONTRACT_REWARD>> _chainContractReward = new Dictionary<byte, List<PB_CAREERMODE_CHAINCONTRACT_REWARD>>();
        private Dictionary<byte, List<PB_CAREERMODE_RECOMMEND_ADVANTAGE>> _recommendBuffAdvantage = new Dictionary<byte, List<PB_CAREERMODE_RECOMMEND_ADVANTAGE>>();
        private Dictionary<byte, List<PB_CAREERMODE_RECOMMEND_ADVANTAGE>> _recommendRewardAdvantage = new Dictionary<byte, List<PB_CAREERMODE_RECOMMEND_ADVANTAGE>>();
        private Dictionary<int, PB_CAREERMODE_RECOMMEND_ADVANTAGE> _recommendAdvantage = new Dictionary<int, PB_CAREERMODE_RECOMMEND_ADVANTAGE>();

        private Dictionary<int, List<PB_CAREERMODE_MYTEAM_LINEUP>>[] _teamLineup = new Dictionary<int, List<PB_CAREERMODE_MYTEAM_LINEUP>>[ CareerModeDefine.DifficultyCount ];

        private Dictionary<int, PB_CAREERMODE_OWNER_GOAL> _ownerGoal = new Dictionary<int, PB_CAREERMODE_OWNER_GOAL>();

        private Dictionary<int, PB_CAREERMODE_RANK_REWARD> _recommendReward = new Dictionary<int, PB_CAREERMODE_RANK_REWARD>();

        //첫번째 array : 국가(0:kbo, 1:mlb, 2:npb, 3:cpbl)
        //두번째 array : 리그등급(0:쉬움, 1:보통 2: 어려움)
        //세번째 array : 경기형태(0:정규, 1:포스트)
        private Dictionary<int, PB_CAREERMODE_RANK_REWARD>[][][] _rankReward = new Dictionary<int, PB_CAREERMODE_RANK_REWARD>[ 4 ][][];

        //첫번째 array : 리그등급(0:쉬움, 1:보통 2: 어려움)
        private Dictionary<int, PB_CAREERMODE_STAGE_REWARD>[] _stageReward = new Dictionary<int, PB_CAREERMODE_STAGE_REWARD>[ CareerModeDefine.DifficultyCount ];

        //첫번째 array : 리그등급(0:쉬움, 1:보통 2: 어려움)
        private Dictionary<int, PB_CAREERMODE_SEASON_MVP_REWARD>[] _seasonMvpReward = new Dictionary<int, PB_CAREERMODE_SEASON_MVP_REWARD>[ CareerModeDefine.DifficultyCount ];

        //스프링캠프 그룹훈련
        private Dictionary<byte, PB_CAREER_SPRING_TRAINING> _springCampTraining = new Dictionary<byte, PB_CAREER_SPRING_TRAINING>();
        private Dictionary<byte, string>[] _springCampTrainingStatString = new Dictionary<byte, string>[ 2 ];

        //스프링캠프 마무리훈련(보너스훈련)
        private List<PB_CAREER_SPRING_RESULT_BONUS> _springCampResultBonus = new List<PB_CAREER_SPRING_RESULT_BONUS>();
        private Dictionary<byte, string> _springCampResultBonusStatString = new Dictionary<byte, string>();

        //특별훈련 array[0:시즌중, 1:시즌말]
        private Dictionary<byte, PB_CAREER_SPECIAL_TRAINING>[] _specialTraining = new Dictionary<byte, PB_CAREER_SPECIAL_TRAINING>[ 2 ];
        private Dictionary<byte, string> _specialTrainingStatString = new Dictionary<byte, string>();

        //관리요소 신규 부상 테이블
        private Dictionary<int, PB_CAREERMODE_MANAGEMENT_INJURY> _allInjury = new Dictionary<int, PB_CAREERMODE_MANAGEMENT_INJURY>();
        private List<PB_CAREERMODE_MANAGEMENT_INJURY>[] _newInjury = new List<PB_CAREERMODE_MANAGEMENT_INJURY>[CareerModeDefine.InjuryGroupCount];
        private readonly int[] _newInjuryTotalRate = new int[CareerModeDefine.InjuryGroupCount];

        //관리주기 이벤트 관련 테이블
        private Dictionary<int, PB_CAREERMODE_MANAGEMENT_EVENT> _allEvent = new Dictionary<int, PB_CAREERMODE_MANAGEMENT_EVENT>();
        private List<PB_CAREERMODE_MANAGEMENT_EVENT>[] _moodEvent = new List<PB_CAREERMODE_MANAGEMENT_EVENT>[3];        //0좋음 1보통 2나쁨
        private readonly List<int>[] _moodEventRate = new List<int>[3];                                                 //0좋음 1보통 2나쁨

        //관리요소 config 값
        public CareerModeManagementConfig ManagementConfig { get; private set; } = new CareerModeManagementConfig();

        public bool LoadTable( MaguPBTableContext context )
        {
            // PB_CAREERMODE_TEAM_GROUP
            foreach ( var data in context.PB_CAREERMODE_TEAM_GROUP.ToList() )
            {
                _teamGroup.Add( data );

                var teamInfo = context.PB_TEAM_INFO.ToList().Find( x => x.team_idx == data.team_idx );

                if ( _leagueTeamGroup.ContainsKey( teamInfo.league_flg ) )
                {
                    _leagueTeamGroup[ teamInfo.league_flg ].Add( data );
                }
                else
                {
                    _leagueTeamGroup.Add( teamInfo.league_flg, new List<PB_CAREERMODE_TEAM_GROUP>() { data } );

                }
            }

            // PB_CAREERMODE_RECOMMEND_ADVANTAGE
            foreach ( var data in context.PB_CAREERMODE_RECOMMEND_ADVANTAGE.ToList() )
            {
                _recommendAdvantage.Add( data.idx, data );

                if ( data.advantage_group == ( byte )RECOMMEND_ADVENTAGE_TYPE.SKILL_BUFF )
                {
                    if ( _recommendBuffAdvantage.ContainsKey( data.country ) )
                    {
                        _recommendBuffAdvantage[ data.country ].Add( data );
                    }
                    else
                    {
                        _recommendBuffAdvantage.Add( data.country, new List<PB_CAREERMODE_RECOMMEND_ADVANTAGE>() { data } );

                    }
                }
                else if ( data.advantage_group == ( byte )RECOMMEND_ADVENTAGE_TYPE.WINNER_REWARD )
                {
                    if ( _recommendRewardAdvantage.ContainsKey( data.country ) )
                    {
                        _recommendRewardAdvantage[ data.country ].Add( data );
                    }
                    else
                    {
                        _recommendRewardAdvantage.Add( data.country, new List<PB_CAREERMODE_RECOMMEND_ADVANTAGE>() { data } );

                    }
                }
            }

            // PB_CAREERMODE_CHAINCONTRACT_REWARD
            foreach ( var data in context.PB_CAREERMODE_CHAINCONTRACT_REWARD.ToList() )
            {
                if ( _chainContractReward.ContainsKey( data.country ) )
                {
                    _chainContractReward[ data.country ].Add( data );
                }
                else
                {
                    _chainContractReward.Add( data.country, new List<PB_CAREERMODE_CHAINCONTRACT_REWARD>() { data } );

                }
            }

            // PB_CAREERMODE_MYTEAM_LINEUP 
            for ( int i = 0; i < _teamLineup.Length; ++i )
            {
                _teamLineup[ i ] = new Dictionary<int, List<PB_CAREERMODE_MYTEAM_LINEUP>>();
            }
            // PB_CAREERMODE_MYTEAM_LINEUP
            foreach ( var data in context.PB_CAREERMODE_MYTEAM_LINEUP.ToList() )
            {
                int difficulty = data.difficulty - 1;
                if ( _teamLineup[ difficulty ].ContainsKey( data.lineup_group_idx ) )
                {
                    _teamLineup[ difficulty ][ data.lineup_group_idx ].Add( data );
                }
                else
                {
                    _teamLineup[ difficulty ].Add( data.lineup_group_idx, new List<PB_CAREERMODE_MYTEAM_LINEUP>() { data } );
                }
            }

            // PB_CAREERMODE_OWNER_GOAL
            foreach ( var data in context.PB_CAREERMODE_OWNER_GOAL.ToList() )
            {
                _ownerGoal.Add( data.idx, data );
            }

            // PB_CAREERMODE_RANK_REWARD
            for ( int i = 0; i < _rankReward.Length; ++i )
            {
                _rankReward[ i ] = new Dictionary<int, PB_CAREERMODE_RANK_REWARD>[ CareerModeDefine.DifficultyCount ][];

                for ( int j = 0; j < _rankReward[ i ].Length; ++j )
                {
                    _rankReward[ i ][ j ] = new Dictionary<int, PB_CAREERMODE_RANK_REWARD>[ 2 ];

                    for ( int k = 0; k < _rankReward[ i ][ j ].Length; ++k )
                    {
                        _rankReward[ i ][ j ][ k ] = new Dictionary<int, PB_CAREERMODE_RANK_REWARD>();
                    }
                }
            }
            foreach ( var data in context.PB_CAREERMODE_RANK_REWARD.ToList() )
            {
                // 추천팀 추가 랭킹 보상 데이터
                if ( data.idx > 10000 )
                {
                    _recommendReward.Add( data.idx, data );
                    continue;
                }

                int contry = data.country - 1;
                int difficulty = data.difficulty - 1;
                int match_type = data.match_type - 1;
                int ranking = data.ranking;

                //유효성 일부러 안함(에러나야 문제있는지 알수있기에)
                _rankReward[ contry ][ difficulty ][ match_type ].Add( ranking, data );

            }

            // PB_CAREERMODE_STAGE_REWARD 
            for ( int i = 0; i < _stageReward.Length; ++i )
            {
                _stageReward[ i ] = new Dictionary<int, PB_CAREERMODE_STAGE_REWARD>();
            }
            foreach ( var data in context.PB_CAREERMODE_STAGE_REWARD.ToList() )
            {
                int difficulty = data.difficulty - 1;
                _stageReward[ difficulty ].Add( data.result_type, data );
            }

            // PB_CAREERMODE_MVP_REWARD
            for ( int i = 0; i < _seasonMvpReward.Length; ++i )
            {
                _seasonMvpReward[ i ] = new Dictionary<int, PB_CAREERMODE_SEASON_MVP_REWARD>();
            }
            foreach ( var data in context.PB_CAREERMODE_SEASON_MVP_REWARD.ToList() )
            {
                int difficulty = data.difficulty - 1;
                _seasonMvpReward[ difficulty ].Add( data.awards_type, data );
            }


            _springCampTrainingStatString[ 0 ] = new Dictionary<byte, string>();       //성공
            _springCampTrainingStatString[ 1 ] = new Dictionary<byte, string>();       //대성공
            foreach ( var data in context.PB_CAREER_SPRING_TRAINING.ToList() )
            {
                string statStr = "{0}/" + ( byte )SPRING_CAMP_MAIN_TYPE.STAT_UP + "," + data.stat1_type + ",{1}";
                string statStrBig = "{0}/" + ( byte )SPRING_CAMP_MAIN_TYPE.STAT_UP + "," + data.stat1_type + ",{1}*" + ( byte )SPRING_CAMP_MAIN_TYPE.OPEN_POTEN + ",0,{2}";

                _springCampTraining.Add( data.training_id, data );
                _springCampTrainingStatString[ 0 ].Add( data.training_id, statStr );
                _springCampTrainingStatString[ 1 ].Add( data.training_id, statStrBig );
            }


            foreach ( var data in context.PB_CAREER_SPRING_RESULT_BONUS.ToList() )
            {

                string statStr = "-1/" + ( byte )SPRING_CAMP_MAIN_TYPE.STAT_UP + "," + data.stat1_type + ",{0}*" + ( byte )SPRING_CAMP_MAIN_TYPE.STAT_UP + "," + data.stat2_type + ",{1}";


                _springCampResultBonus.Add( data );
                _springCampResultBonusStatString.Add( data.idx, statStr );
            }

            for ( int i = 0; i < _specialTraining.Length; ++i )
            {
                _specialTraining[ i ] = new Dictionary<byte, PB_CAREER_SPECIAL_TRAINING>();
            }

            foreach ( var data in context.PB_CAREER_SPECIAL_TRAINING.ToList() )
            {
                if ( data.appear_type == 0 )
                {
                    _specialTraining[ 0 ].Add( data.training_id, data );
                    _specialTraining[ 1 ].Add( data.training_id, data );
                }
                else if ( data.appear_type == 1 )
                {
                    _specialTraining[ 0 ].Add( data.training_id, data );
                }
                else if ( data.appear_type == 2 )
                {
                    _specialTraining[ 1 ].Add( data.training_id, data );
                }
            }

            _specialTrainingStatString.Add( ( byte )SPRING_CAMP_MAIN_TYPE.OPEN_POTEN, "{0}/" + ( byte )SPRING_CAMP_MAIN_TYPE.OPEN_POTEN + ",0,{1}" );            //{0}선수가 {1}잠재력을 얻었다
            _specialTrainingStatString.Add( ( byte )SPRING_CAMP_MAIN_TYPE.OPEN_SUB_POSITON, "{0}/" + ( byte )SPRING_CAMP_MAIN_TYPE.OPEN_SUB_POSITON + ",0,1" );  //{0}선수가 서브포지션을 열었다

            foreach (var data in context.PB_CAREERMODE_MANAGEMENT_STATIC.ToList())
            {
                switch(data.data)
                {
                    case "manage_cycle_mlb":
                        ManagementConfig.manage_cycle_mlb = data.value; break;
                    case "manage_cycle_kbo":
                        ManagementConfig.manage_cycle_kbo = data.value; break;
                    case "manage_cycle_npb":
                        ManagementConfig.manage_cycle_npb = data.value; break;
                    case "manage_cycle_cpbl":
                        ManagementConfig.manage_cycle_cpbl = data.value; break;
                    case "event_appear_new_prob":
                        ManagementConfig.event_appear_new_prob = data.value; break;
                    case "event_appear_new_max":
                        ManagementConfig.event_appear_new_max = data.value; break;
                    case "teammood_default":
                        ManagementConfig.teammood_default = data.value; break;
                    case "teammood_good_value":
                        ManagementConfig.teammood_good_value = data.value; break;
                    case "teammood_bad_value":
                        ManagementConfig.teammood_bad_value = data.value; break;
                    case "teammood_change_win":
                        ManagementConfig.teammood_change_win = data.value; break;
                    case "teammood_change_draw":
                        ManagementConfig.teammood_change_draw = data.value; break;
                    case "teammood_change_lose":
                        ManagementConfig.teammood_change_lose = data.value; break;
                    case "InstantHeal_CostType":
                        ManagementConfig.InstantHeal_CostType = data.value; break;
                    case "InstantHeal_Cost":
                        ManagementConfig.InstantHeal_Cost = data.value; break;
                    case "injury_appear_new_prob":
                        ManagementConfig.injury_appear_new_prob = data.value; break;
                    case "injury_appear_new_max":
                        ManagementConfig.injury_appear_new_max = data.value; break;
                    case "injury_appear_new_cootime":
                        ManagementConfig.injury_appear_new_cootime = data.value; break;
                    case "injury_appear_chain_max":
                        ManagementConfig.injury_appear_chain_max = data.value; break;
                    case "injury_appear_chain_cootime":
                        ManagementConfig.injury_appear_chain_cootime = data.value; break;
                    case "injury_have_max_group1":
                        ManagementConfig.injury_have_max_group1 = data.value; break;
                    case "injury_have_max_group2":
                        ManagementConfig.injury_have_max_group2 = data.value; break;
                    case "injury_have_max_group3":
                        ManagementConfig.injury_have_max_group3 = data.value; break;
                    case "injury_have_max_group4":
                        ManagementConfig.injury_have_max_group4 = data.value; break;
                    case "condition_notice_best":
                        ManagementConfig.condition_notice_best = data.value; break;
                    case "condition_notice_worst":
                        ManagementConfig.condition_notice_worst = data.value; break;
                    default :   return false;
                }
            }

            for (int i = 0; i < _newInjury.Length; ++i)
            {
                _newInjury[i] = new List<PB_CAREERMODE_MANAGEMENT_INJURY>();
            }

            foreach (var data in context.PB_CAREERMODE_MANAGEMENT_INJURY.ToList())
            {
                _allInjury.Add(data.idx, data);

                if (data.injury_type == 0)
                {
                    _newInjuryTotalRate[data.injury_group - 1] += data.ratio;
                    _newInjury[data.injury_group - 1].Add(data);
                }
            }

            //연계 인덱스 다 있는지 유효성 체크
            foreach (var data in context.PB_CAREERMODE_MANAGEMENT_INJURY.ToList())
            {
                if (data.next_injury_idx != 0)
                {
                    if (_allInjury.ContainsKey(data.next_injury_idx) == false || data.injury_group == _allInjury[data.next_injury_idx].injury_group)
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < _moodEvent.Length; ++i)
            {
                _moodEvent[i] = new List<PB_CAREERMODE_MANAGEMENT_EVENT>();
                _moodEventRate[i] = new List<int>();
            }

            int[] _tempMoodRate = new int[3];

            foreach (var data in context.PB_CAREERMODE_MANAGEMENT_EVENT.ToList())
            {
                _allEvent.Add(data.idx, data);

                if (data.ratio_mood_good > 0)
                {
                    _tempMoodRate[0] += data.ratio_mood_good;
                    _moodEventRate[0].Add(_tempMoodRate[0]);
                    _moodEvent[0].Add(data);
                }

                if (data.ratio_mood_normal > 0)
                {
                    _tempMoodRate[1] += data.ratio_mood_normal;
                    _moodEventRate[1].Add(_tempMoodRate[1]);
                    _moodEvent[1].Add(data);
                }

                if (data.ratio_mood_bad > 0)
                {
                    _tempMoodRate[2] += data.ratio_mood_bad;
                    _moodEventRate[2].Add(_tempMoodRate[2]);
                    _moodEvent[2].Add(data);
                }
            }


            return true;
        }

        public void CheckNewOccurInjury(List<PlayerCareerPlayingInfo> gameRunPlayers, ref CareerModeInfo careerMode, ref List<PlayerCareerPlayingInfo> updatePlayerInfos, ref List<PlayerCareerInjuryInfo> injuryPlayerInfo)
        {
            //신규 부상 쿨타임 체크
            if (careerMode.injury_game_no_new != 0 && (careerMode.game_no - careerMode.injury_game_no_new < ManagementConfig.injury_appear_new_cootime))
                return;

            //신규 부상 발생 확률 체크
            if (RandomManager.Instance.GetCount(CareerModeDefine.EventTotalRate) > ManagementConfig.injury_appear_new_prob)
                return;

            //신규부상 가능한 유저수 산출
            List<PlayerCareerPlayingInfo> possiblePlayer = gameRunPlayers.FindAll(x => x.injury_idx == 0);
            if (possiblePlayer == null || possiblePlayer.Count == 0)
                return;

            //몇명 발생하는지 체크(한번에 발생할수 있는 인원이 발생가능한 인원보다 크다면 발생가능할수 있는 인원중에서 구하기)
            int newOccurCount = RandomManager.Instance.GetCount((possiblePlayer.Count < ManagementConfig.injury_appear_new_max )? possiblePlayer.Count : ManagementConfig.injury_appear_new_max);

            //랜덤으로 섞기
            possiblePlayer.ShuffleForSelectedCount(newOccurCount);

            //그룹별 가능한 부상선수 수 체크
            int[] remainGroupCount = new int[CareerModeDefine.InjuryGroupCount]
            {
                ManagementConfig.injury_have_max_group1 - careerMode.injury_group1,
                ManagementConfig.injury_have_max_group2 - careerMode.injury_group2,
                ManagementConfig.injury_have_max_group3 - careerMode.injury_group3,
                ManagementConfig.injury_have_max_group4 - careerMode.injury_group4
            };

            for (int i = 0; i < newOccurCount; ++i)
            {
                int maxRandVal = 0;
                for(int k = 0; k < remainGroupCount.Length; ++k)
                {
                    if (remainGroupCount[k] > 0)
                        maxRandVal += _newInjuryTotalRate[k];
                }
                
                //그룹별 최대 갯수가 맥스라면 더이상 진행않음
                if (maxRandVal == 0)
                    break;

                //부상 인덱스 구하기(신규 중 가능한것 확률 모두 더한것 중 랜덤값 산출)
                int randVal = RandomManager.Instance.GetIndex(maxRandVal);
                int start = 0;
                PB_CAREERMODE_MANAGEMENT_INJURY selectInjury = null;

                for (int grade = 0; grade < _newInjury.Length; ++grade)
                {
                    for(int idx = 0; idx < _newInjury[grade].Count; ++idx)
                    {
                        int end = start + _newInjury[grade][idx].ratio;

                        if (randVal < end)
                        {
                            selectInjury = _newInjury[grade][idx];
                            --remainGroupCount[grade];

                            if (grade == 0)
                                ++careerMode.injury_group1;
                            else if (grade == 1)
                                ++careerMode.injury_group2;
                            else if (grade == 2)
                                ++careerMode.injury_group3;
                            else if (grade == 3)
                                ++careerMode.injury_group4;


                            careerMode.injury_game_no_new = careerMode.game_no;
                            break;
                        }

                        start = end;
                    }

                    if (selectInjury != null)
                        break;
                }

                PlayerCareerPlayingInfo player = updatePlayerInfos.Find(x => x.account_player_idx == possiblePlayer[i].account_player_idx);
                
                if(player == null)
                {
                    player = new PlayerCareerPlayingInfo()
                    {
                        account_player_idx = possiblePlayer[i].account_player_idx,
                        injury_idx = selectInjury.idx,
                        injury_period = (byte)RandomManager.Instance.GetCount(selectInjury.period_min, selectInjury.period_max),
                        injury_add_ratio = 0,
                        injury_cure_no = 0,
                        health_game_no = possiblePlayer[i].health_game_no,
                        player_health = possiblePlayer[i].player_health
                    };

                    updatePlayerInfos.Add(player);
                }
                else
                {
                    player.injury_idx = selectInjury.idx;
                    player.injury_period = (byte)RandomManager.Instance.GetCount(selectInjury.period_min, selectInjury.period_max);
                    player.injury_add_ratio = 0;
                    player.injury_cure_no = 0;
                }

                injuryPlayerInfo.Add(player);
            }
        }

        public void CheckChainOccurInjury(List<PlayerCareerPlayingInfo> gameRunPlayers, ref CareerModeInfo careerMode, ref List<PlayerCareerPlayingInfo> updatePlayerInfos, ref List<PlayerCareerInjuryInfo> injuryPlayerInfo)
        {
            //연계 부상 쿨타임 체크
            if (careerMode.injury_game_no_chain != 0 && (careerMode.game_no - careerMode.injury_game_no_chain < ManagementConfig.injury_appear_chain_cootime))
                return;

            //연계부상 가능한 유저수 산출 (현재 부상중이면서 다음 연계부상이 존재하는 경우)
            List<PlayerCareerPlayingInfo> possiblePlayer = gameRunPlayers.FindAll(x => x.injury_idx > 0 && _allInjury[x.injury_idx].next_injury_idx != 0);
            if (possiblePlayer == null || possiblePlayer.Count == 0)
                return;

            //몇명 발생하는지 체크(한번에 발생할수 있는 인원이 발생가능한 인원보다 크다면 발생가능할수 있는 인원중에서 구하기)
            int chainOccurCount = RandomManager.Instance.GetCount((possiblePlayer.Count < ManagementConfig.injury_appear_chain_max) ? possiblePlayer.Count : ManagementConfig.injury_appear_chain_max);

            //약한 부상 우선순위로 재정렬
            possiblePlayer = possiblePlayer.OrderBy(x => _allInjury[x.injury_idx].injury_group).ToList();

            //그룹별 가능한 부상선수 수 체크
            int[] remainGroupCount = new int[CareerModeDefine.InjuryGroupCount]
            {
                ManagementConfig.injury_have_max_group1 - careerMode.injury_group1,
                ManagementConfig.injury_have_max_group2 - careerMode.injury_group2,
                ManagementConfig.injury_have_max_group3 - careerMode.injury_group3,
                ManagementConfig.injury_have_max_group4 - careerMode.injury_group4
            };

            int finishCount = 0;
            for (int i = 0; i < possiblePlayer.Count; ++ i)
            {
                //연계가 되었을때의 정보를 구한다             
                PB_CAREERMODE_MANAGEMENT_INJURY nowInjuryInfo = _allInjury[possiblePlayer[i].injury_idx];

                PlayerCareerPlayingInfo player = updatePlayerInfos.Find(x => x.account_player_idx == possiblePlayer[i].account_player_idx);

                if (player == null)
                {
                    player = new PlayerCareerPlayingInfo()
                    {
                        account_player_idx = possiblePlayer[i].account_player_idx,
                        injury_idx = possiblePlayer[i].injury_idx,
                        injury_period = possiblePlayer[i].injury_period,
                        injury_add_ratio = possiblePlayer[i].injury_add_ratio + nowInjuryInfo.next_injury_prob_add,
                        injury_cure_no = possiblePlayer[i].injury_cure_no,
                        health_game_no = possiblePlayer[i].health_game_no,
                        player_health = possiblePlayer[i].player_health
                    };

                    updatePlayerInfos.Add(player);
                }
                else
                {
                    player.injury_add_ratio = possiblePlayer[i].injury_add_ratio + nowInjuryInfo.next_injury_prob_add;
                }

                if (finishCount < chainOccurCount)
                {
                    PB_CAREERMODE_MANAGEMENT_INJURY nextInjuryInfo = _allInjury[nowInjuryInfo.next_injury_idx];

                    //해당그룹 최대갯수 넘는지 체크
                    if (remainGroupCount[nextInjuryInfo.injury_group - 1] <= 0)
                    {
                        continue;
                    }

                    //부상발생 체크(연계 부상확률  + 누적 추가확률)
                    if (RandomManager.Instance.GetIndex(CareerModeDefine.EventTotalRate) >= nowInjuryInfo.next_injury_prob + possiblePlayer[i].injury_add_ratio)
                        continue;


                    //연계 대상이 내가 아니라면 몇명인지 추출 (지우는 방향이긴한데 아직 확정이 아니므로 코드 삭제하지말자)
                    //신규부상 줘야되면 아애 위에서부터 "해당그룹 최대갯수 넘는지 체크" 여기부터 인원수만큼 체크 다시 시작해야됨! 완전 분기
                    //또한 신규 부상시 accountPlayerInjuryInfos 이 리스트에 있는 사람은 제외하도록 수정해야함 - 일이 많아짐
                    //if(nowInjuryInfo.next_injury_target == 1 )
                    //{
                    //    int newInjuryCnt = Common.FunctionManager.GetProbabilityNumber(nowInjuryInfo.next_injury_target_value_min, nowInjuryInfo.next_injury_target_value_max);
                    //}

                    //연계 부상 발생!!!
                    //if (nowInjuryInfo.injury_group != nextInjuryInfo.injury_group)        미리 table셋팅에서에서 next랑 같지 않는거 체크 넣었으므로 빼도 될듯
                    //{
                        //가능한 현재 부상 그룹 인원 한개 증가
                        ++remainGroupCount[nowInjuryInfo.injury_group - 1];

                        if (nowInjuryInfo.injury_group == 1)
                            --careerMode.injury_group1;
                        else if (nowInjuryInfo.injury_group == 2)
                            --careerMode.injury_group2;
                        else if (nowInjuryInfo.injury_group == 3)
                            --careerMode.injury_group3;
                        else if (nowInjuryInfo.injury_group == 4)
                            --careerMode.injury_group4;

                        //가능한 다음 부상 그룹 인원 한개 축소
                        --remainGroupCount[nextInjuryInfo.injury_group - 1];

                        if (nextInjuryInfo.injury_group == 1)
                            ++careerMode.injury_group1;
                        else if (nextInjuryInfo.injury_group == 2)
                            ++careerMode.injury_group2;
                        else if (nextInjuryInfo.injury_group == 3)
                            ++careerMode.injury_group3;
                        else if (nextInjuryInfo.injury_group == 4)
                            ++careerMode.injury_group4;
                    //}

                    player.injury_idx = nextInjuryInfo.idx;
                    player.injury_period = (byte)RandomManager.Instance.GetCount(nextInjuryInfo.period_min, nextInjuryInfo.period_max);
                    player.injury_add_ratio = 0;
                    player.injury_cure_no = 0;

                    injuryPlayerInfo.Add(player);

                    careerMode.injury_game_no_chain = careerMode.game_no;
                    ++finishCount;
                }
            }
        }


        public bool CreateTeam( int teamIdx, byte gameLevel, out List<CareerModePlayer> players )
        {
            players = new List<CareerModePlayer>();

            if ( _teamLineup[ gameLevel - 1 ].ContainsKey( teamIdx ) == false )
            {
                _logger.Error( "Not found team lineup - teamIdx:{0}", teamIdx );
                return false;
            }

            PBPlayer pData;

            foreach ( PB_CAREERMODE_MYTEAM_LINEUP item in _teamLineup[ gameLevel - 1 ][ teamIdx ] )
            {
                if ( item.isuse == 0 || item.player_type == ( byte )PLAYER_TYPE.TYPE_COACH )
                    continue;

                pData = CacheManager.PBTable.PlayerTable.GetPlayerData( item.player_idx );

                if ( pData == null )
                {
                    _logger.Error( "Not found player data - teamIdx:{0}, playerIdx:{1}", teamIdx, item.player_idx );
                    return false;
                }

                players.Add( new CareerModePlayer()
                {
                    account_player_idx = item.serial_index,
                    player_type = pData.player_type,
                    player_idx = item.player_idx,
                    is_starting = ( byte )( item.lineup_type == 0 ? 0 : 1 ),
                    position = item.position,
                    order = item.order,
                    reinforce_grade = item.player_reinforce_grade,
                    player_health = pData.player_health,
                    potential_idx1 = 0,     //item.player_potential_1,
                    potential_idx2 = 0,     //item.player_potential_2,
                    potential_idx3 = 0,     //item.player_potential_3,
                    sub_pos_open = 0,
                    rhythm_type = 0               
                });
            }

            if ( players.Count == 0 )
            {
                _logger.Error( "create player count zero - teamIdx:{0}", teamIdx );
                return false;
            }

            return true;
        }

        public bool StageRewardInfo( byte gameLevel, byte gameResult, bool isMvpMyTeam, out List<GameRewardInfo> stageRewardList, out GameRewardInfo mvpReward, out int addManagerExp )
        {
            stageRewardList = null;
            mvpReward = null;
            addManagerExp = 0;

            if ( _stageReward.Length < gameLevel )
            {
                return false;
            }

            bool isFind = _stageReward[ gameLevel - 1 ].TryGetValue( gameResult, out PB_CAREERMODE_STAGE_REWARD reward );
            if ( isFind == false )
            {
                return false;
            }

            stageRewardList = new List<GameRewardInfo>();

            if ( reward.reward_type1 > 0 )
                stageRewardList.Add( new GameRewardInfo( reward.reward_type1, reward.reward_idx1, reward.reward_count1 ) );

            if ( reward.reward_type2 > 0 )
                stageRewardList.Add( new GameRewardInfo( reward.reward_type2, reward.reward_idx2, reward.reward_count2 ) );

            if ( reward.reward_type3 > 0 )
                stageRewardList.Add( new GameRewardInfo( reward.reward_type3, reward.reward_idx3, reward.reward_count3 ) );

            if ( reward.reward_type4 > 0 )
                stageRewardList.Add( new GameRewardInfo( reward.reward_type4, reward.reward_idx4, reward.reward_count4 ) );

            addManagerExp = reward.exp_manager;

            if ( isMvpMyTeam )
            {
                mvpReward = new GameRewardInfo( reward.stage_mvp_reward_type, reward.stage_mvp_reward_idx, reward.stage_mvp_reward_count );
            }

            return true;
        }

        //public bool SeasonRewardInfo(byte country, byte gameLevel, byte matchGroup, byte rank, List<long> ListSeasonAwardPlayers,
        //                                out List<GameRewardInfo> rankRewardList, out List<GameRewardInfo> mvpRewardList)
        public bool SeasonRewardInfo( CareerModeInfo careerMode, byte rank, List<int> ListSeasonAwardPlayers,
                                        out List<GameRewardInfo> rankRewardList, out List<GameRewardInfo> mvpRewardList )
        {

            rankRewardList = null;
            mvpRewardList = null;

            if ( _rankReward.Length < careerMode.country_type )
                return false;

            if ( _rankReward[ careerMode.country_type - 1 ].Length < careerMode.mode_level )
                return false;

            if ( _rankReward[ careerMode.country_type - 1 ][ careerMode.mode_level - 1 ].Length < careerMode.match_group )
                return false;

            if ( true == _rankReward[ careerMode.country_type - 1 ][ careerMode.mode_level - 1 ][ careerMode.match_group - 1 ].TryGetValue( rank, out PB_CAREERMODE_RANK_REWARD reward ) )
            {
                rankRewardList = new List<GameRewardInfo>();

                if ( reward.reward_type1 > 0 )
                    rankRewardList.Add( new GameRewardInfo( reward.reward_type1, reward.reward_idx1, reward.reward_count1 ) );

                if ( reward.reward_type2 > 0 )
                    rankRewardList.Add( new GameRewardInfo( reward.reward_type2, reward.reward_idx2, reward.reward_count2 ) );

                if ( reward.reward_type3 > 0 )
                    rankRewardList.Add( new GameRewardInfo( reward.reward_type3, reward.reward_idx3, reward.reward_count3 ) );

                if ( reward.reward_type4 > 0 )
                    rankRewardList.Add( new GameRewardInfo( reward.reward_type4, reward.reward_idx4, reward.reward_count4 ) );

                if ( rankRewardList.Count == 0 )
                    rankRewardList = null;
            }

            if ( ListSeasonAwardPlayers != null && ListSeasonAwardPlayers.Count > 0 )
            {
                if ( _seasonMvpReward.Length < careerMode.mode_level )
                    return false;

                mvpRewardList = new List<GameRewardInfo>();

                for ( int i = 0; i < ListSeasonAwardPlayers.Count; ++i )
                {
                    if ( false == _seasonMvpReward[ careerMode.mode_level - 1 ].ContainsKey( ListSeasonAwardPlayers[ i ] ) )
                        return false;

                    mvpRewardList.Add( new GameRewardInfo( ListSeasonAwardPlayers[ i ],
                                                        _seasonMvpReward[ careerMode.mode_level - 1 ][ ListSeasonAwardPlayers[ i ] ].reward_type,
                                                        _seasonMvpReward[ careerMode.mode_level - 1 ][ ListSeasonAwardPlayers[ i ] ].reward_idx,
                                                        _seasonMvpReward[ careerMode.mode_level - 1 ][ ListSeasonAwardPlayers[ i ] ].reward_count ) );
                }
            }

            return true;
        }

        public ErrorCode SpringCampSetTraining( byte trainingId, List<Player> players, List<long> alreadyPlayerList, ref List<CareerModeSpringCamp> springCampInfo, ref List<AccountPlayerTrainingInfo> potenPlayerInfo )
        {
            if ( _springCampTraining.ContainsKey( trainingId ) == false )
                return ErrorCode.ERROR_NOT_FOUND_SPRINGCAMP_DATA;

            PB_CAREER_SPRING_TRAINING springCampTraining = _springCampTraining[ trainingId ];

            //기획 데이터의 인원수와 요청온 인원수가 맞지 않다면 에러
            if ( springCampTraining.max_count != players.Count )
            {
                return ErrorCode.ERROR_INVALID_PARAM;
            }

            string detailStr = "";
            //재시도가 아닐때는 선수의 타입이랑 포지션 확인
            foreach ( Player p in players )
            {
                //이미 진행한 선수인지 체크
                if ( true == alreadyPlayerList.Contains( p.account_player_idx ) )
                {
                    return ErrorCode.ERROR_ALREADY_SPRINGCAMP_PLAYER;
                }

                //해당 훈련의 선수 타입 체크
                if ( springCampTraining.target_type != p.player_type )
                {
                    return ErrorCode.ERROR_NOT_MATCHING_PLAYER_TYPE;
                }

                //증가 스탯 구하기
                int upStat = RandomManager.Instance.GetCount( springCampTraining.stat1_value_min, springCampTraining.stat1_value_max );

                //대성공 여부
                bool bigResult = RandomManager.Instance.IsSuccessRatio( springCampTraining.bonus_ratio, CareerModeDefine.SpringCampTotalRate );

                if ( detailStr != "" )
                    detailStr += "|";

                if ( bigResult == true )
                {
                    //잠재력 인덱스 구하기
                    int potenIdx = CacheManager.PBTable.PlayerTable.PlayerPotentialCreateIdx(GAME_MODETYPE.MODE_CAREERMODE, ( PLAYER_TYPE )p.player_type, null );

                    potenPlayerInfo.Add( new AccountPlayerTrainingInfo()
                    {
                        account_player_idx = p.account_player_idx,
                        potential_idx1 = potenIdx,
                        potential_idx2 = 0,
                        potential_idx3 = 0,
                        sub_pos_open = 0

                    } );

                    detailStr += string.Format( _springCampTrainingStatString[ 1 ][ trainingId ], p.account_player_idx, upStat, potenIdx );
                }
                else
                {
                    detailStr += string.Format( _springCampTrainingStatString[ 0 ][ trainingId ], p.account_player_idx, upStat );
                }
            }

            springCampInfo.Add( new CareerModeSpringCamp()
            {
                step = ( byte )SPRING_CAMP_STEP.STEP_TRAINING,
                training_type = trainingId,
                detail_info = detailStr
            } );

            return ErrorCode.SUCCESS;
        }

        public ErrorCode SpringCampSetBonus( ref List<CareerModeSpringCamp> spingCampInfo )
        {
            PB_CAREER_SPRING_RESULT_BONUS springCampBonus = _springCampResultBonus[RandomManager.Instance.GetIndex( _springCampResultBonus.Count ) ];
            int stat1 = RandomManager.Instance.GetCount( springCampBonus.stat1_value_min, springCampBonus.stat1_value_max );
            int stat2 = RandomManager.Instance.GetCount( springCampBonus.stat2_value_min, springCampBonus.stat2_value_max );

            spingCampInfo.Add( new CareerModeSpringCamp()
            {
                step = ( byte )SPRING_CAMP_STEP.STEP_TEAM_BONUS,
                training_type = springCampBonus.idx,
                detail_info = string.Format( _springCampResultBonusStatString[ springCampBonus.idx ], stat1, stat2 )
            } );

            return ErrorCode.SUCCESS;
        }

        // nationalCode는 계정 생성시 아직 정해진건 없어서 추후에 글로벌 계정 처리시 확인후 진행..
        public List<CareerModeCreateTeamInfo> CreateTeamList( byte nationType, byte previousContract = 0 )
        {
            List<CareerModeCreateTeamInfo> createTeamList = new List<CareerModeCreateTeamInfo>();

            const int firstTierNo = 1;
            Random random = new Random();

            // 추천팀 정보 가져옴
            List<int> kboTeam = _leagueTeamGroup[ ( byte )LEAGUE_TYPE.KBO ]
                .GroupBy( g => g.team_tier )
                .Select( s => s.ElementAt( random.Next( s.Count() ) ) )
                .Select( r => r.team_idx ).ToList();
            List<int> mlbAmericanTeam = _leagueTeamGroup[ ( byte )LEAGUE_TYPE.AMERICAN ]
                .GroupBy( g => g.team_tier )
                .Select( s => s.ElementAt( random.Next( s.Count() ) ) )
                .Select( r => r.team_idx ).ToList();
            List<int> mlbNationalTeam = _leagueTeamGroup[ ( byte )LEAGUE_TYPE.NATIONAL ]
                .GroupBy( g => g.team_tier )
                .Select( s => s.ElementAt( random.Next( s.Count() ) ) )
                .Select( r => r.team_idx ).ToList();
            List<int> nplTeam = _leagueTeamGroup[ ( byte )LEAGUE_TYPE.CENTRAL ]
                .Concat( _leagueTeamGroup[ ( byte )LEAGUE_TYPE.PACIFIC ] )
                .GroupBy( g => g.team_tier )
                .Select( s => s.ElementAt( random.Next( s.Count() ) ) )
                .Select( r => r.team_idx ).ToList();
            List<int> cpblTeam = _leagueTeamGroup[ ( byte )LEAGUE_TYPE.CPB ]
                .Where( x => x.team_tier != firstTierNo )
                .Select( r => r.team_idx ).ToList();
            cpblTeam.RemoveAt( random.Next( cpblTeam.Count ) );

            // 거절할 경우 추천팀 1티어 추가로 보임
            if ( previousContract == ( byte )CONTRACT_TYPE.REJECT )
            {
                AddTeamTier( ( byte )LEAGUE_TYPE.KBO, firstTierNo, kboTeam );
                AddTeamTier( ( byte )LEAGUE_TYPE.AMERICAN, firstTierNo, mlbAmericanTeam );
                AddTeamTier( ( byte )LEAGUE_TYPE.NATIONAL, firstTierNo, mlbNationalTeam );
                AddTeamTier( ( byte )LEAGUE_TYPE.CENTRAL, firstTierNo, nplTeam );
                AddTeamTier( ( byte )LEAGUE_TYPE.PACIFIC, firstTierNo, nplTeam );
                AddTeamTier( ( byte )LEAGUE_TYPE.CPB, firstTierNo, cpblTeam );
            }

            // 추천 팀 보상
            Dictionary<int, RecommendTeamInfo> recommendTeams = new Dictionary<int, RecommendTeamInfo>();
            foreach ( var teamIdx in kboTeam )
            {
                recommendTeams.Add( teamIdx, GetRecommendTeamWithAdvantage( ( byte )NATION_LEAGUE_TYPE.KBO, random ) );
            }
            foreach ( var teamIdx in mlbAmericanTeam )
            {
                recommendTeams.Add( teamIdx, GetRecommendTeamWithAdvantage( ( byte )NATION_LEAGUE_TYPE.MLB, random ) );
            }
            foreach ( var teamIdx in mlbNationalTeam )
            {
                recommendTeams.Add( teamIdx, GetRecommendTeamWithAdvantage( ( byte )NATION_LEAGUE_TYPE.MLB, random ) );
            }
            foreach ( var teamIdx in nplTeam )
            {
                recommendTeams.Add( teamIdx, GetRecommendTeamWithAdvantage( ( byte )NATION_LEAGUE_TYPE.NPB, random ) );
            }
            foreach ( var teamIdx in cpblTeam )
            {
                recommendTeams.Add( teamIdx, GetRecommendTeamWithAdvantage( ( byte )NATION_LEAGUE_TYPE.CPB, random ) );
            }

            foreach ( var createTeam in _teamGroup )
            {
                // 계약 미션 정보 가져옴
                List<int> missionList = GetPennantraceMissionList();

                CareerModeCreateTeamInfo createTeamInfo = new CareerModeCreateTeamInfo
                {
                    team_idx = createTeam.team_idx,
                    contract_no = CareerModeDefine.DefaultContractNo,
                    mission_list = missionList
                };

                if ( recommendTeams.ContainsKey( createTeam.team_idx ) )
                {
                    createTeamInfo.recommend_team_info = recommendTeams[ createTeam.team_idx ];
                }

                createTeamList.Add( createTeamInfo );
            }

            return createTeamList;
        }

        public List<int> GetPostMissionList()
        {
            return _ownerGoal.Where(t => t.Value.owner_goal_type > CareerModeDefine.OwnerGoalPostDivisionType)
                .GroupBy(g => g.Value.owner_goal_type)
                .Select(s => s.ElementAt(RandomManager.Instance.GetIndex(s.Count())))
                .Select(r => r.Key).ToList();
        }

        public List<int> GetPennantraceMissionList()
        {
            return _ownerGoal.Where(t => t.Value.owner_goal_type < CareerModeDefine.OwnerGoalPostDivisionType)
                .GroupBy(g => g.Value.owner_goal_type)
                .Select(s => s.ElementAt(RandomManager.Instance.GetIndex(s.Count())))
                .Select(r => r.Key).ToList();
        }

        private void AddTeamTier( byte leagueType, int tierNo, List<int> recommendTeamList )
        {
            var addedTier = _leagueTeamGroup[ leagueType ].Where( x => x.team_tier == tierNo );
            foreach ( var tier in addedTier )
            {
                if ( recommendTeamList.Contains( tier.team_idx ) == false )
                {
                    recommendTeamList.Add( tier.team_idx );
                    return;
                }
            }
        }

        private RecommendTeamInfo GetRecommendTeamWithAdvantage( byte countryType, Random random )
        {
            int buffIdx = 0;
            int rewardIdx = 0;
            if ( _recommendBuffAdvantage.ContainsKey( countryType ) )
            {
                buffIdx = _recommendBuffAdvantage[ countryType ].ElementAt( random.Next( _recommendBuffAdvantage[ countryType ].Count ) ).idx;
            }
            else
            {
                buffIdx = _recommendBuffAdvantage[ ( byte )NATION_LEAGUE_TYPE.WHATEVER ].ElementAt( random.Next( _recommendBuffAdvantage[ ( byte )NATION_LEAGUE_TYPE.WHATEVER ].Count ) ).idx;
            }
            if ( _recommendRewardAdvantage.ContainsKey( countryType ) )
            {
                rewardIdx = _recommendRewardAdvantage[ countryType ].ElementAt( random.Next( _recommendRewardAdvantage[ countryType ].Count ) ).idx;
            }
            else
            {
                rewardIdx = _recommendRewardAdvantage[ ( byte )NATION_LEAGUE_TYPE.WHATEVER ].ElementAt( random.Next( _recommendRewardAdvantage[ ( byte )NATION_LEAGUE_TYPE.WHATEVER ].Count ) ).idx;
            }

            return new RecommendTeamInfo() { buff_val = buffIdx, reward_idx = rewardIdx };
        }

        public List<GameRewardInfo> GetChainContractReward( byte countryType, byte recontractCount )
        {
            List<GameRewardInfo> rewardInfos = null;

            for ( int i = 1; i <= recontractCount; ++i )
            {
                PB_CAREERMODE_CHAINCONTRACT_REWARD reward = null;

                if ( _chainContractReward.ContainsKey( countryType ) )
                {
                    reward = _chainContractReward[ countryType ].Find( x => x.contract_count == i );
                }
                else
                {
                    reward = _chainContractReward[ ( byte )NATION_LEAGUE_TYPE.WHATEVER ].Find( x => x.contract_count == i );
                }
                if ( reward == null )
                {
                    continue;
                }

                if ( rewardInfos == null )
                {
                    rewardInfos = new List<GameRewardInfo>();
                }

                if ( reward.reward_type1 > 0 )
                {
                    rewardInfos.Add( new GameRewardInfo() { etc_info = i, reward_type = reward.reward_type1, reward_idx = reward.reward_idx1, reward_cnt = reward.reward_count1 } );
                }
                if ( reward.reward_type2 > 0 )
                {
                    rewardInfos.Add( new GameRewardInfo() { etc_info = i, reward_type = reward.reward_type2, reward_idx = reward.reward_idx2, reward_cnt = reward.reward_count2 } );
                }
                if ( reward.reward_type3 > 0 )
                {
                    rewardInfos.Add( new GameRewardInfo() { etc_info = i, reward_type = reward.reward_type3, reward_idx = reward.reward_idx3, reward_cnt = reward.reward_count3 } );
                }
            }

            return rewardInfos;
        }

        public bool IsCompleteMission( List<CareerModeMission> careerModeMission )
        {
            int complteCount = 0;
            foreach ( var mission in careerModeMission )
            {
                if ( mission.complete_flag > 0 )
                {
                    ++complteCount;
                }
            }

            if ( complteCount >= CareerModeDefine.MissionCompleteCount )
            {
                return true;
            }

            return false;
        }

        public List<GameRewardInfo> GetMissionRewardInfo( List<CareerModeMission> careerModeMission, out bool isComplteMission )
        {
            isComplteMission = false;

            List<GameRewardInfo> rewardInfos = null;

            int complteCount = 0;
            foreach ( var mission in careerModeMission )
            {
                if ( mission.complete_flag > 0 )
                {
                    ++complteCount;

                    // 이미 미션 보상을 받음
                    if ( mission.reward_flag != 0 )
                    {
                        continue;
                    }

                    if ( rewardInfos == null )
                    {
                        rewardInfos = new List<GameRewardInfo>();
                    }

                    PB_CAREERMODE_OWNER_GOAL goal = _ownerGoal[ mission.mission_idx ];

                    if ( goal.reward_type1 > 0 )
                    {
                        rewardInfos.Add( new GameRewardInfo() { reward_type = goal.reward_type1, reward_idx = goal.reward_idx1, reward_cnt = goal.reward_count1 } );
                    }
                    if ( goal.reward_type2 > 0 )
                    {
                        rewardInfos.Add( new GameRewardInfo() { reward_type = goal.reward_type2, reward_idx = goal.reward_idx2, reward_cnt = goal.reward_count2 } );
                    }
                    if ( goal.reward_type3 > 0 )
                    {
                        rewardInfos.Add( new GameRewardInfo() { reward_type = goal.reward_type3, reward_idx = goal.reward_idx3, reward_cnt = goal.reward_count3 } );
                    }

                }
            }

            if ( complteCount >= CareerModeDefine.MissionCompleteCount )
            {
                isComplteMission = true;
            }

            return rewardInfos;
        }

        public List<GameRewardInfo> GetRecommendTeamRewrad( int rewardIdx )
        {
            if ( _recommendAdvantage.ContainsKey( rewardIdx ) == false )
            {
                return null;
            }
            PB_CAREERMODE_RECOMMEND_ADVANTAGE advantage = _recommendAdvantage[ rewardIdx ];

            if ( _recommendReward.ContainsKey( advantage.reference_idx ) == false )
            {
                return null;
            }

            PB_CAREERMODE_RANK_REWARD reward = _recommendReward[ advantage.reference_idx ];

            List<GameRewardInfo> rewardInfos = new List<GameRewardInfo>();

            if ( reward.reward_type1 > 0 )
            {
                rewardInfos.Add( new GameRewardInfo() { reward_type = reward.reward_type1, reward_idx = reward.reward_idx1, reward_cnt = reward.reward_count1 } );
            }
            if ( reward.reward_type2 > 0 )
            {
                rewardInfos.Add( new GameRewardInfo() { reward_type = reward.reward_type2, reward_idx = reward.reward_idx2, reward_cnt = reward.reward_count2 } );
            }
            if ( reward.reward_type3 > 0 )
            {
                rewardInfos.Add( new GameRewardInfo() { reward_type = reward.reward_type3, reward_idx = reward.reward_idx3, reward_cnt = reward.reward_count3 } );
            }
            if ( reward.reward_type4 > 0 )
            {
                rewardInfos.Add( new GameRewardInfo() { reward_type = reward.reward_type4, reward_idx = reward.reward_idx4, reward_cnt = reward.reward_count4 } );
            }

            return rewardInfos;
        }

        public List<CareerModeMission> GetUpdatedPostSeasonMission(List<CareerModeMission> careerModeMissions, byte rank)
        {
            List<CareerModeMission> resultMissions = null;

            foreach (var mission in careerModeMissions)
            {
                switch (_ownerGoal[mission.mission_idx].action_type)
                {
                    // 포스트 우승
                    case ActionTypeDefine.PostRankWinner:
                    {
                        if (rank == 1)
                        {
                            mission.complete_flag = 1;
                            AddMission(mission, ref resultMissions);
                        }

                        break;
                    }
                    // 포스트 준우승
                    case ActionTypeDefine.PostRankSecondWinner:
                    {
                        if (rank == 2)
                        {
                            mission.complete_flag = 1;
                            AddMission(mission, ref resultMissions);
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
            return resultMissions;

        }
        public List<CareerModeMission> GetUpdatedPennantraceSeasonMission(List<CareerModeMission> careerModeMissions, byte rank, byte titlePlayerCount, bool isEntrancePost)
        {
            List<CareerModeMission> resultMissions = null;

            foreach (var mission in careerModeMissions)
            {
                switch (_ownerGoal[mission.mission_idx].action_type)
                {
                    // 팀순위
                    case ActionTypeDefine.PennantraceLastRank:
                    {
                        mission.count = rank;
                        CheckRankMission(mission, ref resultMissions);
                        break;
                    }
                    // 정규시즌 타이틀 홀더 선수
                    case ActionTypeDefine.PennantraceTitlePlayerCount:
                    {
                        mission.count = titlePlayerCount;
                        CheckAboveMission(mission, ref resultMissions);
                        break;
                    }
                    // 포스트 진출
                    case ActionTypeDefine.EntrancePostSeason:
                    {
                        if (isEntrancePost == true)
                        {
                            mission.complete_flag = 1;
                            AddMission(mission, ref resultMissions);
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
            return resultMissions;
        }

        public List<CareerModeMission> GetUpdatedCareerRecordMission( List<CareerModeMission> careerModeMissions, byte matchGroup, int commendCount, byte gameResult, string teamRecord)
        {
            List<CareerModeMission> resultMissions = null;

            foreach ( var mission in careerModeMissions )
            {
                if(matchGroup == (byte)SEASON_MATCH_GROUP.PENNANTRACE)
                {
                    switch (_ownerGoal[mission.mission_idx].action_type)
                    {
                        // 승수
                        case ActionTypeDefine.PennantraceWinRecord:
                        {
                            if (gameResult != (byte)GAME_RESULT.WIN)
                            {
                                continue;
                            }
                            mission.count += 1;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 패수
                        case ActionTypeDefine.PennantraceLoseRecord:
                        {
                            if (gameResult != (byte)GAME_RESULT.LOSE)
                            {
                                continue;
                            }
                            mission.count += 1;
                            CheckBelowMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 홈런 수
                        case ActionTypeDefine.PennantraceTeam_HR_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.HR], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 안타
                        case ActionTypeDefine.PennantraceTeam_H_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.H], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 도루
                        case ActionTypeDefine.PennantraceTeam_SB_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.SB], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 타점
                        case ActionTypeDefine.PennantraceTeam_AB_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.AB], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 득점
                        case ActionTypeDefine.PennantraceTeam_TR_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.TR], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 탈 삼진
                        case ActionTypeDefine.PennantraceTeam_SO_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.SO], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 볼넷 허용
                        case ActionTypeDefine.PennantraceTeam_BB_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.BB], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckBelowMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 홀드 수
                        case ActionTypeDefine.PennantraceTeam_HLD_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.HLD], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 세이브 수
                        case ActionTypeDefine.PennantraceTeam_SV_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.SV], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 작전 지시
                        case ActionTypeDefine.PennantraceCommendCount:
                        {
                            mission.count += commendCount;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        default:
                            break;
                    }
                }
                else
                {
                    switch (_ownerGoal[mission.mission_idx].action_type)
                    {
                        // 팀 홈런 수
                        case ActionTypeDefine.PostTeam_HR_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.HR], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 안타
                        case ActionTypeDefine.PostTeam_H_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.H], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 도루
                        case ActionTypeDefine.PostTeam_SB_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.SB], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 타점
                        case ActionTypeDefine.PostTeam_AB_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.AB], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 득점
                        case ActionTypeDefine.PostTeam_TR_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.TR], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 탈 삼진
                        case ActionTypeDefine.PostTeam_SO_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.SO], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 볼넷 허용
                        case ActionTypeDefine.PostTeam_BB_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.BB], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckBelowMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 홀드 수
                        case ActionTypeDefine.PostTeam_HLD_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.HLD], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        // 팀 세이브 수
                        case ActionTypeDefine.PostTeam_SV_Record:
                        {
                            string[] splitedRecord = teamRecord.Split(',');
                            if (int.TryParse(splitedRecord[(byte)TEAM_RECORD_TYPE.SV], out int result) == false)
                            {
                                break;
                            }
                            mission.count = result;
                            CheckAboveMission(mission, ref resultMissions);
                            break;
                        }
                        default:
                            break;
                    }
                }
                
            }

            return resultMissions;
        }

        private void AddMission(CareerModeMission mission, ref List<CareerModeMission> resultMissions)
        {
            if (resultMissions == null)
            {
                resultMissions = new List<CareerModeMission>();
            }

            resultMissions.Add(mission);
        }

        private void CheckRankMission( CareerModeMission mission, ref List<CareerModeMission> resultMissions )
        {
            if ( mission.count <= _ownerGoal[ mission.mission_idx ].action_count )
            {
                mission.complete_flag = 1;
            }
            else
            {
                mission.complete_flag = 0;
            }

            if ( resultMissions == null )
            {
                resultMissions = new List<CareerModeMission>();
            }

            resultMissions.Add( mission );
        }

        private void CheckAboveMission(CareerModeMission mission, ref List<CareerModeMission> resultMissions)
        {
            if (mission.count >= _ownerGoal[mission.mission_idx].action_count)
            {
                mission.complete_flag = 1;
            }
            else
            {
                mission.complete_flag = 0;
            }

            if (resultMissions == null)
            {
                resultMissions = new List<CareerModeMission>();
            }

            resultMissions.Add(mission);
        }
        private void CheckBelowMission( CareerModeMission mission, ref List<CareerModeMission> resultMissions )
        {
            if ( mission.count <= _ownerGoal[ mission.mission_idx ].action_count )
            {
                mission.complete_flag = 0;
            }
            else
            {
                mission.complete_flag = 1;
            }

            if ( resultMissions == null )
            {
                resultMissions = new List<CareerModeMission>();
            }

            resultMissions.Add( mission );
        }

        public ErrorCode SpecialTrainingSet( byte step, byte trainingId, List<Player> players, ref List<CareerModeSpecialTraining> setInfo, ref List<AccountPlayerTrainingInfo> trainingPlayerInfoList )
        {
            if ( _specialTraining[ step - 1 ].ContainsKey( trainingId ) == false )
                return ErrorCode.ERROR_NOT_FOUND_SPECIALTRAINING_DATA;

            PB_CAREER_SPECIAL_TRAINING specialTraining = _specialTraining[ step - 1 ][ trainingId ];

            //기획 데이터의 인원수와 요청온 인원수가 맞지 않다면 에러
            if ( specialTraining.max_count != players.Count )
            {
                return ErrorCode.ERROR_INVALID_PARAM;
            }

            string detailStr = "";
            //재시도가 아닐때는 선수의 타입이랑 포지션 확인
            foreach ( Player p in players )
            {
                //해당 훈련의 선수 타입 체크
                if ( specialTraining.target_type != p.player_type )
                {
                    return ErrorCode.ERROR_NOT_MATCHING_PLAYER_TYPE;
                }

                //업데이트 목록에 있는지확인(한선수가 잠재력이랑 멀티포지션 다 할수 있으므로)
                AccountPlayerTrainingInfo trainingPlayerInfo = trainingPlayerInfoList.Find( x => x.account_player_idx == p.account_player_idx );

                if ( trainingPlayerInfo == null )
                {
                    trainingPlayerInfo = new AccountPlayerTrainingInfo()
                    {
                        account_player_idx = p.account_player_idx,
                        potential_idx1 = p.potential_idx1,
                        potential_idx2 = p.potential_idx2,
                        potential_idx3 = p.potential_idx3,
                        sub_pos_open = p.sub_pos_open
                    };

                    trainingPlayerInfoList.Add( trainingPlayerInfo );
                }

                if ( detailStr != "" )
                    detailStr += "|";

                if ( specialTraining.training_type == ( byte )SPRING_CAMP_MAIN_TYPE.OPEN_POTEN )
                {
                    int exceptBasicIdx = -1;
                    if ( step == ( byte )SPECIAL_TRAINING_STEP.STEP_LAST)
                    {
                        exceptBasicIdx = CacheManager.PBTable.PlayerTable.SameBasicPotentialCheck(p.potential_idx1, p.potential_idx2);
                    }

                    //잠재력 인덱스 구하기
                    int potenIdx;

                    if (exceptBasicIdx > 0)
                        potenIdx = CacheManager.PBTable.PlayerTable.PlayerPotentialCreateIdx(GAME_MODETYPE.MODE_CAREERMODE, (PLAYER_TYPE)p.player_type, new List<int> { exceptBasicIdx });
                    else
                        potenIdx = CacheManager.PBTable.PlayerTable.PlayerPotentialCreateIdx(GAME_MODETYPE.MODE_CAREERMODE, (PLAYER_TYPE)p.player_type, null);

                    if ( trainingPlayerInfo.potential_idx2 > 0 )
                    {
                        trainingPlayerInfo.potential_idx3 = potenIdx;
                    }
                    else if ( trainingPlayerInfo.potential_idx1 > 0 )
                    {
                        trainingPlayerInfo.potential_idx2 = potenIdx;
                    }
                    else
                    {
                        trainingPlayerInfo.potential_idx1 = potenIdx;
                    }

                    detailStr += string.Format( _specialTrainingStatString[ specialTraining.training_type ], p.account_player_idx, potenIdx );
                }
                else if ( specialTraining.training_type == ( byte )SPRING_CAMP_MAIN_TYPE.OPEN_SUB_POSITON )
                {
                    //이미 되어있다면 에러
                    if ( trainingPlayerInfo.sub_pos_open == 1 )
                    {
                        return ErrorCode.ERROR_ALREADY_SPECIALTRAINING_TRAINING;
                    }

                    if ( CacheManager.PBTable.PlayerTable.GetPlayerSecondPosition( p.player_idx ) == 0 )
                    {
                        return ErrorCode.ERROR_SUBPOSITION_IMPOSSIBLE_PLAYER;
                    }

                    trainingPlayerInfo.sub_pos_open = 1;

                    detailStr += string.Format( _specialTrainingStatString[ specialTraining.training_type ], p.account_player_idx );
                }
                else
                {
                    return ErrorCode.ERROR_STATIC_DATA;
                }
            }

            setInfo.Add( new CareerModeSpecialTraining()
            {
                step = step,
                training_type = trainingId,
                detail_info = detailStr
            } );

            return ErrorCode.SUCCESS;
        }

        public ErrorCode DoCureInjury( int injuryIdx, CareerModeInfo careerModeInfo )
        {
            if ( _allInjury.ContainsKey( injuryIdx ) == false )
            {
                return ErrorCode.ERROR_STATIC_DATA;
            }

            PB_CAREERMODE_MANAGEMENT_INJURY injuryInfo = _allInjury[ injuryIdx ];

            if ( injuryInfo.injury_group == 1 )
            {
                if ( careerModeInfo.injury_group1 > 0 )
                    --careerModeInfo.injury_group1;
            }
            else if ( injuryInfo.injury_group == 2 )
            {
                if ( careerModeInfo.injury_group2 > 0 )
                    --careerModeInfo.injury_group2;
            }
            else if ( injuryInfo.injury_group == 3 )
            {
                if ( careerModeInfo.injury_group3 > 0 )
                    --careerModeInfo.injury_group3;
            }
            else if ( injuryInfo.injury_group == 4 )
            {
                if ( careerModeInfo.injury_group4 > 0 )
                    --careerModeInfo.injury_group4;
            }

            return ErrorCode.SUCCESS;
        }

        private List<PB_CAREERMODE_MANAGEMENT_EVENT> CreateCycleEvent(int count, int moodVal)
        {
            //mood 0:좋음 1:보통 2:나쁨
            //count 발생 갯수
            List<PB_CAREERMODE_MANAGEMENT_EVENT> createEvent = new List<PB_CAREERMODE_MANAGEMENT_EVENT>();
            List<int> createListIdxs = RandomManager.Instance.GetSuccessIdxListFromAccumulateRatioList(RANDOM_TYPE.GLOBAL, _moodEventRate[moodVal], count);

            for(int i = 0; i < createListIdxs.Count; ++i)
            {
                createEvent.Add(_moodEvent[moodVal][createListIdxs[i]]);
            }

            return createEvent;
        }

        public short GetAddTeamMood(byte gameResult)
        {
            if (gameResult == (byte)GAME_RESULT.WIN)
                return (short)ManagementConfig.teammood_change_win;
            else if (gameResult == (byte)GAME_RESULT.DRAW)
                return (short)ManagementConfig.teammood_change_draw;
            else
                return (short)ManagementConfig.teammood_change_lose;
        }

        public bool IsCycleEvent(byte countryType, int gameNo)
        {
            int divVal = -1;
            if (countryType == (byte)NATION_LEAGUE_TYPE.KBO)
                divVal = ManagementConfig.manage_cycle_kbo;
            else if (countryType == (byte)NATION_LEAGUE_TYPE.MLB)
                divVal = ManagementConfig.manage_cycle_mlb;
            else if (countryType == (byte)NATION_LEAGUE_TYPE.NPB)
                divVal = ManagementConfig.manage_cycle_npb;
            else if (countryType == (byte)NATION_LEAGUE_TYPE.CPB)
                divVal = ManagementConfig.manage_cycle_cpbl;

            if (gameNo % divVal == 0)
                return true;
            else
                return false;
        }

        public List<CareerModeCycleEventInfo> GetNewCycleEventList(int teamMood, out int[] searchCnt)
        {
            searchCnt = new int[] { 0, 0, 0 };

            //1. 신규이벤트 확률 체크
            if (RandomManager.Instance.GetCount(CareerModeDefine.EventTotalRate) > ManagementConfig.event_appear_new_prob)
                return null;

            //2. 신규이벤트 갯수 체크
            int newOccurCount = RandomManager.Instance.GetCount(ManagementConfig.event_appear_new_max);
            
            //3. 팀분위기값 추출
            int moodVal = 1;
            if(teamMood >= ManagementConfig.teammood_good_value)
            {
                moodVal = 0;
            }
            else if(teamMood <= ManagementConfig.teammood_bad_value)
            {
                moodVal = 2;
            }

            List<PB_CAREERMODE_MANAGEMENT_EVENT> eventPBList = CreateCycleEvent(newOccurCount, moodVal);

            if (eventPBList.Count > 0)
            {
                List<CareerModeCycleEventInfo> resultEventList = new List<CareerModeCycleEventInfo>();

                for (int i = 0; i < eventPBList.Count; ++i)
                {
                    resultEventList.Add(new CareerModeCycleEventInfo()
                    {
                        event_idx = eventPBList[i].idx,
                        target_player = eventPBList[i].eventtarget,
                        select_idx = 0
                    });

                    if (eventPBList[i].eventtarget > 0)
                        ++searchCnt[eventPBList[i].eventtarget - 1];        //idx 0:전체, 1:라인업타자, 2:라인업투수

                }

                return resultEventList;
            }

            return null;

        }

        public bool IsValidCycleEventSelectIdx(int eventIdx, byte selectIdx)
        {
            if (_allEvent.ContainsKey(eventIdx) == true)
            {
                if (_allEvent[eventIdx].select_count >= selectIdx)
                    return true;
            }

            return false;
        }
    }
}
