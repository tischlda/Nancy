namespace Nancy.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nancy.Responses;

    /// <summary>
    /// Some simple helpers give some nice authentication syntax in the modules.
    /// </summary>
    public static class ModuleSecurity
    {
        /// <summary>
        /// This module requires authentication
        /// </summary>
        /// <param name="module">Module to enable</param>
        public static void RequiresAuthentication(this INancyModule module)
        {
            module.Before.AddItemToEndOfPipeline(SecurityHooks.RequiresAuthentication());
        }

        /// <summary>
        /// This module requires authentication and certain claims to be present.
        /// </summary>
        /// <param name="module">Module to enable</param>
        /// <param name="requiredClaims">Claim(s) required</param>
        public static void RequiresClaims(this INancyModule module, IEnumerable<string> requiredClaims)
        {
            module.Before.AddItemToEndOfPipeline(SecurityHooks.RequiresAuthentication());
            module.Before.AddItemToEndOfPipeline(SecurityHooks.RequiresClaims(requiredClaims));
        }

        /// <summary>
        /// This module requires authentication and any one of certain claims to be present.
        /// </summary>
        /// <param name="module">Module to enable</param>
        /// <param name="requiredClaims">Claim(s) required</param>
        public static void RequiresAnyClaim(this INancyModule module, IEnumerable<string> requiredClaims)
        {
            module.Before.AddItemToEndOfPipeline(SecurityHooks.RequiresAuthentication());
            module.Before.AddItemToEndOfPipeline(SecurityHooks.RequiresAnyClaim(requiredClaims));
        }

        /// <summary>
        /// This module requires claims to be validated
        /// </summary>
        /// <param name="module">Module to enable</param>
        /// <param name="isValid">Claims validator</param>
        public static void RequiresValidatedClaims(this INancyModule module, Func<IEnumerable<string>, bool> isValid)
        {
            module.Before.AddItemToStartOfPipeline(SecurityHooks.RequiresValidatedClaims(isValid));
            module.Before.AddItemToStartOfPipeline(SecurityHooks.RequiresAuthentication());
        }

        /// <summary>
        /// This module requires https.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/> that requires HTTPS.</param>
        public static void RequiresHttps(this INancyModule module)
        {
            module.RequiresHttps(true);
        }

        /// <summary>
        /// This module requires https.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/> that requires HTTPS.</param>
        /// <param name="redirect"><see langword="true"/> if the user should be redirected to HTTPS if the incoming request was made using HTTP, otherwise <see langword="false"/> if <see cref="HttpStatusCode.Forbidden"/> should be returned.</param>
        public static void RequiresHttps(this INancyModule module, bool redirect)
        {
            module.Before.AddItemToEndOfPipeline(RequiresHttps(redirect));
        }

        private static Func<NancyContext, Response> RequiresHttps(bool redirect)
        {
            return (ctx) =>
                   {
                       Response response = null;
                       var request = ctx.Request;
                       if (!request.Url.IsSecure)
                       {
                           if (redirect && request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                           {
                               var redirectUrl = request.Url.Clone();
                               redirectUrl.Scheme = "https";
                               response = new RedirectResponse(redirectUrl.ToString());
                           }
                           else
                           {
                               response = new Response { StatusCode = HttpStatusCode.Forbidden };                               
                           }
                       }

                       return response;
                   };
        }
    }
}