using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentAPI.Middleware
{
    /// <summary>
    /// Extension method to register the middleware 
    /// </summary>
    public static class WaitForIdentityServerMiddlewareExtensions
    {
        public static IApplicationBuilder UseWaitForIdentityServer(this IApplicationBuilder builder, WaitForIdentityServerOptions options)
        {
            return builder.UseMiddleware<WaitForIdentityServerMiddleware>(options);
        }
    }

    public class WaitForIdentityServerOptions
    {
        public string Authority { get; set; }
    }


    /// <summary>
    /// ASP.NET Core middleware that will wait for IdentityServer to respond
    /// 
    /// It will return a 503 SERVICE UNAVAILABLE if IdentityServer is not responding 
    /// 
    /// This middleware is only in use until the first successfull response from IdentityServer.
    /// After that this module will not do anything.
    /// 
    /// It will add the following response headers to the resonse when we return a 503 error:
    /// 
    /// - x-reason: Waiting for IdentityServer
    /// - Cache-Control: no-store,no-cache,max-age=0
    /// - Retry-After: 5
    /// 
    /// The authority URL will be taken from the 
    /// 
    /// Written by Tore Nestenius to be used in the IdentityServer in production training class.
    /// https://www.edument.se/en/product/identityserver-in-production
    /// 
    /// </summary>
    public class WaitForIdentityServerMiddleware
    {
        /// <summary>
        /// number of seconds between each attempt to contact IdentityServer
        /// </summary>
        private int secondsBetweenRetries = 2;

        /// <summary>
        /// How many seconds should we wait before we give up waiting?
        /// </summary>
        private int httpRequestTimeout = 3;

        /// <summary>
        /// True when we have been able to reach IdentityServer
        /// </summary>
        private bool _identityServerReady = false;

        private readonly RequestDelegate _next;
        private readonly string _discoveryUrl;
        private readonly SemaphoreSlim _refreshLock;
        private DateTimeOffset _syncAfter = DateTimeOffset.MinValue;
        private readonly DateTime _startTime;

        public WaitForIdentityServerMiddleware(RequestDelegate next, IConfiguration configuration, WaitForIdentityServerOptions options)
        {
            _next = next;
            _startTime = DateTime.UtcNow;
            _discoveryUrl = buildDiscoveryUrl(options.Authority);

            _refreshLock = new SemaphoreSlim(1);
        }


        public async Task InvokeAsync(HttpContext context)
        {
            //Has IdentityServer has succesfully responsed yet?
            if (_identityServerReady == false)
            {
                //Fail fast if we should wait a bit or if there is already a request is in progress
                if (_syncAfter > DateTimeOffset.UtcNow ||   
                    _refreshLock.CurrentCount == 0)
                {
                    //We are waiting to not overload IdentitytServer with to many requests
                    //Just terminate the request with a 503 Service Unavailable response
                    CreateServiceUnavailableResponse(context);
                    return;
                }

                //Make sure we only do one request at the time
                await _refreshLock.WaitAsync().ConfigureAwait(false);

                try
                {
                    //Still not answering?
                    if (_identityServerReady == false)
                    {
                        _identityServerReady = await TryToReachIdentityServer(context);
                    }
                }
                catch (Exception exc)
                {
                    Log.Logger.ForContext("SourceContext", "WaitForIdentityServerMiddleware")
                          .ForContext("DiscoveryUrl", _discoveryUrl)
                          .ForContext("Exception", exc.Message)
                          .ForContext("Path", context.Request.Path)
                          .Fatal("Exception while trying to reach IdentityServer");
                }
                finally
                {
                    _refreshLock.Release();
                    _syncAfter = DateTimeOffset.UtcNow.AddSeconds(secondsBetweenRetries);
                }
            }

            if (_identityServerReady)
            {
                // Call the next delegate/middleware in the pipeline
                await _next(context);
            }
            else
            {
                //As we did not succeeed, let's terminate return a 503 SERVICE UNAVAILABLE error back to the client
                CreateServiceUnavailableResponse(context);
                return;
            }
        }


        /// <summary>
        /// Create a service unavailable 503 error response 
        /// </summary>
        /// <param name="context"></param>
        private void CreateServiceUnavailableResponse(HttpContext context)
        {
            context.Response.Headers.Add("x-reason", "Waiting for IdentityServer");
            context.Response.Headers.Add("Retry-After", "5");                               //Add a retry again header, with 5 seconds
            context.Response.Headers.Add("Cache-Control", "no-store,no-cache,max-age=0");   //Don't cache this response
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;           //503 status code
        }


        /// <summary>
        /// Try to reach the IdentityServer discovery endpoint
        /// </summary>
        /// <returns>True if successfull</returns>
        private async Task<bool> TryToReachIdentityServer(HttpContext context)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(httpRequestTimeout);

            var response = await client.GetAsync(_discoveryUrl);

            //Should we log?
            if (response.IsSuccessStatusCode == false)
            {
                var secondsSinceStart = (int)DateTime.UtcNow.Subtract(_startTime).TotalSeconds;

                Log.Logger.ForContext("SourceContext", "WaitForIdentityServerMiddleware")
                                  .ForContext("DiscoveryUrl", _discoveryUrl)
                                  .ForContext("Path", context.Request.Path)
                                  .ForContext("Tried for over", secondsSinceStart.ToString() + " seconds")
                                  .Information("Failed to reach IdentityServer at startup");
            }

            return response.IsSuccessStatusCode;
        }


        /// <summary>
        /// Construct the discovery endpoint URL
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        private string buildDiscoveryUrl(string authority)
        {
            string Url = authority;

            if (!Url.EndsWith("/", StringComparison.Ordinal))
            {
                Url = Url + "/";
            }

            Url = Url + ".well-known/openid-configuration";

            return Url;
        }
    }
}
