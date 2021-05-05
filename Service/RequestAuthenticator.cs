using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace PaceMe.FunctionApp.Authentication
{
    public interface IRequestAuthenticator {
        Task<bool> AuthenticateRequest(Guid userId, HttpRequest request);
    }

    public class RequestAuthenticator : IRequestAuthenticator
    {
        private readonly MsalConfig _msalConfig;

        public RequestAuthenticator(MsalConfig msalConfig) {
            _msalConfig = msalConfig;
        }

        public async Task<bool> AuthenticateRequest(Guid userId, HttpRequest request)
        {
            StringValues jwtToken = default(StringValues);
            if (!request.Headers.TryGetValue("Authorization", out jwtToken)) {
                return false;
            }
            string rawTokenHeader = jwtToken.FirstOrDefault();
            if (rawTokenHeader == null || !rawTokenHeader.StartsWith("Bearer ")){
                return false;
            }
            string rawToken = rawTokenHeader.Remove(0, "Bearer ".Length);

            var claimsPrincipal = await ValidateAndDecode(rawToken);

            if(claimsPrincipal == null){
                //God I miss the result class from rust
                return false;
            }

            //Validate user id
            const string UserIdMsalClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            var identifierClaim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == UserIdMsalClaimType);
            Guid tokenUserId; 
            if(identifierClaim == null || !Guid.TryParse(identifierClaim.Value, out tokenUserId)){
                return false;
            }
            return tokenUserId == userId;
        }

        private async Task<ClaimsPrincipal> ValidateAndDecode(string jwt)
        {

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                _msalConfig.OpenIdConfig,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

            var discoveryDocument = await configurationManager.GetConfigurationAsync();
            var signingKeys = discoveryDocument.SigningKeys;

            var validationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                
                IssuerSigningKeys = signingKeys,
                RequireSignedTokens = true,
                
                RequireExpirationTime = true,
                ValidateLifetime = true,
                
                ValidateAudience = true,
                ValidAudience = _msalConfig.ValidAudience,

                ValidateIssuer = true,
                ValidIssuer = _msalConfig.ValidIssuer
            };

            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

                return claimsPrincipal;
            }
            catch (SecurityTokenValidationException)
            {
                // The token failed validation!
                return null;
            }
            catch (ArgumentException)
            {
                // The token was not well-formed or was invalid for some other reason.
                return null;
            }
        }
    }
} 