#region Licence
/****************************************************************
 *  Filename: HttpChallengeResponseMiddleware.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pod.LetsEncrypt.Services;

namespace Pod.LetsEncrypt.Middleware
{
    /// <summary>
    /// Middleware to handle Challenge Request that come over Http
    /// This middleware should be mapped to a route as <see cref="Config.LetsEncryptConst.ChallengePath"/>
    /// See also <see cref="Extensions.UseLetsEncrypt"/>
    /// </summary>
    public class HttpChallengeResponseMiddleware : IMiddleware
    {
        private readonly ILogger<HttpChallengeResponseMiddleware> _logger;
        private readonly IHttpChallengeResponseStore _httpChallengeResponseStore;
        private readonly CertificateBuilderService _certificateBuilderService;
        private readonly IEnumerable<ILetsEncryptHook> _hooks;

        public HttpChallengeResponseMiddleware(ILogger<HttpChallengeResponseMiddleware> logger,
            IHttpChallengeResponseStore httpChallengeResponseStore,
            CertificateBuilderService certificateBuilderService,
            IEnumerable<ILetsEncryptHook> hooks)
        {
            _logger = logger;
            _httpChallengeResponseStore = httpChallengeResponseStore;
            _certificateBuilderService = certificateBuilderService;
            _hooks = hooks ?? new List<ILetsEncryptHook>();
        }

        /// <summary>
        /// Called by the Framework to invoke the middleware
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="next">Next Middleware to call</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var eventArgs = new LetsEncryptEventArgs()
                            {
                                    Stage = LetsEncryptStage.ChallengeArrived
            };
            try
            {
                //The Token is part of the Request URL
                var token = context.Request.Path.ToString();
                _logger.LogInformation($"Requested challenge request for {token}");
                
                // We try to get the token without prefix as we assumes that this middleware has been mapped
                // and only gets invoked if we really are on the challenge route
                if(token.StartsWith("/"))
                {
                    token = token.Substring(1);
                }

                //Try to get the token for this Challenge
                if(!_httpChallengeResponseStore.TryGetResponse(token, out var orderInfo))
                { 
                    //Invoke hooks as something must gone wrong if we were called back here and did not find the token
                    eventArgs.Exception = new Exception($"The token '{token}' could not be found in the token store!");
                    _ = Task.Run( async () => { await eventArgs.SendArgs(_logger, _hooks); });
                    //Forward to next middleware in the pipeline
                    await next(context);

                    return;
                }

                eventArgs.DomainName = orderInfo.DomainName;
                _logger.LogInformation($"Confirmed challenge request for {token}");
                
                //Set the length for the Key Authorization string
                context.Response.ContentLength = orderInfo.Challenge.KeyAuthz.Length;
                //Sets the Content Type for the Response;
                context.Response.ContentType = "application/octet-stream";
                await context.Response.WriteAsync(orderInfo.Challenge.KeyAuthz, context.RequestAborted);

                _ = Task.Run(
                        async () =>
                        {
                            // Wait  some time to Let´s Encrypt to process our response before we request to download it
                            await Task.Delay(30 * 1000);
                            await _certificateBuilderService.DownloadAndCreateCertificate(orderInfo.Order, orderInfo.DomainName);
                        });

                //This request is done here we don't call any other middleware
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Exception in HttpChallengeResponseMiddleware");
                eventArgs.Exception = e;
                await eventArgs.SendArgs(_logger, _hooks);
            }
        }
    }
}
