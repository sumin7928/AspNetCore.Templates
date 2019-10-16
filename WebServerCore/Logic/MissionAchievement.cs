using System.Collections.Generic;
using Newtonsoft.Json;
using ApiWebServer.Cache;
using ApiWebServer.Database;
using ApiWebServer.Models;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;

namespace ApiWebServer.Logic
{
    public class MissionAchievement
    {
        private readonly long _pcid;
        private readonly GameDB _gameDB;
        private readonly NPWebResponseHeader _resHeader;

        public List<RepeatMission> MissionList { get; private set; }
        public List<Achievement> AchievementList { get; private set; }

        public List<MissionAchievementCount> UpdateMissionList { get; private set; }
        public List<MissionAchievementCount> UpdateAchievementList { get; private set; }

        public Dictionary<int, int> MissionActions { get; private set; }

        public Dictionary<int, int> AchievementActions { get; private set; }

        public MissionAchievement(long pcId, GameDB gameDB, NPWebResponseHeader resHeader)
        {
            _pcid = pcId;
            _gameDB = gameDB;
            _resHeader = resHeader;
        }

        public void Input(List<RepeatMission> userMissionList)
        {
            MissionList = userMissionList;
        }
        public void Input(List<Achievement> userAchievementList)
        {
            AchievementList = userAchievementList;
        }
        public void Input(List<RepeatMission> userMissionList, List<Achievement> userAchievementList)
        {
            MissionList = userMissionList;
            AchievementList = userAchievementList;
        }

        public void AddAction( int actionType, int value )
        {
            AddMissionAction(actionType, value);
            AddAchievementAction(actionType, value);
        }

        public void AddMissionAction(int actionType, int value)
        {
            if (MissionList == null || MissionList.FindIndex(x => x.action_type == actionType) == -1)
                return;

            if (MissionActions == null)
            {
                MissionActions = new Dictionary<int, int>();
            }

            if (MissionActions.ContainsKey(actionType))
            {
                MissionActions[actionType] += value;
            }
            else
            {
                MissionActions.Add(actionType, value);
            }
        }

        public void AddAchievementAction(int actionType, int value)
        {
            if (AchievementList == null || AchievementList.FindIndex(x => x.action_type == actionType) == -1)
                return;

            if (AchievementActions == null)
            {
                AchievementActions = new Dictionary<int, int>();
            }

            if (AchievementActions.ContainsKey(actionType))
            {
                AchievementActions[actionType] += value;
            }
            else
            {
                AchievementActions.Add(actionType, value);
            }
        }



        public bool Update()
        {
            bool isUpdate = false;
            if (MissionActions != null)
            {
                UpdateMissionList = CacheManager.PBTable.MissionAchievementTable.AddMissionCount(MissionList, MissionActions);
                _resHeader.Mission = new Dictionary<string, int>();

                foreach (MissionAchievementCount mission in UpdateMissionList)
                {
                    _resHeader.Mission.Add(mission.idx.ToString(), mission.count);
                }


                MissionActions.Clear();
                MissionActions = null;

                isUpdate = true;
            }

            if (AchievementActions != null)
            {
                UpdateAchievementList = CacheManager.PBTable.MissionAchievementTable.AddAchievementCount(AchievementList, AchievementActions);
                _resHeader.Achievement = new Dictionary<string, int>();

                foreach (MissionAchievementCount achievement in UpdateAchievementList)
                {
                    _resHeader.Achievement.Add(achievement.idx.ToString(), achievement.count);
                }

                AchievementActions.Clear();
                AchievementActions = null;

                isUpdate = true;
            }

            if (isUpdate == true)
            {
                if (_gameDB.USP_GS_GM_ACCOUNT_MISSIONACHIEVEMENT_UPDATE_COUNT(_pcid, UpdateMissionList, UpdateAchievementList) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
