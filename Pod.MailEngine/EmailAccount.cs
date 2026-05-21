#region Licence
/****************************************************************
 *  Filename: EmailAccount.cs
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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Mail;
using Pod.Enums;
using Pod.MailEngine.Interfaces;

namespace Pod.MailEngine
{
    /// <summary>
    /// An Email Account implementation for SMTP accounts with Username and Password authentication
    /// </summary>
    internal class SmtpAccount : IEmailAccount
    {
        private SmtpAccount(IEMailAccountData accountData)
        {
            AccountData = accountData;
        }
        
        /// <summary>
        /// Provides the detailed data about this account, server, credentials and so on
        /// </summary>
        public IEMailAccountData AccountData { get; }
        
        /// <summary>
        /// Creates an new SMTP Account as <see cref="IEmailAccount"/>
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns>email account</returns>
        public static IResult<IEmailAccount> Create(IEMailAccountData accountData)
        {
            var result = new Result<SmtpAccount>();
            return result.Add(new SmtpAccount(accountData));
        }
        
        /// <summary>
        ///  Connects and Sends Emails over this SMTP Account
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="messages">collection of messages to send in one connect</param>
        /// <param name="cancellationToken">token for canceling send</param>
        /// <returns>result</returns>
        public async Task<IResult> ConnectAndSend(ILogger<IEmailTemplateSender> logger, HashSet<MimeMessage> messages, CancellationToken cancellationToken)
        {
            var retval = new Result();
            try
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                    await client.ConnectAsync(AccountData.SmtpServer, AccountData.SmtpPort, AccountData.UseSsl, cancellationToken);
                    await client.AuthenticateAsync(AccountData.Username, AccountData.Password, cancellationToken);

                    foreach (var message in messages)
                    {
                        await client.SendAsync(message, cancellationToken);
                        logger.LogInformation($"Mail send to: {message.To}, with Subject={message.Subject}");
                    }

                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
            catch(Exception e)
            {
                retval.Add(e.Message, UserError.EmailSendFailure);
            }
            return retval;
        }
    }

    /// <summary>
    /// An Email Account implementation for GMail accounts with OAuth2 authentication
    /// </summary>
    internal class GMailAccount : IEmailAccount
    {
        /// <summary>
        /// Secrets for Googles OAUTH2 authentication
        /// </summary>
        private readonly ClientSecrets _clientSecrets;

        /// <summary>
        /// User credentials for googles OAuth 2 authentication
        /// </summary>
        private UserCredential _googleCredentials;

        private GMailAccount(ClientSecrets clientSecrets, IEMailAccountData accountData)
        {
            _clientSecrets = clientSecrets;
            AccountData = accountData;
        }
        
        /// <summary>
        /// Provides the detailed data about this account, server, credentials and so on
        /// </summary>
        public IEMailAccountData AccountData { get; }
        
        /// <summary>
        /// Creates an new Google Account as <see cref="IEmailAccount"/>
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns>email account</returns>
        public static IResult<IEmailAccount> Create(IEMailAccountData accountData)
        {
            var result = new Result<GMailAccount>();
            var clientSecrets = new ClientSecrets()
                                {
                                        ClientId = accountData.Username,
                                        ClientSecret = accountData.Password
                                };
            return result.Add(new GMailAccount(clientSecrets, accountData));

        }
        
        /// <summary>
        ///  Connects and Sends Emails over this SMTP Account
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="messages">collection of messages to send in one connect</param>
        /// <param name="cancellationToken">token for canceling send</param>
        /// <returns>result</returns>
        public async Task<IResult> ConnectAndSend(ILogger<IEmailTemplateSender> logger, HashSet<MimeMessage> messages, CancellationToken cancellationToken)
        {
            var retval = new Result();
            try
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                    await client.ConnectAsync(AccountData.SmtpServer, AccountData.SmtpPort, AccountData.UseSsl, cancellationToken);

                    var token = await GetGoogleCredentialsAsync(cancellationToken);
                    var oauth2 = new SaslMechanismOAuth2(token.UserId, token.Token.AccessToken);
                    await client.AuthenticateAsync(oauth2, cancellationToken);

                    foreach (var message in messages)
                    {
                        await client.SendAsync(message, cancellationToken);
                        logger.LogDebug($"Mail send to: {message.To}, with Subject={message.Subject}");
                    }

                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
            catch(Exception e)
            {
                retval.Add(e.Message, UserError.EmailSendFailure);
            }
            return retval;
        }
        
        /// <summary>
        /// Authentication for Google Account 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<UserCredential> GetGoogleCredentialsAsync(CancellationToken cancellationToken)
        {
            //https://developers.google.com/api-client-library/dotnet/guide/aaa_oauth
            //https://developers.google.com/apis-explorer/?hl=de#p/
            //https://stackoverflow.com/questions/33496290/how-to-send-email-by-using-mailkit
            //https://stackoverflow.com/questions/51081442/sending-mail-using-mailkit-with-gmail-oauth

            _googleCredentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(_clientSecrets, new[] { GmailService.Scope.MailGoogleCom }, AccountData.EmailAddress, cancellationToken);
            if (_googleCredentials.Token.IsExpired(SystemClock.Default))
            {
                await _googleCredentials.RefreshTokenAsync(cancellationToken);
            }
            return _googleCredentials;
        }
    }
}
