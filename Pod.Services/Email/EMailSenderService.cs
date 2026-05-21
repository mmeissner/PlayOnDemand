#region Licence
/****************************************************************
 *  Filename: EMailSenderService.cs
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
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.Extensions.Logging;
//using Pod.Data;
//using Pod.Data.Infrastructure;
//using Pod.Enums;
//using Pod.MailEngine;

//namespace Pod.Services.Email
//{
//    /// <summary>
//    /// Service to send Emails
//    /// </summary>
//    public class EMailSenderService
//    {
//        private readonly ILogger<EMailService> _logger;
//        private readonly EMailService _eMailService;
//        public EMailSenderService(ILogger<EMailService> logger, EMailService eMailService)
//        {
//            _logger = logger;
//            _eMailService = eMailService;
//        }

//        /// <summary>
//        /// Sends a emails
//        /// </summary>
//        /// <param name="senderFactory">Factory for email sender</param>
//        /// <param name="command">The Email Send information</param>
//        /// <returns>The Send Result</returns>
//        public async Task<IResult> SendMail(EMailTemplateSenderFactory senderFactory, SendMailCommand command)
//        {
//            return await SendMail(senderFactory, command, CancellationToken.None);
//        }

//        /// <summary>
//        /// Sends an Email
//        /// </summary>
//        /// <param name="receiver">The receiver email address</param>
//        /// <param name="identifier">The identifier for the template to use</param>
//        /// <param name="variableValues">The variables for replacement in the template</param>
//        /// <returns>The Send Result</returns>
//        public async Task<IResult> SendMail(
//                string receiver, EMailTemplateIdentifier identifier,
//                IReadOnlyDictionary<TemplateVariableKey, string> variableValues)
//        {
//            return await SendMail(receiver, identifier, variableValues, CancellationToken.None);
//        }

//        /// <summary>
//        /// Sends an Email
//        /// </summary>
//        /// <param name="receiver">The receiver email address</param>
//        /// <param name="identifier">The identifier for the template to use</param>
//        /// <param name="variableValues">The variables for replacement in the template</param>
//        /// <param name="cancellationToken">A Cancellation Token to cancel the send</param>
//        /// <returns>The Send Result</returns>
//        public async Task<IResult> SendMail(
//                string receiver, EMailTemplateIdentifier identifier,
//                IReadOnlyDictionary<TemplateVariableKey, string> variableValues, CancellationToken cancellationToken)
//        {
//            var findResult = await _eMailService.FindEmailAccountForTemplateIdentifierAsync(identifier);
//            if(findResult.HasError()) return findResult;
//            var sender = _templateSenderFactory.Create(findResult.ReturnValue.EMailAccountData);
//            if(sender.HasError()) return sender;
//            var template = new EMailTemplate(findResult.ReturnValue.EmailContentTemplate);
//            template.AddReceiver(receiver);
//            template.SetOrReplaceVariable(variableValues);
//            return await sender.ReturnValue.SendEmailAsync(template, cancellationToken);
//        }

//        /// <summary>
//        /// Sends an Email
//        /// </summary>
//        /// <param name="senderFactory">Factory for email sender</param>
//        /// <param name="command">The Email send information</param>
//        /// <param name="cancellationToken">A CancellationToken to cancel the send</param>
//        /// <returns>The Send Result</returns>
//        public async Task<IResult> SendMail(EMailTemplateSenderFactory senderFactory, SendMailCommand command, CancellationToken cancellationToken)
//        {
//            var result = new Result();
//            var resultEmailAccountData = await _eMailService.FindEmailAccountAsync(command.EmailAccountId);
//            if(resultEmailAccountData.HasError())
//            {
//                return result.Add(resultEmailAccountData);
//            }

//            var sender = senderFactory.Create(resultEmailAccountData.ReturnValue);
//            if(result.Add(sender).HasError())
//            {
//                return result;
//            }

//            //Find if Template is linked to Email Account
//            var requestedTemplate =
//                    resultEmailAccountData.ReturnValue.EmailContentTemplates.FirstOrDefault(
//                            x => x.EmailContentTemplate.Identifier ==
//                                 command.TemplateIdentifier);

//            if(requestedTemplate == null)
//            {
//                result.Add(
//                        $"Template with Identifier {command.TemplateIdentifier} not found for EmailAccount",
//                        UserError.EmailTemplateNotFound);
//                return result;
//            }

//            var template = new EMailTemplate(requestedTemplate.EmailContentTemplate);
//            if(result.Add(AddReceiversAndVarsToTemplate(command, template)).HasError())return result;
//            return await sender.ReturnValue.SendEmailAsync(template, cancellationToken);
//        }

        
//        private IResult AddReceiversAndVarsToTemplate(SendMailCommand command, EMailTemplate template)
//        {
//            //Receivers
//            var result = new Result();
//            template.AddReceiver(command.ToReceiverEMail, EmailReceiverType.To);
//            foreach(string bccReceiverEMail in command.BccReceiverEMails)
//            {
//                template.AddReceiver(bccReceiverEMail, EmailReceiverType.BlindCarbonCopy);
//            }

//            foreach(string ccReceiverEMail in command.CcReceiverEMails)
//            {
//                template.AddReceiver(ccReceiverEMail, EmailReceiverType.CarbonCopy);
//            }

//            foreach(KeyValuePair<TemplateVariableKey, string> valuePair in command.VariableValues)
//            {
//                result.Add(template.SetOrReplaceVariable(valuePair.Key, valuePair.Value));
//            }

//            return result;
//        }
//    }
//}