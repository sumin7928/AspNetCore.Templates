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

namespace ApiServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        private readonly IDbService _dbService;

        public ExampleController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        [SwaggerDescription("TestController", "임시 테스트 컨트롤러", typeof(SampleAccount), null)]
        public ActionResult<string> Post([FromBody] SampleAccount account)
        {
            using (var conn = _dbService["Game"])
            {
                //var tt = conn.BeginTransaction();
                var ttt = conn.Query("SELECT * FROM [dbo].[GT_ACCOUNT]");
            }
            return "test";
        }
    }
}
