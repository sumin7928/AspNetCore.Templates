using MsgPack.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Database.Base;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Contents;
using WebSharedLib.Entity;

namespace ApiWebServer.Database
{

    public class GameDB : BaseDB
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public GameDB(string connString, long requestNo)
        {
            ConnString = connString;
            RequestNo = requestNo;
        }

        public virtual bool USP_GS_GM_ACCOUNT_CREATE(long pcId, byte nationType, int playerMaxCount, int coachMaxCount, string nickName, string realNation)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@nation_type", SqlDbType.TinyInt, nationType);
                executor.AddInputParam("@max_player", SqlDbType.Int, playerMaxCount);
                executor.AddInputParam("@max_coach", SqlDbType.Int, coachMaxCount);
                executor.AddInputParam("@pc_name", SqlDbType.NVarChar, 50, nickName);
                executor.AddInputParam("@real_nation", SqlDbType.VarChar, 10, realNation);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_CREATE");

            });
        }

        public virtual DataSet USP_GS_GM_PLAYERLIST_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYERLIST_R");

            });
        }

        public virtual bool USP_GS_GM_PLAYERLIST(long pcId, int nowPlayer, int nowCoach)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@now_player", SqlDbType.Int, nowPlayer);
                executor.AddInputParam("@now_coach", SqlDbType.Int, nowCoach);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYERLIST");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_DATA_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_DATA_R");

            });
        }

        public virtual DataSet USP_GS_GM_PLAYER_TEAMCREATE_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_TEAMCREATE_R");

            });
        }

        public virtual bool USP_GS_GM_PLAYER_TEAMCREATE(long pcId, string playerList, string coachList, string coachSlotList, int teamIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@player_lineup_list", SqlDbType.VarChar, -1, playerList);
                executor.AddInputParam("@coach_lineup_list", SqlDbType.VarChar, -1, coachList);
                executor.AddInputParam("@coach_slot_list", SqlDbType.VarChar, 256, coachSlotList);
                executor.AddInputParam("@team_Idx", SqlDbType.Int, teamIdx);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_TEAMCREATE");
            });
        }

        public virtual bool USP_GM_CREATE_USER_NAME(long pcId, string userName)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@pc_name", SqlDbType.NVarChar, 50, userName);

                return executor.RunStoredProcedure("dbo.USP_GM_CREATE_USER_NAME");

            });
        }

        public virtual DataSet USP_GS_GM_ACCOUNT_LOGIN_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_LOGIN_R");
            });
        }

        public virtual bool USP_GS_GM_ACCOUNT_LOGIN(long pcId, string newMissions, string newAchievements)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@new_missions", SqlDbType.VarChar, 1024, newMissions);
                executor.AddInputParam("@new_achievements", SqlDbType.VarChar, 1024, newAchievements);
                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_LOGIN");
            });
        }

        public virtual DataSet USP_GS_GM_ACCOUNT_CHECK_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_CHECK_R");
            });
        }

        public virtual bool USP_GS_GM_ACCOUNT_CHECK(long pcId, bool isDayChange, int dayIdx, int weekIdx, string DeleteMissionType, List<RepeatMission> newMission, List<Achievement> newAchivement)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@is_day_change", SqlDbType.TinyInt, isDayChange == true ? 1: 0);
                executor.AddInputParam("@day_idx", SqlDbType.Int, dayIdx);
                executor.AddInputParam("@week_idx", SqlDbType.Int, weekIdx);
                executor.AddInputParam("@delete_mission_type", SqlDbType.VarChar, 16, DeleteMissionType);
                executor.AddInputParam("@new_mission", SqlDbType.VarChar, 8000, newMission == null ? "" : JsonConvert.SerializeObject(newMission));
                executor.AddInputParam("@new_achivement", SqlDbType.VarChar, 8000, newAchivement == null ? "" : JsonConvert.SerializeObject(newAchivement));
                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_CHECK");
            });
        }

        public virtual DataSet USP_GS_GM_ACCOUNT_GAME_ONLY_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_GAME_ONLY_R");
            });
        }

        public virtual DataSet USP_GS_GM_PLAYER_LINEUP_CHANGE_R(long pcID, byte modeType, byte playerType, long srcAccountPlayerIdx, long dscAccountPlayerIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, modeType);
                executor.AddInputParam("@player_type", SqlDbType.TinyInt, playerType);
                executor.AddInputParam("@src_account_player_idx", SqlDbType.BigInt, srcAccountPlayerIdx);
                executor.AddInputParam("@dst_account_player_idx", SqlDbType.BigInt, dscAccountPlayerIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_LINEUP_CHANGE_R");
            });
        }

        public virtual bool USP_GS_GM_PLAYER_LINEUP_CHANGE(long pcID, byte modeType, long srcAccountPlayer, byte srcPos, byte srcOrder, byte srcStarting, long dstAccountPlayer, byte dstPos, byte dstOrder, byte dstStarting)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, modeType);
                executor.AddInputParam("@src_account_player_idx", SqlDbType.BigInt, srcAccountPlayer);
                executor.AddInputParam("@src_pos", SqlDbType.TinyInt, srcPos);
                executor.AddInputParam("@src_order", SqlDbType.TinyInt, srcOrder);
                executor.AddInputParam("@src_starting", SqlDbType.TinyInt, srcStarting);
                executor.AddInputParam("@dst_account_player_idx", SqlDbType.BigInt, dstAccountPlayer);
                executor.AddInputParam("@dst_pos", SqlDbType.TinyInt, dstPos);
                executor.AddInputParam("@dst_order", SqlDbType.TinyInt, dstOrder);
                executor.AddInputParam("@dst_starting", SqlDbType.TinyInt, dstStarting);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_LINEUP_CHANGE");

            });

        }

        public virtual DataSet USP_GS_GM_PLAYER_LINEUP_RECOMMEND_R(long pcID, byte modeType, byte playerType, string playerStr)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, modeType);
                executor.AddInputParam("@player_type", SqlDbType.TinyInt, playerType);
                executor.AddInputParam("@player_list", SqlDbType.VarChar, 2000, playerStr);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_LINEUP_RECOMMEND_R");
            });
        }


        public virtual bool USP_GS_GM_PLAYER_LINEUP_RECOMMEND(long pcID, byte modeType, byte playerType, string lineupInfo, byte invenPosition, byte invenOrder)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, modeType);
                executor.AddInputParam("@player_type", SqlDbType.TinyInt, playerType);
                executor.AddInputParam("@lineup_lnfo", SqlDbType.VarChar, 5000, lineupInfo);
                executor.AddInputParam("@inven_position", SqlDbType.TinyInt, invenPosition);
                executor.AddInputParam("@inven_order", SqlDbType.TinyInt, invenOrder);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_LINEUP_RECOMMEND");

            });

        }
        public virtual DataSet USP_GS_GM_ACCOUNT_ITEM_R(long pcID, string itemIdxs)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@item_idxs", SqlDbType.VarChar, 1000, itemIdxs);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_ITEM_R");
            });
        }

        public virtual DataSet USP_GS_GM_RECV_POST(long pcID, AccountGame accountGame, List<ItemInven> rewardItems,
            List<Player> addPlayerList, List<Coach> addCoachList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems == null ? "" : JsonConvert.SerializeObject(rewardItems));
                executor.AddInputParam("@add_player_count", SqlDbType.Int, addPlayerList == null ? 0 : addPlayerList.Count);
                executor.AddInputParam("@add_player_info", SqlDbType.VarChar, 8000, addPlayerList == null ? "" : JsonConvert.SerializeObject(addPlayerList));
                executor.AddInputParam("@add_coach_count", SqlDbType.Int, addCoachList == null ? 0 : addCoachList.Count);
                executor.AddInputParam("@add_coach_info", SqlDbType.VarChar, 8000, addCoachList == null ? "" : JsonConvert.SerializeObject(addCoachList));

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_RECV_POST");

            });
        }

        public virtual bool USP_GS_GM_REWARD_PROCESS(long pcID, AccountGame accountGame, List<ItemInven> rewardItems)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems == null ? "" : JsonConvert.SerializeObject(rewardItems));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_REWARD_PROCESS");

            });
        }

        public virtual DataSet USP_GS_GM_REWARD_PROCESS_WITH_CHARACTER(long pcID, AccountGame accountGame, List<ItemInven> rewardItems, List<Player> addPlayerList, List<Coach> addCoachList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems == null ? "" : JsonConvert.SerializeObject(rewardItems));
                executor.AddInputParam("@add_player_count", SqlDbType.Int, addPlayerList.Count);
                executor.AddInputParam("@add_player_info", SqlDbType.VarChar, 8000, addPlayerList.Count == 0 ? "" : JsonConvert.SerializeObject(addPlayerList));
                executor.AddInputParam("@add_coach_count", SqlDbType.Int, addCoachList.Count);
                executor.AddInputParam("@add_coach_info", SqlDbType.VarChar, 8000, addCoachList.Count == 0 ? "" : JsonConvert.SerializeObject(addCoachList));

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_REWARD_PROCESS_WITH_CHARACTER");

            });
        }

        public virtual bool USP_GS_GM_DAILY_INFO_U(long pcID, int storeType, string missionlist)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@store_type", SqlDbType.Int, storeType);
                executor.AddInputParam("@mission_list", SqlDbType.VarChar, 1000, missionlist);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_DAILY_INFO_U");
            });
        }

        public virtual bool USP_GS_GM_ACCOUNT_GAME_U(long pcID, string achievementList, string jsonMasteryList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@achievement_list", SqlDbType.VarChar, 1000, achievementList);
                executor.AddInputParam("@mastery_list", SqlDbType.VarChar, 128, jsonMasteryList);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_GAME_U");
            });
        }


        public virtual bool USP_GS_GM_ACCOUNT_MISSIONACHIEVEMENT_UPDATE_COUNT(long pcID, List<MissionAchievementCount> userMissionlist, List<MissionAchievementCount> userAchievementlist)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mission_list", SqlDbType.VarChar, 512, userMissionlist == null ? "" : JsonConvert.SerializeObject(userMissionlist));
                executor.AddInputParam("@achievement_list", SqlDbType.VarChar, 512, userAchievementlist == null ? "" : JsonConvert.SerializeObject(userAchievementlist));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_MISSIONACHIEVEMENT_UPDATE_COUNT");
            });
        }

        public virtual DataSet USP_GS_GM_ACCOUNT_MISSION_REWARD_R(long pcID, string finishMissionlist)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mission_list", SqlDbType.VarChar, 128, finishMissionlist);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_MISSION_REWARD_R");
            });
        }
        public virtual bool USP_GS_GM_ACCOUNT_MISSION_REWARD(long pcID, string finishMissionlist, AccountGame accountGame, List<ItemInven> rewardItems)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mission_list", SqlDbType.VarChar, 128, finishMissionlist);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems != null ? JsonConvert.SerializeObject(rewardItems) : "");

                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_MISSION_REWARD");
            });
        }

        public virtual DataSet USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD_R(long pcID, string finishAchievementlist)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@achievement_list", SqlDbType.VarChar, 128, finishAchievementlist);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD_R");
            });
        }
        public virtual bool USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD(long pcID, List<AchievementNextIdx> finishAchievementlnfo, AccountGame accountGame, List<ItemInven> rewardItems)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@achievement_info", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(finishAchievementlnfo));
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems != null ? JsonConvert.SerializeObject(rewardItems) : "");

                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_ACHIEVEMENT_REWARD");
            });
        }

        public virtual DataSet USP_GS_GM_ACCOUNT_MODE_RECORD_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_MODE_RECORD_R");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_RECORD_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_RECORD_R");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_CREATE_TEAM_R(long pcID, int teamIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@team_idx", SqlDbType.Int, teamIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_CREATE_TEAM_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_CREATE_TEAM(long pcID, string jsonPlayers, int teamIdx, PB_TEAM_INFO teamInfo, byte halfType,
                                            byte modeLevel, byte contractNo, string jsonMissions, RecommendTeamInfo recommendTeamInfo)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@player_lineup_list", SqlDbType.VarChar, -1, jsonPlayers);
                executor.AddInputParam("@team_idx", SqlDbType.Int, teamIdx);
                executor.AddInputParam("@country_type", SqlDbType.TinyInt, teamInfo.country_flg);
                executor.AddInputParam("@league_type", SqlDbType.TinyInt, teamInfo.league_flg);
                executor.AddInputParam("@area_type", SqlDbType.TinyInt, teamInfo.area_flg);
                executor.AddInputParam("@half_type", SqlDbType.TinyInt, halfType);
                executor.AddInputParam("@mode_level", SqlDbType.TinyInt, modeLevel);
                executor.AddInputParam("@contract_no", SqlDbType.TinyInt, contractNo);
                executor.AddInputParam("@recommend_buff_val", SqlDbType.Int, recommendTeamInfo != null ? recommendTeamInfo.buff_val : -1);
                executor.AddInputParam("@recommend_reward_idx", SqlDbType.Int, recommendTeamInfo != null ? recommendTeamInfo.reward_idx : -1);
                executor.AddInputParam("@mission_list", SqlDbType.VarChar, 1024, jsonMissions);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_CREATE_TEAM");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_SKIP_R(long pcID)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_SKIP_R");

            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_SKIP(long pcID, CareerModeInfo careerModeInfo, string matchTeamRecord, byte[] gameRecord )
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@now_career_cnt", SqlDbType.Int, careerModeInfo.total_career_cnt);
                executor.AddInputParam("@next_degree_no", SqlDbType.Int, careerModeInfo.degree_no);
                executor.AddInputParam("@match_group", SqlDbType.TinyInt, careerModeInfo.match_group);
                executor.AddInputParam("@match_type", SqlDbType.TinyInt, careerModeInfo.match_type);

                executor.AddInputParam("@finish_match_group", SqlDbType.TinyInt, careerModeInfo.finish_match_group);
                executor.AddInputParam("@rank", SqlDbType.TinyInt, careerModeInfo.now_rank);

                executor.AddInputParam("@match_team_record", SqlDbType.VarChar, -1, matchTeamRecord);
                executor.AddInputParam("@game_record", SqlDbType.VarBinary, -1, gameRecord);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_SKIP");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_GAME_START_R(long pcID, int CheckItemIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@check_item_idx", SqlDbType.Int, CheckItemIdx);
                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_GAME_START_R");

            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_GAME_START(long pcID, string battleKey, byte simulItemUse)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@battle_key", SqlDbType.VarChar, 128, battleKey);
                executor.AddInputParam("@is_simul_item", SqlDbType.TinyInt, simulItemUse);
                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_GAME_START");

            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_GAME_END_R(long pcID, long mvpPlayer, string playingPlayer)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mvp_player", SqlDbType.BigInt, mvpPlayer);
                executor.AddInputParam("@playing_player", SqlDbType.VarChar, 2000, playingPlayer);
                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_GAME_END_R");

            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_GAME_END_RANDOM_PLAYER(long pcID, int[] searchPlayerCount)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@all_player", SqlDbType.Int, searchPlayerCount[0]);
                executor.AddInputParam("@all_batter", SqlDbType.Int, searchPlayerCount[1]);
                executor.AddInputParam("@all_picther", SqlDbType.Int, searchPlayerCount[2]);
                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_GAME_END_RANDOM_PLAYER");

            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_GAME_END(long pcID, ReqCareerModeGameEnd reqCareerModeInfo, CareerModeInfo careerModeInfo, List<PlayerCareerPlayingInfo> updatePlayerInfo, AccountGame accountGame, List<ItemInven> rewardItems,
            List<CareerModeMission> missions, string teamRecord, byte[] gameRecord, byte deleteEventFlag, List<CareerModeCycleEventInfo> newCycleEventList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@career_no", SqlDbType.SmallInt, careerModeInfo.career_no);
                executor.AddInputParam("@team_idx", SqlDbType.Int, careerModeInfo.team_idx);
                executor.AddInputParam("@match_group", SqlDbType.TinyInt, careerModeInfo.match_group);
                executor.AddInputParam("@match_type", SqlDbType.TinyInt, careerModeInfo.match_type);
                executor.AddInputParam("@half_type", SqlDbType.TinyInt, careerModeInfo.half_type);
                executor.AddInputParam("@finish_match_group", SqlDbType.TinyInt, careerModeInfo.finish_match_group);
                executor.AddInputParam("@specialtraining_step", SqlDbType.TinyInt, careerModeInfo.specialtraining_step);

                executor.AddInputParam("@teammood", SqlDbType.SmallInt, careerModeInfo.teammood);
                executor.AddInputParam("@event_flag", SqlDbType.TinyInt, careerModeInfo.event_flag);
                executor.AddInputParam("@injury_game_no_new", SqlDbType.Int, careerModeInfo.injury_game_no_new);
                executor.AddInputParam("@injury_game_no_chain", SqlDbType.Int, careerModeInfo.injury_game_no_chain);
                executor.AddInputParam("@injury_group1", SqlDbType.TinyInt, careerModeInfo.injury_group1);
                executor.AddInputParam("@injury_group2", SqlDbType.TinyInt, careerModeInfo.injury_group2);
                executor.AddInputParam("@injury_group3", SqlDbType.TinyInt, careerModeInfo.injury_group3);
                executor.AddInputParam("@injury_group4", SqlDbType.TinyInt, careerModeInfo.injury_group4);

                executor.AddInputParam("@game_result", SqlDbType.TinyInt, reqCareerModeInfo.GameResult);
                executor.AddInputParam("@rank", SqlDbType.TinyInt, reqCareerModeInfo.Rank);

                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems != null ? JsonConvert.SerializeObject(rewardItems) : "");
                executor.AddInputParam("@update_mission", SqlDbType.VarChar, 1024, missions != null ? JsonConvert.SerializeObject(missions) : "");

                executor.AddInputParam("@team_record", SqlDbType.VarChar, 512, teamRecord);
                executor.AddInputParam("@score_record", SqlDbType.VarChar, 128, reqCareerModeInfo.ScoreRecord);
                executor.AddInputParam("@match_team_record", SqlDbType.VarChar, -1, JsonConvert.SerializeObject(reqCareerModeInfo.MatchTeamRecord));
                executor.AddInputParam("@game_record", SqlDbType.VarBinary, -1, gameRecord);

                executor.AddInputParam("@update_player", SqlDbType.VarChar, 8000, JsonConvert.SerializeObject(updatePlayerInfo));

                executor.AddInputParam("@delete_event_flag", SqlDbType.TinyInt, deleteEventFlag);
                executor.AddInputParam("@new_event_list", SqlDbType.VarChar, 1024, newCycleEventList != null ? JsonConvert.SerializeObject(newCycleEventList) : "");

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_GAME_END");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_CONTRACT_R(long pcID)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_CONTRACT_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_CONTRACT(long pcID, string jsonCareerInfo, byte contractType, string newMissionJson)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@career_info", SqlDbType.VarChar, 2048, jsonCareerInfo);
                executor.AddInputParam("@contract_type", SqlDbType.TinyInt, contractType);
                executor.AddInputParam("@new_mission", SqlDbType.VarChar, 1024, newMissionJson);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_CONTRACT");
            });
        }


        public virtual DataSet USP_GS_GM_CAREERMODE_INFO_R(long pcID)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_INFO_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_INFO(long pcID, string jsonCareerInfo, string jsonTeamInfo)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@career_info", SqlDbType.VarChar, 2048, jsonCareerInfo);
                executor.AddInputParam("@create_team_list", SqlDbType.VarChar, -1, jsonTeamInfo);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_INFO");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_SEASON_END_R(long pcID, int careerNo)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@career_no", SqlDbType.Int, careerNo);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_SEASON_END_R");
            });
        }
        public virtual bool USP_GS_GM_CAREERMODE_SEASON_END(long pcID, CareerModeInfo careerModeInfo, AccountGame accountGame, List<ItemInven> rewardItems, List<CareerModeMission> missions, List<CareerModeMission> newMissions)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@last_rank", SqlDbType.VarChar, 1024, careerModeInfo.last_rank);
                executor.AddInputParam("@match_group", SqlDbType.TinyInt, careerModeInfo.match_group);
                executor.AddInputParam("@specialtraining_step", SqlDbType.TinyInt, careerModeInfo.specialtraining_step);

                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems != null ? JsonConvert.SerializeObject(rewardItems) : "");
                executor.AddInputParam("@update_mission", SqlDbType.VarChar, 1024, missions != null ? JsonConvert.SerializeObject(missions) : "");
                executor.AddInputParam("@new_mission", SqlDbType.VarChar, 1024, newMissions != null ? JsonConvert.SerializeObject(newMissions) : "");

                executor.AddInputParam("@previous_contract", SqlDbType.TinyInt, careerModeInfo.previous_contract);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_SEASON_END");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_SEASON_END_RECORD_INFO(long pcID)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_SEASON_END_RECORD_INFO");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_RECONTRACT_REWARD_R(long pcID)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_RECONTRACT_REWARD_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_RECONTRACT_REWARD(long pcID, AccountGame accountGame, List<ItemInven> rewardItems)
        {
            return DBExecute(executor =>
           {
               executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
               executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
               executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems == null ? "" : JsonConvert.SerializeObject(rewardItems));

               return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_RECONTRACT_REWARD");
           });
        }


        public virtual DataSet USP_GS_GM_COACH_OPEN_SLOT_R(long pcId, int coachSlotOpenCostType)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@coach_slot_open_cost_type", SqlDbType.Int, coachSlotOpenCostType);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_OPEN_SLOT_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_OPEN_SLOT(long pcId, PB_COACH_SLOT_BASE coachSlotBaseInfo)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@coach_slot_idx", SqlDbType.Int, coachSlotBaseInfo.idx);
                executor.AddInputParam("@coach_slot_open_cost_type", SqlDbType.Int, coachSlotBaseInfo.coach_slot_open_cost_type);
                executor.AddInputParam("@coach_slot_open_cost_count", SqlDbType.Int, coachSlotBaseInfo.coach_slot_open_cost_count);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_OPEN_SLOT");
            });
        }

        public virtual DataSet USP_GS_GM_COACH_INIT_POSITION_R(long pcId, int slotIdx, int position)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@slotIdx", SqlDbType.Int, slotIdx);
                executor.AddInputParam("@position", SqlDbType.Int, position);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_INIT_POSITION_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_INIT_POSITION(long pcId, int slotIdx, int position, byte deleteFlag)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@coach_slot_idx", SqlDbType.Int, slotIdx);
                executor.AddInputParam("@position", SqlDbType.Int, position);
                executor.AddInputParam("@delete_flag", SqlDbType.TinyInt, deleteFlag);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_INIT_POSITION");
            });
        }

        public virtual DataSet USP_GS_GM_COACH_SLOT_INFO_R(long pcId, byte slotType)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@slot_type", SqlDbType.TinyInt, slotType);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_SLOT_INFO_R");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_SPRINGCAMP_SET_R(long pcId, byte step, string playerList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@step", SqlDbType.TinyInt, step);
                executor.AddInputParam("@player_list", SqlDbType.VarChar, 1024, playerList);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_SPRINGCAMP_SET_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_SPRINGCAMP_SET(long pcId, byte step, List<CareerModeSpringCamp> springCampInfo, List<AccountPlayerTrainingInfo> potenPlayerInfo, byte allStepFinish)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@step", SqlDbType.TinyInt, step);
                executor.AddInputParam("@springcamp_info", SqlDbType.VarChar, -1, JsonConvert.SerializeObject(springCampInfo));
                executor.AddInputParam("@poten_player", SqlDbType.VarChar, 8000, potenPlayerInfo.Count > 0 ? JsonConvert.SerializeObject(potenPlayerInfo) : "");
                executor.AddInputParam("@all_step_finish", SqlDbType.TinyInt, allStepFinish);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_SPRINGCAMP_SET");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET_R(long pcId, byte step, string playerList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@step", SqlDbType.TinyInt, step);
                executor.AddInputParam("@player_list", SqlDbType.VarChar, 1024, playerList);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET(long pcId, byte step, List<CareerModeSpecialTraining> specialTrainingInfo, List<AccountPlayerTrainingInfo> trainingPlayerInfo, byte isTrainingFinish)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@step", SqlDbType.TinyInt, step);
                executor.AddInputParam("@training_info", SqlDbType.VarChar, 8000, JsonConvert.SerializeObject(specialTrainingInfo));
                executor.AddInputParam("@player_info", SqlDbType.VarChar, 8000, JsonConvert.SerializeObject(trainingPlayerInfo));
                executor.AddInputParam("@now_step_finish", SqlDbType.TinyInt, isTrainingFinish);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_SPECIALTRAINING_SET");
            });
        }

        public virtual DataSet USP_GS_GM_COACH_LINEUP_CHANGE_R(long pcID, byte mode_type, int slotIdx, long srcAccountCoachIdx, long dstAccountCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, mode_type);
                executor.AddInputParam("@slot_idx", SqlDbType.Int, slotIdx);
                executor.AddInputParam("@src_account_coach_idx", SqlDbType.BigInt, srcAccountCoachIdx);
                executor.AddInputParam("@dst_account_coach_idx", SqlDbType.BigInt, dstAccountCoachIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_LINEUP_CHANGE_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_LINEUP_CHANGE(long pcID, byte mode_type, long src_account_coach, long dst_account_coach, int slot_idx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, mode_type);
                executor.AddInputParam("@src_account_coach_idx", SqlDbType.BigInt, src_account_coach);
                executor.AddInputParam("@dst_account_coach_idx", SqlDbType.BigInt, dst_account_coach);
                executor.AddInputParam("@slot_idx", SqlDbType.Int, slot_idx);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_LINEUP_CHANGE");

            });

        }
        public virtual DataSet USP_GM_BATTLE_INFO_R(long enemyPcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@enemy_pc_id", SqlDbType.BigInt, enemyPcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GM_BATTLE_INFO_R");
            });
        }

        public virtual DataSet USP_GM_BATTLE_LINEUP_R(long enemyPcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, enemyPcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GM_BATTLE_LINEUP_R");
            });
        }

        public virtual DataSet USP_GS_GM_LIVESEASON_COMPETITION_DETAIL_INFO_R(long pcId, bool targetFlag)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_flag", SqlDbType.TinyInt, targetFlag ? 1 : 0);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_LIVESEASON_COMPETITION_DETAIL_INFO_R");
            });
        }

        public virtual DataSet USP_GS_GM_LIVESEASON_COMPETITION_SEASON_REWARD_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_LIVESEASON_COMPETITION_SEASON_REWARD_R");
            });
        }

        public virtual bool USP_GS_GM_LIVESEASON_COMPETITION_SEASON_REWARD(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_LIVESEASON_COMPETITION_SEASON_REWARD");
            });

        }

        public virtual DataSet USP_GS_GM_LIVESEASON_COMPETITION_END_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_LIVESEASON_COMPETITION_END_R");
            });
        }

        public virtual DataSet USP_GS_GM_LIVESEASON_COMPETITION_END_REWARD(long pcId, int ratingIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@rating_idx", SqlDbType.Int, ratingIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_LIVESEASON_COMPETITION_END_REWARD");
            });
        }

        public virtual bool USP_GS_GM_LIVESEASON_COMPETITION_END(long pcId, string competitionInfo, int promotionRewardIdx, byte gameResult, string accountGame, string rewardItems, byte[] batterRecord, byte[] pitcherRecord, string updatePlayer)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@competition_info", SqlDbType.VarChar, 256, competitionInfo);
                executor.AddInputParam("@promotion_reward_idx", SqlDbType.Int, promotionRewardIdx);
                executor.AddInputParam("@game_result", SqlDbType.TinyInt, gameResult);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, accountGame);
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, rewardItems);
                executor.AddInputParam("@batter_record", SqlDbType.VarBinary, -1, batterRecord);
                executor.AddInputParam("@pitcher_record", SqlDbType.VarBinary, -1, pitcherRecord);
                executor.AddInputParam("@update_player", SqlDbType.VarChar, 8000, updatePlayer);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_LIVESEASON_COMPETITION_END");

            });

        }

        public virtual DataSet USP_GS_GM_LIVESEASON_COMPETITION_START_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_LIVESEASON_COMPETITION_START_R");
            });
        }

        public virtual bool USP_GS_GM_LIVESEASON_COMPETITION_START(long pcId, string battleKey, string matchHistory)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@battle_key", SqlDbType.VarChar, 128, battleKey);
                executor.AddInputParam("@match_history", SqlDbType.VarChar, 128, matchHistory);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_LIVESEASON_COMPETITION_START");

            });

        }

        public virtual DataSet USP_GS_GM_LIVESEASON_INFO_RECORD(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_LIVESEASON_INFO_RECORD");
            });
        }

        public virtual DataSet USP_GS_GM_LIVESEASON_INFO_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_LIVESEASON_INFO_R");
            });
        }

        public virtual bool USP_GS_GM_LIVESEASON_INFO(long pcId, string competitionInfo, byte commendFlag, int lastRatingIdx, long lastRanking)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@competition_info", SqlDbType.VarChar, 512, competitionInfo);
                executor.AddInputParam("@commend_flag", SqlDbType.TinyInt, commendFlag);
                executor.AddInputParam("@last_rating_idx", SqlDbType.Int, lastRatingIdx);
                executor.AddInputParam("@last_ranking", SqlDbType.BigInt, lastRanking);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_LIVESEASON_INFO");

            });

        }

        public virtual DataSet USP_GS_GM_SKILL_MASTERY_REGISTER_R(long pcId, byte category)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@category", SqlDbType.TinyInt, category);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_SKILL_MASTERY_REGISTER_R");
            });
        }

        public virtual bool USP_GS_GM_SKILL_MASTERY_REGISTER(long pcID, List<SkillMastery> masteryInfo, AccountGame account_game)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mastery_info", SqlDbType.VarChar, 8000, JsonConvert.SerializeObject(masteryInfo));
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(account_game));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_SKILL_MASTERY_REGISTER");

            });
        }
        public virtual bool USP_GS_GM_SKILL_MASTERY_CONDITION_REGISTER(long pcID, byte masteryConditionType, string masteryConditionInfo)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@mastery_condition_type", SqlDbType.TinyInt, masteryConditionType);
                executor.AddInputParam("@mastery_condition_info", SqlDbType.VarChar, 64, masteryConditionInfo);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_SKILL_MASTERY_CONDITION_REGISTER");
            });
        }

        public virtual DataSet USP_GS_GM_SKILL_MASTERY_RESET_R(long pcId, byte category)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@category", SqlDbType.TinyInt, category);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_SKILL_MASTERY_RESET_R");
            });
        }

        public virtual bool USP_GS_GM_SKILL_MASTERY_RESET(long pcID, byte category, AccountGame accountGame)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@category", SqlDbType.TinyInt, category);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_SKILL_MASTERY_RESET");

            });
        }
        public virtual DataSet USP_GS_GM_COACH_LINEUP_RECOMMEND_R(long pcId, byte mode_type, string coach_lineup_list)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, mode_type);
                executor.AddInputParam("@coach_lineup_list", SqlDbType.VarChar, 1000, coach_lineup_list);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_LINEUP_RECOMMEND_R");
            });
        }

        public virtual bool USP_GS_GM_COACH_LINEUP_RECOMMEND(long pcId, byte mode_type, string coach_lineup_list)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@mode_type", SqlDbType.TinyInt, mode_type);
                executor.AddInputParam("@coach_lineup_list", SqlDbType.VarChar, 1000, coach_lineup_list);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_LINEUP_RECOMMEND");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_INJURY_CURE_START_R(long pcId, string playerList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@player_list", SqlDbType.VarChar, 1024, playerList);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_INJURY_CURE_START_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_INJURY_CURE_START(long pcId, List<PlayerCareerInjuryInfo> updatePlayerInjuryInfo)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@player_injury_info", SqlDbType.VarChar, 4096, JsonConvert.SerializeObject(updatePlayerInjuryInfo));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_INJURY_CURE_START");
            });
        }

        public virtual DataSet USP_GS_GM_CAREERMODE_INJURY_CURE_END_R(long pcId, string playerList)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@player_list", SqlDbType.VarChar, 1024, playerList);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_INJURY_CURE_END_R");
            });
        }

        public virtual bool USP_GS_GM_CAREERMODE_INJURY_CURE_END(long pcId, CareerModeInfo careerInfo, string updatePlayerInjuryInfo, AccountGame accountGame, bool isDirectCure)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@injury_group1", SqlDbType.TinyInt, careerInfo.injury_group1);
                executor.AddInputParam("@injury_group2", SqlDbType.TinyInt, careerInfo.injury_group2);
                executor.AddInputParam("@injury_group3", SqlDbType.TinyInt, careerInfo.injury_group3);
                executor.AddInputParam("@injury_group4", SqlDbType.TinyInt, careerInfo.injury_group4);
                executor.AddInputParam("@player_injury_info", SqlDbType.VarChar, 1024, updatePlayerInjuryInfo);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, isDirectCure == true ? JsonConvert.SerializeObject(accountGame) : "");

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_INJURY_CURE_END");
            });
        }

        public virtual DataSet USP_GS_GM_ACCOUNT_ITEM_DELETE_R(long pcID, int itemIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@item_idx", SqlDbType.Int, itemIdx);
                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_ACCOUNT_ITEM_DELETE_R");
            });
        }

        public virtual bool USP_GS_GM_ACCOUNT_ITEM_DELETE(long pcID, int itemIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@item_idx", SqlDbType.Int, itemIdx);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_ACCOUNT_ITEM_DELETE");
            });
        }
        public virtual DataSet USP_GS_GM_COACH_POWER_TRAINNING_R(long pcId, long accountCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_POWER_TRAINNING_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_POWER_TRAINING(long pcId, long accountCoachIdx, AccountCoachPowerTrainingInfo powerTrainingGrade, AccountGame accountGame, AccountCoachLeadershipInfo leadershipSlot)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);
                executor.AddInputParam("@power_training_grade", SqlDbType.VarChar, 256, JsonConvert.SerializeObject(powerTrainingGrade));
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@leadership_slot", SqlDbType.VarChar, 256, JsonConvert.SerializeObject(leadershipSlot));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_POWER_TRAINING");
            });
        }
        public virtual DataSet USP_GS_GM_COACH_LEADERSHIP_OPEN_R(long pcId, long accountCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_LEADERSHIP_OPEN_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_LEADERSHIP_OPEN(long pcId, long targetCoachIdx, bool isReOpenFlag, byte slotIdx, int leadershipIdx, AccountGame accountGame)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_coach_idx", SqlDbType.BigInt, targetCoachIdx);
                executor.AddInputParam("@reopen_flag", SqlDbType.TinyInt, (isReOpenFlag == true) ? 1 : 0);
                executor.AddInputParam("@slot_idx", SqlDbType.TinyInt, slotIdx);
                executor.AddInputParam("@leadership_idx", SqlDbType.Int, leadershipIdx);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_LEADERSHIP_OPEN");
            });
        }

        public virtual DataSet USP_GS_GM_COACH_LEADERSHIP_OPEN_END_R(long pcId, long accountCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_coach_idx", SqlDbType.BigInt, accountCoachIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_LEADERSHIP_OPEN_END_R");
            });
        }

        public virtual bool USP_GS_GM_COACH_LEADERSHIP_OPEN_END(long pcId, bool changeFlag, long targetCoachIdx, byte slotIdx, int changeLeadershipIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@change_flag", SqlDbType.TinyInt, changeFlag == true ? 1 : 0);
                executor.AddInputParam("@target_coach_idx", SqlDbType.BigInt, targetCoachIdx);
                executor.AddInputParam("@slot_idx", SqlDbType.TinyInt, slotIdx);
                executor.AddInputParam("@leadership_idx", SqlDbType.Int, changeLeadershipIdx);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_LEADERSHIP_OPEN_END");
            });
        }

        public virtual DataSet USP_GS_GM_COACH_PASSON_LEADERSHIP_START_R(long pcId, long accountCoachIdx, List<long> materialCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);
                executor.AddInputParam("@material_coach_idx", SqlDbType.VarChar, 128, JsonConvert.SerializeObject(materialCoachIdx));

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_PASSON_LEADERSHIP_START_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_PASSON_LEADERSHIP_START(long pcId, long accountCoachIdx, AccountTrainingResult trainingResult, AccountGame accountGame, List<long> materialCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);
                executor.AddInputParam("@coach_training_info", SqlDbType.VarChar, 512, JsonConvert.SerializeObject(trainingResult));
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@material_coach_idx", SqlDbType.VarChar, 128, JsonConvert.SerializeObject(materialCoachIdx));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_PASSON_LEADERSHIP_START");
            });
        }
        public virtual DataSet USP_GS_GM_COACH_PASSON_LEADERSHIP_END_R(long pcId, long accountCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_PASSON_LEADERSHIP_END_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_PASSON_LEADERSHIP_END(long pcId, long accountCoachIdx, AccountTrainingResult accountTraining, bool selectType)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);
                executor.AddInputParam("@coach_training_info", SqlDbType.VarChar, 512, JsonConvert.SerializeObject(accountTraining));
                executor.AddInputParam("@select_flag", SqlDbType.TinyInt, selectType);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_PASSON_LEADERSHIP_END");
            });
        }
        public virtual DataSet USP_GS_GM_COACH_PASSON_COACHINGSKILL_R(long pcId, long accountCoachIdx, long materialCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);
                executor.AddInputParam("@material_coach_idx", SqlDbType.BigInt, materialCoachIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_COACH_PASSON_COACHINGSKILL_R");
            });
        }
        public virtual bool USP_GS_GM_COACH_PASSON_COACHINGSKILL(long pcId, long accountCoachIdx, int coachingSkillIdx, AccountGame accountGame, int failrevision, long materialCoachIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@account_coach_idx", SqlDbType.BigInt, accountCoachIdx);
                executor.AddInputParam("@coaching_skill_idx", SqlDbType.Int, coachingSkillIdx);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@failrevision", SqlDbType.Int, failrevision);
                executor.AddInputParam("@material_coach_idx", SqlDbType.BigInt, materialCoachIdx);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_COACH_PASSON_COACHINGSKILL");
            });
        }
        public virtual DataSet USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT_R(long pcId, int eventIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@event_idx", SqlDbType.Int, eventIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT_R");
            });
        }
        public virtual bool USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT(long pcId, int eventIdx, byte selectIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@event_idx", SqlDbType.Int, eventIdx);
                executor.AddInputParam("@select_idx", SqlDbType.TinyInt, selectIdx);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_CAREERMODE_CYCLE_EVENT_SELECT");
            });
        }

        public virtual DataSet USP_GS_GM_PLAYER_REINFORCE_R(long pcId, long mainPlayerIdx, long materialPlayerIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, mainPlayerIdx);
                executor.AddInputParam("@material_player_idx", SqlDbType.BigInt, materialPlayerIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_REINFORCE_R");
            });
        }

        public virtual bool USP_GS_GM_PLAYER_REINFORCE(long pcId, long mainPlayerIdx, byte reinforceGrade, int failAddRate, byte openSlotIdx, long materialPlayerIdx, AccountGame accountGame)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, mainPlayerIdx);
                executor.AddInputParam("@reinforce_grade", SqlDbType.TinyInt, reinforceGrade);
                executor.AddInputParam("@fail_add_rate", SqlDbType.Int, failAddRate);
                executor.AddInputParam("@open_slot_idx", SqlDbType.TinyInt, openSlotIdx);
                executor.AddInputParam("@material_player_idx", SqlDbType.BigInt, materialPlayerIdx);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_REINFORCE");
            });
        }

        public virtual DataSet USP_GS_GM_PLAYER_POTENTIAL_CREATE_R(long pcId, long mainPlayerIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, mainPlayerIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_POTENTIAL_CREATE_R");
            });
        }

        public virtual bool USP_GS_GM_PLAYER_POTENTIAL_CREATE(long pcId, long targetPlayerIdx, bool isReCreateFlag, byte slotIdx, int potentialIdx, AccountGame accountGame)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, targetPlayerIdx);
                executor.AddInputParam("@recreate_flag", SqlDbType.TinyInt, (isReCreateFlag == true) ? 1 : 0);
                executor.AddInputParam("@slot_idx", SqlDbType.TinyInt, slotIdx);
                executor.AddInputParam("@potential_idx", SqlDbType.Int, potentialIdx);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_POTENTIAL_CREATE");
            });
        }

        public virtual DataSet USP_GS_GM_PLAYER_POTENTIAL_CREATE_END_R(long pcId, long mainPlayerIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, mainPlayerIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_POTENTIAL_CREATE_END_R");
            });
        }

        public virtual bool USP_GS_GM_PLAYER_POTENTIAL_CREATE_END(long pcId, bool changeFlag, long targetPlayerIdx, byte slotIdx, int changePotentialIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@change_flag", SqlDbType.TinyInt, changeFlag == true ? 1 : 0);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, targetPlayerIdx);
                executor.AddInputParam("@slot_idx", SqlDbType.TinyInt, slotIdx);
                executor.AddInputParam("@potential_idx", SqlDbType.Int, changePotentialIdx);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_POTENTIAL_CREATE_END");
            });
        }

        public virtual DataSet USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START_R(long pcId, string playerIdxStr)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@player_list", SqlDbType.VarChar, 512, playerIdxStr);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START_R");
            });
        }

        public virtual bool USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START(long pcId, Player targetPlayer, string playerIdxStr, AccountGame accountGame)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@material_players", SqlDbType.VarChar, 512, playerIdxStr);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, targetPlayer.account_player_idx);
                executor.AddInputParam("@potential_idx1", SqlDbType.Int, targetPlayer.potential_idx1);
                executor.AddInputParam("@potential_idx2", SqlDbType.Int, targetPlayer.potential_idx2);
                executor.AddInputParam("@potential_idx3", SqlDbType.Int, targetPlayer.potential_idx3);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_START");
            });
        }

        public virtual DataSet USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END_R(long pcId, long mainPlayerIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, mainPlayerIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END_R");
            });
        }

        public virtual bool USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END(long pcId, bool changeFlag, long targetPlayerIdx, int potentialIdx1, int potentialIdx2, int potentialIdx3)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@change_flag", SqlDbType.TinyInt, changeFlag == true ? 1 : 0);
                executor.AddInputParam("@target_player_idx", SqlDbType.BigInt, targetPlayerIdx);
                executor.AddInputParam("@potential_idx1", SqlDbType.Int, potentialIdx1);
                executor.AddInputParam("@potential_idx2", SqlDbType.Int, potentialIdx2);
                executor.AddInputParam("@potential_idx3", SqlDbType.Int, potentialIdx3);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END");
            });
        }
        public virtual DataSet USP_GS_GM_PLAYER_LOCK_R(long pcID, List<long> accountPlayerIdxList, int playerType)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_player_idx", SqlDbType.VarChar, 4000, JsonConvert.SerializeObject(accountPlayerIdxList));
                executor.AddInputParam("@player_type", SqlDbType.TinyInt, playerType);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_LOCK_R");
            });
        }
        public virtual bool USP_GS_GM_PLAYER_LOCK(long pcID, List<PlayerLockDeleteCheck> playerLocks, int playerType)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_player_lock", SqlDbType.VarChar, 4000, JsonConvert.SerializeObject(playerLocks));
                executor.AddInputParam("@player_type", SqlDbType.TinyInt, playerType);

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_LOCK");
            });
        }
        public virtual DataSet USP_GS_GM_PLAYER_DELETE_R(long pcID, List<long> accountPlayerIdxList, int playerType)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_player_idx", SqlDbType.VarChar, 4000, JsonConvert.SerializeObject(accountPlayerIdxList));
                executor.AddInputParam("@player_type", SqlDbType.TinyInt, playerType);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_PLAYER_DELETE_R");
            });
        }
        public virtual bool USP_GS_GM_PLAYER_DELETE(long pcID, List<long> accountPlayerIdxList, int playerType, AccountGame accountGame)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_player_idx", SqlDbType.VarChar, 4000, JsonConvert.SerializeObject(accountPlayerIdxList));
                executor.AddInputParam("@player_type", SqlDbType.TinyInt, playerType);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_PLAYER_DELETE");
            });
        }

        public virtual DataSet USP_GS_GM_SCOUT_INFO_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_SCOUT_INFO_R");

            });
        }

        public virtual bool USP_GS_GM_SCOUT_INFO(long pcID, byte isCreate, AccountScoutBinder userBinderInfo)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@is_create", SqlDbType.TinyInt, isCreate);
                executor.AddInputParam("@binder_info", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(userBinderInfo));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_SCOUT_INFO");
            });
        }

        public virtual DataSet USP_GS_GM_SCOUT_BINDER_RESET_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_SCOUT_BINDER_RESET_R");

            });
        }

        public virtual bool USP_GS_GM_SCOUT_BINDER_RESET(long pcID, AccountScoutBinder userBinderInfo, AccountGame accountGame, List<ItemInven> updateItems)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@binder_info", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(userBinderInfo));
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, updateItems != null ? JsonConvert.SerializeObject(updateItems) : "");

                return executor.RunStoredProcedure("dbo.USP_GS_GM_SCOUT_BINDER_RESET");
            });
        }

        public virtual DataSet USP_GS_GM_SCOUT_SEARCH_START_R(long pcId, byte searchSlotIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@search_slot", SqlDbType.TinyInt, searchSlotIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_SCOUT_SEARCH_START_R");

            });
        }

        public virtual bool USP_GS_GM_SCOUT_SEARCH_START(long pcID, AccountScoutSlot scoutSlotInfo, AccountGame accountGame)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@slot_idx", SqlDbType.TinyInt, scoutSlotInfo.slot_idx);
                executor.AddInputParam("@character_type", SqlDbType.TinyInt, scoutSlotInfo.character_type);
                executor.AddInputParam("@search_sec", SqlDbType.Int, scoutSlotInfo.remain_sec);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));

                return executor.RunStoredProcedure("dbo.USP_GS_GM_SCOUT_SEARCH_START");
            });
        }

        public virtual DataSet USP_GS_GM_SCOUT_SEARCH_END_R(long pcId, byte searchSlotIdx)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@search_slot", SqlDbType.TinyInt, searchSlotIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_SCOUT_SEARCH_END_R");

            });
        }

        public virtual DataSet USP_GS_GM_SCOUT_SEARCH_END(long pcID, string binderInfo, byte searchSlotIdx, List<Player> addPlayerList, List<Coach> addCoachList, AccountGame accountGame, List<ItemInven> updateItems)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@account_game", SqlDbType.VarChar, 1024, JsonConvert.SerializeObject(accountGame));
                executor.AddInputParam("@update_item", SqlDbType.VarChar, 1024, updateItems != null ? JsonConvert.SerializeObject(updateItems) : "");
                executor.AddInputParam("@add_player_count", SqlDbType.Int, addPlayerList.Count);
                executor.AddInputParam("@add_player_info", SqlDbType.VarChar, 8000, addPlayerList.Count == 0 ? "" : JsonConvert.SerializeObject(addPlayerList));
                executor.AddInputParam("@add_coach_count", SqlDbType.Int, addCoachList.Count);
                executor.AddInputParam("@add_coach_info", SqlDbType.VarChar, 8000, addCoachList.Count == 0 ? "" : JsonConvert.SerializeObject(addCoachList));
                executor.AddInputParam("@binder_info", SqlDbType.VarChar, 1024, binderInfo);
                executor.AddInputParam("@search_slot_idx", SqlDbType.TinyInt, searchSlotIdx);

                return executor.RunStoredProcedureWithResult("dbo.USP_GS_GM_SCOUT_SEARCH_END");

            });
        }
    }
}
