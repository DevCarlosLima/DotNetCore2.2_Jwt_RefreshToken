using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly TokenService _service;

        public AuthController (TokenService service) {
            _service = service;
        }

        [HttpPost ("get-token")]
        public IActionResult GetToken (Credentials model) {
            if (!ModelState.IsValid) return BadRequest ();

            if (model.User != "user.test" && model.Password != "Test@123*") return BadRequest (new string[] { "Error: Invalid user or password." });

            var result = _service.New (
                "0ff18cb3-1486-4ad5-b94a-0e52a5634153" // User Id
            );

            return Ok (result);
        }

        [HttpPost ("refresh-token")]
        public IActionResult RefreshToken (RefreshCredentials model) {
            if (!ModelState.IsValid) return BadRequest ();

            var result = _service.Refresh (model);

            if (result == null) return BadRequest (new string[] { "Error: Invalid token." });

            return Ok (result);
        }

        [Authorize]
        [HttpGet ("test-auth")]
        public IActionResult TestAuth () {
            return Ok ();
        }
    }
}