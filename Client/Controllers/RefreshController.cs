using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Controllers
{
    /// <summary>
    /// Refreshtoken demo controller. 
    /// 
    /// Written by Tore Nestenius to be used in the IdentityServer in production training class.
    /// https://www.edument.se/en/product/identityserver-in-production
    /// </summary>
    public class RefreshController : Controller
    {
        private readonly ILogger<RefreshController> _logger;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private static DiscoveryDocumentResponse disco;

        public RefreshController(ILogger<RefreshController> logger, 
                                 IConfiguration configuration,
                                 IHttpClientFactory clientFactory)
        {
            _logger = logger;
            this.configuration = configuration;
            _httpClientFactory = clientFactory;

            if (disco == null)
            {
                var client = new HttpClient();
                disco = client.GetDiscoveryDocumentAsync(configuration["openid:authority"]).Result;
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        /*
        //With refresh token
        public async Task<IActionResult> GetData()
        {
            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Content("{ \"name\":" + "\"Refresh token missing, please login again\" }");
                }


                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(accessToken);
                var ValidFrom = token.ValidFrom;
                var ValidTo = token.ValidTo;

                //var expiresAt = DateTimeOffset.Parse(await HttpContext.GetTokenAsync("expires_at"));

                int tokenExpiresInSec = CalculateWhenTokenExpires(accessToken ?? "");

                if (tokenExpiresInSec < 5)
                {
                    accessToken = GetNewAccessToken(refreshToken).Result;
                }

                HttpClient? client = _httpClientFactory.CreateClient("paymentapi");
                client.SetBearerToken(accessToken);

                var content = await client.GetStringAsync("/payments/get");

                var json = JObject.Parse(content);
                json.TryGetValue("name", out var value);
                string name = value.ToString();

                string str = "Got Data " + name + " (Access token exires in " + ((int)tokenExpiresInSec).ToString() + " sec)";

                return Content("{ \"name\":" + "\"" + str + "\" }");

            }
            catch (Exception exc)
            {
                return Content("{ \"name\":" + "\"" + exc.Message + "\" }");
            }
        }
        */
        public async Task<IActionResult> GetData()
        {
            try
            {
                int tokenExpiresInSec = CalculateWhenAccessTokenExpires();

                HttpClient? client = _httpClientFactory.CreateClient("paymentapi");

                var content = await client.GetStringAsync("/payments/get");

                var json = JObject.Parse(content);
                json.TryGetValue("name", out var value);
                string name = value.ToString();
                string str = "Got Data " + name + " (Access token exires in " + ((int)tokenExpiresInSec).ToString() + " sec)";

                return Content("{ \"name\":" + "\"" + str + "\" }");
            }
            catch (Exception exc)
            {
                return Content("{ \"name\":" + "\"" + exc.Message + "\" }");
            }
        }


        private int CalculateWhenTokenExpires(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(accessToken);
            //var ValidFrom = token.ValidFrom;
            var ValidTo = token.ValidTo;
            var tokenExpiresInSec = (int)ValidTo.Subtract(DateTime.UtcNow).TotalSeconds;

            return tokenExpiresInSec;
        }


        /// <summary>
        /// Get a new access token and refresh token from IdentitytServer and update it in the cookie	 
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private async Task<string> GetNewAccessToken(string? refreshToken)
        {
            var client = new HttpClient();
            TokenResponse? response = client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = configuration["openid:clientid"],
                ClientSecret = "mysecret",

                RefreshToken = refreshToken
            }).Result;

            //Store new tokens

            var expires = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

            var authService = HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();

            var authenticationInfo = await HttpContext.AuthenticateAsync();
            _ = authenticationInfo.Properties!.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, response.AccessToken);
            _ = authenticationInfo.Properties!.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, response.RefreshToken);
            _ = authenticationInfo.Properties!.UpdateTokenValue(OpenIdConnectParameterNames.ExpiresIn, expires.ToString("O"));

            await authService.SignInAsync(HttpContext, null, authenticationInfo.Principal!, authenticationInfo?.Properties);


            return response.AccessToken;
        }

        

        private int CalculateWhenAccessTokenExpires()
        {
            //We get the access token just to be able to calculate when it expires
            var accessToken = HttpContext.GetUserAccessTokenAsync().Result;

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(accessToken);
            var ValidFrom = token.ValidFrom;
            var ValidTo = token.ValidTo;

            var tokenExpiresInSec = (int)ValidTo.Subtract(DateTime.UtcNow).TotalSeconds;

            return tokenExpiresInSec;
        }

    }
}
