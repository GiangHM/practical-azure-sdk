using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VictorNetCoreApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SecureController : ControllerBase
    {
        [HttpGet("GetString")]
        [Authorize(Roles = "admin")]
        public string GetString()
        {
            return "Demo security";
        }
    }
}
