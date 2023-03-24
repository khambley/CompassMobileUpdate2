using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CompassMobileUpdate.Models
{
    public static class JWTAuth
    {
        public static string GetNewJWTToken(string userID)
        {
            return GetJWTToken(userID, DateTime.UtcNow);
        }

        private static string GetJWTToken(string userID, DateTime tokenValidFrom)
        {
            //Set the Start
            DateTime start = tokenValidFrom;
            DateTime expire = DateTime.UtcNow.AddMinutes(AppVariables.TokenLifeInMinutes);
            if (expire > start.AddMinutes(AppVariables.TokenMaxMinutesSinceCreation))
            {
                expire = start.AddMinutes(AppVariables.TokenMaxMinutesSinceCreation);
            }
            //Create Claims
            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userID));

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor();
            tokenDescriptor.Subject = identity;
            tokenDescriptor.Issuer = AppVariables.JWTIssuer;
            tokenDescriptor.Audience = AppVariables.JWTAudience;
            tokenDescriptor.SigningCredentials = new SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(AppVariables.JWTSecretKey), SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);

            //Create Token
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            Microsoft.IdentityModel.Tokens.SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
        private static string GetExtendedJWTToken(Microsoft.IdentityModel.Tokens.SecurityToken token, ClaimsPrincipal principal)
        {
            string userID = principal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            return GetJWTToken(userID, token.ValidFrom);
        }


        /// <summary>
        /// Validates the provided token string. If it has determined that you should recieve a new token with an extended liftime
        /// It will provide it to you through the out parameter
        /// </summary>
        /// <param name="tokenString"></param>
        /// <param name="extendedTokenString">Will provide a value if the token's lifetime has been extended</param>
        /// <returns></returns>
        public static bool ValidateJWTToken(string tokenString, out string extendedTokenString, out IEnumerable<Claim> claims)
        {
            //Default extendedTokenString to null
            extendedTokenString = null;

            TokenValidationParameters validationParameters = new TokenValidationParameters();
            validationParameters.ValidIssuer = AppVariables.JWTIssuer;
            validationParameters.ValidAudience = AppVariables.JWTAudience;
            validationParameters.ValidateLifetime = true;
            validationParameters.ValidateIssuerSigningKey = true;

            ClaimsPrincipal principal = null;
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                DateTime now = DateTime.UtcNow;
                Microsoft.IdentityModel.Tokens.SecurityToken validatedToken;
                principal = tokenHandler.ValidateToken(tokenString, validationParameters, out validatedToken);
                claims = principal.Claims;
                DateTime from = validatedToken.ValidFrom;
                DateTime to = validatedToken.ValidTo;
                //If the token is going to expire in the next threshold minutes AND extending it by the tokenLifeInMinutes does not Exceed the max lifetime - then refresh the token
                if ((to < now.AddMinutes(AppVariables.TokenRefreshThresholdInMinutes)) && (from.AddMinutes(AppVariables.TokenMaxMinutesSinceCreation) > now.AddMinutes(AppVariables.TokenLifeInMinutes)))
                {
                    extendedTokenString = GetExtendedJWTToken(validatedToken, principal);
                }
                return true;
            }
            catch (Exception)
            {
                claims = null;
                return false;
            }

        }
    }
}

