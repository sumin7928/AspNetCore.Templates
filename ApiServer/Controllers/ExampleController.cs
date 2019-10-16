using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiWebPacket.Apis;
using ApiWebServer.Core.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiWebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : ControllerBase
    {
        [HttpPost]
        [SwaggerExtend("TestController", typeof(ExamplePacket))]
        public ResExamplePacket Post(
            [FromHeader(Name ="sequence")] int sequence,
            [FromHeader(Name = "session-token")] string sessionToken,
            [FromBody] ReqExamplePacket body)
        {


            HttpContext.Response.Headers["error-code"] = "0";

            return new ResExamplePacket();
        }
    }
}
