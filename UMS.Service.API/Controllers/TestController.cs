using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UMS.Service.API.Resources;

namespace UMS.Service.API.Controllers
{
  
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TestController(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            return Ok(new
            {
                message = _localizer["WelcomeMessage"]
            });
        }
    }

}
