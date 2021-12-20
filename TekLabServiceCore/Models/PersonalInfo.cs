using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TekLabServiceCore.Models
{
    public class PersonalInfo
    {
       [JsonProperty("firstName")]
       public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("educationalBackground")]
        public string EducationalBackground { get; set; }

        [JsonProperty("birthDay")]
        public string DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        public string ZipCode { get; set; }

        [JsonProperty("visitorSource")]
        public string VisitorSource { get; set; }

    }
}
