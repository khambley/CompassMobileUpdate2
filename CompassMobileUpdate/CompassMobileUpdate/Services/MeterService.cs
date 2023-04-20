﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Models;
using Newtonsoft.Json;

namespace CompassMobileUpdate.Services
{
    using VoltageRule = LocalVoltageRule;

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
        public delegate void GetMeterAttributesCompletedHandler(MeterAttributesResponse meter, Exception ex);

        public async Task GetMeterAttributesAsync(Meter meter, GetMeterAttributesCompletedHandler handler, CancellationToken token)
        {
            //TODO: Add Application logging
            //AppLogger.Debug("  AppService.GetMeterAttributes: MethodStart");

            MeterAttributesResponse response = null;
            Exception e = null;

            try
            {
                var authResponse = await _authService.GetAPIToken();

                var url = new Uri(_baseUri, $"MeterAttributes/{meter.DeviceSSNID}");

                _headers["Authorization"] = "Bearer " + authResponse.AccessToken;

                response = await SendRequestAsync<MeterAttributesResponse>(url, HttpMethod.Get, _headers);
            }
            catch (Exception ex)
            {
                if (AppHelper.ContainsNullResponseException(ex))
                {
                    e = new MeterNotFoundException(AppVariables.MeterNotFound);
                }
                else
                {
                    e = ex;
                }
            }
            if (!token.IsCancellationRequested)
            {
                handler(response, e);
            }
        }

        public delegate void GetMeterStatusCompletedHandler(MeterStatusResponse response, Exception ex);

        public async void GetMeterStatusAsync(Meter meter, int? activityID, GetMeterStatusCompletedHandler handler, CancellationToken token)
        {
            //AppLogger.Debug("  AppService.GetMeterStatus: MethodStart");

            string correlationID = "unknown";
            if (activityID.HasValue)
            {
                correlationID = activityID.Value.ToString();
            }

            MeterStatusResponse response = null;
            Exception e = null;
            var authResponse = await _authService.GetAPIToken();

            var url = new Uri(_baseUri, $"MeterStatus?meterMACID={meter.MacID}&deviceSSNID={meter.DeviceSSNID}&correlationID={correlationID}&source={AppVariables.SourceForAMICalls}");

            _headers["Authorization"] = "Bearer " + authResponse.AccessToken;

            response = await SendRequestAsync<MeterStatusResponse>(url, HttpMethod.Get, _headers);

            handler(response, e);
        }

        public async Task<ActivityMessage.ActivityResponse> PerformActivityRequest(ActivityMessage.ActivityRequest requestBody)
        {
            var authResponse = await _authService.GetAPIToken();

            var url = new Uri(_baseUri, $"Activity/{requestBody.ActionName}");

            _headers["Authorization"] = "Bearer " + authResponse.AccessToken;

            var response = await SendRequestAsync<ActivityMessage.ActivityResponse>(url, HttpMethod.Post, _headers);

            return response;

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

        public async Task<List<Meter>> GetMetersWithinBoxBoundsAsync(BoundingCoords bc)
        {
            var authResponse = await _authService.GetAPIToken();

            var url = new Uri(_baseUri, $"meter?left={bc.Left}&top={bc.Top}&right={bc.Right}&bottom={bc.Bottom}");

            _headers["Authorization"] = "Bearer " + authResponse.AccessToken;

            var response = await SendRequestAsync<List<Meter>>(url, HttpMethod.Get, _headers);

            return response;
        }

        /// <summary>
        /// Gets a list of Meters within a specified radius (in miles)
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="radiusInMiles"></param>
        /// <returns>List of Meters</returns>
        public async Task<List<Meter>> GetMetersWithinXRadiusAsync(double latitude, double longitude, double radiusInMiles)
        {
            var authResponse = await _authService.GetAPIToken();

            var url = new Uri(_baseUri, $"meter?sourceLatitude={latitude}&sourceLongitude={longitude}&radiusInMiles={radiusInMiles}");

            _headers["Authorization"] = "Bearer " + authResponse.AccessToken;

            var response = await SendRequestAsync<List<Meter>>(url, HttpMethod.Get, _headers);

            return response;
        }
        public string GetCustomerNameAndDeviceUtilityID(Meter meter)
        {
            var returnString = meter.DeviceUtilityID;

            if (!string.IsNullOrWhiteSpace(meter.CustomerName))
            {
                returnString = meter.CustomerName + " - " + meter.DeviceUtilityID;
            }

            return returnString;
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

        public async Task<List<VoltageRule>> GetVoltageRulesAsync()
        {
            var authResponse = await _authService.GetAPIToken();

            _headers["Authorization"] = "Bearer " + authResponse.AccessToken;

            var url = new Uri(_baseUri, "VoltageRule");

            var response = await SendRequestAsync<List<VoltageRule>>(url, HttpMethod.Get, _headers);

            return response;
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

