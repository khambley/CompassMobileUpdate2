using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;
using Newtonsoft.Json;

namespace CompassMobileUpdate.Services
{
	public class AuthService : BaseHttpService, IAuthService
	{
        Uri _baseUrl = AppSettings._ApiGeeAPIAuthTokenUrl;
        Uri _baseAuthUrl = new Uri("https://exccommonap301.azure-api.net/api/");
        string _baseAuthRoute = "v2/auth?subscription-key=c5d7c537da7644dba62f38306a7d40b2";

        public async Task<AuthResponse> GetAPIToken()
        {
            var grantType = "grant_type=client_credentials";
            var clientId = $"client_id={AppSettings._ApiGeeAPIClientCredentialsKey}";
            var separator = "&";
            var clientSecret = $"client_secret={AppSettings._ApiGeeAPIClientCredentialsSecret}";
            var myParameters = grantType + separator + clientId + separator + clientSecret;

            var webclient = new WebClient();
            webclient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            //returns token, accessToken, and ExpiresAt
            var token = webclient.UploadString(_baseUrl, myParameters);

            var response = JsonConvert.DeserializeObject<AuthResponse>(token);

            return response;
        }

        public async Task<AuthLoginResponse> AuthenticateUser(string userName, string password, CancellationToken token)
        {
            var client = new WebClient();

            var url = new Uri(_baseAuthUrl + _baseAuthRoute);

            var debugValue = true;

            var requestData = new AuthLoginRequest { UserID = userName, Password = password, Debug = debugValue};

            var response = await SendRequestAsync<AuthLoginResponse>(url, HttpMethod.Post, null, requestData);

            if (response == null || (response.IsAuthenticated == false && response.Message == null))
            {
                var reason = "MCOE Authentication Response was null";

                if (response != null)
                {
                    reason = "MCOE Response was not authenticated and it didn't contain any messages";
                }

                //TODO: Add Logging
                //AppVariables.LogError(userName, new Exception(reason), principal);
            }

            if (response.IsAuthenticated)
            {
                return response;
            }
            else
            {
                response.IsAuthenticated = false;
                response.Message = response.FormattedMessage;
                return response;
            } 
        }
    }
}

