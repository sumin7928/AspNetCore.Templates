using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using ApiWebServer.Cache;
using ApiWebServer.Core;
using ApiWebServer.PBTables;

namespace ApiWebServer.Controllers.AdminControllers
{
    [Route("api/Admin/[controller]")]
    public class SystemController : Controller
    {
        private readonly ILogger<SystemController> _logger;
        private readonly IConfiguration _config;
        private readonly IDBService _dbService;
        private readonly MaguPBTableContext _pbTableContext;

        public SystemController(
            ILogger<SystemController> logger,
            IConfiguration config,
            IDBService dbService,
            MaguPBTableContext pBTableContext)
        {
            _logger = logger;
            _config = config;
            _dbService = dbService;
            _pbTableContext = pBTableContext;
        }

        [HttpPost("ReloadContext")]
        [ApiExplorerSettings(GroupName = "admin")]
        [SwaggerOperation(Summary = "PB Table 리로딩", Description = "Cache 리로딩을 통해 변경된 데이터를 서버 재시작 없이 바로 적용")]
        public ActionResult ReloadContext()
        {
            // 캐시 데이터 초기화
            if (CacheManager.LoadPBTable(_pbTableContext) == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
