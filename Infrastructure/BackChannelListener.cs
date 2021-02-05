using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    /// <summary>
    /// Backchannel listener, that will log requests made to our IdentityServer 
    /// </summary>
    public class BackChannelListener : DelegatingHandler
    {
        public BackChannelListener() : base(new HttpClientHandler())
        {
            Console.WriteLine();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("BackChannelListener");
            Console.WriteLine("@@@@ SendASync: " + request.RequestUri);

            var sw = new Stopwatch();
            sw.Start();

            var result = base.SendAsync(request, cancellationToken);

            result.ContinueWith(t =>
            {
                sw.Stop();

                Console.WriteLine("@@@@ success: " + result.IsFaulted);
                Console.WriteLine("@@@@ loadtime: " + sw.ElapsedMilliseconds.ToString());
                Console.WriteLine("@@@@ url: " + request.RequestUri);

                Serilog.Log.Logger.ForContext("SourceContext", "BackChannelListener")
                                  .ForContext("url", request.RequestUri)
                                  .ForContext("loadtime", sw.ElapsedMilliseconds.ToString() + " ms")
                                  .ForContext("success", result.IsCompletedSuccessfully)
                                  .Information("Loading IdentityServer configuration");
            });

            return result;
        }
    }

}
