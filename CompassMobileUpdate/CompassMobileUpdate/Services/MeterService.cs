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
using Xamarin.Forms;

namespace CompassMobileUpdate.Services
{
    using VoltageRule = LocalVoltageRule;

    public class MeterService : BaseHttpService, IMeterService
    {
        public string ServiceEnvironment => "Internal";

        readonly Uri _baseUri;

        readonly IDictionary<string, string> _headers;

        private AuthService _authService;

        public MeterService(AuthService authService)
        {
            _authService = authService;
            _headers = new Dictionary<string, string>();

            // TODO: Add header with auth-based token

            if(ServiceEnvironment == "Apigee")
            {
                _baseUri = new Uri("https://apir-integration.exeloncorp.com/comed/compassmobile/");
            }
            else
            {
                // have to be connected to the VPN in order for this to make successful calls - for testing only KLH
                _baseUri = new Uri("https://compassmobiletest.exeloncorp.com/api/");
            }
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
                await AddHeadersAsync();

                var url = new Uri(_baseUri, $"MeterAttributes/{meter.DeviceSSNID}");
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

        private async Task AddHeadersAsync()
        {
            if (ServiceEnvironment == "Apigee")
            {
                var authResponse = await _authService.GetAPIToken();
                _headers["Authorization"] = "Bearer " + authResponse.AccessToken;
            }
            else
            {
                _headers["X-API-Key"] = "A221F9A024E112AA5FC9A20F071E42A";
                _headers["Accept"] = "application/json, text/json, text/x-json, text/javascript, application/xml, text/xml";
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

            await AddHeadersAsync();

            var url = new Uri(_baseUri, $"MeterStatus?meterMACID={meter.MacID}&deviceSSNID={meter.DeviceSSNID}&correlationID={correlationID}&source={AppVariables.SourceForAMICalls}");

            response = await SendRequestAsync<MeterStatusResponse>(url, HttpMethod.Get, _headers);

            handler(response, e);
        }

        public async Task<ActivityMessage.ActivityResponse> PerformActivityRequest(ActivityMessage.ActivityRequest requestBody)
        {
            await AddHeadersAsync();

            var url = new Uri(_baseUri, $"Activity/{requestBody.ActionName}");

            var response = await SendRequestAsync<ActivityMessage.ActivityResponse>(url, HttpMethod.Post, _headers);

            return response;

        }

        public async Task<List<Meter>> GetMetersByCustomerName(string name, string firstName = null, string lastName = null)
        {
            //TODO: Add error handling, invalid or null auth token here.

            //await AddHeadersAsync();
            if (ServiceEnvironment == "Apigee")
            {
                var authResponse = await _authService.GetAPIToken();
                _headers["Authorization"] = "Bearer " + authResponse.AccessToken;
            }
            else
            {
                _headers["X-API-Key"] = "A221F9A024E112AA5FC9A20F071E42A";
                _headers["Accept"] = "application/json, text/json, text/x-json, text/javascript, application/xml, text/xml";
            }

            var url = new Uri(_baseUri, $"meter?name={name}&firstName={firstName}&lastName={lastName}");

            var response = await SendRequestAsync<List<Meter>>(url, HttpMethod.Get, _headers);
            
            if(response == null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Service Error", "Service is not available, please try again later", "OK");
                });
            }

            return response;

        }

        public async Task<List<Meter>> GetMetersWithinBoxBoundsAsync(BoundingCoords bc)
        {
            await AddHeadersAsync();

            var url = new Uri(_baseUri, $"meter?left={bc.Left}&top={bc.Top}&right={bc.Right}&bottom={bc.Bottom}");

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
            await AddHeadersAsync();

            var url = new Uri(_baseUri, $"meter?sourceLatitude={latitude}&sourceLongitude={longitude}&radiusInMiles={radiusInMiles}");

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

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://compassmobiletest.exeloncorp.com/api/Meter/{deviceUtilityID}");

                //_headers["X-API-Key"] = "A221F9A024E112AA5FC9A20F071E42A";
                //_headers["Accept"] = "application/json, text/json, text/x-json, text/javascript, application/xml, text/xml";
                //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiAccessToken);

                request.Headers.Add("X-API-Key", "A221F9A024E112AA5FC9A20F071E42A");
                request.Headers.Add("Accept", "application/json, text/json, text/x-json, text/javascript, application/xml, text/xml");

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
            await AddHeadersAsync();

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

