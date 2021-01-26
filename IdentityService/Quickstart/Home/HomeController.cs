// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using Microsoft.Extensions.Configuration;
using Infrastructure;

namespace IdentityServerHost.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment, ILogger<HomeController> logger,
                              IConfiguration configuration)
        {
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            if (!_environment.IsProduction())
            {
                // only show in development
                return View();
            }
            else
            {
                // only show in production
                _logger.LogInformation("Homepage is disabled in production. Returning 404.");

                string build = "Debug build";
                if (Settings.IsReleaseBuild)
                {
                    build = "Release build";
                }

                //Print out the first 8 characters of the GitHub SHA when deploying to production
                //Should of course be a bit more hidden in real life, perhaps as a HTML comment?
                var gitHubSha = _configuration["GITHUB:SHA"] ?? "";
                if (gitHubSha.Length > 8)
                {
                    gitHubSha = " " + gitHubSha.Substring(0, 8);
                }

                string msg = $"Service started: {Settings.StartupTime} ({_environment.EnvironmentName}, {build}{gitHubSha})";

                return Ok(msg);
            }
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;

                if (!_environment.IsDevelopment())
                {
                    // only show in development
                    message.ErrorDescription = null;
                }
            }

            return View("Error", vm);
        }
    }
}