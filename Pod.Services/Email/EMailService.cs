#region Licence
/****************************************************************
 *  Filename: EMailService.cs
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Mail;
using Pod.Enums;
using Pod.MailEngine;
using Pod.ViewModels.Expressions;
using Pod.ViewModels.Mail;
using WebMarkupMin.Core;
using WebMarkupMin.Core.Loggers;

namespace Pod.Services.Email
{
    /// <summary>
    /// Service providing functions for EMail support in the System
    /// </summary>
    public class EMailService
    {
        public const int MaximumEmailSendAttempts = 1;
        private readonly PodDbContext _podDbContext;
        private readonly EMailTemplateSenderFactory _senderFactory;
        private readonly IVariableParser _variableParser;
        private readonly ILogger<EMailService> _logger;


        /// <summary>
        /// Creates an new EMail Service
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="podDbContext">The Database Context</param>
        /// <param name="senderFactory"></param>
        /// <param name="variableParser">Parser for Variables</param>
        public EMailService(ILogger<EMailService> logger, PodDbContext podDbContext, EMailTemplateSenderFactory senderFactory, IVariableParser variableParser)
        {
            _logger = logger;
            _podDbContext = podDbContext;
            _senderFactory = senderFactory;
            _variableParser = variableParser;
        }

        /// <summary>
        /// Gets all EMail Accounts from the Db async
        /// </summary>
        /// <returns>Collection of all EMail Accounts known to the system</returns>
        public async Task<IResult<IEnumerable<EMailAccountViewModel>>> EmailAccountGetAllAsync()
        {
            var result = new Result<IEnumerable<EMailAccountViewModel>>();
            var emailAccountsResult = await _podDbContext.EMailAccounts.
                                                          Include(x => x.EmailContentTemplates).
                                                          ThenInclude(x => x.EmailContentTemplate).
                                                          ThenInclude(x => x.Variables).
                                                          AsNoTracking().
                                                          ToArrayAsync();
            var retval = new List<EMailAccountViewModel>();
            if(emailAccountsResult.Any())
                retval.AddRange(emailAccountsResult.Select(x => ToEMailAccountVm.FuncFromEmailAccountData(x)));
            return result.Add(retval);
        }

        /// <summary>
        /// Gets all EMail Accounts from the Db
        /// </summary>
        /// <returns>Collection of all EMail Accounts known to the system</returns>
        public IResult<IEnumerable<EMailAccountViewModel>> EmailAccountGetAll()
        {
            var result = new Result<IEnumerable<EMailAccountViewModel>>();
            var emailAccountsResult = _podDbContext.EMailAccounts.
                                                    Include(x => x.EmailContentTemplates).
                                                    ThenInclude(x => x.EmailContentTemplate).
                                                    ThenInclude(x => x.Variables).
                                                    AsNoTracking().
                                                    ToArray();
            var retval = new List<EMailAccountViewModel>();
            if(emailAccountsResult.Any())
                retval.AddRange(emailAccountsResult.Select(x => ToEMailAccountVm.FuncFromEmailAccountData(x)));
            return result.Add(retval);
        }

        /// <summary>
        /// Gets the Account Data for an Email Account
        /// </summary>
        /// <param name="emailAccountId">The Id of the Email Account</param>
        /// <returns>The Email Account Data</returns>
        public IResult<EMailAccountData> EmailAccountGetAccountData(Guid emailAccountId)
        {
            var result = new Result<EMailAccountData>();
            result.Add(FindEmailAccount(emailAccountId, out var emailAccountData));
            if(result.HasError()) return result;
            return result.Add(emailAccountData);
        }

        /// <summary>
        /// Creates a new Email Account async
        /// </summary>
        /// <param name="displayName">The name of the Email Account</param>
        /// <returns>The created Email Account</returns>
        public async Task<IResult<EMailAccountViewModel>> EMailAccountCreateAsync(string displayName)
        {
            var result = new Result<EMailAccountViewModel>();
            var createResult = EMailAccountData.Create(displayName);
            if(createResult.HasError()) return result.Add(createResult);
            _podDbContext.EMailAccounts.Add(createResult.ReturnValue);
            await _podDbContext.SaveChangesAsync();
            return result.Add(ToEMailAccountVm.FuncFromEmailAccountData(createResult.ReturnValue));
        }

        /// <summary>
        /// Creates a new Email Account
        /// </summary>
        /// <param name="displayName">The name of the Email Account</param>
        /// <returns>The created Email Account</returns>
        public IResult<EMailAccountViewModel> EMailAccountCreate(string displayName)
        {
            var result = new Result<EMailAccountViewModel>();
            var createResult = EMailAccountData.Create(displayName);
            if(createResult.HasError()) return result.Add(createResult);
            _podDbContext.EMailAccounts.Add(createResult.ReturnValue);
            _podDbContext.SaveChanges();
            return result.Add(ToEMailAccountVm.FuncFromEmailAccountData(createResult.ReturnValue));
        }

        /// <summary>
        /// Creates a new Email Account async
        /// </summary>
        /// <param name="displayName">The name of the Email Account</param>
        /// <param name="emailAddress">The email address for the account</param>
        /// <param name="smtpServer">The url for the smtp server</param>
        /// <param name="smtpPort">The Smtp Port for the smtp server</param>
        /// <param name="useSsl">If the server uses ssl</param>
        /// <param name="authMethod">The authentication method for the server</param>
        /// <param name="username">The Username</param>
        /// <param name="password">The Password</param>
        /// <param name="senderName">The Sender name to use with this account</param>
        /// <returns>The created Email Account</returns>
        public async Task<IResult<EMailAccountViewModel>> EMailAccountCreateAsync(
                string displayName, string emailAddress, string smtpServer, int smtpPort, bool useSsl,
                SmtpAuthentication authMethod, string username, string password, string senderName)
        {
            var result = new Result<EMailAccountViewModel>();
            var createResult = EMailAccountData.CreateSmtp(
                    displayName,
                    emailAddress,
                    smtpServer,
                    smtpPort,
                    useSsl,
                    authMethod,
                    username,
                    password,
                    senderName);
            if(createResult.HasError()) return result.Add(createResult);
            _podDbContext.EMailAccounts.Add(createResult.ReturnValue);
            await _podDbContext.SaveChangesAsync();
            return result.Add(ToEMailAccountVm.FuncFromEmailAccountData(createResult.ReturnValue));
        }

        /// <summary>
        /// Sets the authentication for an SMTP server of an EMail Account
        /// </summary>
        /// <param name="emailAccountId">The Id of the email account</param>
        /// <param name="username">The username for authentication</param>
        /// <param name="password">The password for authentication</param>
        /// <param name="authentication">The authentication method</param>
        /// <returns></returns>
        public async Task<IResult<EMailAccountViewModel>> EMailAccountSetSmtpAuth(
                Guid emailAccountId, string username, string password, SmtpAuthentication authentication)
        {
            var result = new Result<EMailAccountViewModel>();
            var emailAccountResult = await FindEmailAccountAsync(emailAccountId);
            result.Add(emailAccountResult);
            if(result.HasError()) return result;
            result.Add(emailAccountResult.ReturnValue.SetSmtpAuth(username, password, authentication));
            if(result.IsSuccess())
            {
                await _podDbContext.SaveChangesAsync();
            }

            return result.Add(ToEMailAccountVm.FuncFromEmailAccountData(emailAccountResult.ReturnValue));
        }
        /// <summary>
        /// Sets the SMTP Server for an EMail Account
        /// </summary>
        /// <param name="emailAccountId">The Id of the email account</param>
        /// <param name="smtpServer">The url for the smtp server</param>
        /// <param name="smtpPort">The Smtp Port for the smtp server</param>
        /// <param name="useSsl">If the server uses ssl</param>
        /// <returns></returns>
        public async Task<IResult<EMailAccountViewModel>> EMailAccountSetSmtpServer(
                Guid emailAccountId, string smtpServer, int smtpPort, bool useSsl)
        {
            var result = new Result<EMailAccountViewModel>();
            var emailAccountResult = await FindEmailAccountAsync(emailAccountId);
            result.Add(emailAccountResult);
            if(result.HasError()) return result;
            result.Add(emailAccountResult.ReturnValue.SetSmtpServer(smtpServer, smtpPort, useSsl));
            if(result.IsSuccess())
            {
                await _podDbContext.SaveChangesAsync();
            }

            return result.Add(ToEMailAccountVm.FuncFromEmailAccountData(emailAccountResult.ReturnValue));
        }

        /// <summary>
        /// Sets the sender information for an EMail Account
        /// </summary>
        /// <param name="emailAccountId">The id of the email account</param>
        /// <param name="emailAddress">The email address of the email account</param>
        /// <param name="senderName">The senders name for the email account</param>
        /// <returns></returns>
        public async Task<IResult<EMailAccountViewModel>> EMailAccountSetSender(
                Guid emailAccountId, string emailAddress, string senderName = null)
        {
            var result = new Result<EMailAccountViewModel>();
            var emailAccountResult = await FindEmailAccountAsync(emailAccountId);
            result.Add(emailAccountResult);
            if(result.HasError()) return result;
            result.Add(emailAccountResult.ReturnValue.SetSender(emailAddress, senderName));
            if(result.IsSuccess())
            {
                await _podDbContext.SaveChangesAsync();
            }

            return result.Add(ToEMailAccountVm.FuncFromEmailAccountData(emailAccountResult.ReturnValue));
        }

        /// <summary>
        /// Enables or Disables an email account
        /// </summary>
        /// <param name="emailAccountId">The Id of the Email Account</param>
        /// <param name="isEnabled">True to enable, false to disable</param>
        /// <returns>The Email Account</returns>
        public async Task<IResult<EMailAccountViewModel>> EMailAccountSetEnabled(Guid emailAccountId, bool isEnabled)
        {
            var result = new Result<EMailAccountViewModel>();
            var emailAccountResult = await FindEmailAccountAsync(emailAccountId);
            result.Add(emailAccountResult);
            if(result.HasError()) return result;
            result.Add(emailAccountResult.ReturnValue.SetEnabled(isEnabled));
            if(result.IsSuccess())
            {
                await _podDbContext.SaveChangesAsync();
            }

            return result.Add(ToEMailAccountVm.FuncFromEmailAccountData(emailAccountResult.ReturnValue));
        }

        /// <summary>
        /// Deletes an email account
        /// </summary>
        /// <param name="emailAccountId">The Email Account Id</param>
        /// <returns>The Result</returns>
        public async Task<IResult> EMailAccountDelete(Guid emailAccountId)
        {
            var result = new Result();
            var emailAccountResult = await FindEmailAccountAsync(emailAccountId);
            result.Add(emailAccountResult);
            if(result.HasError()) return result;
            foreach(var contentTemplateLink in emailAccountResult.ReturnValue.EmailContentTemplates)
            {
                _podDbContext.Remove(contentTemplateLink);
            }

            _podDbContext.Remove(emailAccountResult.ReturnValue);
            await _podDbContext.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Links an Template to an Email Account async, so that this account can be used with the Template
        /// </summary>
        /// <param name="emailAccountId">The email account Id</param>
        /// <param name="emailContentTemplateId">The Template Id</param>
        /// <returns>The Result</returns>
        public async Task<IResult> EMailAccountAddTemplateAsync(Guid emailAccountId, Guid emailContentTemplateId)
        {
            var result = new Result();

            if(!result.ValueIdValid(
                       emailAccountId,
                       nameof(emailAccountId),
                       UserError.EMailAccountDataInvalid) ||
               !result.ValueIdValid(
                       emailContentTemplateId,
                       nameof(emailContentTemplateId),
                       UserError.EMailTemplateDataInvalid)) return result;

            // Note: FindAsync returns ValueTask<T> in EF Core 3+, so we cannot use Task.WhenAll directly.
            // Run them sequentially (EF Core does not support concurrent operations on the same context anyway).
            var emailAccount = await _podDbContext.EMailAccounts.Where(x => x.Id == emailAccountId).
                                                   Include(x => x.EmailContentTemplates).
                                                   FirstOrDefaultAsync();
            var emailTemplate = await _podDbContext.EmailContentTemplates.FindAsync(emailContentTemplateId);
            result.ArgNotNull(emailAccount, nameof(emailAccount), UserError.EMailAccountNotFound);
            result.ArgNotNull(
                    emailTemplate,
                    nameof(emailTemplate),
                    UserError.EmailTemplateNotFound);
            if(result.HasError()) return result;
            if(result.Add(emailAccount.AddEMailContentTemplate(emailTemplate)).HasError())
                return result;
            await _podDbContext.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Links an Template to an Email Account, so that this account can be used with the Template
        /// </summary>
        /// <param name="emailAccountId">The email account Id</param>
        /// <param name="emailContentTemplateId">The Template Id</param>
        /// <returns>The Result</returns>
        public IResult EMailAccountAddTemplate(Guid emailAccountId, Guid emailContentTemplateId)
        {
            var result = new Result();
            if(!result.ValueIdValid(
                       emailAccountId,
                       nameof(emailAccountId),
                       UserError.EMailAccountDataInvalid) ||
               !result.ValueIdValid(
                       emailContentTemplateId,
                       nameof(emailContentTemplateId),
                       UserError.EMailTemplateDataInvalid)) return result;

            var emailAccountTask = _podDbContext.EMailAccounts.Where(x => x.Id == emailAccountId).
                                                 Include(x => x.EmailContentTemplates).
                                                 FirstOrDefault();
            var emailTemplateTask = _podDbContext.EmailContentTemplates.Find(emailContentTemplateId);
            result.ArgNotNull(emailAccountTask, nameof(emailAccountTask), UserError.EMailAccountNotFound);
            result.ArgNotNull(
                    emailTemplateTask,
                    nameof(emailTemplateTask),
                    UserError.EmailTemplateNotFound);
            if(result.HasError()) return result;
            if(result.Add(emailAccountTask.AddEMailContentTemplate(emailTemplateTask)).HasError()) return result;
            _podDbContext.SaveChanges();
            return result;
        }

        /// <summary>
        /// Unlink an Template from an Email Account
        /// </summary>
        /// <param name="emailAccountId">The Email account Id</param>
        /// <param name="emailContentTemplateId">The Template Id</param>
        /// <returns>The Result</returns>
        public async Task<IResult> EMailAccountRemoveTemplate(Guid emailAccountId, Guid emailContentTemplateId)
        {
            var result = new Result();
            if(!result.ValueIdValid(
                       emailAccountId,
                       nameof(emailAccountId),
                       UserError.EMailAccountDataInvalid) ||
               !result.ValueIdValid(
                       emailContentTemplateId,
                       nameof(emailContentTemplateId),
                       UserError.EMailTemplateDataInvalid)) return result;

            var emailAccount = await _podDbContext.EMailAccounts.Where(x => x.Id == emailAccountId).
                                                   Include(x => x.EmailContentTemplates).
                                                   FirstOrDefaultAsync();
            if(!result.ArgNotNull(emailAccount, nameof(emailAccount), UserError.EMailAccountNotFound)) return result;
            var removeResult = emailAccount.RemoveEMailContentTemplateBy(emailContentTemplateId);
            if(removeResult.HasError()) return removeResult;
            if(removeResult.ReturnValue != null)
            {
                _podDbContext.Remove(removeResult.ReturnValue);
                await _podDbContext.SaveChangesAsync();
            }

            return result;
        }

        /// <summary>
        /// Provides information about all Email template identifiers and Template Variables
        /// </summary>
        /// <returns></returns>
        public IResult<EmailTemplateInfo> EMailTemplateGetInfo()
        {
            return new Result<EmailTemplateInfo>().Add(
                    new EmailTemplateInfo
                    {
                            Identifiers = Enum.GetNames(typeof(EMailTemplateIdentifier)),
                            VariableKeys = Enum.GetNames(typeof(TemplateVariableKey))
                    });
        }

        /// <summary>
        /// Provides all available EMail Templates async
        /// </summary>
        /// <returns>Collection of Templates</returns>
        public async Task<IResult<IEnumerable<EMailTemplateDetailsViewModel>>> EMailTemplateGetAllAsync()
        {
            var result = new Result<IEnumerable<EMailTemplateDetailsViewModel>>();
            var emailTemplates = await _podDbContext.EmailContentTemplates.
                                                     Include(x => x.Variables).
                                                     AsNoTracking().
                                                     ToArrayAsync();

            var retval = new List<EMailTemplateDetailsViewModel>();
            if(emailTemplates != null && emailTemplates.Any())
            {
                retval.AddRange(
                        emailTemplates.Select(
                                x => ToEmailTemplateDetailsVm.FuncToEmailContentTemplateDetailsViewModel(x)));
            }

            return result.Add(retval);
        }

        /// <summary>
        /// Provides all available EMail Templates
        /// </summary>
        /// <returns>Collection of Templates</returns>
        public IResult<IEnumerable<EMailTemplateDetailsViewModel>> EMailTemplateGetAll()
        {
            var result = new Result<IEnumerable<EMailTemplateDetailsViewModel>>();
            var emailTemplates = _podDbContext.EmailContentTemplates.
                                               Include(x => x.Variables).
                                               AsNoTracking().
                                               ToArray();

            var retval = new List<EMailTemplateDetailsViewModel>();
            if(emailTemplates.Any())
            {
                retval.AddRange(
                        emailTemplates.Select(
                                x => ToEmailTemplateDetailsVm.FuncToEmailContentTemplateDetailsViewModel(x)));
            }

            return result.Add(retval);
        }

        /// <summary>
        /// Creates an new Email Template async
        /// </summary>
        /// <param name="displayName">The Name of the Template</param>
        /// <param name="identifier">The identifier for the template </param>
        /// <param name="variableControlChar">The control char used to escape variables</param>
        /// <param name="subject">The Subject</param>
        /// <param name="content">The Text Content</param>
        /// <param name="contentHtml">The Html Content</param>
        /// <returns>The created Template</returns>
        public async Task<IResult<EMailTemplateDetailsViewModel>> EMailTemplateCreateAsync(
                string displayName,
                EMailTemplateIdentifier identifier,
                char variableControlChar,
                string subject,
                string content,
                string contentHtml)
        {
            var result = new Result<EMailTemplateDetailsViewModel>();

            //Minify HTML Content to reduce size
            if(!string.IsNullOrWhiteSpace(contentHtml))
            {
                var settings = new HtmlMinificationSettings();
                var cssMinifier = new KristensenCssMinifier();
                var jsMinifier = new CrockfordJsMinifier();
                var logger = new NullLogger();
                var htmlMinifier = new HtmlMinifier(settings, cssMinifier,
                        jsMinifier, logger);
                var minificationResult = htmlMinifier.Minify(contentHtml, Encoding.UTF8);
                if(!minificationResult.Errors.Any())
                {
                    contentHtml = minificationResult.MinifiedContent;
                }
            }

            var createResult = EmailContentTemplate.Create(
                    _variableParser,
                    displayName,
                    identifier,
                    variableControlChar,
                    subject,
                    content,
                    contentHtml);
            if(createResult.HasError()) return result.Add(createResult);
            _podDbContext.EmailContentTemplates.Add(createResult.ReturnValue);
            await _podDbContext.SaveChangesAsync();
            return result.Add(
                    ToEmailTemplateDetailsVm.FuncToEmailContentTemplateDetailsViewModel(createResult.ReturnValue));
        }

        /// <summary>
        /// Creates an new Email Template
        /// </summary>
        /// <param name="displayName">The Name of the Template</param>
        /// <param name="identifier">The identifier for the template </param>
        /// <param name="variableControlChar">The control char used to escape variables</param>
        /// <param name="subject">The Subject</param>
        /// <param name="content">The Text Content</param>
        /// <param name="contentHtml">The Html Content</param>
        /// <returns>The created Template</returns>
        public IResult<EMailTemplateDetailsViewModel> EMailTemplateCreate(
                string displayName,
                EMailTemplateIdentifier identifier,
                char variableControlChar,
                string subject,
                string content,
                string contentHtml)
        {
            var result = new Result<EMailTemplateDetailsViewModel>();
            var createResult = EmailContentTemplate.Create(
                    _variableParser,
                    displayName,
                    identifier,
                    variableControlChar,
                    subject,
                    content,
                    contentHtml);
            if(createResult.HasError()) return result.Add(createResult);
            _podDbContext.EmailContentTemplates.Add(createResult.ReturnValue);
            _podDbContext.SaveChanges();
            return result.Add(
                    ToEmailTemplateDetailsVm.FuncToEmailContentTemplateDetailsViewModel(createResult.ReturnValue));
        }

        /// <summary>
        /// Deletes an Email Template
        /// </summary>
        /// <param name="templateId">The template Id</param>
        /// <returns>The Result</returns>
        public async Task<IResult> EmailTemplateDelete(Guid templateId)
        {
            var result = new Result();
            if(!result.ValueIdValid(
                    templateId,
                    nameof(templateId),
                    UserError.EMailTemplateDataInvalid)) return result;
            var emailTemplate = await _podDbContext.EmailContentTemplates.
                                                    Where(x => x.Id == templateId).
                                                    Include(x => x.EmailAccounts).
                                                    FirstOrDefaultAsync();
            if(!result.ArgNotNull(emailTemplate, nameof(emailTemplate), UserError.EmailTemplateNotFound)) return result;
            foreach(var emailAccountLink in emailTemplate.EmailAccounts)
            {
                _podDbContext.Remove(emailAccountLink);
            }

            _podDbContext.Remove(emailTemplate);
            await _podDbContext.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Creates an Order to send an Email
        /// </summary>
        /// <param name="identifier">Identifier for template</param>
        /// <param name="emailAddress">Email Address of receiver</param>
        /// <param name="variableValues">Variables to replace in template</param>
        /// <returns></returns>
        public async Task<IResult> CreateEmailSendOrder(
                EMailTemplateIdentifier identifier,
                string emailAddress,
                IReadOnlyDictionary<TemplateVariableKey, string> variableValues)
        {
            var result = new Result();
            var receiverResult = EMailReceiver.Create(EmailReceiverType.To, emailAddress);
            var createOrderResult = EmailSendOrder.CreateOrder(
                    identifier,
                    new[] {receiverResult.ReturnValue},
                    variableValues);

            result.Add(receiverResult).Add(createOrderResult);
            if(result.HasError()) return result;

            _podDbContext.EmailSendOrders.Add(createOrderResult.ReturnValue);
            await _podDbContext.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Provides overview about Orders for Emails
        /// </summary>
        /// <param name="take">amount to display</param>
        /// <param name="skip">amount to skip</param>
        /// <returns>result</returns>
        public async Task<IResult<IEnumerable<EmailSendOrderViewModel>>> GetMailsToSend(int take, int skip)
        {
            var result = new Result<IEnumerable<EmailSendOrderViewModel>>();
            var emailOrders = await _podDbContext.EmailSendOrders.
                                                  Skip(skip).
                                                  Take(take).
                                                  Include(x => x.Variables).
                                                  Include(x => x.Receivers).
                                                  AsNoTracking().
                                                  ToListAsync();
            return result.Add(emailOrders.Select(EMailSendOrderVm.FuncToEmailSendOrderViewModel));
        }

        /// <summary>
        /// Provides overview about Orders for Emails with a specific state
        /// </summary>
        /// <param name="filterState">the state of the order</param>
        /// <param name="take">amount to display</param>
        /// <param name="skip">amount to skip</param>
        /// <returns>result</returns>
        public async Task<IResult<IEnumerable<EmailSendOrderViewModel>>> GetMailsToSend(EmailSendState filterState, int take = 50, int skip = 0)
        {
            var result = new Result<IEnumerable<EmailSendOrderViewModel>>();
            var emailOrders = await _podDbContext.EmailSendOrders.Where(x => x.SendState == filterState).
                                          Skip(skip).
                                          Take(take).
                                          Include(x => x.Variables).
                                          Include(x => x.Receivers).
                                          AsNoTracking().
                                          ToListAsync();
            return result.Add(emailOrders.Select(EMailSendOrderVm.FuncToEmailSendOrderViewModel));
        }

        /// <summary>
        /// Sends all ordered eMails
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IResult> SendEmailOrders(CancellationToken cancellationToken)
        {
            var result= new Result();
            var openEmailsSendOrders =
                    await _podDbContext.EmailSendOrders
                                       .Where(x => x.SendState == EmailSendState.Unsend)
                                       .Include(x=> x.Variables)
                                       .Include(x=> x.Receivers)
                                       .ToArrayAsync(cancellationToken);

            var sendDict = new Dictionary<EMailAccountData,List<(EMailTemplate, EmailSendOrder)>>();

            //Create Send information from Orders
            foreach(EmailSendOrder order in openEmailsSendOrders)
            {
                //Try to get the EMailAccount and Template to use
                var findResult = await FindEmailAccountForTemplateIdentifierAsync(order.TemplateIdentifier);
                if(findResult.HasError())
                {
                    result.Add(findResult);
                    order.SetSendAttemptResult(false, MaximumEmailSendAttempts, findResult.ToErrorString());
                    continue;
                }

                //Create EMail to send template from Generic Template
                var template = new EMailTemplate(findResult.ReturnValue.EmailContentTemplate);
                foreach(EMailReceiver receiver in order.Receivers)
                {
                    template.AddReceiver(receiver.Name, receiver.EmailAddress,receiver.Type);
                }
                //Set Values for Variables in Template
                var setVariableResult = template.SetOrReplaceVariable(order.Variables.ToDictionary(x=> x.Key,y=> y.Value));
                if(setVariableResult.HasError())
                {
                    result.Add(setVariableResult);
                    order.SetSendAttemptResult(false, MaximumEmailSendAttempts, setVariableResult.ToErrorString());
                    continue;
                }

                // Add Email Template for Send and Order to Dictionary by Email Account to send from
                if(sendDict.ContainsKey(findResult.ReturnValue.EMailAccountData))
                {
                    sendDict[findResult.ReturnValue.EMailAccountData].Add((template,order));
                }
                else
                {
                    sendDict.Add(findResult.ReturnValue.EMailAccountData,new List<(EMailTemplate, EmailSendOrder)> { (template, order) });
                }
            }

            //Check for Cancellation
            if(cancellationToken.IsCancellationRequested)
            {
                return result.Add("Request was canceled before any e-mail was send!", UserError.InternalError);
            }

            //Send EMails Batched per Account to send from
            foreach(KeyValuePair<EMailAccountData, List<(EMailTemplate template, EmailSendOrder order)>> sendInfo in sendDict)
            {
                var sender = _senderFactory.Create(sendInfo.Key);
                if (sender.HasError())
                {
                    var errorString = sender.ToErrorString();
                    foreach ((EMailTemplate template, EmailSendOrder order) sendOrderInfo in sendInfo.Value)
                    {
                        sendOrderInfo.order.SetSendAttemptResult(false,MaximumEmailSendAttempts, errorString);
                    }
                    result.Add(sender);
                }
                else
                {
                    try
                    {
                        var sendMailResult = await sender.ReturnValue.SendEmailAsync(sendInfo.Value.Select(x => x.template).ToArray(), cancellationToken);
                        var isSuccess = sendMailResult.IsSuccess();
                        string errorString = null;
                        if (!isSuccess) errorString = sendMailResult.ToErrorString();
                        foreach ((EMailTemplate template, EmailSendOrder order) sendOrderInfo in sendInfo.Value)
                        {
                            sendOrderInfo.order.SetSendAttemptResult(isSuccess, MaximumEmailSendAttempts, errorString);
                        }
                    }
                    catch(TaskCanceledException)
                    {
                        _logger.LogWarning("The send email Task was canceled during processing, and the results will not be stored");
                        return result.Add("Request was canceled during sending of e-mails and the results will not be stored!", UserError.InternalError);
                    }
                }

                await _podDbContext.SaveChangesAsync();
            }

            return result;
        }

        /// <summary>
        /// Sends a emails
        /// </summary>
        /// <param name="senderFactory">Factory for email sender</param>
        /// <param name="command">The Email Send information</param>
        /// <returns>The Send Result</returns>
        public async Task<IResult> SendMail( SendMailCommand command)
        {
            return await SendMail(command, CancellationToken.None);
        }

        /// <summary>
        /// Sends an Email
        /// </summary>
        /// <param name="senderFactory">Factory for email sender</param>
        /// <param name="command">The Email send information</param>
        /// <param name="cancellationToken">A CancellationToken to cancel the send</param>
        /// <returns>The Send Result</returns>
        public async Task<IResult> SendMail(SendMailCommand command, CancellationToken cancellationToken)
        {
            var result = new Result();
            var resultEmailAccountData = await FindEmailAccountAsync(command.EmailAccountId);
            if (resultEmailAccountData.HasError())
            {
                return result.Add(resultEmailAccountData);
            }

            var sender = _senderFactory.Create(resultEmailAccountData.ReturnValue);
            if (result.Add(sender).HasError())
            {
                return result;
            }

            //Find if Template is linked to Email Account
            var requestedTemplate =
                    resultEmailAccountData.ReturnValue.EmailContentTemplates.FirstOrDefault(
                            x => x.EmailContentTemplate.Identifier ==
                                 command.TemplateIdentifier);

            if (requestedTemplate == null)
            {
                result.Add(
                        $"Template with Identifier {command.TemplateIdentifier} not found for EmailAccount",
                        UserError.EmailTemplateNotFound);
                return result;
            }

            var template = new EMailTemplate(requestedTemplate.EmailContentTemplate);
            if (result.Add(AddReceiversAndVarsToTemplate(command, template)).HasError()) return result;
            return await sender.ReturnValue.SendEmailAsync(template, cancellationToken);
        }

        internal async Task<Result<EMailAccountData>> FindEmailAccountAsync(Guid emailAccountId)
        {
            var result = new Result<EMailAccountData>();
            result.ValueIdValid(
                    emailAccountId,
                    nameof(emailAccountId),
                    UserError.EMailAccountDataInvalid);
            if(result.HasError()) return result;
            var eMailAccount = await _podDbContext.EMailAccounts.Where(x => x.Id == emailAccountId).
                                                   Include(x => x.EmailContentTemplates).
                                                   ThenInclude(x => x.EmailContentTemplate).
                                                   ThenInclude(x => x.Variables).
                                                   FirstOrDefaultAsync();
            result.ArgNotNull(eMailAccount, nameof(eMailAccount), UserError.EMailAccountNotFound);
            return result.Add(eMailAccount);
        }

        internal async Task<Result<EMailAccountDataEMailContentTemplate>> FindEmailAccountForTemplateIdentifierAsync(
                EMailTemplateIdentifier identifier)
        {
            var result = new Result<EMailAccountDataEMailContentTemplate>();
            var emailAccountTemplateLink = await _podDbContext.EmailAccTemplateLinks.
                                                               Include(x => x.EmailContentTemplate).
                                                               ThenInclude(x=> x.Variables).
                                                               Include(x => x.EMailAccountData).
                                                               SingleOrDefaultAsync(
                                                                       x => x.EmailContentTemplate.Identifier ==
                                                                            identifier);
            if(result.RefNotNull(
                    emailAccountTemplateLink,
                    nameof(emailAccountTemplateLink),
                    UserError.EMailAccountForTemplateNotFound)) result.Add(emailAccountTemplateLink);
            return result;
        }

        private IResult AddReceiversAndVarsToTemplate(SendMailCommand command, EMailTemplate template)
        {
            //Receivers
            var result = new Result();
            template.AddReceiver(command.ToReceiverEMail, EmailReceiverType.To);
            foreach (string bccReceiverEMail in command.BccReceiverEMails)
            {
                template.AddReceiver(bccReceiverEMail, EmailReceiverType.BlindCarbonCopy);
            }

            foreach (string ccReceiverEMail in command.CcReceiverEMails)
            {
                template.AddReceiver(ccReceiverEMail, EmailReceiverType.CarbonCopy);
            }

            foreach (KeyValuePair<TemplateVariableKey, string> valuePair in command.VariableValues)
            {
                result.Add(template.SetOrReplaceVariable(valuePair.Key, valuePair.Value));
            }

            return result;
        }

        private Result FindEmailAccount(Guid emailAccountId, out EMailAccountData eMailAccount)
        {
            eMailAccount = null;
            var result = new Result();
            result.ValueIdValid(
                    emailAccountId,
                    nameof(emailAccountId),
                    UserError.EMailAccountDataInvalid);
            if(result.HasError()) return result;
            eMailAccount = _podDbContext.EMailAccounts.Where(x => x.Id == emailAccountId).
                                         Include(x => x.EmailContentTemplates).
                                         ThenInclude(x => x.EmailContentTemplate).
                                         ThenInclude(x => x.Variables).
                                         FirstOrDefault();
            result.ArgNotNull(eMailAccount, nameof(eMailAccount), UserError.EMailAccountNotFound);
            return result;
        }
    }
}