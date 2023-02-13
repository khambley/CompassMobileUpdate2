using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;
using Newtonsoft.Json;

namespace CompassMobileUpdate.Services
{
	public class MeterService : BaseHttpService, IMeterService
	{
        // Apigee 
        readonly Uri _baseUri = new Uri("https://apir-integration.exeloncorp.com/comed/compassmobile/");

        readonly IDictionary<string, string> _headers;

        private AuthService _authService;

		public MeterService(AuthService authService)
		{
            _authService = authService;
            _headers = new Dictionary<string, string>();

            // TODO: Add header with auth-based token
		}

        public async Task<IList<Meter>> GetMetersByCustomerName(string name, string firstName, string lastName)
        {
            var url = new Uri(_baseUri, "meter?name={name}&firstName={firstName}&lastName={lastName}");

            var response = await SendRequestAsync<Meter[]>(url, HttpMethod.Post, _headers);

            return response;
        }

        public async Task<Meter> GetMeterByDeviceUtilityIDAsync(string deviceUtilityID)
        {
            var apiAccessToken = _authService.GetAPIToken().Result.AccessToken;
            var result = new Meter();
            if (apiAccessToken != null)
            {
                
                var url = new Uri(_baseUri, $"Meter/{deviceUtilityID}");

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiAccessToken);

                using (var client = new HttpClient())
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {
                    var content = response.Content == null ? null : await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<Meter>(content);
                    }
                }
            }
            return result;
        }
    }
}

