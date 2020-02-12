using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace QueueService.ExampleMessageEndpoint.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageEndpointController : ControllerBase
    {

        private readonly ILogger<MessageEndpointController> logger;

        public MessageEndpointController(ILogger<MessageEndpointController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Authorize]
        public ActionResult Post(Testclass model)
        {
            if(model.param2 == "valuetwo")
            {
                Random rand = new Random();

                if (rand.Next(0, 2) != 0)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "completely random error i'm afraid" });
                }

                return Ok($"(randomsuccess) you sent in {model.param1} and {model.param2}");
            }


            if(model.param1 == "correct")
                return Ok($"you sent in {model.param1} and {model.param2}");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "incorrect param1 (it should be the word 'correct')" });
        }


        [HttpGet]
        public ActionResult Get()
        {
            return Ok("MessageEndpointController");
        }
    }

    public class Testclass
    {
        public string param1 { get; set; }
        public string param2 { get; set; }
    }
}
