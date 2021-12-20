using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TekLabServiceCore.Models
{
    public class Enquiry
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
