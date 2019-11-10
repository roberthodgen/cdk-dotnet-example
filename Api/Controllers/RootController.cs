using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet("")]
        public ActionResult<string> Index()
        {
            return "Hello, world.";
        }
    }
}
