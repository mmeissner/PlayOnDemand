#region Licence
/****************************************************************
 *  Filename: ApiKeySecretValidator.cs
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
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pod.Services.Authentication;
using Pod.Services.Station;
using Pod.Web.Authentication.ApiKeySecret;

namespace Pod.Web.Center.Authentication
{
    /// <summary>
    /// Validate Http Requests for Authentication with an API Key/Secret
    /// </summary>
    public class ApiKeySecretValidator : IApiKeySecretValidator
    {
        private readonly ILogger<ApiKeySecretValidator> _logger;
        private const long MaxClockSkewBetweenClientAndServerInMs = 10000;

        //MemoryCache should be a thread safe class, but its not really
        //https://github.com/aspnet/Caching/issues/359
        //https://github.com/aspnet/Extensions/issues/708
        //ToDo Fix if fixed by .Net Core 3.0 
        private readonly IMemoryCache _cache;
        private readonly StationApiKeyService _apiKeyService;

        public ApiKeySecretValidator(
                ILogger<ApiKeySecretValidator> logger,
                IMemoryCache memoryCache,
                StationApiKeyService apiKeyService)
        {
            _logger = logger;
            _cache = memoryCache;
            _apiKeyService = apiKeyService;
        }

        /// <summary>
        /// Validates an Authorization Request for amx Scheme.
        /// Validates Route, ApiKey, Hash/Data. Prevents Replay Attacks 
        /// </summary>
        /// <param name="request">The Request</param>
        public async Task<IApiKeySecretResponse> Validate(IApiKeySecretRequest request)
        {
            //Must be thread safe
            var publicKey = request.AuthorizationHeaderArray[0];
            var incomingBase64Signature = request.AuthorizationHeaderArray[1];
            var nonce = request.AuthorizationHeaderArray[2];
            var requestTimeStamp = request.AuthorizationHeaderArray[3];


            return await IsValidRequest(
                    request.HttpRequest,
                    publicKey,
                    incomingBase64Signature,
                    nonce,
                    requestTimeStamp);
        }

        /// <summary>
        /// Checks if the Authentication Request is valid
        /// </summary>
        /// <param name="request">The Http Request for authentication</param>
        /// <param name="publicKey">The Public Key from the Request</param>
        /// <param name="receivedSignatureAsBase64">The HMAC SHA 256 hash signature as Base64 string</param>
        /// <param name="nonce">The nonce used by the client that was used in creating the signature</param>
        /// <param name="requestTimeStamp">The timestamp used by the client that was used in creating the signature</param>
        /// <returns></returns>
        private async Task<IApiKeySecretResponse> IsValidRequest(
                HttpRequest request,
                string publicKey,
                string receivedSignatureAsBase64,
                string nonce,
                string requestTimeStamp)
        {
            var apiKeySecretResponse = new ApiKeySecretResponse();
            //Try to get info for this Public Key
            var apiKeyResult = await _apiKeyService.GetStationApiKey(publicKey);

            //Return a failed validation if we have an error
            if(apiKeyResult.HasError())
            {
                return apiKeySecretResponse;
            }

            //Check if this request was already received not long ago (Replay Attack)
            if(!IsTimestampValid(nonce, requestTimeStamp)) return apiKeySecretResponse;

            //Replicate the data signature for hashing
            string requestUri =
                    HttpUtility.UrlEncode(
                            request.Scheme +
                            "://" +
                            request.Host +
                            request.Path); //http://localhost:43326/api/values
            string requestHttpMethod = request.Method;

            //We need to enable buffering and reset the stream position
            //otherwise the controllers will not be able to
            //read the body themselves
            request.EnableBuffering();

            //Compute the Hash from the Body
            byte[] hash = ComputeMd5Hash(request.Body);

            //Reset the position for everyone that comes after
            request.Body.Position = 0;

            //Check if we could have generated a hash
            string requestContentBase64String = "";
            if(hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }

            //Short explanation why we build our signature string in this way:
            //The nonce and timestamp gives the request a unique note, but we can not store this request forever in a db
            //That's why we need to work with a nonce and a timestamp. The timestamp allows us to reject request
            //that are to far away from the current server time 
            //The nonce allows us to identify each single request as check it if it was not already send as long its in the allowed time range
            var data =
                    $"{publicKey}{requestHttpMethod}{requestUri}{requestTimeStamp}{nonce}{requestContentBase64String}";

            //Create an HMAC SHA256 Hash and check if the signature matches
            var secretKeyAsBytes = Convert.FromBase64String(apiKeyResult.ReturnValue.SecretKey);
            byte[] signatureAsBytes = Encoding.UTF8.GetBytes(data);
            using(var hmac = new HMACSHA256(secretKeyAsBytes))
            {
                byte[] signatureBytes = hmac.ComputeHash(signatureAsBytes);
                var calculatedSignatureAsBase64 = Convert.ToBase64String(signatureBytes);
                apiKeySecretResponse.IsSuccess = receivedSignatureAsBase64.Equals(calculatedSignatureAsBase64,StringComparison.Ordinal);
            }

            if(apiKeySecretResponse.IsSuccess)
            {
                var identity = new ClaimsIdentity(
                        new List<Claim>
                        {
                                new Claim(
                                        PodClaimsTypes.ApiKeyUserId,
                                        apiKeyResult.ReturnValue.Station.ApplicationUserId.ToString()),
                                new Claim(
                                        PodClaimsTypes.ApiKeyStationId,
                                        apiKeyResult.ReturnValue.StationId.ToString())
                        }, "StationApiKey");
                apiKeySecretResponse.ClaimsPrincipal = new ClaimsPrincipal(identity);
            }

            return apiKeySecretResponse;
        }

        /// <summary>
        /// Checks if the Request is not to far off by time from the server
        /// and checks if it was not already once processed
        /// </summary>
        /// <param name="nonce"></param>
        /// <param name="requestTimeStamp"></param>
        /// <returns></returns>
        private bool IsTimestampValid(string nonce, string requestTimeStamp)
        {
            //This can return true for multiple calls in a multi threaded environment
            //and is flawed, however it might be better then nothing and a good 
            if(_cache.TryGetValue(nonce, out object _)) return false;

            var serverTotalSeconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var requestTotalSeconds = Convert.ToInt64(requestTimeStamp);

            long timeSkewInMs = Math.Abs(serverTotalSeconds - requestTotalSeconds);
            if(timeSkewInMs > MaxClockSkewBetweenClientAndServerInMs) return false;

            _cache.Set(nonce, requestTimeStamp, DateTime.UtcNow.AddSeconds(MaxClockSkewBetweenClientAndServerInMs));

            return true;
        }

        /// <summary>
        /// Computes the Md5 Hash from a stream 
        /// </summary>
        /// <param name="body">The stream</param>
        /// <returns>The hash as bytes or null</returns>
        private static byte[] ComputeMd5Hash(Stream body)
        {
            using(var md5 = MD5.Create())
            {
                var content = GetBytes(body);

                byte[] hash = content.Length != 0
                        ? md5.ComputeHash(content)
                        : null;
                return hash;
            }
        }

        /// <summary>
        /// Reads Bytes from an Input Stream with undefined length to a
        /// memory stream and returns a byte array when the Input stream is
        /// completely read
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <returns>The input streams content as byte array</returns>
        private static byte[] GetBytes(Stream input)
        {
            //16Kb Buffer
            byte[] buffer = new byte[16 * 1024];

            //Rounds buffer filled until 20 MB reached
            const int roundsForMaxSize = 1280;

            //Rounds counter
            int rounds = 0;
            using(var ms = new MemoryStream())
            {
                //amount of bytes read from stream
                int read;
                while((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    rounds++;
                    ms.Write(buffer, 0, read);
                    //Watch the memory limit
                    if(rounds > roundsForMaxSize) break;
                }

                //Must have been an interruption due to max size as the stream has still some data
                if(read > 0)
                {
                    throw new NotSupportedException(
                            "Hashing of messages that are more then 20MB in size is not supported!");
                }

                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// Authentication Validation Response
    /// </summary>
    class ApiKeySecretResponse : IApiKeySecretResponse
    {
        /// <summary>
        /// Indicates if the Authentication Succeeded
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Indicates if all was ok except that the signature did not match
        /// </summary>
        public bool IsInvalidSignature { get; set; }

        /// <summary>
        /// The authenticated Identity
        /// </summary>
        public ClaimsPrincipal ClaimsPrincipal { get; set; }
    }
}