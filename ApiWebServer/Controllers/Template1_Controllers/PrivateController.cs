using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiServer.Controllers.Template1_Controllers
{
    [Route("api/Template1/[controller]")]
    [ApiController]
    public class PrivateController : ControllerBase
    {
        [HttpPost]
        [SwaggerOperation(Summary = "temp2", Description = "test2 desc", Tags = new[] { "Template1" })]
        public void Postt([FromBody] string value)
        {
        }

        [HttpGet("{id}")]
        [ApiExplorerSettings(GroupName = "private")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        [ApiExplorerSettings(GroupName = "private")]
        public void Post([FromBody] string value)
        {
        }
    }
}
