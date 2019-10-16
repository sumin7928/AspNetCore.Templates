using System;
using System.Collections.Generic;
using System.Linq;
using ApiWebServer.Common;
using ApiWebServer.Common.Define;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Contents;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Cache.PBTables
{
    public class PlayerTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // 타자 정보
        private Dictionary<int, PB_PLAYER_BATTER> _batter = new Dictionary<int, PB_PLAYER_BATTER>();
        // 투수 정보
        private Dictionary<int, PB_PLAYER_PITCHER> _pitcher = new Dictionary<int, PB_PLAYER_PITCHER>();
        //선수 정보
        private Dictionary<int, PBPlayer> _player = new Dictionary<int, PBPlayer>();
        // 코치 정보
        private Dictionary<int, PB_COACH> _coach = new Dictionary<int, PB_COACH>();

        public List<PB_COACH> _gachaCoachList;
        public List<PBPlayer> _gachaPlayerList;
        public List<PBPlayer> _gachaBatterList;
        public List<PBPlayer> _gachaPitcherList;

        // 팀 정보 [ teamIdx, 팀정보 ]
        private Dictionary<int, PB_TEAM_INFO> _teamInfo = new Dictionary<int, PB_TEAM_INFO>();
        public Dictionary<int, List<PB_TEAM_INFO>> _teamCountryInfo = new Dictionary<int, List<PB_TEAM_INFO>>();
        //public List<PB_TEAM_INFO> _teamInfoList = new List<PB_TEAM_INFO>();

        // 코치 포지션 정보
        private Dictionary<int, PB_COACH_POSITION> _coachPosition = new Dictionary<int, PB_COACH_POSITION>();

        // 선수 강화 정보
        private Dictionary<int, PB_PLAYER_REINFORCE_POWER> _playerReinforceInfo = new Dictionary<int, PB_PLAYER_REINFORCE_POWER>();
        private Dictionary<int, List<GameRewardInfo>> _playerReinforceConsumeInfo = new Dictionary<int, List<GameRewardInfo>>();
        // 코치 슬롯 정보
        private Dictionary<int, PB_COACH_SLOT_BASE> _coachSlotInfo = new Dictionary<int, PB_COACH_SLOT_BASE>();
        // 코치 지도력 연수 정보
        private List<PB_COACH_REINFORCE_POWER> _coachReinforcePowerList = new List<PB_COACH_REINFORCE_POWER>();
        // 코치 리더십 정보
        //private List<PB_SKILL_LEADERSHIP> _skillLeadershipList = new List<PB_SKILL_LEADERSHIP>();
        private Dictionary<int, PB_SKILL_LEADERSHIP> _skillLeadership = new Dictionary<int, PB_SKILL_LEADERSHIP>();

        //코치 등급별 리더십 리스트
        private List<PB_SKILL_LEADERSHIP>[] _batterLeadershipGradeList = new List<PB_SKILL_LEADERSHIP>[PlayerDefine.LeadershipGradeCount];
        private List<PB_SKILL_LEADERSHIP>[] _pitcherLeadershipGradeList = new List<PB_SKILL_LEADERSHIP>[PlayerDefine.LeadershipGradeCount];
        private List<PB_SKILL_LEADERSHIP>[] _trainerLeadershipGradeList = new List<PB_SKILL_LEADERSHIP>[PlayerDefine.LeadershipGradeCount];
        private List<PB_SKILL_LEADERSHIP>[] _allLeadershipGradeList = new List<PB_SKILL_LEADERSHIP>[PlayerDefine.LeadershipGradeCount];

        //코치 d등급 리더십 누적 확률
        private List<int> _pitcherLeadershipCreateRate = new List<int>();
        private List<int> _batterLeadershipCreateRate = new List<int>();
        private List<int> _trainerLeadershipCreateRate = new List<int>();

        // 코칭 스킬 정보
        private List<PB_SKILL_COACHING> _coachingSkillList = new List<PB_SKILL_COACHING>();
        // 코칭 스킬 등급업 정보
        private List<PB_COACH_SKILL_RANKUP> _coachSkillRankupList = new List<PB_COACH_SKILL_RANKUP>();

        // 전체 선수 잠재력 사전
        private Dictionary<int, PB_PLAYER_SKILL_POTENTIAL> _playerPotential = new Dictionary<int, PB_PLAYER_SKILL_POTENTIAL>();

        //타자 / 투수 등급별 잠재력 리스트
        private List<PB_PLAYER_SKILL_POTENTIAL>[] _pitcherPotentialGradeList = new List<PB_PLAYER_SKILL_POTENTIAL>[PlayerDefine.PotentialGradeCount];
        private List<PB_PLAYER_SKILL_POTENTIAL>[] _batterPotentialGradeList = new List<PB_PLAYER_SKILL_POTENTIAL>[PlayerDefine.PotentialGradeCount];

        //선수 잠재력 등급별 누적 확률 (커리어 모드 용)
        private List<int>[] _pitcherPotentialGradeCareerRate = new List<int>[PlayerDefine.PotentialGradeCount];
        private List<int>[] _batterPotentialGradeCareerRate = new List<int>[PlayerDefine.PotentialGradeCount];

        //선수 잠재력 d등급 누적 확률(PVP 용)
        private List<int> _pitcherPotentialGradePvpRate = new List<int>();
        private List<int> _batterPotentialGradePvpRate = new List<int>();

        //선수 잠재력 등급별 누적확률(선수)
        private int[] _playerPotentialPvpGradeRatio = new int[PlayerDefine.PotentialGradeCount];

        //코치 리더쉽 등급별 누적확률(선수)
        private int[] _coachLeaderShipGradeRatio = new int[PlayerDefine.LeadershipGradeCount];

        //리그별 선수 지급 정보
        private Dictionary<int, List<TeamPaidPlayerList>> _teamPaidPlayerInfoKBO = new Dictionary<int, List<TeamPaidPlayerList>>();
        private Dictionary<int, List<TeamPaidPlayerList>> _teamPaidPlayerInfoMLB = new Dictionary<int, List<TeamPaidPlayerList>>();
        private Dictionary<int, List<TeamPaidPlayerList>> _teamPaidPlayerInfoNPB = new Dictionary<int, List<TeamPaidPlayerList>>();
        private Dictionary<int, List<TeamPaidPlayerList>> _teamPaidPlayerInfoCPBL = new Dictionary<int, List<TeamPaidPlayerList>>();

        private List<PB_TEAM_COUNTRY_SQUAD> _teamCountrySquad = new List<PB_TEAM_COUNTRY_SQUAD>();
        private List<PB_TEAM_SELECT_PLAYER> _teamSelectPlayer = new List<PB_TEAM_SELECT_PLAYER>();

        private Dictionary<int, PB_INVENTORY_LEVEL> _playerInvenInfo = new Dictionary<int, PB_INVENTORY_LEVEL>();
        private Dictionary<int, PB_INVENTORY_LEVEL> _coachInvenInfo = new Dictionary<int, PB_INVENTORY_LEVEL>();

        private Dictionary<int, int> _playerInvenCountDic = new Dictionary<int, int>();
        private Dictionary<int, int> _coachInvenCountDic = new Dictionary<int, int>();

        // 팀 생성 그룹 정보
        private List<PB_TEAM_CREATE_GROUP> _teamCreateGroup = new List<PB_TEAM_CREATE_GROUP>();
        // 팀 라인업 정보
        private Dictionary<int, List<PB_TEAM_LINEUP>> _teamLineup = new Dictionary<int, List<PB_TEAM_LINEUP>>();

        public bool LoadTable(MaguPBTableContext context)
        {
            // PB_PLAYER_BATTER
            foreach (var data in context.PB_PLAYER_BATTER.ToList())
            {
                if (data.isuse == 1)
                {
                    _batter.Add(data.player_idx, data);
                    _player.Add(data.player_idx, new PBPlayer()
                    {
                        team_idx = data.teamidx,
                        position = data.position,
                        second_position = data.second_position,
                        player_unique_idx = data.player_unique_idx,
                        player_type = (byte)PLAYER_TYPE.TYPE_BATTER,
                        player_name = data.player_name,
                        player_idx = data.player_idx,
                        player_health = 0,
                        overall = data.overall,
                        get_rate = data.get_rate
                    });
                }

            }

            // PB_PLAYER_PITCHER
            foreach (var data in context.PB_PLAYER_PITCHER.ToList())
            {
                if (data.isuse == 1)
                {
                    _pitcher.Add(data.player_idx, data);
                    _player.Add(data.player_idx, new PBPlayer()
                    {
                        team_idx = data.teamidx,
                        position = data.position,
                        second_position = 0,    //data.second_position,
                        player_unique_idx = data.player_unique_idx,
                        player_type = (byte)PLAYER_TYPE.TYPE_PITCHER,
                        player_name = data.player_name,
                        player_idx = data.player_idx,
                        player_health = data.player_health,
                        overall = data.overall,
                        get_rate = data.get_rate

                    });
                }
            }

            // PB_PLAYER_COACH
            foreach (var data in context.PB_COACH.ToList())
            {
                if (data.isuse == 1)
                {
                    _coach.Add(data.coach_idx, data);
                }
            }

            for(int i = (int)NATION_LEAGUE_TYPE.KBO; i <= (int)NATION_LEAGUE_TYPE.NPB_MLB; ++i)
                _teamCountryInfo.Add(i, new List<PB_TEAM_INFO>());

            // PB_TEAM_INFO
            foreach (var data in context.PB_TEAM_INFO.ToList())
            {
                if (data.isuse == 1)
                {
                    _teamInfo.Add(data.team_idx, data);

                    NATION_LEAGUE_TYPE country = (NATION_LEAGUE_TYPE)data.country_flg;

                    if (country == NATION_LEAGUE_TYPE.KBO)
                    {
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.KBO].Add(data);
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.KBO_MLB].Add(data);
                    }
                    else if (country == NATION_LEAGUE_TYPE.MLB)
                    {
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.MLB].Add(data);
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.KBO_MLB].Add(data);
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.CPB_MLB].Add(data);
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.NPB_MLB].Add(data);
                    }
                    else if (country == NATION_LEAGUE_TYPE.NPB)
                    {
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.NPB].Add(data);
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.NPB_MLB].Add(data);
                    }
                    else if (country == NATION_LEAGUE_TYPE.CPB)
                    {
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.CPB].Add(data);
                        _teamCountryInfo[(int)NATION_LEAGUE_TYPE.CPB_MLB].Add(data);
                    }

                    //_teamInfoList.Add(data);
                }
            }

            // PB_TEAM_CREATE_GROUP
            foreach (var data in context.PB_TEAM_CREATE_GROUP.ToList())
            {
                _teamCreateGroup.Add(data);
            }

            int[] inven_order = { (int)PLAYER_ORDER.INVEN_BATTER, (int)PLAYER_ORDER.INVEN_PITCHER, (int)PLAYER_ORDER.INVEN_COACH };

            // PB_TEAM_LINEUP
            foreach (var data in context.PB_TEAM_LINEUP.ToList())
            {
                if(data.order >= (int)PLAYER_ORDER.INVEN_BATTER || data.position == (int)PLAYER_POSITION.INVEN)
                {
                    if (inven_order[data.player_type] != data.order)
                        return false;

                    if (data.position != (int)PLAYER_POSITION.INVEN)
                        return false;
                }

                if (_teamLineup.ContainsKey(data.lineup_group_idx) == false)
                {
                    _teamLineup.Add(data.lineup_group_idx, new List<PB_TEAM_LINEUP>());
                }

                _teamLineup[data.lineup_group_idx].Add(data);
            }
            // PB_COACH_POSITION
            foreach (var data in context.PB_COACH_POSITION.ToList())
            {
                _coachPosition.Add(data.idx, data);
            }

            // PB_COACH_SLOT_BASE
            foreach (var data in context.PB_COACH_SLOT_BASE.ToList())
            {
                _coachSlotInfo.Add(data.idx, data);
            }
            // PB_COACH_REINFORCE_POWER
            foreach (var data in context.PB_COACH_REINFORCE_POWER.ToList())
            {
                _coachReinforcePowerList.Add(data);
            }
            // PB_SKILL_LEADERSHIP
            for (int i = 0; i < PlayerDefine.LeadershipGradeCount; ++i)
            {
                _batterLeadershipGradeList[i] = new List<PB_SKILL_LEADERSHIP>();
                _pitcherLeadershipGradeList[i] = new List<PB_SKILL_LEADERSHIP>();
                _trainerLeadershipGradeList[i] = new List<PB_SKILL_LEADERSHIP>();
                _allLeadershipGradeList[i] = new List<PB_SKILL_LEADERSHIP>();
            }
            
            int _tempPitcherLeadershipCreateRate = 0;
            int _tempBatterLeadershipCreateRate = 0;
            int _tempTrainerLeadershipCreateRate = 0;

            Dictionary<int, int[]> checkLeadership = new Dictionary<int, int[]>();
            Dictionary<int, int> checkLeadershipType = new Dictionary<int, int>();
            foreach (var data in context.PB_SKILL_LEADERSHIP.ToList())
            {
                _skillLeadership.Add(data.idx, data);

                if (checkLeadership.ContainsKey(data.basic_idx) == false)
                {
                    checkLeadership.Add(data.basic_idx, new int[PlayerDefine.LeadershipGradeCount]);
                    checkLeadershipType.Add(data.basic_idx, data.category);
                }
                else
                {
                    if (data.category != checkLeadershipType[data.basic_idx])
                        return false;
                }

                ++checkLeadership[data.basic_idx][data.grade - 1];
                
                if (data.category == (byte)COACH_MASTER_TYPE.TYPE_BATTER || data.category == (byte)COACH_MASTER_TYPE.TYPE_ALL)
                {
                    _batterLeadershipGradeList[data.grade - 1].Add(data);

                    if (data.grade == 1)
                    {
                        if (data.basic_idx != data.idx)
                            return false;

                        _tempBatterLeadershipCreateRate += data.add_rate;
                        _batterLeadershipCreateRate.Add(_tempBatterLeadershipCreateRate);
                    }
                }

                if (data.category == (byte)COACH_MASTER_TYPE.TYPE_PITCHER || data.category == (byte)COACH_MASTER_TYPE.TYPE_ALL)
                {
                    _pitcherLeadershipGradeList[data.grade - 1].Add(data);

                    if (data.grade == 1)
                    {
                        if (data.basic_idx != data.idx)
                            return false;

                        _tempPitcherLeadershipCreateRate += data.add_rate;
                        _pitcherLeadershipCreateRate.Add(_tempPitcherLeadershipCreateRate);
                    }
                }

                if (data.category == (byte)COACH_MASTER_TYPE.TYPE_TRAINER || data.category == (byte)COACH_MASTER_TYPE.TYPE_ALL)
                {
                    _trainerLeadershipGradeList[data.grade - 1].Add(data);

                    if (data.grade == 1)
                    {
                        if (data.basic_idx != data.idx)
                            return false;

                        _tempTrainerLeadershipCreateRate += data.add_rate;
                        _trainerLeadershipCreateRate.Add(_tempTrainerLeadershipCreateRate);
                    }
                }

                _allLeadershipGradeList[data.grade - 1].Add(data);
            }


            //코치 리더십 유효성 체크
            foreach (KeyValuePair<int, int[]> pair in checkLeadership)
            {
                if (-1 != Array.FindIndex(pair.Value, x => x != 1))
                    return false;
            }


            // PB_SKILL_COACHING
            foreach (var data in context.PB_SKILL_COACHING.ToList())
            {
                _coachingSkillList.Add(data);
            }
            // PB_COACH_SKILL_RANKUP
            foreach (var data in context.PB_COACH_SKILL_RANKUP.ToList())
            {
                _coachSkillRankupList.Add(data);
            }
            // PB_TEAM_COUNTRY_SQUAD
            foreach (var data in context.PB_TEAM_COUNTRY_SQUAD.ToList())
            {
                _teamCountrySquad.Add(data);
            }
            // PB_TEAM_SELECT_PLAYER
            foreach (var data in context.PB_TEAM_SELECT_PLAYER.ToList())
            {
                _teamSelectPlayer.Add(data);
            }
            //PB_PLAYER_REINFORCE_POWER
            foreach (var data in context.PB_PLAYER_REINFORCE_POWER.ToList())
            {
                if (data.probability == 0)
                    break;

                if (data.price_type1 == 0 || data.price_count1 <= 0)
                    return false;

                _playerReinforceInfo.Add(data.idx, data);

                _playerReinforceConsumeInfo.Add(data.idx, new List<GameRewardInfo>());
                _playerReinforceConsumeInfo[data.idx].Add(new GameRewardInfo(data.price_type1, 0, data.price_count1));

                if (data.price_type2 != 0)
                {
                    if (data.price_count2 <= 0)
                        return false;

                    _playerReinforceConsumeInfo[data.idx].Add(new GameRewardInfo(data.price_type2, 0, data.price_count2));

                    if (data.price_type3 != 0)
                    {
                        if (data.price_count3 <= 0)
                            return false;

                        _playerReinforceConsumeInfo[data.idx].Add(new GameRewardInfo(data.price_type3, 0, data.price_count3));
                    }
                }


            }

            //PB_PLAYER_SKILL_POTENTIAL
            for (int i = 0; i < PlayerDefine.PotentialGradeCount; ++i)
            {
                _pitcherPotentialGradeList[i] = new List<PB_PLAYER_SKILL_POTENTIAL>();
                _batterPotentialGradeList[i] = new List<PB_PLAYER_SKILL_POTENTIAL>();

                _pitcherPotentialGradeCareerRate[i] = new List<int>();
                _batterPotentialGradeCareerRate[i] = new List<int>();
            }

            int[] _tempPitcherPotentialGradeCareerRate = new int[PlayerDefine.PotentialGradeCount];
            int[] _tempBatterPotentialGradeCareerRate = new int[PlayerDefine.PotentialGradeCount];

            int _tempPitcherPotentialGradePvpRate = 0;
            int _tempBatterPotentialGradePvpRate = 0;

            Dictionary<int, int[]> checkPlayerPotential = new Dictionary<int, int[]>();
            Dictionary<int, int> checkPlayerPotentialType = new Dictionary<int, int>();
            foreach (var data in context.PB_PLAYER_SKILL_POTENTIAL.ToList())
            {
                _playerPotential.Add(data.idx, data);

                if (checkPlayerPotential.ContainsKey(data.basic_idx) == false)
                {
                    checkPlayerPotential.Add(data.basic_idx, new int[PlayerDefine.PotentialGradeCount]);
                    checkPlayerPotentialType.Add(data.basic_idx, data.PotenType);
                }
                else
                {
                    if (data.PotenType != checkPlayerPotentialType[data.basic_idx])
                        return false;
                }

                ++checkPlayerPotential[data.basic_idx][data.Grade - 1];

                if (data.PotenType == (byte)PLAYER_TYPE.TYPE_BATTER || data.PotenType == 2)
                {
                    _tempBatterPotentialGradeCareerRate[data.Grade - 1] += data.career_poten_rate;

                    _batterPotentialGradeList[data.Grade - 1].Add(data);
                    _batterPotentialGradeCareerRate[data.Grade - 1].Add(_tempBatterPotentialGradeCareerRate[data.Grade - 1]);

                    if (data.Grade == 1)
                    {
                        if (data.basic_idx != data.idx)
                            return false;

                        _tempBatterPotentialGradePvpRate += data.PotenActiveRate;
                        _batterPotentialGradePvpRate.Add(_tempBatterPotentialGradePvpRate);
                    }
                }

                if (data.PotenType == (byte)PLAYER_TYPE.TYPE_PITCHER || data.PotenType == 2)
                {
                    _tempPitcherPotentialGradeCareerRate[data.Grade - 1] += data.career_poten_rate;

                    _pitcherPotentialGradeList[data.Grade - 1].Add(data);
                    _pitcherPotentialGradeCareerRate[data.Grade - 1].Add(_tempPitcherPotentialGradeCareerRate[data.Grade - 1]);

                    if (data.Grade == 1)
                    {
                        if (data.basic_idx != data.idx)
                            return false;

                        _tempPitcherPotentialGradePvpRate += data.PotenActiveRate;
                        _pitcherPotentialGradePvpRate.Add(_tempPitcherPotentialGradePvpRate);
                    }
                }
            }

            //선수 잠재력 유효성 체크
            foreach (KeyValuePair<int, int[]> pair in checkPlayerPotential)
            {
                if (-1 != Array.FindIndex(pair.Value, x => x != 1))
                    return false;
            }

            int _gradeCheck = 0;

            // PB_SKILL_RANKUP
            foreach (var data in context.PB_SKILL_RANKUP.ToList())
            {
                if (data.potential_weight == 0 || data.leadership_weight == 0)
                    return false;

                if (++_gradeCheck != data.idx)
                    return false;

                if (data.idx == 1)
                {
                    _playerPotentialPvpGradeRatio[data.idx - 1] = data.potential_weight;
                    _coachLeaderShipGradeRatio[data.idx - 1] = data.leadership_weight;
                }
                else
                {
                    _playerPotentialPvpGradeRatio[data.idx - 1] = _playerPotentialPvpGradeRatio[_gradeCheck - 2] + data.potential_weight;
                    _coachLeaderShipGradeRatio[data.idx - 1] = _coachLeaderShipGradeRatio[_gradeCheck - 2] + data.leadership_weight;
                }
            }

            //rankup 테이블 유효성 체크
            if (_gradeCheck != PlayerDefine.PotentialGradeCount)
                return false;


            //PB_TEAM_PAID_PLAYER_LIST
            foreach (var data in context.PB_TEAM_PAID_PLAYER_LIST.ToList())
            {
                if (_teamPaidPlayerInfoKBO.ContainsKey(data.position) == false)
                {
                    _teamPaidPlayerInfoKBO.Add(data.position, new List<TeamPaidPlayerList>());
                }
                if (_teamPaidPlayerInfoMLB.ContainsKey(data.position) == false)
                {
                    _teamPaidPlayerInfoMLB.Add(data.position, new List<TeamPaidPlayerList>());
                }
                if (_teamPaidPlayerInfoNPB.ContainsKey(data.position) == false)
                {
                    _teamPaidPlayerInfoNPB.Add(data.position, new List<TeamPaidPlayerList>());
                }
                if (_teamPaidPlayerInfoCPBL.ContainsKey(data.position) == false)
                {
                    _teamPaidPlayerInfoCPBL.Add(data.position, new List<TeamPaidPlayerList>());
                }

                if (data.player_idx_1 > 0)
                {
                    _teamPaidPlayerInfoKBO[data.position].Add(new TeamPaidPlayerList()
                    {
                        idx = data.idx,
                        position = data.position,
                        country = data.country_1,
                        player_idx = data.player_idx_1
                    });
                }
                if (data.player_idx_2 > 0)
                {
                    _teamPaidPlayerInfoMLB[data.position].Add(new TeamPaidPlayerList()
                    {
                        idx = data.idx,
                        position = data.position,
                        country = data.country_2,
                        player_idx = data.player_idx_2
                    });
                }
                if (data.player_idx_3 > 0)
                {
                    _teamPaidPlayerInfoNPB[data.position].Add(new TeamPaidPlayerList()
                    {
                        idx = data.idx,
                        position = data.position,
                        country = data.country_3,
                        player_idx = data.player_idx_3
                    });
                }
                if (data.player_idx_4 > 0)
                {
                    _teamPaidPlayerInfoCPBL[data.position].Add(new TeamPaidPlayerList()
                    {
                        idx = data.idx,
                        position = data.position,
                        country = data.country_4,
                        player_idx = data.player_idx_4
                    });
                }

                Dictionary<int, int> tempKBODic = new Dictionary<int, int>();
                Dictionary<int, int> tempMLBDic = new Dictionary<int, int>();
                Dictionary<int, int> tempNPBDic = new Dictionary<int, int>();
                Dictionary<int, int> tempCPBLDic = new Dictionary<int, int>();

                // 중복 유효성 체크
                foreach ( var a in _teamPaidPlayerInfoKBO)
                {
                    foreach (var b in a.Value)
                    {
                        if (tempKBODic.ContainsKey(b.player_idx) == false)
                        {
                            tempKBODic.Add(b.player_idx, b.idx);
                        }
                        else
                            return false;
                    }
                }
                // 중복 유효성 체크
                foreach (var a in _teamPaidPlayerInfoMLB)
                {
                    foreach (var b in a.Value)
                    {
                        if (tempMLBDic.ContainsKey(b.player_idx) == false)
                        {
                            tempMLBDic.Add(b.player_idx, b.idx);
                        }
                        else
                            return false;
                    }
                }
                // 중복 유효성 체크
                foreach (var a in _teamPaidPlayerInfoNPB)
                {
                    foreach (var b in a.Value)
                    {
                        if (tempNPBDic.ContainsKey(b.player_idx) == false)
                        {
                            tempNPBDic.Add(b.player_idx, b.idx);
                        }
                        else
                            return false;
                    }
                }
                // 중복 유효성 체크
                foreach (var a in _teamPaidPlayerInfoCPBL)
                {
                    foreach (var b in a.Value)
                    {
                        if (tempCPBLDic.ContainsKey(b.player_idx) == false)
                        {
                            tempCPBLDic.Add(b.player_idx, b.idx);
                        }
                        else
                            return false;
                    }
                }
            }

            //PB_INVENTORY_LEVEL
            foreach (var data in context.PB_INVENTORY_LEVEL.ToList())
            {
                if (data.type == 0)
                {
                    _playerInvenInfo.Add(data.extend_level, data);
                    _playerInvenCountDic.Add(data.max_count, data.extend_level);
                }
                else if (data.type == 1)
                {
                    _coachInvenInfo.Add(data.extend_level, data);
                    _coachInvenCountDic.Add(data.max_count, data.extend_level);
                }
            }

            _gachaCoachList = _coach.Values.ToList().FindAll(x => x.get_rate > 0);
            _gachaPlayerList = _player.Values.ToList().FindAll(x=>x.get_rate > 0);
            _gachaBatterList = _gachaPlayerList.FindAll(x => x.player_type == (byte)PLAYER_TYPE.TYPE_BATTER);
            _gachaPitcherList = _gachaPlayerList.FindAll(x => x.player_type == (byte)PLAYER_TYPE.TYPE_PITCHER);

            return true;
        }

        public int GetInvenMaxCount(CHARACTER_INVEN_TYPE invenType, int lv)
        {
            int result_count = -1;

            if(invenType == CHARACTER_INVEN_TYPE.PLAYER)
            {
                if (_playerInvenInfo.ContainsKey(lv) == true)
                    result_count = _playerInvenInfo[lv].max_count;
            }
            else if (invenType == CHARACTER_INVEN_TYPE.COACH)
            {
                if (_coachInvenInfo.ContainsKey(lv) == true)
                    result_count = _coachInvenInfo[lv].max_count;
            }
            return result_count;
        }

        public ErrorCode InvenExtend(byte invenType, int tryLv, AccountGame accountGame, out byte costType, out int costCount)
        {
            costType = 0;
            costCount = 0;

            Dictionary<int, PB_INVENTORY_LEVEL> invenInfo;
            Dictionary<int, int> invenCountDic;
            int preMaxCount;
            if (invenType == (byte)CHARACTER_INVEN_TYPE.PLAYER)
            {
                invenInfo = _playerInvenInfo;
                invenCountDic = _playerInvenCountDic;
                preMaxCount = accountGame.max_player;
            }
            else
            {
                invenInfo = _coachInvenInfo;
                invenCountDic = _coachInvenCountDic;
                preMaxCount = accountGame.max_coach;
            }

            if (invenCountDic.ContainsKey(preMaxCount) == false)
            {
                return ErrorCode.ERROR_DB_DATA;
            }

            int invenLv = invenCountDic[preMaxCount];

            if (tryLv != invenLv + 1)
            {
                return ErrorCode.ERROR_REQUEST_DATA;
            }

            if (invenInfo.ContainsKey(tryLv) == false)
            {
                return ErrorCode.EERROR_NOT_EXTEND_INVEN;
            }

            if (invenType == (byte)CHARACTER_INVEN_TYPE.PLAYER)
            {
                accountGame.max_player = invenInfo[tryLv].max_count;
            }
            else
            {
                accountGame.max_coach = invenInfo[tryLv].max_count;
            }

            costType = invenInfo[invenLv].cost_type;
            costCount = invenInfo[invenLv].cost_value;

            return ErrorCode.SUCCESS;
        }
        public ErrorCode CreateTeam(int teamIdx, out List<Player> players, out List<TeamCoach> coaches)
        {
            players = null;
            coaches = null;

            int lineupGroupIdx = _teamCreateGroup.Find(x => x.team_idx == teamIdx).lineup_group_idx;

            if (_teamLineup.TryGetValue(lineupGroupIdx, out List<PB_TEAM_LINEUP> teamLineup) == false)
            {
                _logger.Warn("Not found teamLineup data - teamIdx:{0}, lineupGroupIdx:{1}", teamIdx, lineupGroupIdx);
                return ErrorCode.ERROR_NOT_MATCHING_PB_PLAYER;
            }

            List<Player> tempPlayers = new List<Player>();
            List<TeamCoach> tempCoaches = new List<TeamCoach>();

            for (int i = 0; i < teamLineup.Count; i++)
            {
                if (teamLineup[i].isuse == 0)
                    continue;

                int playerIdx = teamLineup[i].player_idx;
                byte lineupType = teamLineup[i].lineup_type;
                byte playerType = teamLineup[i].player_type;

                /*if (playerIdx >= (int)PLAYER_IDX_RANGE.MIN_PITCHER_IDX && playerIdx <= (int)PLAYER_IDX_RANGE.MAX_PITCHER_IDX)
                {
                    playerType = (byte)PLAYER_TYPE.TYPE_PITCHER;
                }
                else if (playerIdx >= (int)PLAYER_IDX_RANGE.MIN_BATTER_IDX && playerIdx <= (int)PLAYER_IDX_RANGE.MAX_BATTER_IDX)
                {
                    playerType = (byte)PLAYER_TYPE.TYPE_BATTER;
                }
                else if (playerIdx >= (int)PLAYER_IDX_RANGE.MIN_COACH_IDX && playerIdx <= (int)PLAYER_IDX_RANGE.MAX_COACH_IDX)
                {
                    playerType = (byte)PLAYER_TYPE.TYPE_COACH;
                }
                else
                {
                    continue;
                }*/

                if (playerType == (byte)PLAYER_TYPE.TYPE_COACH)
                {
                    if (_coach.TryGetValue(playerIdx, out PB_COACH coach) == false)
                    {
                        _logger.Warn("Not found coach data - teamIdx:{0}, lineupGroupIdx:{1}, playerIdx:{2}", teamIdx, lineupGroupIdx, playerIdx);
                        return ErrorCode.ERROR_NOT_MATCHING_PB_COACH;
                    }

                    tempCoaches.Add(new TeamCoach()
                    {
                        coach_idx = coach.coach_idx,
                        player_type = playerType,
                        position = teamLineup[i].position,
                        is_starting = (byte)(teamLineup[i].order < PlayerDefine.InvenOrderStartIdx ? 1 : 0),        // 선발 4인만 선발로 등록
                        coaching_skill = coach.coaching_skill
                    });
                }
                else
                {
                    if (playerType == (byte)PLAYER_TYPE.TYPE_BATTER)
                    {
                        if (_batter.TryGetValue(playerIdx, out PB_PLAYER_BATTER player) == false)
                        {
                            _logger.Warn("Not found player data - teamIdx:{0}, lineupGroupIdx:{1}, playerIdx:{2}", teamIdx, lineupGroupIdx, playerIdx);
                            return ErrorCode.ERROR_NOT_MATCHING_PB_PLAYER;
                        }

                        tempPlayers.Add(new Player()
                        {
                            player_idx = player.player_idx,
                            player_type = playerType,
                            is_starting = (byte)(teamLineup[i].order < PlayerDefine.InvenOrderStartIdx ? 1 : 0),    // 선발 26인만 선발로 등록
                            order = teamLineup[i].order,
                            position = teamLineup[i].position,
                            reinforce_grade = 0,
                            player_health = 0,
                            potential_idx1 = -1,//teamLineup[ i ].player_potential_1,
                            potential_idx2 = -1,//teamLineup[ i ].player_potential_2,
                            potential_idx3 = -1,//teamLineup[ i ].player_potential_3,
                            sub_pos_open = 0
                        });
                    }
                    else if (playerType == (byte)PLAYER_TYPE.TYPE_PITCHER)
                    {
                        if (_pitcher.TryGetValue(playerIdx, out PB_PLAYER_PITCHER player) == false)
                        {
                            _logger.Warn("Not found player data - teamIdx:{0}, lineupGroupIdx:{1}, playerIdx:{2}", teamIdx, lineupGroupIdx, playerIdx);
                            return ErrorCode.ERROR_NOT_MATCHING_PB_PLAYER;
                        }

                        tempPlayers.Add(new Player()
                        {
                            player_idx = player.player_idx,
                            player_type = playerType,
                            is_starting = (byte)(teamLineup[i].order < PlayerDefine.InvenOrderStartIdx ? 1 : 0),    // 선발 26인만 선발로 등록
                            order = teamLineup[i].order,
                            position = teamLineup[i].position,
                            reinforce_grade = 0,
                            player_health = player.player_health,
                            potential_idx1 = -1,//teamLineup[ i ].player_potential_1,
                            potential_idx2 = -1,//teamLineup[ i ].player_potential_2,
                            potential_idx3 = -1,//teamLineup[ i ].player_potential_3,
                            sub_pos_open = 0
                        });
                    }
                }
            }

            players = tempPlayers;
            coaches = tempCoaches;

            return ErrorCode.SUCCESS;
        }
        public ErrorCode CreateNewTeam(int teamIdx, out List<Player> players, out List<TeamCoach> coaches, int pitcherIdx, int batterIdx, int coachIdx)
        {
            players = null;
            coaches = null;
            byte country = 0;

            country = GetTeamInfo(teamIdx).country_flg;
            List<PB_TEAM_COUNTRY_SQUAD> tempTeamSquad = new List<PB_TEAM_COUNTRY_SQUAD>();
            tempTeamSquad = _teamCountrySquad.FindAll(x => x.league_flg == country);
            List<Player> tempPlayers = new List<Player>();
            List<TeamCoach> tempCoaches = new List<TeamCoach>();
            Dictionary<int, List<TeamPaidPlayerList>> tempTeamPaidPlayerInfo = new Dictionary<int, List<TeamPaidPlayerList>>();
            PBPlayer selectedPitcher = null;
            PBPlayer selectedBatter = null;
            PB_COACH selectedCoach = null;
            Dictionary<int, int> teamSquadPositionCnt = new Dictionary<int, int>();
            Dictionary<int, bool> isDuplicated = new Dictionary<int, bool>();
            int j = 0;

            for (int i = 1; i <= (int)PLAYER_POSITION.CB; i++)
            {
                teamSquadPositionCnt.Add(i, tempTeamSquad.FindAll(x => x.position == i).Count);
            }
            
            switch (country)
            {
                case (byte)Common.Define.NATION_LEAGUE_TYPE.KBO:
                    tempTeamPaidPlayerInfo = _teamPaidPlayerInfoKBO;
                    break;
                case (byte)Common.Define.NATION_LEAGUE_TYPE.MLB:
                    tempTeamPaidPlayerInfo = _teamPaidPlayerInfoMLB;
                    break;
                case (byte)Common.Define.NATION_LEAGUE_TYPE.NPB:
                    tempTeamPaidPlayerInfo = _teamPaidPlayerInfoNPB;
                    break;
                case (byte)Common.Define.NATION_LEAGUE_TYPE.CPB:
                    tempTeamPaidPlayerInfo = _teamPaidPlayerInfoCPBL;
                    break;

            }
            for (int i = 0; i < tempTeamSquad.Count; i++)
            {
                byte position = tempTeamSquad[i].position;
                byte order = tempTeamSquad[i].order;
                byte playerType = tempTeamSquad[i].player_type;
                int playerIdx = 0;

                if (teamSquadPositionCnt[position] == 1)
                {
                    int rndIdx = 0;
                    int minIdx = tempTeamPaidPlayerInfo[position].First().idx;
                    int maxIdx = tempTeamPaidPlayerInfo[position].Last().idx;

                    rndIdx = RandomManager.Instance.GetCount(minIdx, maxIdx);
                    playerIdx = tempTeamPaidPlayerInfo[position].Find(x => x.idx == rndIdx).player_idx;
                }
                else if (teamSquadPositionCnt[position] > 1) // 중복 포지션 처리.
                {
                    if (isDuplicated.ContainsKey(position) == false)
                    {
                        tempTeamPaidPlayerInfo[position].ShuffleForSelectedCount(teamSquadPositionCnt[position]);
                        isDuplicated.Add(position, true);
                    }
                    playerIdx = tempTeamPaidPlayerInfo[position][j].player_idx;
                    j++;
                }
                else
                {
                    return ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION;
                }

                if (playerType == (byte)PLAYER_TYPE.TYPE_BATTER)
                {
                    tempPlayers.Add(new Player()
                    {
                        player_idx = playerIdx,
                        player_type = playerType,
                        is_starting = (byte)(order < PlayerDefine.InvenOrderStartIdx ? 1 : 0),
                        order = order,
                        position = position,
                        reinforce_grade = 0,
                        player_health = 0,
                        potential_idx1 = -1,
                        potential_idx2 = -1,
                        potential_idx3 = -1,
                        sub_pos_open = 0
                    });
                }
                else if (playerType == (byte)PLAYER_TYPE.TYPE_PITCHER)
                {
                    tempPlayers.Add(new Player()
                    {
                        player_idx = playerIdx,
                        player_type = playerType,
                        is_starting = (byte)(order < PlayerDefine.InvenOrderStartIdx ? 1 : 0),
                        order = order,
                        position = position,
                        reinforce_grade = 0,
                        player_health = _pitcher[playerIdx].player_health,
                        potential_idx1 = -1,
                        potential_idx2 = -1,
                        potential_idx3 = -1,
                        sub_pos_open = 0
                    });
                }
                else if (playerType == (byte)PLAYER_TYPE.TYPE_COACH)
                {
                    tempCoaches.Add(new TeamCoach()
                    {
                        coach_idx = playerIdx,
                        player_type = playerType,
                        position = position,
                        is_starting = (byte)(order < PlayerDefine.InvenOrderStartIdx ? 1 : 0),        
                        coaching_skill = _coach[playerIdx].coaching_skill
                    });
                }

            }
            // 선택 코치 넣어주기.
            if (pitcherIdx > 0)
            {
                selectedPitcher = new PBPlayer();
                if (_teamSelectPlayer.Find(x => x.team_idx == teamIdx && (x.pitcher_idx_1 == pitcherIdx || x.pitcher_idx_2 == pitcherIdx || x.pitcher_idx_3 == pitcherIdx)) != null)
                {
                    selectedPitcher = GetPlayerData(pitcherIdx);
                    tempPlayers.Add(new Player()
                    {
                        player_idx = selectedPitcher.player_idx,
                        player_type = selectedPitcher.player_type,
                        is_starting = 0,
                        order = (byte)PLAYER_ORDER.INVEN_PITCHER,
                        position = (byte)PLAYER_POSITION.INVEN,
                        reinforce_grade = 0,
                        player_health = selectedPitcher.player_health,
                        potential_idx1 = -1,
                        potential_idx2 = -1,
                        potential_idx3 = -1,
                        sub_pos_open = 0
                    });
                }
                else
                {
                    return ErrorCode.ERROR_NOT_MATCHING_TEAM_SELECT_PLAYER;
                }
            }
            else
            {
                return ErrorCode.ERROR_NOT_PLAYER;
            }

            if (batterIdx > 0)
            {
                selectedBatter = new PBPlayer();
                if (_teamSelectPlayer.Find(x => x.team_idx == teamIdx && (x.batter_idx_1 == batterIdx || x.batter_idx_2 == batterIdx || x.batter_idx_3 == batterIdx)) != null)
                {
                    selectedBatter = GetPlayerData(batterIdx);
                    tempPlayers.Add(new Player()
                    {
                        player_idx = selectedBatter.player_idx,
                        player_type = selectedBatter.player_type,
                        is_starting = 0,
                        order = (byte)PLAYER_ORDER.INVEN_BATTER,
                        position = (byte)PLAYER_POSITION.INVEN,
                        reinforce_grade = 0,
                        player_health = 0,
                        potential_idx1 = -1,
                        potential_idx2 = -1,
                        potential_idx3 = -1,
                        sub_pos_open = 0
                    });
                }
                else
                {
                    return ErrorCode.ERROR_NOT_MATCHING_TEAM_SELECT_PLAYER;
                }
            }
            else
            {
                return ErrorCode.ERROR_NOT_PLAYER;
            }

            if (coachIdx > 0)
            {
                selectedCoach = new PB_COACH();
                if (_teamSelectPlayer.Find(x => x.team_idx == teamIdx && (x.coach_idx_1 == coachIdx || x.coach_idx_2 == coachIdx || x.coach_idx_3 == coachIdx)) != null)
                {
                    selectedCoach = GetCoachData(coachIdx);
                    tempCoaches.Add(new TeamCoach()
                    {
                        coach_idx = selectedCoach.coach_idx,
                        player_type = (byte)Common.Define.PLAYER_TYPE.TYPE_COACH,
                        position = -1,
                        is_starting = 0,
                        coaching_skill = selectedCoach.coaching_skill
                    });
                }
                else
                {
                    return ErrorCode.ERROR_NOT_MATCHING_TEAM_SELECT_PLAYER;
                }
            }
            else
            {
                return ErrorCode.ERROR_NOT_COACH;
            }
            players = tempPlayers;
            coaches = tempCoaches;

            return ErrorCode.SUCCESS;
        }

        public PB_TEAM_INFO GetTeamInfo(int teamIdx)
        {
            if (_teamInfo.ContainsKey(teamIdx) == true)
                return _teamInfo[teamIdx];
            else
                return null;
        }

        /// <summary>
        /// 캐릭터 데이터 조회
        /// </summary>
        public PBPlayer GetPlayerData(int index)
        {
            if (_player.TryGetValue(index, out PBPlayer data) == false)
            {
                return null;
            }

            return data;
        }

        public byte GetPlayerSecondPosition(int index)
        {
            if (_player.ContainsKey(index) == false)
                return 0;

            return _player[index].second_position;
        }

        /// <summary>
        /// 코치 데이터 조회
        /// </summary>
        public PB_COACH GetCoachData(int index)
        {
            if (_coach.TryGetValue(index, out PB_COACH data) == false)
            {
                return null;
            }

            return data;
        }

        /// <summary>
        /// 경험치 정보 조회
        /// </summary>>
        /*public int GetPlayerExpInfo(int grade)
        {
            if(true == _dicEvolutionInfo.ContainsKey(grade))
            {
                return _dicEvolutionInfo[grade].exp;
            }
           
            return 0;
        }*/

        public PB_COACH_POSITION GetCoachPositionData(int position)
        {
            if (_coachPosition.TryGetValue(position, out PB_COACH_POSITION data) == false)
            {
                return null;
            }
            return data;
        }

        public PB_COACH_SLOT_BASE GetCoachSlotBaseData(int idx)
        {
            if (_coachSlotInfo.TryGetValue(idx, out PB_COACH_SLOT_BASE data) == false)
            {
                return null;
            }
            return data;
        }
        public PB_SKILL_LEADERSHIP GetCoachLeadershipData(int idx)
        {
            if (_skillLeadership.ContainsKey(idx) == false)
                return null;

            return _skillLeadership[idx];
        }
        public PB_SKILL_COACHING GetCoachSkillData(int idx)
        {
            return _coachingSkillList.Find(x => x.idx == idx);
        }
        
        public Dictionary<int, int> GetDefaultCoachSlotPosition()
        {
            Dictionary<int, int> defaultCoachSlotPosition = new Dictionary<int, int>();
            // coach_slot_base 에서 기본 슬롯번호를 가져오고 coach_position에서 슬롯 번호에 맞는 포지션을 가져와서 슬롯번호,포지션 으로 리턴
            foreach (var data in _coachSlotInfo)
            {
                if (data.Value.coach_slot_type == 0)
                {
                    foreach (var data2 in _coachPosition)
                    {
                        if (data2.Value.lineup_num == data.Key)
                        {
                            defaultCoachSlotPosition.Add(data.Key, data2.Key);
                        }
                    }
                }
            }
            return defaultCoachSlotPosition;
        }

        public ErrorCode InitCoachPosition(ReqCoachInitPosition request, AccountCoach accountCoach, int coachPositionCnt, out byte deleteFlag, out long rtnAccountCoachIdx)
        {
            deleteFlag = 0;
            rtnAccountCoachIdx = -1;

            if (request.SlotIdx > accountCoach.coach_slot_idx)
            {
                return ErrorCode.ERROR_NOT_ENOUGH_COACH_SLOT;
            }

            if (_coachPosition.TryGetValue(request.Position, out PB_COACH_POSITION coachPositionInfo) == false)
            {
                return ErrorCode.ERROR_NOT_MATCHING_PB_COACH_POSITION;
            }

            if (coachPositionInfo.lineup_max_value <= coachPositionCnt)
            {
                return ErrorCode.ERROR_NOT_MATCHING_COACH_POSITION;
            }
            if (accountCoach.coach_idx > 0)
            {
                if (_coach.TryGetValue(accountCoach.coach_idx, out PB_COACH coachPBInfo) == false)
                {
                    return ErrorCode.ERROR_NOT_MATCHING_PB_COACH;
                }

                // 스플릿 해서 보직에 마스터 포지션이 포함되는지 확인
                string[] tempWords = coachPositionInfo.master_position_num.Split('|');
                if (Array.Exists(tempWords, e => e == coachPBInfo.master_position.ToString()) == false)
                {
                    deleteFlag = 1;
                    rtnAccountCoachIdx = accountCoach.account_coach_idx;
                }
            }

            return ErrorCode.SUCCESS;
        }

        public ErrorCode CoachLeadershipOpenCheck(ReqCoachLeadershipOpen request, AccountCoach accountCoach, AccountCoachLeadershipInfo accountCoachLeadershipInfo, out int leadershipIdx)
        {
            leadershipIdx = 0;

            PB_COACH coachData = CacheManager.PBTable.PlayerTable.GetCoachData(accountCoach.coach_idx);

            if (coachData == null)
            {
                return ErrorCode.ERROR_DB_DATA;
            }

            //리더쉽 타입 가능한지 체크
            if (coachData.master_position != (byte)COACH_MASTER_TYPE.TYPE_ALL && request.CategoryType != coachData.master_position)
            {
                return ErrorCode.ERROR_INVALID_MASTER_POSITION;
            }

            List<int> exceptBasicIdxList = new List<int>();
            int exceptLeadershipIdx = -1;
            int preLeadershipIdx = -1;

            if (request.SlotIdx == 1)
            {
                preLeadershipIdx = accountCoachLeadershipInfo.leadership_idx1;
                exceptLeadershipIdx = CacheManager.PBTable.PlayerTable.SameBasicLeadershipCheck(accountCoachLeadershipInfo.leadership_idx2, accountCoachLeadershipInfo.leadership_idx3);
            }
            else if (request.SlotIdx == 2)
            {
                if (accountCoachLeadershipInfo.leadership_idx1 <= 0)
                    return ErrorCode.ERROR_NOT_OPEN_COACH_LEADERSHIP;

                preLeadershipIdx = accountCoachLeadershipInfo.leadership_idx2;
                exceptLeadershipIdx = CacheManager.PBTable.PlayerTable.SameBasicLeadershipCheck(accountCoachLeadershipInfo.leadership_idx1, accountCoachLeadershipInfo.leadership_idx3);
            }
            else if (request.SlotIdx == 3)
            {
                if (accountCoachLeadershipInfo.leadership_idx1 <= 0 || accountCoachLeadershipInfo.leadership_idx2 <= 0)
                    return ErrorCode.ERROR_NOT_OPEN_COACH_LEADERSHIP;

                preLeadershipIdx = accountCoachLeadershipInfo.leadership_idx3;
                exceptLeadershipIdx = CacheManager.PBTable.PlayerTable.SameBasicLeadershipCheck(accountCoachLeadershipInfo.leadership_idx1, accountCoachLeadershipInfo.leadership_idx2);
            }

            if (preLeadershipIdx < 0)
            {
                return ErrorCode.ERROR_NOT_OPEN_COACH_LEADERSHIP;
            }

            if (exceptLeadershipIdx > 0)
            {
                exceptBasicIdxList.Add(exceptLeadershipIdx);
            }

            if (request.ReOpenFlag == true)
            {
                //재개발 일때 
                if (preLeadershipIdx == 0)
                {
                    return ErrorCode.ERROR_INVALID_PARAM;
                }

                int nowBasicIdx = CacheManager.PBTable.PlayerTable.GetLeadershipBasicIdx(preLeadershipIdx);
                //중복이 될수가 없음
                if (exceptBasicIdxList.FindIndex(x => x == nowBasicIdx) != -1)
                {
                    return ErrorCode.ERROR_DB_DATA;
                }

                exceptBasicIdxList.Add(nowBasicIdx);
            }
            else
            {
                //개발 일때 
                if (preLeadershipIdx > 0)
                {
                    return ErrorCode.ERROR_INVALID_PARAM;
                }
            }

            leadershipIdx = CacheManager.PBTable.PlayerTable.CoachLeadershipCreateIdx((COACH_MASTER_TYPE)request.CategoryType, exceptBasicIdxList);

            return ErrorCode.SUCCESS;
        }

        public ErrorCode InputCoach(ReqCoachLineupChange request, List<AccountCoach> listCoach, int coachPosition, out AccountCoach mainCoach, out AccountCoach subCoach)
        {
            mainCoach = null;
            subCoach = null;

            AccountCoach targetCoach = listCoach.Find(x => x.account_coach_idx == request.SrcAccountCoachIdx);
            if (targetCoach == null)
            {
                return ErrorCode.ERROR_INVALID_COACH_DATA;
            }

            ErrorCode inputSlotResult = CheckInputCoachToSlot(targetCoach.coach_idx, coachPosition);
            if (inputSlotResult != ErrorCode.SUCCESS)
            {
                return inputSlotResult;
            }

            ErrorCode inputUniqueIdxResult = CheckInputUniqueIdx(listCoach, targetCoach);
            if (inputUniqueIdxResult != ErrorCode.SUCCESS)
            {
                return inputUniqueIdxResult;
            }

            PB_COACH_SLOT_BASE coachSlotBase = GetCoachSlotBaseData(request.CoachSlotIdx);
            mainCoach = targetCoach;
            subCoach = new AccountCoach
            {
                account_coach_idx = -1
            };

            return ErrorCode.SUCCESS;
        }

        public ErrorCode OutputCoach(ReqCoachLineupChange request, List<AccountCoach> listCoach, out AccountCoach mainCoach, out AccountCoach subCoach)
        {
            mainCoach = null;
            subCoach = null;

            AccountCoach targetCoach = listCoach.Find(x => x.account_coach_idx == request.DstAccountCoachIdx);
            if (targetCoach == null)
            {
                return ErrorCode.ERROR_INVALID_COACH_DATA;
            }

            subCoach = targetCoach;
            mainCoach = new AccountCoach
            {
                account_coach_idx = -1
            };

            return ErrorCode.SUCCESS;
        }

        public ErrorCode ChangeCoachLineup(ReqCoachLineupChange request, List<AccountCoach> listCoach, out AccountCoach mainCoach, out AccountCoach subCoach)
        {
            mainCoach = null;
            subCoach = null;
            List<AccountCoach> coachs = null;

            if (request.ModeType == 0)
            {
                coachs = listCoach.FindAll(x => x.account_coach_idx == request.SrcAccountCoachIdx ||
                                                            x.account_coach_idx == request.DstAccountCoachIdx)
                                                            .OrderByDescending(x => x.is_starting).ToList();
            }
            else if (request.ModeType == 1)
            {
                coachs = listCoach.FindAll(x => x.account_coach_idx == request.SrcAccountCoachIdx ||
                                                            x.account_coach_idx == request.DstAccountCoachIdx)
                                                            .OrderByDescending(x => x.cr_is_starting).ToList();
            }

            //선수 있는지 체크
            if (coachs.Count != 2)
            {
                return ErrorCode.ERROR_INVALID_COACH_DATA;
            }

            mainCoach = coachs[0];
            subCoach = coachs[1];

            //보관함선수끼리는 교체가안됨
            if ((request.ModeType == 0 && mainCoach.is_starting == 0 && subCoach.is_starting == 0) ||
                (request.ModeType == 1 && mainCoach.cr_is_starting == 0 && subCoach.cr_is_starting == 0))
            {
                return ErrorCode.ERROR_REQUEST_DATA;
            }

            // 보관함 선수와 교체
            if ((request.ModeType == 0 && subCoach.is_starting == 0) || (request.ModeType == 1 && subCoach.cr_is_starting == 0))
            {
                ErrorCode inputSlotResult = CheckInputCoachToSlot(subCoach.coach_idx, mainCoach.position);
                if (inputSlotResult != ErrorCode.SUCCESS)
                {
                    return inputSlotResult;
                }
            }
            // 라인업끼리 교체 
            else
            {
                return CheckSwapCoach(mainCoach.coach_idx, mainCoach.position, subCoach.coach_idx, subCoach.position);
            }

            return ErrorCode.SUCCESS;
        }

        private ErrorCode CheckInputCoachToSlot(int coachIdx, int coachPosition)
        {
            //코치 삽입 넣으려는 코치가 포지션 정보에 맞는 코치 인지 확인.
            PB_COACH_POSITION coachPositionInfo = GetCoachPositionData(coachPosition);
            if (coachPositionInfo == null)
            {
                return ErrorCode.ERROR_NOT_MATCHING_PB_COACH_POSITION;
            }

            // 마스터 포지션을 만족 하여야 하는 보직이라면 보관함 선수가 마스터 포지션을 만족하는지.
            PB_COACH coachInfo = GetCoachData(coachIdx);
            string[] positionNums = coachPositionInfo.master_position_num.Split('|');
            if (Array.Exists(positionNums, e => e == coachInfo.master_position.ToString()) == false)
            {
                return ErrorCode.ERROR_NOT_MATCHING_COACH_POSITION;
            }

            return ErrorCode.SUCCESS;
        }

        private ErrorCode CheckInputUniqueIdx(List<AccountCoach> listCoach, AccountCoach inputCoach)
        {
            PB_COACH inputCoachInfo = GetCoachData(inputCoach.coach_idx);

            // unique idx  확인
            foreach (AccountCoach coach in listCoach)
            {
                if (coach.account_coach_idx == inputCoach.account_coach_idx)
                    continue;

                PB_COACH coachInfo = GetCoachData(coach.coach_idx);
                if (coachInfo == null)
                {
                    return ErrorCode.ERROR_NOT_MATCHING_PB_COACH;
                }

                if (coachInfo.coach_unique_idx == inputCoachInfo.coach_unique_idx)
                {
                    return ErrorCode.ERROR_OVERLAP_UNIQUEIDX;
                }
            }

            return ErrorCode.SUCCESS;
        }

        private ErrorCode CheckSwapCoach(int mainCoachIdx, int mainCoachPosition, int subCoachIdx, int subCoachPosition)
        {
            PB_COACH_POSITION mainCoachPositionInfo = GetCoachPositionData(mainCoachPosition);
            PB_COACH_POSITION subCoachPositionInfo = GetCoachPositionData(subCoachPosition);
            if (mainCoachPositionInfo == null || subCoachPositionInfo == null)
            {
                return ErrorCode.ERROR_NOT_MATCHING_PB_COACH_POSITION;
            }

            // 마스터 포지션을 만족 하여야 하는 보직이라면 보관함 선수가 마스터 포지션을 만족하는지.
            PB_COACH mainCoachInfo = GetCoachData(mainCoachIdx);
            PB_COACH subCoachInfo = GetCoachData(subCoachIdx);

            string[] mainPositionNums = mainCoachPositionInfo.master_position_num.Split('|');
            if (Array.Exists(mainPositionNums, e => e == subCoachInfo.master_position.ToString()) == false)
            {
                return ErrorCode.ERROR_NOT_MATCHING_COACH_POSITION;
            }

            string[] subPositionNums = subCoachPositionInfo.master_position_num.Split('|');
            if (Array.Exists(subPositionNums, e => e == mainCoachInfo.master_position.ToString()) == false)
            {
                return ErrorCode.ERROR_NOT_MATCHING_COACH_POSITION;
            }

            return ErrorCode.SUCCESS;
        }

        public ErrorCode CheckPlayerLineupValid(List<PlayerLineupInfo> playerLineupList, Dictionary<long, CareerModePlayer> dicPlayers, byte pType)
        {
            if (pType == (byte)PLAYER_TYPE.TYPE_BATTER)
            {

                if (PlayerDefine.LineupBatterCount != dicPlayers.Count || PlayerDefine.LineupBatterCount != playerLineupList.Count)
                {
                    return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                }

                int[] orderMain = new int[PlayerDefine.PlayBatterCount];
                int[] positionMain = new int[PlayerDefine.PlayBatterCount];
                int[] orderSub = new int[PlayerDefine.LineupBatterCount - PlayerDefine.PlayBatterCount];
                Dictionary<int, int> dicUniqueIdxs = new Dictionary<int, int>();

                foreach (PlayerLineupInfo p in playerLineupList)
                {
                    PBPlayer pbInfo = GetPlayerData(dicPlayers[p.account_player_idx].player_idx);

                    if (pbInfo == null)
                    {
                        return ErrorCode.ERROR_NOT_MATCHING_PB_PLAYER;
                    }

                    //같은선수(유니크인덱스) 중복 체크
                    if (true == dicUniqueIdxs.ContainsKey(pbInfo.player_unique_idx))
                    {
                        return ErrorCode.ERROR_OVERLAP_UNIQUEIDX;
                    }

                    //후보 타자
                    if (p.position == (byte)PLAYER_POSITION.CB)
                    {
                        //후보오더는 9~13 이므로 -9를 해준다
                        ++orderSub[p.order - PlayerDefine.PlayBatterCount];
                    }
                    //선발 타자
                    else
                    {
                        //선발이라면 포지션 맞는지 체크도 하자(DH는 제외)
                        if (p.position != (byte)PLAYER_POSITION.DH)
                        {
                            if (false == (p.position == pbInfo.position || (p.position == pbInfo.second_position && dicPlayers[p.account_player_idx].sub_pos_open == 1)))
                            {
                                return ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION;
                            }
                        }

                        ++orderMain[p.order];               //order는 0베이스
                        ++positionMain[p.position - 1];     //포지션은 1베이스(0은 인벤)
                    }

                    dicUniqueIdxs.Add(pbInfo.player_unique_idx, 1);
                }

                for (int i = 0; i < orderMain.Length; ++i)
                {
                    if (orderMain[i] != 1 || positionMain[i] != 1)
                    {
                        return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                    }
                }

                for (int i = 0; i < orderSub.Length; ++i)
                {
                    if (orderSub[i] == 0)
                    {
                        return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                    }
                }
            }
            else
            {
                if (PlayerDefine.LineupPitcherCount != dicPlayers.Count || PlayerDefine.LineupPitcherCount != playerLineupList.Count)
                {
                    return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                }

                int[] orderMain = new int[PlayerDefine.LineupPitcherCount];
                int[] positionMain = new int[3];
                Dictionary<int, int> dicUniqueIdxs = new Dictionary<int, int>();

                foreach (PlayerLineupInfo p in playerLineupList)
                {
                    PBPlayer pbInfo = GetPlayerData(dicPlayers[p.account_player_idx].player_idx);

                    if (pbInfo == null)
                    {
                        return ErrorCode.ERROR_NOT_MATCHING_PB_PLAYER;
                    }

                    //같은선수(유니크인덱스) 중복 체크
                    if (true == dicUniqueIdxs.ContainsKey(pbInfo.player_unique_idx))
                    {
                        return ErrorCode.ERROR_OVERLAP_UNIQUEIDX;
                    }

                    //선발이라면 포지션 맞는지 체크도 하자(DH는 제외)
                    if (p.position == (byte)PLAYER_POSITION.SP)
                    {
                        if (p.order < PlayerDefine.PitcherOrderStartSP || p.order > PlayerDefine.PitcherOrderStartRP)
                        {
                            return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                        }
                    }
                    else if (p.position == (byte)PLAYER_POSITION.RP)
                    {
                        if (p.order < PlayerDefine.PitcherOrderStartRP || p.order > PlayerDefine.PitcherOrderStartCP)
                        {
                            return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                        }
                    }
                    else if (p.position == (byte)PLAYER_POSITION.CP)
                    {
                        if (p.order != PlayerDefine.PitcherOrderStartCP)
                        {
                            return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                        }
                    }

                    ++orderMain[p.order - PlayerDefine.LineupBatterCount];               //order는 0베이스
                    ++positionMain[p.position - (int)PLAYER_POSITION.SP];     //포지션은 1베이스(0은 인벤)

                    dicUniqueIdxs.Add(pbInfo.player_unique_idx, 1);
                }

                if (positionMain[0] != PlayerDefine.PlayPitcherSPCount ||
                    positionMain[1] != PlayerDefine.PlayPitcherRPCount ||
                    positionMain[2] != PlayerDefine.PlayPitcherCPCount)
                {
                    return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                }

                for (int i = 0; i < orderMain.Length; ++i)
                {
                    if (orderMain[i] != 1)
                    {
                        return ErrorCode.ERROR_INVALID_LINEUP_LIST;
                    }
                }
            }

            return ErrorCode.SUCCESS;
        }
        public ErrorCode CheckRecommendCoachLineup(List<AccountCoach> listCoach, List<AccountCoachSlot> listCoachSlot)
        {
            foreach (var coach in listCoach)
            {
                var slotData = listCoachSlot.Find(x => x.idx == coach.coach_slot_idx);

                if (slotData == null)
                {
                    return ErrorCode.ERROR_INVALID_SLOTIDX;
                }

                PB_COACH_POSITION coachPositionInfo = GetCoachPositionData(slotData.position);
                PB_COACH coachData = GetCoachData(coach.coach_idx);

                string[] positionNums = coachPositionInfo.master_position_num.Split('|');

                if (Array.Exists(positionNums, e => e == coachData.master_position.ToString()) == false)
                {
                    return ErrorCode.ERROR_NOT_MATCHING_COACH_POSITION;
                }
            }

            return ErrorCode.SUCCESS;
        }

        public void CoachPowerTraning(long accountCoachIdx, ref AccountCoachPowerTrainingInfo accountCoachPowerTrainingInfo)
        {
            int[] probRateList = new int[_coachReinforcePowerList.Count()];
            int rateMaxValue = 0;
            foreach (var data in _coachReinforcePowerList)
            {
                probRateList[data.idx - 1] = data.rank_rate;
                rateMaxValue += data.rank_rate;
            }

            if (accountCoachPowerTrainingInfo == null)
            {
                accountCoachPowerTrainingInfo = new AccountCoachPowerTrainingInfo
                {
                    account_coach_idx = accountCoachIdx,
                    coaching_psychology = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1,
                    coaching_theory = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1,
                    communication = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1,
                    technical_training_theory = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1,
                    training_theory = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1
                };
            }
            else
            {
                int nextGrade = 0;

                // 심리학에서 아직 최고 등급 달성 못했다면
                if (accountCoachPowerTrainingInfo.coaching_psychology < _coachReinforcePowerList.Count())
                {
                    nextGrade = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1;

                    if (accountCoachPowerTrainingInfo.coaching_psychology < nextGrade)
                    {
                        accountCoachPowerTrainingInfo.coaching_psychology = nextGrade;
                    }
                }

                // 코칭이론에서 아직 최고 등급 달성 못했다면
                if (accountCoachPowerTrainingInfo.coaching_theory < _coachReinforcePowerList.Count())
                {
                    nextGrade = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1;

                    if (accountCoachPowerTrainingInfo.coaching_theory < nextGrade)
                    {
                        accountCoachPowerTrainingInfo.coaching_theory = nextGrade;
                    }
                }

                // 커뮤니케이션에서 아직 최고 등급 달성 못했다면
                if (accountCoachPowerTrainingInfo.communication < _coachReinforcePowerList.Count())
                {
                    nextGrade = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1;

                    if (accountCoachPowerTrainingInfo.communication < nextGrade)
                    {
                        accountCoachPowerTrainingInfo.communication = nextGrade;
                    }
                }

                // 기술훈련에서 아직 최고 등급 달성 못했다면
                if (accountCoachPowerTrainingInfo.technical_training_theory < _coachReinforcePowerList.Count())
                {
                    nextGrade = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1;

                    if (accountCoachPowerTrainingInfo.technical_training_theory < nextGrade)
                    {
                        accountCoachPowerTrainingInfo.technical_training_theory = nextGrade;
                    }
                }

                // 훈련이론에서 아직 최고 등급 달성 못했다면
                if (accountCoachPowerTrainingInfo.training_theory < _coachReinforcePowerList.Count())
                {
                    nextGrade = RandomManager.Instance.GetSuccessIdxFromRatioList(probRateList, rateMaxValue) + 1;

                    if (accountCoachPowerTrainingInfo.training_theory < nextGrade)
                    {
                        accountCoachPowerTrainingInfo.training_theory = nextGrade;
                    }
                }
            }
        }

        public int GetMaxCoachPowerTrainingGrade()
        {
            return _coachReinforcePowerList.Count();
        }

        public int CoachLeadershipCreateIdx(COACH_MASTER_TYPE category, List<int> exceptBasicIdx)
        {
            List<int> accumulateRateList;
            List<PB_SKILL_LEADERSHIP> LeadershipList;

            if (category == COACH_MASTER_TYPE.TYPE_PITCHER)
            {
                accumulateRateList = _pitcherLeadershipCreateRate;
                LeadershipList = _pitcherLeadershipGradeList[0];
            }
            else if (category == COACH_MASTER_TYPE.TYPE_BATTER)
            {
                accumulateRateList = _batterLeadershipCreateRate;
                LeadershipList = _batterLeadershipGradeList[0];
            }
            else
            {
                accumulateRateList = _trainerLeadershipCreateRate;
                LeadershipList = _trainerLeadershipGradeList[0];
            }

            List<int> exceptListIdxs = new List<int>();

            if (exceptBasicIdx != null && exceptBasicIdx.Count > 0)
            {
                foreach (int bIdx in exceptBasicIdx)
                {
                    int idx = LeadershipList.FindIndex(x => x.basic_idx == bIdx);
                    if (idx > -1)
                    {
                        exceptListIdxs.Add(idx);
                    }
                }
            }

            int listIdx = RandomManager.Instance.GetSuccessIdxFromAccumulateRatioList(accumulateRateList, exceptListIdxs);
            return LeadershipList[listIdx].idx;
        }

        public ErrorCode CoachingSkillRankUp(int coachingSkillIdx, ref int coachingSkillFailrevision, out int nextCoachingSkillIdx)
        {
            nextCoachingSkillIdx = 0;
            //코칭 스킬 등급 가져오기
            PB_SKILL_COACHING coachingSkillInfo = GetCoachSkillData(coachingSkillIdx);

            int rate = _coachSkillRankupList.Find(x => x.idx == coachingSkillInfo.grade).coachskill_rankup_rate + coachingSkillFailrevision;
            int probMax = 0;
            foreach (var data in _coachSkillRankupList)
            {
                probMax += data.coachskill_rankup_rate;
            }

            if (RandomManager.Instance.IsSuccessRatio(rate, probMax) == true)
            {
                nextCoachingSkillIdx = coachingSkillInfo.next_grade_idx;
                coachingSkillFailrevision = 0;
                if (nextCoachingSkillIdx == 0)
                {
                    return ErrorCode.ERROR_NOT_MATCHING_PB_SKILL_COACHING;
                }
            }
            else
            {
                coachingSkillFailrevision += _coachSkillRankupList.Find(x => x.idx == coachingSkillInfo.grade).coachskill_rankup_rate_failrevision;
            }

            return ErrorCode.SUCCESS;
        }
        public int GetCoachAddedReinforcePower(int grade1, int grade2, int grade3, int grade4, int grade5)
        {
            int power = 0;
            if (grade1 > 0)
                power += _coachReinforcePowerList.Find(x => x.idx == grade1).add_power;
            if (grade2 > 0)
                power += _coachReinforcePowerList.Find(x => x.idx == grade2).add_power;
            if (grade3 > 0)
                power += _coachReinforcePowerList.Find(x => x.idx == grade3).add_power;
            if (grade4 > 0)
                power += _coachReinforcePowerList.Find(x => x.idx == grade4).add_power;
            if (grade5 > 0)
                power += _coachReinforcePowerList.Find(x => x.idx == grade5).add_power;

            return power;
        }
        public ErrorCode LeadershipRankup(Coach coach, int correction, out int[] nextLeadershipIdx)
        {
            nextLeadershipIdx = new int[PlayerDefine.LeadershipMaxSlot];
            int[] leadershipBasicIdx = new int[3];

            COACH_MASTER_TYPE masterType = (COACH_MASTER_TYPE)GetCoachData(coach.coach_idx).master_position;
            List<PB_SKILL_LEADERSHIP>[] LeadershipList;

            if (masterType == COACH_MASTER_TYPE.TYPE_PITCHER)
                LeadershipList = _pitcherLeadershipGradeList;
            else if (masterType == COACH_MASTER_TYPE.TYPE_BATTER)
                LeadershipList = _batterLeadershipGradeList;
            else if (masterType == COACH_MASTER_TYPE.TYPE_TRAINER)
                LeadershipList = _trainerLeadershipGradeList;
            else
                LeadershipList = _allLeadershipGradeList;

            int loopCount = 0;

            if (coach.leadership_idx1 > 0)
            {
                leadershipBasicIdx[0] = _skillLeadership[coach.leadership_idx1].basic_idx;
                loopCount = 1;

                if (coach.leadership_idx2 > 0)
                {
                    leadershipBasicIdx[1] = _skillLeadership[coach.leadership_idx2].basic_idx;
                    loopCount = 2;

                    if (coach.leadership_idx3 > 0)
                    {
                        leadershipBasicIdx[2] = _skillLeadership[coach.leadership_idx3].basic_idx;
                        loopCount = 3;
                    }
                }
            }

            // a등급에 대한 보정치 추가
            int probMax = _coachLeaderShipGradeRatio[_coachLeaderShipGradeRatio.Length - 1] + correction;
            int grade = 0;

            for (int i = 0; i < loopCount; i++)
            {
                int randVal = RandomManager.Instance.GetCount(probMax);

                for (int j = 0; j < _coachLeaderShipGradeRatio.Length; j++)
                {
                    if (randVal <= _coachLeaderShipGradeRatio[j])
                    {
                        grade = j;
                        break;
                    }
                }

                nextLeadershipIdx[i] = LeadershipList[grade].Find(x => x.basic_idx == leadershipBasicIdx[i]).idx;
            }
            return ErrorCode.SUCCESS;
        }

        public void LeadershipRankup(Coach coach, int correction, out AccountTrainingResult accountTrainingResult)
        {
            accountTrainingResult = new AccountTrainingResult
            {
                account_object_idx = coach.account_coach_idx
            };

            COACH_MASTER_TYPE masterType = (COACH_MASTER_TYPE)GetCoachData(coach.coach_idx).master_position;
            List<PB_SKILL_LEADERSHIP>[] LeadershipList;

            if (masterType == COACH_MASTER_TYPE.TYPE_PITCHER)
                LeadershipList = _pitcherLeadershipGradeList;
            else if (masterType == COACH_MASTER_TYPE.TYPE_BATTER)
                LeadershipList = _batterLeadershipGradeList;
            else if (masterType == COACH_MASTER_TYPE.TYPE_TRAINER)
                LeadershipList = _trainerLeadershipGradeList;
            else
                LeadershipList = _allLeadershipGradeList;

            // a등급에 대한 보정치 추가
            int probMax = _coachLeaderShipGradeRatio[_coachLeaderShipGradeRatio.Length - 1] + correction;

            if (coach.leadership_idx1 > 0)
            {
                accountTrainingResult.select_idx1 = GetLeadershipRankupValue(LeadershipList, probMax, coach.leadership_idx1);
            }
            else
            {
                accountTrainingResult.select_idx1 = -1;
            }
            if (coach.leadership_idx2 > 0)
            {
                accountTrainingResult.select_idx2 = GetLeadershipRankupValue(LeadershipList, probMax, coach.leadership_idx2);
            }
            else
            {
                accountTrainingResult.select_idx2 = -1;
            }
            if (coach.leadership_idx3 > 0)
            {
                accountTrainingResult.select_idx3 = GetLeadershipRankupValue(LeadershipList, probMax, coach.leadership_idx3);
            }
            else
            {
                accountTrainingResult.select_idx3 = -1;
            }
        }

        private int GetLeadershipRankupValue(List<PB_SKILL_LEADERSHIP>[] leadershipList, int probMax, int leadershipIdx)
        {
            if (leadershipIdx <= 0)
            {
                return leadershipIdx;
            }

            int basicIdx = _skillLeadership[leadershipIdx].basic_idx;
            int randVal = RandomManager.Instance.GetCount(probMax);
            int grade = 0;

            for (int i = 0; i < _coachLeaderShipGradeRatio.Length; i++)
            {
                int ratioValue = 0;
                if (i == _coachLeaderShipGradeRatio.Length - 1)
                {
                    ratioValue = probMax;
                }
                else
                {
                    ratioValue = _coachLeaderShipGradeRatio[i];
                }

                if (randVal <= ratioValue)
                {
                    grade = i;
                    break;
                }
            }

            return leadershipList[grade].Find(x => x.basic_idx == basicIdx).idx;
        }

        public (byte costType, int costValue) GetCoachingSkillRankUpCost(int coachingSkillIdx)
        {
            return (_coachSkillRankupList.Find(x => x.idx == GetCoachSkillData(coachingSkillIdx).grade).coachskill_rankup_cost_type, _coachSkillRankupList.Find(x => x.idx == GetCoachSkillData(coachingSkillIdx).grade).coachskill_rankup_cost_count);
        }
        public ErrorCode PlayerReinforceTry(Player targetPlayer, out bool isSuccess, out byte openSlotIdx, out List<GameRewardInfo> ConsumeList)
        {
            ConsumeList = null;
            isSuccess = false;
            openSlotIdx = 0;
            int tryGrade = targetPlayer.reinforce_grade + 1;

            if (_playerReinforceInfo.ContainsKey(tryGrade) == false)
                return ErrorCode.ERROR_INVALID_REINFORCE_LEVEL;

            int successRatio = _playerReinforceInfo[tryGrade].probability;

            //if (meterialReinforceGrade > 0)
            //    successRatio += _playerReinforceInfo[meterialReinforceGrade].poten_rankup_reinforce_const;


            int randVal = RandomManager.Instance.GetIndex(PlayerDefine.PlayerReinforceTotalRate);

            if (randVal < successRatio + targetPlayer.reinforce_add_rate)
            {
                isSuccess = true;
                targetPlayer.reinforce_grade = (byte)tryGrade;
                targetPlayer.reinforce_add_rate = 0;

                openSlotIdx = _playerReinforceInfo[tryGrade].potential_slot_open;
            }
            else
            {
                targetPlayer.reinforce_add_rate += _playerReinforceInfo[tryGrade].fail_add_probability;
            }

            ConsumeList = new List<GameRewardInfo>();

            foreach (GameRewardInfo reward in _playerReinforceConsumeInfo[tryGrade])
            {
                ConsumeList.Add(new GameRewardInfo(reward.reward_type, 0, reward.reward_cnt));
            }

            return ErrorCode.SUCCESS;
        }
        public int SameBasicPotentialCheck(int potenIdx1, int potenIdx2)
        {
            if (potenIdx1 <= 0 || potenIdx2 <= 0)
                return -1;

            if (_playerPotential[potenIdx1].basic_idx == _playerPotential[potenIdx2].basic_idx)
                return _playerPotential[potenIdx1].basic_idx;

            return -1;
        }

        public int SameBasicLeadershipCheck(int leadershipIdx1, int leadershipIdx2)
        {
            if (leadershipIdx1 <= 0 || leadershipIdx2 <= 0)
                return -1;

            if (_skillLeadership[leadershipIdx1].basic_idx == _skillLeadership[leadershipIdx2].basic_idx)
                return _skillLeadership[leadershipIdx1].basic_idx;

            return -1;
        }

        public int GetPotentialBasicIdx(int potenIdx)
        {
            return _playerPotential[potenIdx].basic_idx;
        }

        public int GetLeadershipBasicIdx(int leadershipIdx)
        {
            return _skillLeadership[leadershipIdx].basic_idx;
        }

        public byte PlayerPotentialPossibleSlotCount(int reinforceGrade)
        {
            if (_playerReinforceInfo.ContainsKey(reinforceGrade) == false)
                return 0;

            return _playerReinforceInfo[reinforceGrade].potential_slot_count;
        }

        public int PlayerPotentialCreateIdx(GAME_MODETYPE gameMode, PLAYER_TYPE playerType, List<int> exceptBasicIdx)
        {
            List<int> accumulateRateList;
            List<PB_PLAYER_SKILL_POTENTIAL> PotentialListGrade;

            if (gameMode == GAME_MODETYPE.MODE_PVP)
            {
                if (playerType == PLAYER_TYPE.TYPE_BATTER)
                {
                    accumulateRateList = _batterPotentialGradePvpRate;
                    PotentialListGrade = _batterPotentialGradeList[0];
                }
                else
                {
                    accumulateRateList = _pitcherPotentialGradePvpRate;
                    PotentialListGrade = _pitcherPotentialGradeList[0];
                }
            }
            else
            {
                int grade = RandomManager.Instance.GetIndex(PlayerDefine.PotentialGradeCount);

                if (playerType == PLAYER_TYPE.TYPE_BATTER)
                {
                    accumulateRateList = _batterPotentialGradeCareerRate[grade];
                    PotentialListGrade = _batterPotentialGradeList[grade];
                }
                else
                {
                    accumulateRateList = _pitcherPotentialGradeCareerRate[grade];
                    PotentialListGrade = _pitcherPotentialGradeList[grade];
                }
            }

            List<int> exceptListIdxs = new List<int>();

            if (exceptBasicIdx != null && exceptBasicIdx.Count > 0)
            {
                foreach (int bIdx in exceptBasicIdx)
                {
                    int idx = PotentialListGrade.FindIndex(x => x.basic_idx == bIdx);
                    if (idx > -1)
                    {
                        exceptListIdxs.Add(idx);
                    }
                }
            }

            int listIdx = RandomManager.Instance.GetSuccessIdxFromAccumulateRatioList(accumulateRateList, exceptListIdxs);

            return PotentialListGrade[listIdx].idx;
        }


        public ErrorCode CheckMaterialPlayers(List<Player> materialPlayers, out int addRate)
        {
            ErrorCode errorCode = ErrorCode.SUCCESS;

            // a등급 가중치 설정
            addRate = 0;
            int havePotentialCount = 0;
            foreach (Player meterial in materialPlayers)
            {
                //라인업인지 체크
                if (meterial.is_starting == 1)
                {
                    errorCode = ErrorCode.ERROR_NOT_USE_LINEUP_PLAYER;
                    break;
                }
                else if (meterial.is_lock == true)
                {
                    errorCode = ErrorCode.ERROR_ALREADY_LOCKED;
                    break;
                }

                if (meterial.reinforce_grade > 0)
                    addRate += _playerReinforceInfo[meterial.reinforce_grade].poten_rankup_reinforce_const;

                if (meterial.potential_idx1 > 0)
                    ++havePotentialCount;

                if (meterial.potential_idx2 > 0)
                    ++havePotentialCount;

                if (meterial.potential_idx3 > 0)
                    ++havePotentialCount;
            }

            if (havePotentialCount > 0)
                addRate += havePotentialCount * CacheManager.PBTable.ConstantTable.Const.potential_rankup_skill_const;

            return errorCode;
        }

        public void PlayerPotentialTraining(Player targetPlayer, int possibleSlotCount, int addRate)
        {
            // a등급에 대한 보정치 추가
            int probMax = _playerPotentialPvpGradeRatio[_playerPotentialPvpGradeRatio.Length - 1] + addRate;

            for (int i = 0; i < possibleSlotCount; ++i)
            {
                if (i == 0)
                    targetPlayer.potential_idx1 = GetPlayerPotentialValue(targetPlayer.player_type, targetPlayer.potential_idx1, probMax);
                else if(i == 1)
                    targetPlayer.potential_idx2 = GetPlayerPotentialValue(targetPlayer.player_type, targetPlayer.potential_idx2, probMax);
                else
                    targetPlayer.potential_idx3 = GetPlayerPotentialValue(targetPlayer.player_type, targetPlayer.potential_idx3, probMax);
            }
        }

        private int GetPlayerPotentialValue(byte playerType, int prePotenIdx, int probMax)
        {
            int resultVal = -1;

            if (prePotenIdx <= 0)
            {
                return resultVal = 0;
            }
            
            int randVal = RandomManager.Instance.GetCount(probMax);
            int grade;
            for (grade = 0; grade < _playerPotentialPvpGradeRatio.Length; ++grade)
            {
                int ratioValue = 0;
                if (grade == _playerPotentialPvpGradeRatio.Length - 1)
                    ratioValue = probMax;
                else
                    ratioValue = _playerPotentialPvpGradeRatio[grade];

                if (randVal <= ratioValue)
                {
                    break;
                }
            }

            if (playerType == (byte)PLAYER_TYPE.TYPE_BATTER)
                resultVal = _batterPotentialGradeList[grade].Find(x => x.basic_idx == _playerPotential[prePotenIdx].basic_idx).idx;
            else
                resultVal = _pitcherPotentialGradeList[grade].Find(x => x.basic_idx == _playerPotential[prePotenIdx].basic_idx).idx;

            return resultVal;

        }

        public Player CreatePlayerInfo(int playerIdx, int reinforceGrade)
        {
            if (_player.TryGetValue(playerIdx, out PBPlayer player) == false)
            {
                return null;
            }

            int possiblePotentialCount = PlayerPotentialPossibleSlotCount(reinforceGrade);

            Player newPlayer = new Player()
            {
                player_idx = player.player_idx,
                player_type = player.player_type,
                is_starting = 0,
                order = (player.player_type == (byte)PLAYER_TYPE.TYPE_BATTER) ? (byte)PLAYER_ORDER.INVEN_BATTER : (byte)PLAYER_ORDER.INVEN_PITCHER,
                position = (byte)PLAYER_POSITION.INVEN,
                reinforce_grade = (byte)reinforceGrade,
                player_health = player.player_health,
                potential_idx1 = (possiblePotentialCount) > 0 ? 0 : -1,
                potential_idx2 = (possiblePotentialCount) > 1 ? 0 : -1,
                potential_idx3 = (possiblePotentialCount) > 2 ? 0 : -1,
                sub_pos_open = 0
            };

            return newPlayer;

        }

        public Coach CreateCoachInfo(int coachIdx)
        {
            if (_coach.TryGetValue(coachIdx, out PB_COACH coach) == false)
            {
                return null;
            }

            Coach newCoach = new Coach()
            {
                coach_idx = coach.coach_idx,
                player_type = (byte)PLAYER_TYPE.TYPE_COACH,
                coaching_skill = coach.coaching_skill,
                is_starting = 0,
                cr_is_starting = 0,
                coach_slot_idx = -1,
                leadership_idx1 = -1,
                leadership_idx2 = -1,
                leadership_idx3 = -1,
                coaching_theory = -1,
                coaching_psychology = -1,
                training_theory = -1,
                technical_training_theory = -1,
                communication = -1
            };

            return newCoach;
        }

        public ErrorCode PlayerLockCheck(ref List<PlayerLockDeleteCheck> accountPlayerList)
        {
            foreach (var player in accountPlayerList)
            {
                if (player.is_lock == 0)
                    player.is_lock = 1;
                else if (player.is_lock == 1)
                    player.is_lock = 0;
            }
            return ErrorCode.SUCCESS;
        }
        public ErrorCode PlayerDeleteCheck(ref List<PlayerLockDeleteCheck> accountPlayerList, byte playerType, byte priceType, int priceValue, out List<GameRewardInfo> consumeList, ref AccountGame accountGameInfo, out int nowPlayerCnt)
        {
            consumeList = new List<GameRewardInfo>();
            nowPlayerCnt = 0;
            int playerCnt = accountPlayerList.Count();

            foreach (var player in accountPlayerList)
            {
                if (player.is_lock == 1)
                    return ErrorCode.ERROR_ALREADY_LOCKED;
                if (player.is_starting == 1)
                    return ErrorCode.ERROR_IS_STARTING;
                if (player.cr_is_starting == 1)
                    return ErrorCode.ERROR_IS_CR_STARTING;
            }

            // 비용 및 선수 수량 감소 처리
            if (playerType == 0 || playerType == 1)
            {
                priceType = (byte)ApiWebServer.Cache.CacheManager.PBTable.ConstantTable.Const.release_player_price_type;
                priceValue = (int)ApiWebServer.Cache.CacheManager.PBTable.ConstantTable.Const.release_player_price_value;

                accountGameInfo.now_player -= playerCnt;
                nowPlayerCnt = accountGameInfo.now_player;
            }
            else if (playerType == 2)
            {
                priceType = (byte)ApiWebServer.Cache.CacheManager.PBTable.ConstantTable.Const.release_coach_price_type;
                priceValue = (int)ApiWebServer.Cache.CacheManager.PBTable.ConstantTable.Const.release_coach_price_value;

                accountGameInfo.now_coach -= playerCnt;
                nowPlayerCnt = accountGameInfo.now_coach;
            }

            consumeList.Add(new GameRewardInfo((byte)priceType, 0, priceValue * playerCnt));

            return ErrorCode.SUCCESS;
        }

    }
}
