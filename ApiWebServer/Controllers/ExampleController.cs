using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiWebServer.Core.Swagger;
using ApiWebServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiWebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        [HttpPost]
        [SwaggerDescription("TestController", "임시 테스트 컨트롤러", typeof(TestAccount), null)]
        public ActionResult Post([FromBody] TestAccount account)
        {
            return null;
        }
    }
}
