using System;
using System.Collections.Generic;
using System.Linq;
using ApiWebServer.Common;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Entity;
using ApiWebServer.Common.Define;

namespace ApiWebServer.Cache.PBTables
{
    public class MissionAchievementTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<int, PB_REPEAT_MISSION> _missionDic = new Dictionary<int, PB_REPEAT_MISSION>();
        private Dictionary<int, List<int>> _missionListType = new Dictionary<int, List<int>>();

        private Dictionary<int, PB_ACHIEVEMENT> _achievementDic = new Dictionary<int, PB_ACHIEVEMENT>();
        private Dictionary<int, int> _achievementStartIdx = new Dictionary<int, int>();
        private Dictionary<int, int> _achievementMaxValue = new Dictionary<int, int>();

        private Dictionary<int, List<short>> _achievementActionTypeList = new Dictionary<int, List<short>>();


        public bool LoadTable(MaguPBTableContext context)
        {
            // PB_MISSION
            foreach (var data in context.PB_REPEAT_MISSION.ToList())
            {
                _missionDic.Add(data.idx, data);

                if (_missionListType.ContainsKey(data.type) == false)
                    _missionListType.Add(data.type, new List<int>());

                _missionListType[data.type].Add(data.idx);
            }

            //일일미션, 주간미션 고정이 없으면 에러
            for (int i = (int)MISSION_TYPE_PB.SUNDAY; i <= (int)MISSION_TYPE_PB.WEEK; ++i)
            {
                if (_missionListType[i] == null)
                    return false;
            }

            //가변미션이 기준치보다 없으면 에러
            if (_missionListType[(int)MISSION_TYPE_PB.RANDOM] == null || _missionListType[(int)MISSION_TYPE_PB.RANDOM].Count < MissionAchievementDefine.DayRandomMissionCount)
                return false;

            for (int i = (int)SERVICE_NATION_TYPE.KOREA; i < (int)SERVICE_NATION_TYPE.MAX; ++i)
            {
                _achievementActionTypeList.Add(i, new List<short>());
            }

                // PB_ACHIEVEMENT
            foreach (var data in context.PB_ACHIEVEMENT.ToList())
            {
                //_achievement.Add(data);
                _achievementDic.Add(data.idx, data);

                //액션타입 모음
                if (data.flow_number == 1)
                {
                    _achievementStartIdx.Add(data.action_type, data.idx);

                    if (data.korea == 1)
                    {
                        _achievementActionTypeList[(int)SERVICE_NATION_TYPE.KOREA].Add(data.action_type);
                    }

                    if (data.america == 1)
                    {
                        _achievementActionTypeList[(int)SERVICE_NATION_TYPE.AMERICA].Add(data.action_type);
                    }

                    if (data.japan == 1)
                    {
                        _achievementActionTypeList[(int)SERVICE_NATION_TYPE.JAPAN].Add(data.action_type);
                    }

                    if (data.taiwan == 1)
                    {
                        _achievementActionTypeList[(int)SERVICE_NATION_TYPE.TAIWAN].Add(data.action_type);
                    }


                }

                // max value
                if (data.next_level_idx == 0)
                {
                    if (_achievementStartIdx.ContainsKey(data.action_type) == false)
                        return false;

                    _achievementMaxValue.Add(data.action_type, data.action_count);
                }
            }

            if (_achievementStartIdx.Count != _achievementMaxValue.Count)
                return false;

            //유효성 체크
            foreach (KeyValuePair<int, PB_ACHIEVEMENT> pair in _achievementDic)
            {
                if (pair.Value.next_level_idx != 0 && _achievementDic.ContainsKey(pair.Value.next_level_idx) == false)
                    return false;
            }

            return true;
        }

        public void AddMissionDay(List<RepeatMission> missionList, int dayType)
        {
            foreach (int mission in _missionListType[dayType])
            {
                missionList.Add(new RepeatMission()
                {
                    action_type = _missionDic[mission].action_type,
                    idx = mission,
                    type = (byte)MISSION_TYPE_DB.DAY
                });
            }


            List<int> randomMissionIdxList = RandomManager.Instance.GetSuccessIdxListFromIndexList(RANDOM_TYPE.GLOBAL, _missionListType[(int)MISSION_TYPE_PB.RANDOM].Count, MissionAchievementDefine.DayRandomMissionCount);
            foreach (int listIdx in randomMissionIdxList)
            {
                int missionIdx = _missionListType[(int)MISSION_TYPE_PB.RANDOM][listIdx];
                missionList.Add(new RepeatMission()
                {
                    action_type = _missionDic[missionIdx].action_type,
                    idx = missionIdx,
                    type = (byte)MISSION_TYPE_DB.DAY
                });
            }
        }

        public void AddMissionWeek(List<RepeatMission> missionList)
        {
            foreach (int mission in _missionListType[(int)MISSION_TYPE_PB.WEEK])
            {
                missionList.Add(new RepeatMission()
                {
                    action_type = _missionDic[mission].action_type,
                    idx = mission,
                    type = (byte)MISSION_TYPE_DB.WEEK
                });
            }
        }

        public List<Achievement> GetNewAchievement(int nation, List<Achievement> nowAchivementList)
        {
            List<short> newAchievementActionType = _achievementActionTypeList[nation].FindAll(x => nowAchivementList.FindIndex(z => z.action_type == x) == -1);

            if (newAchievementActionType == null || newAchievementActionType.Count == 0)
                return null;

            List<Achievement> newAchievement = new List<Achievement>(); 

            for (int i = 0; i < newAchievementActionType.Count; ++i)
            {
                newAchievement.Add(new Achievement()
                {
                    action_type = newAchievementActionType[i],
                    idx = _achievementStartIdx[newAchievementActionType[i]]
                });
            }

            return newAchievement;
        }

        public List<MissionAchievementCount> AddMissionCount(List<RepeatMission> accountMissionList, Dictionary<int, int> actions)
        {
            List<MissionAchievementCount> updatedMissionList = new List<MissionAchievementCount>();

            foreach (var action in actions)
            {
                List<RepeatMission> missionList = accountMissionList.FindAll(x => x.action_type == action.Key);
                foreach (RepeatMission mission in missionList)
                {
                    mission.count += action.Value;

                    if (mission.count >= _missionDic[mission.idx].action_count)
                    {
                        updatedMissionList.Add(new MissionAchievementCount() { idx = mission.idx, count = _missionDic[mission.idx].action_count });

                        //이제 카운팅할필요없으므로 삭제
                        accountMissionList.Remove(mission);
                    }
                    else
                    {
                        updatedMissionList.Add(new MissionAchievementCount() { idx = mission.idx, count = mission.count });
                    }
                }
            }

            return updatedMissionList;
        }

        public List<MissionAchievementCount> AddAchievementCount(List<Achievement> accountAchievementList, Dictionary<int, int> actions)
        {
            List<MissionAchievementCount> updatedAchievementList = new List<MissionAchievementCount>();

            foreach (var action in actions)
            {
                Achievement achievement = accountAchievementList.Find(x => x.action_type == action.Key);

                achievement.count += action.Value;
                if (achievement.count >= _achievementMaxValue[achievement.action_type])
                {
                    updatedAchievementList.Add(new MissionAchievementCount() { idx = achievement.idx, count = _achievementMaxValue[achievement.action_type] });

                    //이제 카운팅할필요없으므로 삭제
                    accountAchievementList.Remove(achievement);
                }
                else
                {
                    updatedAchievementList.Add(new MissionAchievementCount() { idx = achievement.idx, count = achievement.count });
                }

                
            }

            return updatedAchievementList;
        }

        public bool MissionRewardCheck(List<RepeatMission> accountMissionList, out List<GameRewardInfo> rewardInfo)
        {
            rewardInfo = new List<GameRewardInfo>();

            if (accountMissionList.Count > 0)
            {
                // pb 테이블에서 맥스값과 비교해서 보상 가능한지 확인
                for (var i = 0; i < accountMissionList.Count; i++)
                {
                    int missionIdx = accountMissionList[i].idx;

                    if (accountMissionList[i].count < _missionDic[missionIdx].action_count)
                    {
                        return false;
                    }
                    // 보상 세팅
                    rewardInfo.Add(new GameRewardInfo(_missionDic[missionIdx].reward_type1, _missionDic[missionIdx].reward_idx1, _missionDic[missionIdx].reward_count1));
                    if (_missionDic[missionIdx].reward_type2 > 0)
                    {
                        rewardInfo.Add(new GameRewardInfo(_missionDic[missionIdx].reward_type2, _missionDic[missionIdx].reward_idx2, _missionDic[missionIdx].reward_count2));

                        if (_missionDic[missionIdx].reward_type3 > 0)
                        {
                            rewardInfo.Add(new GameRewardInfo(_missionDic[missionIdx].reward_type3, _missionDic[missionIdx].reward_idx3, _missionDic[missionIdx].reward_count3));
                        }
                    }
                }
            }
            return true;
        }
        public bool AchievementRewardCheck(List<Achievement> accountAchievementist, out List<GameRewardInfo> rewardInfo, out List<AchievementNextIdx> nextAchievementList)
        {
            rewardInfo = new List<GameRewardInfo>();
            nextAchievementList = new List<AchievementNextIdx>();

            if (accountAchievementist.Count > 0)
            {
                // pb 테이블에서 맥스값과 비교해서 보상 가능한지 확인
                for (var i = 0; i < accountAchievementist.Count; i++)
                {
                    int achievementIdx = accountAchievementist[i].idx;

                    if (accountAchievementist[i].count < _achievementDic[achievementIdx].action_count)
                    {
                        return false;
                    }

                    //다음인덱스 정보 저장
                    nextAchievementList.Add(new AchievementNextIdx() { idx = achievementIdx, nextIdx = _achievementDic[achievementIdx].next_level_idx });

                    // 보상 세팅
                    rewardInfo.Add(new GameRewardInfo(_achievementDic[achievementIdx].reward_type1, _achievementDic[achievementIdx].reward_idx1, _achievementDic[achievementIdx].reward_count1));
                    if (_achievementDic[achievementIdx].reward_type2 > 0)
                    {
                        rewardInfo.Add(new GameRewardInfo(_achievementDic[achievementIdx].reward_type2, _achievementDic[achievementIdx].reward_idx2, _achievementDic[achievementIdx].reward_count2));

                        if (_achievementDic[achievementIdx].reward_type3 > 0)
                        {
                            rewardInfo.Add(new GameRewardInfo(_achievementDic[achievementIdx].reward_type3, _achievementDic[achievementIdx].reward_idx3, _achievementDic[achievementIdx].reward_count3));
                        }
                    }
                }
            }
            return true;
        }

        public List<RepeatMission> GetUserMissionList(List<RepeatMission> userMissionList)
        {
            return userMissionList.FindAll(x => x.reward_flag == 0 && x.count < _missionDic[x.idx].action_count);
        }

        public List<Achievement> GetUserAchievementList(List<Achievement> userAchievementList)
        {
            return userAchievementList.FindAll(x => x.idx != 0 && x.count < _achievementMaxValue[x.action_type]);
        }
    }
}
