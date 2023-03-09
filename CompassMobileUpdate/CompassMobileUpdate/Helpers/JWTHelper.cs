using System;
using System.Json;
using System.Text;

namespace CompassMobileUpdate.Helpers
{
    public static class JWTHelper
    {
        public static string DecodedStringFromBase64URL(string encoded)
        {
            Byte[] bytes = Base64UrlDecode(encoded);
            string decoded = UTF8Encoding.UTF8.GetString(bytes);
            return decoded;
        }
        public static byte[] Base64UrlDecode(string encoded)
        {
            encoded = encoded.Replace('-', '+'); // 62nd char of encoding
            encoded = encoded.Replace('_', '/'); // 63rd char of encoding
            switch (encoded.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: encoded += "=="; break; // Two pad chars
                case 3: encoded += "="; break; // One pad char
                default:
                    throw new System.Exception(
                "Illegal base64url string!");
            }
            return Convert.FromBase64String(encoded); // Standard base64 decoder
        }
        public static DateTime GetUTCTimeSince1970(long secondsSince1970)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSince1970);
        }

        public static DateTime GetExpirationTimeFromJWT(string jwt)
        {
            //Claims in JWT is the second set of encoded data. 0 is Header, 1 is Payload (claims), 3 is Signature
            string decodedClaims = DecodedStringFromBase64URL(jwt.Split('.')[1]);
            JsonValue values = JsonValue.Parse(decodedClaims);
            long secondsSince1970 = (long)values["exp"];
            return GetUTCTimeSince1970(secondsSince1970);
        }
    }
}

