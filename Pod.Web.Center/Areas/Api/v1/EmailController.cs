#region Licence
/****************************************************************
 *  Filename: EmailController.cs
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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Pod.Data.Config;
using Pod.DtoModels;
using Pod.Enums;
using Pod.MailEngine;
using Pod.Services.Email;
using Pod.ViewModels.Customer;
using Pod.ViewModels.Mail;
using Pod.Web.Center.Presenter;
using Pod.Web.Center.Swagger;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;

namespace Pod.Web.Center.Areas.Api.v1
{
    [Produces("application/json")]
    [Route("api/v1/internal")]
    [ApiController]
    [SwaggerTag("Mailing related functions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RolesConfig.AdministratorRole)]
    public class EmailController : ControllerBase
    {
        private readonly EMailService _eMailService;

        public EmailController(EMailService eMailService)
        {
            _eMailService = eMailService;
        }

        /// <summary>
        /// Get all available Email Accounts
        /// </summary>
        // GET api/v1/internal/email/emailAccounts
        [ProducesResponseType(typeof(IEnumerable<EMailAccountViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("system/email/accounts")]
        public async Task<ActionResult> EMailAccountsGetAll()
        {
            var result = await _eMailService.EmailAccountGetAllAsync();
            return ResultPresenter.GetResult(result);
        }

        /// <summary>
        /// Creates a new Email Account
        /// </summary>
        /// <param name="requestEmailAccountCreateDto">The account data</param>
        /// <returns>The created Account</returns>
        // POST api/v1/internal/email/emailAccounts
        [ProducesResponseType(typeof(EMailAccountViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts")]
        public async Task<ActionResult> EMailAccountCreate(
                [FromBody] RequestCreateEmailAccountDto requestEmailAccountCreateDto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            return ResultPresenter.GetResult(
                    await
                            _eMailService.EMailAccountCreateAsync(
                                    requestEmailAccountCreateDto.DisplayName,
                                    requestEmailAccountCreateDto.EMailAddress,
                                    requestEmailAccountCreateDto.SmtpServer,
                                    requestEmailAccountCreateDto.SmtpPort,
                                    requestEmailAccountCreateDto.UseSsl,
                                    requestEmailAccountCreateDto.AuthMethod,
                                    requestEmailAccountCreateDto.Username,
                                    requestEmailAccountCreateDto.Password,
                                    requestEmailAccountCreateDto.SenderName));
        }

        [ProducesResponseType(typeof(EMailAccountViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts/{accountId}/enable")]
        public async Task<ActionResult> EMailAccountSetEnabled(
                [BindRequired, FromRoute] Guid accountId,
                [BindRequired, FromQuery] bool isEnabled)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await _eMailService.EMailAccountSetEnabled(
                            accountId,
                            isEnabled));
        }


        [ProducesResponseType(typeof(EMailAccountViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts/{accountId}/auth")]
        public async Task<ActionResult> EMailAccountSetAuth(
                [FromBody] RequestSetSmtpAuthSettingsDto setAuthSettingsRequest,
                [BindRequired, FromRoute] Guid accountId)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await _eMailService.EMailAccountSetSmtpAuth(
                            accountId,
                            setAuthSettingsRequest.Username,
                            setAuthSettingsRequest.Password,
                            setAuthSettingsRequest.AuthMethod));
        }

        [ProducesResponseType(typeof(EMailAccountViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts/{accountId}/smtp")]
        public async Task<ActionResult> EMailAccountSetSmtp(
                [FromBody] RequestSetSmtpServerDto setSmtpServerRequest,
                [BindRequired, FromRoute] Guid accountId)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await _eMailService.EMailAccountSetSmtpServer(
                            accountId,
                            setSmtpServerRequest.SmtpServer,
                            setSmtpServerRequest.SmtpPort,
                            setSmtpServerRequest.UseSsl));
        }

        [ProducesResponseType(typeof(EMailAccountViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts/{accountId}/sender")]
        public async Task<ActionResult> EMailAccountSetSender(
                [FromBody] RequestSetEmailSenderDto setEmailSenderRequest,
                [BindRequired, FromRoute] Guid accountId)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await _eMailService.EMailAccountSetSender(
                            accountId,
                            setEmailSenderRequest.EMailAddress,
                            setEmailSenderRequest.SenderName));
        }
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts/{accountId}/sendMail")]
        public async Task<ActionResult> EMailAccountSendMail(
                [FromBody] RequestSendEmailDto sendMailRequest,
                [BindRequired, FromRoute] Guid accountId)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await _eMailService.SendMail(
                            new SendMailCommand(
                                    accountId,
                                    sendMailRequest.Identifier,
                                    sendMailRequest.ToReceiverEMail,
                                    sendMailRequest.Variables.ToDictionary(x=> x.Key,x=> x.Value))));
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpDelete("system/email/accounts/{accountId}")]
        public async Task<ActionResult> EMailAccountDelete([BindRequired, FromRoute] Guid accountId)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(await _eMailService.EMailAccountDelete(accountId));
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts/{accountId}/templates/add/{templateId}")]
        public async Task<ActionResult> EMailAccountAddTemplate(
                [BindRequired, FromRoute] Guid accountId,
                [BindRequired, FromRoute] Guid templateId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await _eMailService.EMailAccountAddTemplateAsync(
                            accountId,
                            templateId));
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/accounts/{accountId}/templates/remove/{templateId}")]
        public async Task<ActionResult> EMailAccountRemoveTemplate(
                [BindRequired, FromRoute] Guid accountId,
                [BindRequired, FromRoute] Guid templateId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await _eMailService.EMailAccountRemoveTemplate(
                            accountId,
                            templateId));
        }

        [ProducesResponseType(typeof(EmailTemplateInfo), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("system/email/templates/info")]
        public ActionResult EMailTemplatesGetInfo()
        {
            return ResultPresenter.GetResult(_eMailService.EMailTemplateGetInfo());
        }

        [ProducesResponseType(typeof(IEnumerable<EMailTemplateDetailsViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("system/email/templates")]
        public async Task<ActionResult> EMailTemplatesGetAll()
        {
            var result = await _eMailService.EMailTemplateGetAllAsync();
            return ResultPresenter.GetResult(result);
        }

        [ProducesResponseType(typeof(EMailTemplateDetailsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/templates")]
        public async Task<ActionResult> EMailTemplateCreate(
                [FromBody] RequestCreateEmailTemplateDto requestEmailTemplateCreateDto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            return ResultPresenter.GetResult(
                    await
                            _eMailService.EMailTemplateCreateAsync(
                                    requestEmailTemplateCreateDto.DisplayName,
                                    requestEmailTemplateCreateDto.Identifier,
                                    requestEmailTemplateCreateDto.VariableControlChar,
                                    requestEmailTemplateCreateDto.SubjectText,
                                    requestEmailTemplateCreateDto.ContentText,
                                    requestEmailTemplateCreateDto.ContentHtml));
        }

        [ProducesResponseType(typeof(EMailTemplateDetailsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/email/templates/html")]
        public async Task<ActionResult> EMailHtmlTemplateCreate(
        [FromForm] RequestCreateHtmlEmailTemplateDto requestEmailTemplateCreateDto,
        [SwaggerParameter(Description = "File with Text Content (UTF-8)", Required = true)] IFormFile textContent,
        [SwaggerParameter(Description = "File with Html Content (UTF-8)", Required = true)] IFormFile htmlContent)
        {
            return ResultPresenter.GetResult(
                    await
                            _eMailService.EMailTemplateCreateAsync(
                                    requestEmailTemplateCreateDto.DisplayName,
                                    requestEmailTemplateCreateDto.Identifier,
                                    requestEmailTemplateCreateDto.VariableControlChar,
                                    requestEmailTemplateCreateDto.SubjectText,
                                    ToEncodedString(textContent.OpenReadStream(), Encoding.UTF8),
                                    ToEncodedString(htmlContent.OpenReadStream(), Encoding.UTF8)));
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpDelete("system/email/templates/{templateId}")]
        public async Task<ActionResult> EMailTemplateDelete([BindRequired, FromRoute] Guid templateId)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(await _eMailService.EmailTemplateDelete(templateId));
        }

        /// <summary>
        /// Provides overview about Emails
        /// </summary>
        /// <param name="sendState">filter for states</param>
        /// <param name="take">Entries to return</param>
        /// <param name="skip">Entries to skip</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<EmailSendOrderViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("system/email")]
        public async Task<ActionResult> GetEmailsToSend(
                [FromQuery, Range(1, 300)] int take = 50,
                [FromQuery] int skip = 0,
                [FromQuery] EmailSendState? sendState = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetUserId(out var userId)) return BadRequest();
            if(sendState == null)
            {
                return ResultPresenter.GetResult(
                        await _eMailService.GetMailsToSend(
                                take,
                                skip));
            }
            return ResultPresenter.GetResult(
                    await _eMailService.GetMailsToSend(
                            sendState.Value,
                            take,
                            skip));
        }
        public static String ToEncodedString(Stream stream, Encoding enc = null)
        {
            enc = enc ?? Encoding.UTF8;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            string data = enc.GetString(bytes);
            return enc.GetString(bytes);
        }
    }
}