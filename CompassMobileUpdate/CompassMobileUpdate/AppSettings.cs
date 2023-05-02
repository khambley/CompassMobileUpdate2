using System;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate
{
	public static class AppSettings
	{
		public static Uri _ApiGeeAPIAuthTokenUrl
		{
			get
			{
				if (AppVariables.AppEnvironment == AppEnvironment.Integration)
				{
					return new Uri("https://api-integration.exeloncorp.com/edgemicro-auth/token?");
				}
				else if (AppVariables.AppEnvironment == AppEnvironment.Stage)
				{
					return new Uri("https://api-stage.exeloncorp.com/edgemicro-auth/token?");
				}
				else if (AppVariables.AppEnvironment == AppEnvironment.Production)
				{
					//TODO: Implement change request to deploy Apigee Prod environment
					throw new NotImplementedException("Production Not Implemented Yet");
				}
				else
				{
                    throw new NotImplementedException("URI Not Found");
                }
			}
		}
		public static Uri _ApiGeeBaseUrl
		{
            get
            {
                if (AppVariables.AppEnvironment == AppEnvironment.Integration)
                {
                    return new Uri("https://apir-integration.exeloncorp.com/comed/compassmobile/");
                }
                else if (AppVariables.AppEnvironment == AppEnvironment.Stage)
                {
                    return new Uri("https://apir-stage.exeloncorp.com/comed/compassmobile/");
                }
                else if (AppVariables.AppEnvironment == AppEnvironment.Production)
                {
                    //TODO: Implement change request to deploy Apigee Prod environment
                    throw new NotImplementedException("Production Not Implemented Yet");
                }
                else
                {
                    throw new NotImplementedException("URI Not Found");
                }
            }
        }
		public static string _ApiGeeAPIClientCredentialsKey
		{
			get
			{
				if (AppVariables.AppEnvironment == AppEnvironment.Integration)
				{
					return "je8W7RIQVXwXK0HvNPxCrFuwTOFc2PUd";
				}
				else if (AppVariables.AppEnvironment == AppEnvironment.Stage)
				{
					return "HYSMLDNFuSBNuMO4tnTkuZbERGRyGqx7";
                }
                else if (AppVariables.AppEnvironment == AppEnvironment.Production)
                {
                    //TODO: Implement change request to deploy Apigee Prod environment
                    throw new NotImplementedException("Production Not Implemented Yet");
                }
                else
                {
                    throw new NotImplementedException("API Client Key Not Found");
                }
            }
		}

		public static string _ApiGeeAPIClientCredentialsSecret
		{
			get
			{
				if (AppVariables.AppEnvironment == AppEnvironment.Integration)
				{
					return "yFIhyYuS3nXgbjs0";
				}
				else if (AppVariables.AppEnvironment == AppEnvironment.Stage)
				{
					return "M1EyyjqlJzw1gUqx";
                }
                else if (AppVariables.AppEnvironment == AppEnvironment.Production)
                {
                    //TODO: Implement change request to deploy Apigee Prod environment
                    throw new NotImplementedException("Production Not Implemented Yet");
                }
                else
                {
                    throw new NotImplementedException("API Client Secret Not Found");
                }
            }
		}
	}
}

