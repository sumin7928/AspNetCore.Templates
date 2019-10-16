using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using ApiWebServer.Models;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Controllers.PostBoxControllers
{
    [Route("api/Postbox/[controller]")]
    [ApiController]
    public class RecvPostController : SessionContoller<ReqPostboxRecvPost, ResPostboxRecvPost>
    {
        public RecvPostController(
            ILogger<RecvPostController> logger,
            IConfiguration config, 
            IWebService<ReqPostboxRecvPost, ResPostboxRecvPost> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client", IgnoreApi = false)]
        [SwaggerExtend( "우편 받기", typeof( PostboxRecvPostPacket ) )]
        public NPWebResponse Contoller([FromBody] NPWebRequest requestBody )
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
            var accountDB = _dbService.CreateAccountDB(_webService.RequestNo);
            var postDB = _dbService.CreatePostDB( _webService.RequestNo, webSession.DBNo );
            var gameDB = _dbService.CreateGameDB( _webService.RequestNo, webSession.DBNo );

            if( reqData.PostNoList == null || reqData.PostNoList.Count == 0 )
            {
                return _webService.End( ErrorCode.ERROR_POSTBOX_NOT_EXISTS_POST_NO );
            }

            int remainDays = Common.ServerUtils.GetConfigValue( _config.GetSection( "GlobalConfig" ), "PostBoxRemainDays", Common.Define.PostDefine.RemainDay );


            MakeStartEndIndex( reqData.PostNoList, webSession.DBNo, remainDays, out long startPostNo, out long endPostNo );

            // 요청된 선물 내용을 가져옴
            DataSet postDataSet = postDB.USP_GS_PO_POSTBOX_R(webSession.TokenInfo.Pcid, webSession.DBNo, startPostNo, endPostNo);
            if (postDataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_POSTBOX_NOT_EXISTS_PUBID );
            }

            DataSetWrapper postDataSetWrapper = new DataSetWrapper( postDataSet );
            List<PostboxData> recvTargetList = postDataSetWrapper.GetObjectList<PostboxData>( 0 );

            // 유저 정보 가져옴
            DataSet gameDataSet = gameDB.USP_GS_GM_ACCOUNT_GAME_ONLY_R(webSession.TokenInfo.Pcid);
            if (gameDataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_ACCOUNT_GAME_ONLY_R");
            }

            DataSetWrapper gameDataSetWrapper = new DataSetWrapper( gameDataSet );
            AccountGame accountGameInfo = gameDataSetWrapper.GetObject<AccountGame>( 0 );

            if (accountGameInfo == null)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            // 보상 셋팅
            List<GameRewardInfo> rewardInfo = new List<GameRewardInfo>();
            foreach ( PostboxData post in recvTargetList )
            {
                // 보상 정보 넣어줌
                if ( 0 != reqData.PostNoList.Find( x => x == post.post_no ) )
                {
                    rewardInfo.Add( new GameRewardInfo( post.post_no, ( byte )post.item_code, post.item_idx, post.item_cnt ) );
                }
            }

            if (rewardInfo.Count == 0)
            {
                return _webService.End(ErrorCode.ERROR_DB);
            }

            // 보상 리스트 세팅
            ConsumeReward rewardProcess = new ConsumeReward(webSession.TokenInfo.Pcid, gameDB, Common.Define.CONSUME_REWARD_TYPE.REWARD, true, true);
            rewardProcess.AddReward( rewardInfo );
            ErrorCode rewardResult = rewardProcess.Run(ref accountGameInfo, false);
            if (rewardResult != ErrorCode.SUCCESS)
            {
                return _webService.End(rewardResult);
            }

            List<PostDelInfo> delInfoList = new List<PostDelInfo>();

            // 보상 실패된 부분 제거
            List<GameRewardInfo> failedList = rewardProcess.GetFailedRewadList();
            if ( failedList.Count > 0)
            {
                rewardInfo = rewardInfo.Except(rewardProcess.GetFailedRewadList()).ToList();
            }
            for ( int i = 0; i < rewardInfo.Count; ++i )
            {
                delInfoList.Add( new PostDelInfo { idx = i + 1, post_no = rewardInfo[ i ].etc_info } );
            }
            if ( delInfoList.Count == 0 )
            {
                ErrorCode firstError = rewardProcess.GetRewardErrorFirst();

                if (firstError == ErrorCode.SUCCESS)
                    return _webService.End( ErrorCode.ERROR_NOT_FOUND_RECV_POSTS );
                else
                    return _webService.End(firstError);
            }

            // 보상 처리
            DataSet characterDataSet = gameDB.USP_GS_GM_RECV_POST(webSession.TokenInfo.Pcid, accountGameInfo, rewardProcess.GetUpdateItemList(),
                rewardProcess.GetAddPlayerList(), rewardProcess.GetAddCoachList());
            if (characterDataSet == null)
            {
                return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_REWARD_PROCESS" );
            }

            // 처리된 선물을 삭제
            if ( postDB.USP_GS_PO_DELETE_POST( webSession.TokenInfo.Pcid, JsonConvert.SerializeObject( delInfoList ) ) == false )
            {
                return _webService.End( ErrorCode.ERROR_POSTBOX_NOT_EXISTS_PUBID );
            }


            //선수 / 코치 인덱스 처리
            if (rewardProcess.GetAddPlayerList() != null || rewardProcess.GetAddCoachList() != null)
            {
                DataSetWrapper dataSetWrapperCharacter = new DataSetWrapper(characterDataSet);

                if (rewardProcess.GetAddPlayerList() != null)
                {
                    string playerAccountIdxs = dataSetWrapperCharacter.GetValue<string>(0, "account_player_idxs");

                    string[] strIdxArr = playerAccountIdxs.Split(',');

                    for (int i = 0; i < rewardProcess.GetAddPlayerList().Count; ++i)
                    {
                        rewardProcess.GetAddPlayerList()[i].account_player_idx = long.Parse(strIdxArr[i]);
                    }
                }

                if (rewardProcess.GetAddCoachList() != null)
                {
                    string coachAccountIdxs = dataSetWrapperCharacter.GetValue<string>(0, "account_coach_idxs");
                    string[] strIdxArr = coachAccountIdxs.Split(',');

                    for (int i = 0; i < rewardProcess.GetAddCoachList().Count; ++i)
                    {
                        rewardProcess.GetAddCoachList()[i].account_coach_idx = long.Parse(strIdxArr[i]);
                    }
                }
            }

            resData.RewardPlayers = rewardProcess.GetAddPlayerList();
            resData.RewardCoachs = rewardProcess.GetAddCoachList();


            //using ( gameDB.BeginTransaction() )
            //{
            //    // 보상 처리
            //    if ( gameDB.USP_GS_GM_RECV_POST( webSession.TokenInfo.Pcid, accountGameInfo, rewardProcess.GetUpdateItemList() ) == false )
            //    {
            //        return _webService.End( ErrorCode.ERROR_DB, "USP_GS_GM_REWARD_PROCESS" );
            //    }
            //    using( postDB.BeginTransaction() )
            //    {
            //        // 처리된 선물을 삭제
            //        if ( postDB.USP_GS_PO_DELETE_POST( webSession.TokenInfo.Pcid, JsonConvert.SerializeObject( delInfoList ) ) == false )
            //        {
            //            return _webService.End( ErrorCode.ERROR_POSTBOX_NOT_EXISTS_PUBID );
            //        }
            //        postDB.Commit();
            //    }
            //    gameDB.Commit();
            //}
            resData.RewardInfo = rewardInfo;

            return _webService.End();
        }


        private void MakeStartEndIndex( List<long> postList, byte dbNum, int daysAgo, out long startNo, out long endNo )
        {
            DateTime today = DateTime.Today;
            DateTime startDay = DateTime.Today.AddDays( -daysAgo );

            // 안전장치 시작 및 종료 인덱스
            string start = $"{dbNum}{startDay.Year - 2000}{startDay.Month.ToString( "d2" )}{startDay.Day.ToString( "d2" )}0000000000";
            string end = $"{dbNum}{today.Year - 2000}{today.Month.ToString( "d2" )}{today.Day.ToString( "d2" )}9999999999";

            startNo = long.Parse( start );
            endNo = long.Parse( end );

            if ( postList.Count > 1 )
            {
                // 오름차순 정렬
                postList.Sort();

                startNo = postList[ 0 ];
                endNo = postList[ postList.Count - 1 ];
            }
            else
            {
                startNo = endNo = postList[ 0 ];
            }
        }

        private List<long> BuildPacket(List<PostDelInfo> delInfoList)
        {
            List<long> list = new List<long>();

            foreach (PostDelInfo delInfo in delInfoList)
            {
                list.Add(delInfo.post_no);
            }

            return list;
        }
    }
}
