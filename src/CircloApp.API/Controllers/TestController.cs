using CircloApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CircloApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("email")]
        public async Task<IActionResult> TestEmail()
        {
            await _emailService.SendOtpAsync(
                "malshanhd11@gmail.com",
                "Malshan",
                "582491");

            return Ok("Email Sent");
        }
    }
}
