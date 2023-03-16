using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;
using Newtonsoft.Json;

namespace CompassMobileUpdate.Services
{
	public class AuthService : BaseHttpService, IAuthService
	{
        Uri _baseUrl = new Uri("https://api-integration.exeloncorp.com/edgemicro-auth/token?");

        public async Task<AuthResponse> GetAPIToken()
        {
            var grantType = "grant_type=client_credentials";
            var clientId = "client_id=hMYFMivXirIwAZ3Kw1RBxxqSW0MWOHrX";
            var separator = "&";
            var clientSecret = "client_secret=kfpMonMoBMTogFKg";
            var myParameters = grantType + separator + clientId + separator + clientSecret;

            var webclient = new WebClient();
            webclient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            //returns token, accessToken, and ExpiresAt
            var token = webclient.UploadString(_baseUrl, myParameters);

            var response = JsonConvert.DeserializeObject<AuthResponse>(token);

            return response;
        }


    }
}

