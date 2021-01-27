using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure
{
    public static class SecurityExtensions
    {
        /// <summary>
        /// Tries to add the necessary security headers to every response
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
                headers.TryAdd("X-Content-Type-Options", "nosniff");

                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
                headers.TryAdd("X-Frame-Options", "SAMEORIGIN");

                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
                headers.TryAdd("Referrer-Policy", "no-referrer");

                //https://infosec.mozilla.org/guidelines/web_security#x-xss-protection
                headers.TryAdd("X-XSS-Protection", "1; mode=block");

                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
                //https://csp-evaluator.withgoogle.com
				//
                //We don't set the form-action CSP directive, due to various complications, see:
                //https://github.com/w3c/webappsec-csp/issues/8
                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy/form-action 
                var csp = "default-src 'none'; script-src 'self'; connect-src 'self'; img-src 'self'; style-src 'self'; base-uri 'self'; frame-ancestors 'none'";
                headers.TryAdd("Content-Security-Policy", csp);

                await next();
            });

            return app;
        }
    }
}
