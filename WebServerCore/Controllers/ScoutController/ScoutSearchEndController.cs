using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;
using ApiWebServer.Models;
using ApiWebServer.Logic;
using ApiWebServer.Common.Define;
using Newtonsoft.Json;

namespace ApiWebServer.Controllers.ScoutControllers
{
    [Route("api/Scout/[controller]")]
    [ApiController]
    public class ScoutSearchEndController : SessionContoller<ReqScoutSearchEnd, ResScoutSearchEnd>
    {
        public ScoutSearchEndController(
            ILogger<ScoutSearchEndController> logger,
            IConfiguration config,
            IWebService<ReqScoutSearchEnd, ResScoutSearchEnd> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "스카우트 탐색 종료(즉시완료 포함)", typeof(ScoutSearchEndPacket) )]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS )
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            // 스카우트 정보 조회
            DataSet dataSet = gameDB.USP_GS_GM_SCOUT_SEARCH_END_R(webSession.TokenInfo.Pcid, reqData.SearchSlotIdx);
            if (dataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_SEARCH_END_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper( dataSet );

            int dateNo = dataSetWrapper.GetValue<int>(0, "no");
            int remainSec = dataSetWrapper.GetValue<int>(0, "remain_sec");

            if (remainSec < 0)
            {
                return _webService.End(ErrorCode.ERROR_DB_DATA, "remainSec error");
            }

            AccountScoutBinder scoutBinderInfo = dataSetWrapper.GetObject<AccountScoutBinder>(1);
            AccountScoutSlot scoutSlotInfo = dataSetWrapper.GetObject<AccountScoutSlot>(2);
            AccountGame accountGameInfo = dataSetWrapper.GetObject<AccountGame>(3);

            if (scoutBinderInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA, "not user binder row data");
            }
            
            if (scoutSlotInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA, "not user slot row data");
            }

            //이미 바인더 인덱스가 바뀌엇다면 다시 인포부터 타서 바인더 갱신하고 해야함
            if(scoutBinderInfo.date_no != dateNo)
            {
                return _webService.End(ErrorCode.ERROR_MATCHING_BINDER_INFO);
            }

            //진행중인 탐색이 없다면 에러
            if(scoutSlotInfo.character_type == (byte)SCOUT_USE_TYPE.NONE )
            {
                return _webService.End(ErrorCode.ERROR_REQUEST_DATA);
            }

            //일반탐색
            if (reqData.FinishType == (byte)SCOUT_SEARCH_FINISH_TYPE.NORMAL)
            {
                //아직 탐색 안끝났다면 에러
                if (scoutSlotInfo.remain_sec > 0)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_ENOUGH_SCOUT_SEARCH_TIME);
                }
            }
            //즉시탐색
            else
            {
                //이미 탐색 끝났다면 에러
                if (scoutSlotInfo.remain_sec <= 0)
                {
                    return _webService.End(ErrorCode.ERROR_FINISH_SCOUT_SEARCH);
                }

                if (reqData.FinishType == (byte)SCOUT_SEARCH_FINISH_TYPE.GOODS)
                {
                    //오차 +- 5초 허용
                    int costDirectMin = (scoutSlotInfo.remain_sec - 5) / Cache.CacheManager.PBTable.ConstantTable.Const.scout_finish_const;
                    int costDirectMax = (scoutSlotInfo.remain_sec + 5) / Cache.CacheManager.PBTable.ConstantTable.Const.scout_finish_const;

                    if (costDirectMin < 1)
                        costDirectMin = 1;

                    if (costDirectMax < 1)
                        costDirectMax = 1;

                    if (reqData.DirectCostValue < costDirectMin || reqData.DirectCostValue > costDirectMax)
                    {
                        return _webService.End(ErrorCode.ERROR_MATCHING_DIRECT_SEARCH_COST);
                    }
                }
            }

            //선수영입 프로세스
            ErrorCode errorCode = Cache.CacheManager.PBTable.ItemTable.SetScoutSearchEnd(out List<Player> obtainPlayer, out List<Coach> obtainCoach, webSession.NationType, scoutSlotInfo.character_type, accountGameInfo);
            if (errorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(errorCode);
            }

            bool isAllComplete = false;
            bool isBinderUpdate = false;
            //바인더 체크
            if (scoutSlotInfo.character_type == (byte)SCOUT_USE_TYPE.PLAYER)
            {
                if (obtainPlayer.Count == 0 || obtainCoach.Count != 0)
                {
                    return _webService.End(ErrorCode.ERROR_STATIC_DATA);
                }

                List<int> playerIdxs = new List<int>();
                for (int i = 0; i < obtainPlayer.Count; ++i)
                    playerIdxs.Add(obtainPlayer[i].player_idx);

                //바인더에 있는 미완료 선수가 나왔는지 체크
                isBinderUpdate = CheckBinderNewComplete(scoutBinderInfo, playerIdxs);

            }
            else
            {
                if (obtainCoach.Count == 0 || obtainPlayer.Count != 0)
                {
                    return _webService.End(ErrorCode.ERROR_STATIC_DATA);
                }

                List<int> coachIdxs = new List<int>();
                for (int i = 0; i < obtainCoach.Count; ++i)
                    coachIdxs.Add(obtainCoach[i].coach_idx);

                //바인더에 있는 미완료 선수가 나왔는지 체크
                isBinderUpdate = CheckBinderNewComplete(scoutBinderInfo, coachIdxs);
            }

            GameRewardInfo allCompleteReward = null;

            if (isBinderUpdate == true)
            {
                if (scoutBinderInfo.slot1_complete == 1 &&
                    scoutBinderInfo.slot2_complete == 1 &&
                    scoutBinderInfo.slot3_complete == 1 &&
                    scoutBinderInfo.slot4_complete == 1 &&
                    scoutBinderInfo.slot5_complete == 1)
                {
                    isAllComplete = true;
                    allCompleteReward = Cache.CacheManager.PBTable.ItemTable.GetBinderAllCompleteReward(scoutBinderInfo.binder_idx);
                    scoutBinderInfo.reward_flag = 1;
                }
            }
            ConsumeReward consumeRewardProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, CONSUME_REWARD_TYPE.CONSUMEREWARD, false);
            ErrorCode consumeRewardErrorcode;
            //즉시탐색(유료)
            if (reqData.FinishType != (byte)SCOUT_SEARCH_FINISH_TYPE.NORMAL)
            {


                if (isAllComplete == true)
                {
                    consumeRewardProcess.AddReward(allCompleteReward);
                }

                if (reqData.FinishType == (byte)SCOUT_SEARCH_FINISH_TYPE.GOODS)
                {
                    consumeRewardProcess.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.DIA, 0, reqData.DirectCostValue));         
                }
                else
                {
                    consumeRewardProcess.AddConsume(new GameRewardInfo((byte)REWARD_TYPE.NORMAL_ITEM, Cache.CacheManager.PBTable.ItemTable.itemIdxScoutSearchDirect, 1));
                }

                consumeRewardErrorcode = consumeRewardProcess.Run(ref accountGameInfo, false);
                if (consumeRewardErrorcode != ErrorCode.SUCCESS)
                {
                    return _webService.End(consumeRewardErrorcode);
                }
            }
            //일반탐색
            else
            {
                if (isAllComplete == true)
                {
                    consumeRewardProcess.AddReward(allCompleteReward);

                    consumeRewardErrorcode = consumeRewardProcess.Run(ref accountGameInfo, false);
                    if (consumeRewardErrorcode != ErrorCode.SUCCESS)
                    {
                        return _webService.End(consumeRewardErrorcode);
                    }
                }
            }


            DataSet characterDataSet = gameDB.USP_GS_GM_SCOUT_SEARCH_END(webSession.TokenInfo.Pcid, isBinderUpdate == true ? JsonConvert.SerializeObject(scoutBinderInfo) : "",
                    scoutSlotInfo.slot_idx, obtainPlayer, obtainCoach, accountGameInfo, consumeRewardProcess.GetUpdateItemList());

            if (characterDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_SCOUT_SEARCH_END");
            }

            //선수 / 코치 인덱스 처리
            DataSetWrapper dataSetWrapperCharacter = new DataSetWrapper(characterDataSet);

            if (scoutSlotInfo.character_type == (byte)SCOUT_USE_TYPE.PLAYER)
            {
                string playerAccountIdxs = dataSetWrapperCharacter.GetValue<string>(0, "account_player_idxs");

                string[] strIdxArr = playerAccountIdxs.Split(',');

                for (int i = 0; i < obtainPlayer.Count; ++i)
                {
                    obtainPlayer[i].account_player_idx = long.Parse(strIdxArr[i]);
                }
            }
            else
            {
                string coachAccountIdxs = dataSetWrapperCharacter.GetValue<string>(0, "account_coach_idxs");
                string[] strIdxArr = coachAccountIdxs.Split(',');

                for (int i = 0; i < obtainCoach.Count; ++i)
                {
                    obtainCoach[i].account_coach_idx = long.Parse(strIdxArr[i]);
                }
            }
            
            resData.BinderResetRemainSec = remainSec;
            resData.UserBinderInfo = scoutBinderInfo;
            resData.ObtainPlayers = obtainPlayer;
            resData.ObtainCoachs = obtainCoach;
            resData.NowHavePlayerCount = accountGameInfo.now_player;
            resData.NowHaveCoachCount = accountGameInfo.now_coach;
            resData.AllCompleteReward = allCompleteReward;
            resData.ResultAccountCurrency = accountGameInfo;
            resData.UpdateItemInfo = consumeRewardProcess.GetUpdateItemList();


            return _webService.End();
        }

        private bool CheckBinderNewComplete(AccountScoutBinder scoutBinderInfo, List<int> obtainCharacter)
        {
            bool isNewComplete = false;     //true라면 db에 바인더 정보 갱신

            if (scoutBinderInfo.reward_flag == 1)
                return isNewComplete;

            //아직 완료 못했으면서, 이번 획득 선수에 해당 선수가 포함되어있다면 컴플리트!
            if(scoutBinderInfo.slot1_complete == 0 && obtainCharacter.Contains(scoutBinderInfo.slot1_character_idx) == true)
            {
                scoutBinderInfo.slot1_complete = 1;
                isNewComplete = true;
            }

            if (scoutBinderInfo.slot2_complete == 0 && obtainCharacter.Contains(scoutBinderInfo.slot2_character_idx) == true)
            {
                scoutBinderInfo.slot2_complete = 1;
                isNewComplete = true;
            }

            if (scoutBinderInfo.slot3_complete == 0 && obtainCharacter.Contains(scoutBinderInfo.slot3_character_idx) == true)
            {
                scoutBinderInfo.slot3_complete = 1;
                isNewComplete = true;
            }

            if (scoutBinderInfo.slot4_complete == 0 && obtainCharacter.Contains(scoutBinderInfo.slot4_character_idx) == true)
            {
                scoutBinderInfo.slot4_complete = 1;
                isNewComplete = true;
            }

            if (scoutBinderInfo.slot5_complete == 0 && obtainCharacter.Contains(scoutBinderInfo.slot5_character_idx) == true)
            {
                scoutBinderInfo.slot5_complete = 1;
                isNewComplete = true;
            }

            return isNewComplete;
        }

    }
}
