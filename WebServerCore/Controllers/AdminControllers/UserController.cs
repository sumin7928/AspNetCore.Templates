using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using ApiWebServer.Cache;
using ApiWebServer.Common.Define;
using ApiWebServer.Core;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Entity;

namespace ApiWebServer.Controllers.AdminControllers
{
    [Route("api/Admin/[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _config;
        private readonly IDBService _dbService;

        public UserController(
            ILogger<UserController> logger,
            IConfiguration config,
            IDBService dbService)
        {
            _logger = logger;
            _config = config;
            _dbService = dbService;
        }

        [HttpPost("InsertPost")]
        [ApiExplorerSettings(GroupName = "admin")]
        [SwaggerOperation(Summary = "우편함 지급", Description = "해당 유저에 우편함 바로 지급\n 보상타입 - 1:다이아, 2:골드, 3:훈련포인트, 4:마스터리포인트, 101:선수카드, 102:코치카드, 103:장비아이템, 104:일반아이템, 105:보너스다이아\n 선수 및 코치일 경우에는 카운트에 선수/코치인덱스를 넣어주세요.(인덱스는 강화레벨)")]
        public ActionResult InsertPost(byte pubType = 1, string pubId = "", byte rewardType = 2, int rewardIdx = 0, int rewardCount = 1000)
        {
            var accountDB = _dbService.CreateAccountDB(0);
            DataTable pcIdDataTable = accountDB.Select($"SELECT pc_id FROM AC_ACCOUNT_PUBLISHER WHERE pub_type = {pubType} AND pub_id = '{pubId}'");
            if (pcIdDataTable.Rows.Count != 1)
            {
                return Ok("DB 계정정보 조회 에러");
            }
            long pcId = (long)pcIdDataTable.Rows[0]["pc_id"];

            DataTable dbNumDataTable = accountDB.Select($"SELECT db_num FROM AC_ACCOUNT_INFO WHERE pc_id = {pcId}");
            if (dbNumDataTable.Rows.Count != 1)
            {
                return Ok("DB 계정정보 조회 에러");
            }
            byte dbNum = (byte)dbNumDataTable.Rows[0]["db_num"];

            if(rewardCount <= 0)
            {
                return Ok("요청 데이터 카운트(선수/코치일 경우 인덱스) 에러");
            }

            if(rewardType == (byte)REWARD_TYPE.EQUIP_ITEM)
            {
                //장비가 없음
                return Ok("장비 데이터는 현재 없음");
            }


            if (rewardType == (byte)REWARD_TYPE.NORMAL_ITEM)
            {
                if (rewardIdx <= 0)
                {
                    return Ok("아이템 인덱스 에러");
                }

                var item = CacheManager.PBTable.ItemTable.GetItemData(rewardIdx);
                if(item == null)
                {
                    return Ok("존재하지 않는 아이템 인덱스");
                }
            }
            else if (rewardType == (byte)REWARD_TYPE.COACH_CARD)
            {
                var coach = CacheManager.PBTable.PlayerTable.GetCoachData(rewardCount);
                if (coach == null)
                {
                    return Ok("존재하지 않는 코치 인덱스");
                }
            }
            else if (rewardType == (byte)REWARD_TYPE.PLAYER_CARD)
            {
                var player = CacheManager.PBTable.PlayerTable.GetPlayerData(rewardCount);
                if (player == null)
                {
                    return Ok("존재하지 않는 선수 인덱스");
                }
            }

            var postDB = _dbService.CreatePostDB(0, dbNum);

            List<GameRewardInfo> rewards = new List<GameRewardInfo>();
            rewards.Add(new GameRewardInfo()
            {
                reward_type = rewardType,
                reward_idx = rewardIdx,
                reward_cnt = rewardCount
            });

            PostInsert postInsert = new PostInsert(pubId, rewards);

            if (postDB.USP_GS_PO_POST_SEND(pcId, "inserter", -1, "admin swagger", postInsert, (byte)POST_ADD_TYPE.ONE_BY_ONE) == false)
            {
                return Ok("프로시져 에러(서버팀 문의)");
            }


            return Ok("성공");
        }

        [HttpPost("SelectAccountInfo")]
        [ApiExplorerSettings(GroupName = "admin")]
        [SwaggerOperation(Summary = "닉네임으로 계정정보 찾기", Description = "")]
        public ActionResult SelectAccountInfo(string nickName)
        {
            if(nickName == null || nickName.Trim().Length == 0)
            {
                return Ok("파라미터 에러");
            }

            nickName = nickName.Trim();

            var accountDB = _dbService.CreateAccountDB(0);
            DataTable accountDataTable = accountDB.Select($"SELECT pub_type, pub_id, pc_id FROM AC_ACCOUNT_PUBLISHER WHERE pc_id = (SELECT pc_id FROM AC_ACCOUNT_INFO where pc_name = '{nickName}')");
            if (accountDataTable.Rows.Count != 1)
            {
                return Ok("DB 계정정보 조회 에러 ACCOUNTDB");
            }

            byte pub_type = (byte)accountDataTable.Rows[0]["pub_type"];
            string pub_id = (string)accountDataTable.Rows[0]["pub_id"];
            long pc_id = (long)accountDataTable.Rows[0]["pc_id"];

            DataTable dbNumDataTable = accountDB.Select($"SELECT db_num FROM AC_ACCOUNT_INFO WHERE pc_id = {pc_id}");
            if (dbNumDataTable.Rows.Count != 1)
            {
                return Ok("DB 계정정보 조회 에러 GAMEDB");
            }

            return Ok("성공/"+ pub_type.ToString()+"/"+ pub_id + "/" + pc_id.ToString());
        }

        [HttpPost("CareermodeDataModify")]
        [ApiExplorerSettings(GroupName = "admin")]
        [SwaggerOperation(Summary = "커리어모드 게임차수 수정", Description = "seekType 0 : 정규시즌 중간, 1 : 정규시즌 모두완료 (시즌앤드보내기직전)\n\n※ 해당 기능사용시 1차 특별훈련 스탭은 스킵됩니다.\n※ 또한 정규시즌을 최소1경기이상 진행한 상태에서 사용하셔야 합니다(기록이슈때문)")]
        public ActionResult CareermodeDataModify(byte pubType = 1, string pubId = "", int seekType = 0, byte myTeamRank = 1)
        {
            const int midSeek = 0;
            const int endSeek = 1;

            const int gameCount_KBO = 144;
            const int gameCount_MLB = 162;
            const int gameCount_NPB = 144;
            const int gameCount_CPB = 120;

            if(seekType != midSeek && seekType != endSeek)
            {
                Ok("데이터범위에러(0:정규시즌중간, 1:정규시즌모두완료(시즌앤드보내기직전))");
            }

            var accountDB = _dbService.CreateAccountDB(0);
            DataTable pcIdDataTable = accountDB.Select($"SELECT pc_id FROM AC_ACCOUNT_PUBLISHER WHERE pub_type = {pubType} AND pub_id = '{pubId}'");
            if (pcIdDataTable.Rows.Count != 1)
            {
                return Ok("DB 계정정보 조회 에러");
            }
            long pcId = (long)pcIdDataTable.Rows[0]["pc_id"];

            DataTable dbNumDataTable = accountDB.Select($"SELECT db_num FROM AC_ACCOUNT_INFO WHERE pc_id = {pcId}");
            if (dbNumDataTable.Rows.Count != 1)
            {
                return Ok("DB 계정정보 조회 에러");
            }
            byte dbNum = (byte)dbNumDataTable.Rows[0]["db_num"];

            var gameDB = _dbService.CreateGameDB(0, dbNum);

            DataTable careerDataTable = gameDB.Select($"select * from GM_ACCOUNT_CAREERMODE_INFO where pc_id = {pcId} and team_idx != 0 ");
            if (careerDataTable.Rows.Count != 1)
            {
                return Ok("커리어모드 데이터가 없습니다");
            }

            int game_no = (int)careerDataTable.Rows[0]["game_no"];
            byte match_group = (byte)careerDataTable.Rows[0]["match_group"];
            byte finish_match_group = (byte)careerDataTable.Rows[0]["finish_match_group"];
            byte country_type = (byte)careerDataTable.Rows[0]["country_type"];

            if (finish_match_group != (byte)SEASON_MATCH_GROUP.NONE)
            {
                return Ok("이미 끝난 시즌입니다.");
            }

            if (match_group != (byte)SEASON_MATCH_GROUP.PENNANTRACE)
            {
                return Ok("정규시즌이 아닙니다.");
            }

            if(game_no <= 1)
            {
                return Ok("경기를 최소1경기 이상 진행후 사용가능합니다.");
            }
            int updateSeek = -1;

            if (country_type == (byte)NATION_LEAGUE_TYPE.KBO)
                updateSeek = gameCount_KBO;
            else if (country_type == (byte)NATION_LEAGUE_TYPE.MLB)
                updateSeek = gameCount_MLB;
            else if (country_type == (byte)NATION_LEAGUE_TYPE.NPB)
                updateSeek = gameCount_NPB;
            else if (country_type == (byte)NATION_LEAGUE_TYPE.CPB)
                updateSeek = gameCount_CPB;
            else
            {
                return Ok("DB country_type 에러.");
            }

            if ( seekType == midSeek)
            {
                updateSeek /= 2;
            }
            else
            {
                finish_match_group = (byte)SEASON_MATCH_GROUP.PENNANTRACE;
            }

            //다음번할차례를 미리 저장해놓기때문에 +1을 한다.
            ++updateSeek;

            int count = gameDB.Update($"UPDATE GM_ACCOUNT_CAREERMODE_INFO set game_no = {updateSeek}, degree_no = {updateSeek}, springcamp_step = {(byte)SPRING_CAMP_STEP.FINISH}, " +
                                    $"specialtraining_step = {(byte)SPECIAL_TRAINING_STEP.NULL}, now_rank = {myTeamRank}, finish_match_group = {finish_match_group} where  pc_id = {pcId}");
            if (count <= 0)
            {
                return Ok("db 업데이트 에러(서버팀 문의)");
            }

            return Ok("성공");
        }

        [HttpPost("UpdateRatingIdx")]
        [ApiExplorerSettings(GroupName = "admin")]
        [SwaggerOperation(Summary = "경쟁전 등급 지정", Description = "해당 유저의 등급을 강제로 셋팅 - 포인트 및 연승정보도 초기화\n 등급정보 - 1~5:비기너E~A, 101~105:아마추어E~A, 201~205:세미프로E~A, 301~305:프로2군E~A, 401~405:프로1군E~A, 501~505:올스타E~A, 601~605:월드클래스E~A, 701:레전드")]
        public ActionResult UpdateRatingIdx(byte pubType = 1, string pubId = "", int ratingIdx = 1)
        {
            var accountDB = _dbService.CreateAccountDB(0);
            DataTable pcIdDataTable = accountDB.Select($"SELECT pc_id FROM AC_ACCOUNT_PUBLISHER WHERE pub_type = {pubType} AND pub_id = '{pubId}'");
            if (pcIdDataTable.Rows.Count != 1)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            long pcId = (long)pcIdDataTable.Rows[0]["pc_id"];

            DataTable dbNumDataTable = accountDB.Select($"SELECT db_num FROM AC_ACCOUNT_INFO WHERE pc_id = {pcId}");
            if (dbNumDataTable.Rows.Count != 1)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            byte dbNum = (byte)dbNumDataTable.Rows[0]["db_num"];

            if (CacheManager.PBTable.LiveSeasonTable.IsContainRaingIdx(ratingIdx) == false)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var gameDB = _dbService.CreateGameDB(0, dbNum);
            int count = gameDB.Update($"UPDATE GM_ACCOUNT_LIVESEASON_COMPETITION_INFO SET rating_idx = {ratingIdx}, point = 0, winning_streak = 0 WHERE pc_id = {pcId}");
            if (count <= 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
