using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CompassMobileUpdate.Models
{
	public class AuthLoginResponse
	{
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("upn")]
        public string Upn { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("empId")]
        public string EmpID { get; set; }

        [JsonProperty("fname")]
        public string FirstName { get; set; }

        [JsonProperty("lname")]
        public string LastName { get; set; }

        [JsonProperty("exp")]
        public string Exp { get; set; }

        [JsonProperty("iat")]
        public string IAT { get; set; }

        [JsonProperty("oiat")]
        public string OIAT { get; set; }

        [JsonProperty("dateStamp")]
        public string DateStamp { get; set; }

        [JsonProperty("expDate")]
        public string ExpDate { get; set; }

        [JsonProperty("iatDate")]
        public string IATDate { get; set; }

        [JsonProperty("dateStampDate")]
        public string DateStampDate { get; set; }

        public List<AuthenticationMessage> Messages { get; set; }

        public string Message { get; set; }

        public string FormattedMessage { get; set; }

        public bool IsAuthenticated {
            get {
                if (this.Messages != null && this.Messages.Count > 0 && !string.IsNullOrWhiteSpace(this.Messages[0].Code))
                {
                    if (this.Messages[0].Code.Contains("0401"))
                    {
                        FormattedMessage = "Invalid Username and/or Password";
                        return false;
                    }
                    else
                    {
                        FormattedMessage = this.Messages[0].Message;
                        return false;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Token))
                    {
                        return true;
                    }
                    else
                    {
                        FormattedMessage = "Unknown Error please try again";
                        return false;
                    }
                }
            }
            set {

            }
        }

        
    }

    public class AuthenticationMessage
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

    }
}

