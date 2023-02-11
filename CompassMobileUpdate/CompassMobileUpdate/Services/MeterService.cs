using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;

namespace CompassMobileUpdate.Services
{
	public class MeterService : BaseHttpService, IMeterService
	{
        // Apigee 
        readonly Uri _baseUri = new Uri("https://apir-integration.exeloncorp.com/comed/compassmobile");

        readonly IDictionary<string, string> _headers;

		public MeterService(Uri baseUri)
		{
            _baseUri = baseUri;
            _headers = new Dictionary<string, string>();

            // TODO: Add header with auth-based token
		}

        public async Task<IList<Meter>> GetMetersByCustomerName(string name, string firstName, string lastName)
        {
            var url = new Uri(_baseUri, "meter?name={name}&firstName={firstName}&lastName={lastName}");

            var response = await SendRequestAsync<Meter[]>(url, HttpMethod.Post, _headers);

            return response;
        }
    }
}

