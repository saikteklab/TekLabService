using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TekLabServiceCore.Models;
using MongoClient;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TekLabServiceCore.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;

namespace TekLabServiceCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    [EnableCors("frontend")]
    public class VisitorController : Controller
    {
        private MongoDBWrapper mongoClient { get; set; }
        IConfiguration configuration { get; set; }

        private readonly ILogger<VisitorController> _logger;
        public VisitorController(IConfiguration _configuration, MongoDBWrapper mongoDBWrapper,
                                 ILogger<VisitorController> logger)
        {
            mongoClient = mongoDBWrapper;
            _logger = logger;
            configuration = _configuration;
        }
        [HttpPost]
        public async Task<IActionResult> SaveVistorInformation(PersonalInfo personalInfo)
        {
            try
            {
               var documentId = await mongoClient.InsertAsync(JsonConvert.SerializeObject(personalInfo),"visitors");
               return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.LogError("SaveVistorInformation", ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("sendemail")]
     
        public async Task<IActionResult> SendEmail([FromForm]Enquiry enquiry)
        {
            try
            {
                EmailSender emailsender = new EmailSender(configuration);
                emailsender.SendEnquiryEmail(enquiry);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.LogError("SendEmail", ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
    }
}
