using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class PlayerLineupChangeController : SessionContoller<ReqPlayerLineupChange, ResPlayerLineupChange>
    {
        public PlayerLineupChangeController(
            ILogger<PlayerLineupChangeController> logger,
            IConfiguration config, 
            IWebService<ReqPlayerLineupChange, ResPlayerLineupChange> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client" )]
        [SwaggerExtend( "선수 라인업 변경", typeof( PlayerLineupChangePacket ) )]
        public NPWebResponse Controller( [FromBody] NPWebRequest requestBody )
        {
            WrapWebService( requestBody );
            if ( _webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End( _webService.ErrorCode );
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);

            if ( reqData.SrcAccountPlayerIdx == reqData.DstAccountPlayerIdx )
            {
                return _webService.End( ErrorCode.ERROR_INVALID_PARAM );
            }

            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_LINEUP_CHANGE_R(webSession.TokenInfo.Pcid, reqData.ModeType, reqData.PlayerType, reqData.SrcAccountPlayerIdx, reqData.DstAccountPlayerIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_LINEUP_CHANGE_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            List<CareerModePlayer> listPlayer = dataSetWrapper.GetObjectList<CareerModePlayer>(0);
            CareerModePlayer mainPlayer = null;
            CareerModePlayer subPlayer = null;

            bool invenPlayerInclude = false;

            //선수 있는지 체크
            if (listPlayer.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_LINEUP_LIST);
            }

            for (int i = 0; i < listPlayer.Count; ++i)
            {
                if (listPlayer[i].account_player_idx == reqData.SrcAccountPlayerIdx || listPlayer[i].account_player_idx == reqData.DstAccountPlayerIdx)
                {
                    //보관함 선수면 무조건 subPlayer에 셋팅
                    if (listPlayer[i].is_starting == 0)
                    {
                        invenPlayerInclude = true;
                        subPlayer = listPlayer[i];
                    }
                    else
                    {
                        //라인업선수라면 main부터 넣기 main에 찼다면 sub에 넣기
                        if(mainPlayer == null)
                        {
                            mainPlayer = listPlayer[i];
                        }
                        else
                        {
                            subPlayer = listPlayer[i];
                        }
                    }
                }
            }

            //선수 있는지 체크
            if (mainPlayer == null || subPlayer == null)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_PARAM);
            }

            // db에 데이터가 이상하게 들어있는지 체크
            if (mainPlayer.order == subPlayer.order)
            {
                return _webService.End(ErrorCode.ERROR_INVALID_LINEUP_LIST);
            }

            PBPlayer mainPlayerPBInfo = CacheManager.PBTable.PlayerTable.GetPlayerData(mainPlayer.player_idx);
            PBPlayer subPlayerPBInfo = CacheManager.PBTable.PlayerTable.GetPlayerData(subPlayer.player_idx);

            if (mainPlayerPBInfo == null || subPlayerPBInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PB_PLAYER);
            }

            //인벤선수가 sub라면
            if (invenPlayerInclude == true)  //sub이 보관함 선수라면
            {
                //커리어모드이면서, 현재 치료소에 있는 선수라면 에러
                if(reqData.ModeType == (byte)GAME_MODETYPE.MODE_CAREERMODE && subPlayer.injury_cure_no != 0)
                {
                    return _webService.End(ErrorCode.ERROR_NOT_LINEUP_INJURY_CURE_PLAYER);
                }


                if(reqData.PlayerType == (byte)PLAYER_TYPE.TYPE_BATTER)
                {
                    if (listPlayer.Count != PlayerDefine.LineupBatterCount + 1)
                    {
                        return _webService.End(ErrorCode.ERROR_INVALID_LINEUP_LIST);
                    }

                    if (false == (mainPlayer.position == (byte)PLAYER_POSITION.CB
                                || mainPlayer.position == (byte)PLAYER_POSITION.DH
                                || mainPlayer.position == subPlayerPBInfo.position
                                || (mainPlayer.position == subPlayerPBInfo.second_position && subPlayer.sub_pos_open == 1)))
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION);
                    }
                }
                else
                {
                    if (listPlayer.Count != PlayerDefine.LineupPitcherCount + 1)
                    {
                        return _webService.End(ErrorCode.ERROR_INVALID_LINEUP_LIST);
                    }

                    //투수는 서브포지션이 없다
                    if (mainPlayer.position != subPlayerPBInfo.position)
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION);
                    }
                }

                //유니크인덱스 체크
                foreach (Player p in listPlayer)
                {
                    //해당선수라면 유니크인덱스 체크 무시( 없어질꺼니까)
                    if (p.account_player_idx == mainPlayer.account_player_idx || p.account_player_idx == subPlayer.account_player_idx)
                        continue;
                    else
                    {
                        PBPlayer tempPlayerPBInfo = CacheManager.PBTable.PlayerTable.GetPlayerData(p.player_idx);

                        if (tempPlayerPBInfo == null)
                        {
                            return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PB_PLAYER);
                        }

                        if (tempPlayerPBInfo.player_unique_idx == subPlayerPBInfo.player_unique_idx)
                        {
                            return _webService.End(ErrorCode.ERROR_OVERLAP_UNIQUEIDX);

                        }
                    }
                }

                subPlayer.is_starting = 1;
                subPlayer.position = mainPlayer.position;
                subPlayer.order = mainPlayer.order;

                mainPlayer.is_starting = 0;
                mainPlayer.position = (byte)PLAYER_POSITION.INVEN;
                mainPlayer.order = (reqData.PlayerType == (byte)PLAYER_TYPE.TYPE_BATTER) ? (byte)PLAYER_ORDER.INVEN_BATTER : (byte)PLAYER_ORDER.INVEN_PITCHER;

            }
            else  //라인업끼리의 교체라면
            {
                byte tempPosition;
                byte tempOrder;

                if (reqData.PlayerType == (byte)PLAYER_TYPE.TYPE_BATTER)
                {
                    if (listPlayer.Count != PlayerDefine.LineupBatterCount)
                    {
                        return _webService.End(ErrorCode.ERROR_INVALID_LINEUP_LIST);
                    }

                    if (mainPlayer.position != subPlayer.position)
                    {
                        //main이 후보선수라면
                        if (mainPlayer.position == (byte)PLAYER_POSITION.CB)
                        {
                            if (false == (subPlayer.position == (byte)PLAYER_POSITION.DH
                                        || subPlayer.position == mainPlayerPBInfo.position
                                        || (subPlayer.position == mainPlayerPBInfo.second_position && mainPlayer.sub_pos_open == 1)))
                            {
                                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION);
                            }

                            tempPosition = mainPlayer.position;
                            tempOrder = mainPlayer.order;

                            mainPlayer.position = subPlayer.position;
                            mainPlayer.order = subPlayer.order;

                            subPlayer.position = tempPosition;
                            subPlayer.order = tempOrder;
                        }
                        //sub가 후보선수라면
                        else if (subPlayer.position == (byte)PLAYER_POSITION.CB)
                        {
                            if (false == (mainPlayer.position == (byte)PLAYER_POSITION.DH
                                || mainPlayer.position == subPlayerPBInfo.position
                                || (mainPlayer.position == subPlayerPBInfo.second_position && subPlayer.sub_pos_open == 1)))
                            {
                                return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION);
                            }

                            tempPosition = mainPlayer.position;
                            tempOrder = mainPlayer.order;

                            mainPlayer.position = subPlayer.position;
                            mainPlayer.order = subPlayer.order;

                            subPlayer.position = tempPosition;
                            subPlayer.order = tempOrder;
                        }
                        else
                        {
                            //둘다 후보가 아닌 선발이라면 타순만 바꾼다.

                            tempOrder = mainPlayer.order;
                            mainPlayer.order = subPlayer.order;
                            subPlayer.order = tempOrder;
                        }
                    }
                    else
                    {
                        //타자에서 둘 포지션이 같은경우는 후보일때밖에 없다.
                        if (mainPlayer.position != (byte)PLAYER_POSITION.CB)
                        {
                            return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION);
                        }

                        //둘간의 후보 순서만 바꾸자
                        tempOrder = mainPlayer.order;
                        mainPlayer.order = subPlayer.order;
                        subPlayer.order = tempOrder;
                    }

                }
                else
                {
                    if (listPlayer.Count != PlayerDefine.LineupPitcherCount)
                    {
                        return _webService.End(ErrorCode.ERROR_INVALID_LINEUP_LIST);
                    }

                    //투수라면 포지션이 같아야교체가능
                    if (mainPlayer.position != subPlayer.position)
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION);
                    }

                    //투수에서 둘 포지션이 같은경우는 sp와 rp밖에 없다.
                    if (mainPlayer.position != (byte)PLAYER_POSITION.SP && mainPlayer.position != (byte)PLAYER_POSITION.RP)
                    {
                        return _webService.End(ErrorCode.ERROR_NOT_MATCHING_PLAYER_POSITION);
                    }

                    //둘간의 순서만 바꾸기
                    tempOrder = mainPlayer.order;
                    mainPlayer.order = subPlayer.order;
                    subPlayer.order = tempOrder;
                }
            }

            if (gameDB.USP_GS_GM_PLAYER_LINEUP_CHANGE(webSession.TokenInfo.Pcid, reqData.ModeType, mainPlayer.account_player_idx, mainPlayer.position, mainPlayer.order, mainPlayer.is_starting, 
                                                                                                    subPlayer.account_player_idx, subPlayer.position, subPlayer.order, subPlayer.is_starting) == false)
            {
                _logger.LogError("USP_GS_GM_PLAYER_LINEUP_CHANGE - Not exist dataSet. pcID:{0}", webSession.TokenInfo.Pcid);
                return _webService.End(ErrorCode.ERROR_DB);
            }

            if(reqData.SrcAccountPlayerIdx == mainPlayer.account_player_idx)
            {
                resData.SrcPosition     = mainPlayer.position;
                resData.SrcOrder        = mainPlayer.order;
                resData.SrcIsStarting   = mainPlayer.is_starting;

                resData.DstPosition     = subPlayer.position;
                resData.DstOrder        = subPlayer.order;
                resData.DstIsStarting   = subPlayer.is_starting;
            }
            else
            {
                resData.SrcPosition     = subPlayer.position;
                resData.SrcOrder        = subPlayer.order;
                resData.SrcIsStarting   = subPlayer.is_starting;

                resData.DstPosition     = mainPlayer.position;
                resData.DstOrder        = mainPlayer.order;
                resData.DstIsStarting   = mainPlayer.is_starting;
            }

            return _webService.End();
        }
    }
}
