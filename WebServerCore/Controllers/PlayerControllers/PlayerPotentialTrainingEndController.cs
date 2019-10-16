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

namespace ApiWebServer.Controllers.PlayerControllers
{
    [Route("api/Player/[controller]")]
    [ApiController]
    public class PlayerPotentialTrainingEndController : SessionContoller<ReqPlayerPotentialTrainingEnd, ResPlayerPotentialTrainingEnd>
    {
        public PlayerPotentialTrainingEndController(
            ILogger<PlayerPotentialTrainingEndController> logger,
            IConfiguration config, 
            IWebService<ReqPlayerPotentialTrainingEnd, ResPlayerPotentialTrainingEnd> webService, 
            IDBService dbService )
            : base( logger, config, webService, dbService )
        {
        }

        [HttpPost]
        [ApiExplorerSettings(GroupName = "client")]
        [SwaggerExtend("선수 잠재력 훈련 선택", typeof(PlayerPotentialTrainingEndPacket))]
        public NPWebResponse Controller([FromBody] NPWebRequest requestBody)
        {
            WrapWebService(requestBody);
            if (_webService.ErrorCode != ErrorCode.SUCCESS)
            {
                return _webService.End(_webService.ErrorCode);
            }

            // Business
            var webSession = _webService.WebSession;
            var reqData = _webService.WebPacket.ReqData;
            var resData = _webService.WebPacket.ResData;
            var gameDB = _dbService.CreateGameDB(_webService.RequestNo, webSession.DBNo);


            // 포지션 변경하려는 슬롯이 열려 있는 슬롯인지, 껴져있는 선수가 있는지, 설정된 보직값과 비교하여 pb테이블 조건에 맞는지 체크.
            DataSet dataSet = gameDB.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END_R(webSession.TokenInfo.Pcid, reqData.AccountPlayerIdx);
            if (dataSet == null)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END_R");
            }

            DataSetWrapper dataSetWrapper = new DataSetWrapper(dataSet);
            int accountRowCount = dataSetWrapper.GetRowCount(0);
            Player targetPlayer = dataSetWrapper.GetObject<Player>(1);
            AccountTrainingResult trainingResult = dataSetWrapper.GetObject<AccountTrainingResult>(2);

            if (accountRowCount == 0)
            {
                return _webService.End(ErrorCode.ERROR_NO_ACCOUNT);
            }

            if (targetPlayer == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_PLAYER);
            }

            if (trainingResult == null)
            {
                return _webService.End(ErrorCode.ERROR_NOT_POTENTIAL_TRAINING_RESULT);
            }

            if (gameDB.USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END(webSession.TokenInfo.Pcid, reqData.IsChangeFlag, reqData.AccountPlayerIdx, trainingResult.select_idx1, trainingResult.select_idx2, trainingResult.select_idx3) == false)
            {
                return _webService.End(ErrorCode.ERROR_DB, "USP_GS_GM_PLAYER_POTENTIAL_TRAINING_END");
            }

            if(reqData.IsChangeFlag == true)
            {
                resData.PlayerPotentialIdx1 = trainingResult.select_idx1;
                resData.PlayerPotentialIdx2 = trainingResult.select_idx2;
                resData.PlayerPotentialIdx3 = trainingResult.select_idx3;
            }
            else
            {
                resData.PlayerPotentialIdx1 = targetPlayer.potential_idx1;
                resData.PlayerPotentialIdx2 = targetPlayer.potential_idx2;
                resData.PlayerPotentialIdx3 = targetPlayer.potential_idx3;
            }

            
            return _webService.End();
        }
    }
}
