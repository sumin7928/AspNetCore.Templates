using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Core;
using ApiWebServer.Core.Controller;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Database.Utils;
using ApiWebServer.Logic;
using WebSharedLib.Contents;
using WebSharedLib.Contents.Api;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Entity;
using WebSharedLib.Error;
using ApiWebServer.Common.Define;

namespace ApiWebServer.Controllers.PostBoxControllers
{
    [Route("api/Postbox/[controller]")]
    [ApiController]
    public class PostboxListController : SessionContoller<ReqPostboxList, ResPostboxList>
    {
        public PostboxListController( 
            ILogger<PostboxListController> logger,
            IConfiguration config, 
            IWebService<ReqPostboxList, ResPostboxList> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings( GroupName = "client", IgnoreApi = false )]
        [SwaggerExtend( "우편함 리스트 조회", typeof( PostboxListPacket ) )]
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
            var postDB = _dbService.CreatePostDB( _webService.RequestNo, webSession.DBNo );

            int remainDays = Common.ServerUtils.GetConfigValue( _config.GetSection( "GlobalConfig" ), "PostBoxRemainDays", PostDefine.RemainDay );

            // 인덱스 범위 가져오기
            GetStartAndEndPostNo(webSession.DBNo, remainDays, out long startPostNo, out long endPostNo);

            DataSet postDataSet = postDB.USP_GS_PO_POSTBOX_R(webSession.TokenInfo.Pcid, webSession.DBNo, startPostNo, endPostNo);
            if (postDataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_PO_POSTBOX_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(postDataSet);
            List<PostboxData> postList = dataSetWrapper.GetObjectList<PostboxData>(0);

            foreach ( PostboxData post in postList)
            {
                post.exp_remain_seconds = (int)(post.exp_time - DateTime.Now).TotalSeconds;
            }

            resData.PostList = postList;

            return _webService.End();
        }

        private void GetStartAndEndPostNo(int dbNum, int daysAgo, out long startPostNo, out long endPostNo)
        {
            // 게임 서버에 시간에 따른 약간의 일간 오차가 있음
            DateTime today = DateTime.Today;
            DateTime startDay = DateTime.Today.AddDays( -daysAgo );

            string start = $"{dbNum}{startDay.Year - 2000}{startDay.Month.ToString("d2")}{startDay.Day.ToString("d2")}0000000000";
            string end = $"{dbNum}{today.Year - 2000}{today.Month.ToString("d2")}{today.Day.ToString("d2")}9999999999";

            startPostNo = long.Parse( start );
            endPostNo = long.Parse( end );
        }
    }
}
