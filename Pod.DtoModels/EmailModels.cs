#region Licence
/****************************************************************
 *  Filename: EmailModels.cs
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pod.Enums;

namespace Pod.DtoModels
{
    public class RequestCreateEmailAccountDto
    {
        [Required, MinLength(5), MaxLength(30)]
        public string DisplayName { get; set; }
        [MinLength(5), MaxLength(30)]
        public string SenderName { get; set; }
        [Required(), EmailAddress()]
        public string EMailAddress { get; set; }
        [Required, MinLength(5), MaxLength(30)]
        public string SmtpServer { get; set; }
        [Required,Range(1, 65535)]
        public int SmtpPort { get; set; }
        [Required]
        public bool UseSsl { get; set; }
        [Required]
        public SmtpAuthentication AuthMethod { get; set; }
        [Required,MinLength(1), MaxLength(200)]
        public string Username { get; set; }
        [Required,MinLength(1),MaxLength(200)]
        public string Password { get; set; }
    }

    public class RequestSetSmtpAuthSettingsDto
    {
        [Required, MinLength(1), MaxLength(200)]
        public string Username { get; set; }
        [Required, MinLength(1), MaxLength(200)]
        public string Password { get; set; }
        public SmtpAuthentication AuthMethod { get; set; }
    }

    public class RequestSetSmtpServerDto
    {
        [MinLength(1)]
        public string SmtpServer { get; set; }
        [Required, Range(1, 65535)]
        public int SmtpPort { get; set; }
        [Required]
        public bool UseSsl { get; set; }
    }

    public class RequestSetEmailSenderDto
    {
        public string SenderName { get; set; }
        [Required(), EmailAddress()]
        public string EMailAddress { get; set; }
    }
    public class RequestSendEmailDto
    {
        [Required]
        public EMailTemplateIdentifier Identifier { get; set; }
        [Required(), EmailAddress()]
        public string ToReceiverEMail { get; set; }
        public string[] CcReceiverEMails { get; set; }
        public string[] BccReceiverEMails { get; set; }
        public ICollection<TemplateVariable> Variables { get; set; }

        public class TemplateVariable
        {
            [Required()]
            public TemplateVariableKey Key { get; set; }
            [Required, MinLength(1), MaxLength(1000)]
            public string Value { get; set; }
        }
    }


    public class RequestCreateEmailTemplateDto
    {
        [Required, MinLength(5), MaxLength(80)]
        public string DisplayName { get; set; }
        [Required()]
        public EMailTemplateIdentifier Identifier { get; set; }
        [Required()]
        public char VariableControlChar { get; set; }
        [Required, MinLength(1), MaxLength(1000)]
        public string SubjectText { get; set; }
        public string ContentText { get; set; }
        public string ContentHtml { get; set; }
    }
    public class RequestCreateHtmlEmailTemplateDto
    {
        [Required, MinLength(5), MaxLength(80)]
        public string DisplayName { get; set; }
        [Required()]
        public EMailTemplateIdentifier Identifier { get; set; }
        [Required()]
        public char VariableControlChar { get; set; }
        [Required, MinLength(1), MaxLength(1000)]
        public string SubjectText { get; set; }
    }
}
