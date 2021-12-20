using AuthService;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoClient;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace TekLabServiceCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public class AuthenticateRequest
        {
            [Required]
            public string IdToken { get; set; }
        }

        private readonly JwtGenerator _jwtGenerator;
        private MongoDBWrapper mongoClient { get; set; }
        public UserController(MongoDBWrapper mongoDBWrapper,IConfiguration configuration)
        {
            mongoClient = mongoDBWrapper;
            _jwtGenerator = new JwtGenerator(configuration.GetValue<string>("JwtPrivateSigningKey"));
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest data)
        {
            GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();

            // Change this to your google client ID
            settings.Audience = new List<string>() { "37196382195-4fnpftkark94p47km1k70qqlv7omg7ct.apps.googleusercontent.com" };

            GoogleJsonWebSignature.Payload payload = GoogleJsonWebSignature.ValidateAsync(data.IdToken, settings).Result;
            return Ok(new { AuthToken = _jwtGenerator.CreateUserAuthToken(payload.Email) });
        }

        [AllowAnonymous]
        [HttpGet]
        public  IActionResult ValidateEmail([FromQuery] string emailId)
        {
            bool isUserExists = mongoClient.IsDocumentExist("email", emailId, "users").GetAwaiter().GetResult();
            return Ok(isUserExists);
        }
    }
}
