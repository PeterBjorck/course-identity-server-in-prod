using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class SerilogHttpMessageHandler : DelegatingHandler
    {
        private Microsoft.Extensions.Logging.ILogger _logger;

        public SerilogHttpMessageHandler(ILogger<SerilogHttpMessageHandler> logger)
        {
            _logger = logger;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
                      
            var response = await base.SendAsync(request, cancellationToken);

            string status = ((int)response.StatusCode).ToString() + " " + response.StatusCode.ToString();
            string executiontime = stopwatch.ElapsedMilliseconds.ToString() + "ms";

            Log.Logger.ForContext("SourceContext", "SerilogHttpMessageHandler")
            .ForContext("ApiPath", request?.RequestUri?.ToString() ?? "Unknown")
            .ForContext("ApiExecutionTime", executiontime)
            .ForContext("ApiStatusCode", status)
            .Debug("Client API Request");

            //Debugger.Break();

            return response;
        }
    }

}
