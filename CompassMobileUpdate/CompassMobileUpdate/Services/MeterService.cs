using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
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

        public async Task<List<Meter>> GetMetersByCustomerName(string name, string firstName = null, string lastName = null)
        {
                //TODO: Add error handling, invalid or null auth token here.

                var authResponse = await _authService.GetAPIToken();
            
                var url = new Uri(_baseUri, $"meter?name={name}&firstName={firstName}&lastName={lastName}");

                _headers["Authorization"] = "Bearer " + authResponse.AccessToken;

                var response = await SendRequestAsync<List<Meter>>(url, HttpMethod.Get, _headers);

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

        public bool IsValidSerialNumber(string serialNumber, out SerialNumberFormatException ex)
        {
            int temp;
            ex = null;

            if (serialNumber.Length != 10)
            {
                ex = new SerialNumberFormatException("Meter ID should have the Manufacturer Letter followed by 9 digits");
                return false;
            }

            if (int.TryParse(serialNumber, out temp))
            {
                ex = new SerialNumberFormatException("Meter number needs to be preceded with the Manufacturer letter");
            }
            else
            {
                Regex regEx = new Regex("[a-zA-Z]"); //any character a to z or A to Z
                if (regEx.IsMatch(serialNumber.Substring(0, 1)))
                {
                    string manufacturerLetter = serialNumber[0].ToString();
                    string tempMeterNumber = serialNumber.Substring(1, serialNumber.Length - 1);
                    if (!int.TryParse(tempMeterNumber, out temp))
                    {
                        ex = new SerialNumberFormatException("Meter ID should have the Manufacturer Letter followed by 9 digits");
                    }
                    else
                    {
                        ex = null;
                    }
                }
                else
                {
                    ex = new SerialNumberFormatException("Meter number needs to be preceded with the Manufacturer letter");
                }
            }
            if (ex == null)
                return true;
            else
                return false;

        }
    }
}

