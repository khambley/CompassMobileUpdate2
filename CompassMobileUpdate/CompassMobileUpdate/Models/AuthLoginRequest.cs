using System;
using Newtonsoft.Json;

namespace CompassMobileUpdate.Models
{
	public class AuthLoginRequest
	{
        [JsonProperty("userId")]
        public string UserID { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }
    }
}

