using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiServer.Core.DB;
using ApiServer.Core.Swagger;
using ApiServer.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiServer.Controllers.Template_Controllers
{
    [Route("api/[controller]")]
    [SwaggerTag("This is api controller generated by ASP.NET Core 2.x - Template")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        private readonly ILogger<ExampleController> _logger;
        private readonly IConfiguration _config;
        private readonly IDbService _dbService;


        public ExampleController(ILogger<ExampleController> logger, IConfiguration config, IDbService dbService)
        {
            _logger = logger;
            _config = config;
            _dbService = dbService;
        }

        [HttpPost("Sample1")]
        [ApiExplorerSettings(GroupName = "v1", IgnoreApi = false)]
        [SwaggerDescription("Call sample api with body docs", typeof(SampleAccount), typeof(SampleResult))]
        public ActionResult<SampleResult> SampleApi([FromBody] SampleAccount account)
        {
            // api call unique sequence
            long requestNo = (long)HttpContext.Items["ResultNo"];
            _logger.LogInformation($"[{requestNo}] start api controller...");

            // db process
            //using (var conn = _dbService["DB"])
            //{
            //    var result = conn.Query("SELECT * FROM [dbo].[GT_ACCOUNT]");
            //}

            SampleResult result = new SampleResult();
            result.ResultCode = 0;
            result.ResultMessage = "Success api call";

            _logger.LogInformation($"[{requestNo}] end api controller");

            return result;
        }

        [HttpPost("Sample2")]
        [ApiExplorerSettings(GroupName = "v1", IgnoreApi = false)]
        [SwaggerDescription("Call sample api without body docs", null, typeof(SampleResult))]
        public ActionResult<SampleResult> Update([FromBody] SampleAccount account)
        {
            // api call unique sequence
            long requestNo = (long)HttpContext.Items["ResultNo"];
            _logger.LogInformation($"[{requestNo}] start api controller...");

            // db process
            //using (var conn = _dbService["DB"])
            //{
            //    var result = conn.Query("SELECT * FROM [dbo].[GT_ACCOUNT]");
            //}

            SampleResult result = new SampleResult();
            result.ResultCode = 0;
            result.ResultMessage = "Success api call";

            _logger.LogInformation($"[{requestNo}] end api controller");

            return result;
        }
    }
}
