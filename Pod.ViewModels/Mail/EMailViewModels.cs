#region Licence
/****************************************************************
 *  Filename: EMailViewModels.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Pod.Enums;

namespace Pod.ViewModels.Mail
{
    public class EmailTemplateInfo
    {
        public IEnumerable<string> Identifiers { get; set; }
        public IEnumerable<string> VariableKeys { get; set; }

    }

    public class EMailAccountViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; }
        public string SenderName { get; set; }
        public string EMailAddress { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; }
        public SmtpAuthentication AuthMethod { get; set; }
        public string Username { get; set; }
        public IEnumerable<EMailTemplateViewModel> AssignedTemplates { get; set; }

    }

    public class EMailTemplateViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public EMailTemplateIdentifier Identifier { get; set; }
    }


    public class EMailTemplateDetailsViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public EMailTemplateIdentifier Identifier { get; set; }
        public char VariableControlChar { get; set; }
        public string SubjectText { get; set; }
        public string ContentText { get; set; }
        public string ContentHtml { get; set; }
        public IEnumerable<EMailVariableViewModel> Variables { get; set; }

    }

    public class EMailVariableViewModel
    {
        public EmailVariableType Type { get; set; }
        public string VariableKeyString { get; set; }
        public int StartChar { get; set; }
        public int Length { get;set; }
    }

    public class EmailSendOrderViewModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedOnUtc { get;  set; }
        public DateTime LastSendAttemptUtc { get;  set; }
        public EMailTemplateIdentifier TemplateIdentifier { get;  set; }
        public IEnumerable<EMailReceiverViewModel> Receivers { get; set; }
        public IEnumerable<EMailVariableValueViewModel> Variables { get; set; }
        public EmailSendState SendState { get; set; }
        public uint SendAttempts { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class EMailReceiverViewModel
    {
        public EmailReceiverType Type { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
    }

    public class EMailVariableValueViewModel
    {
        public TemplateVariableKey Key { get; set; }
        public string Value { get; set; }
    }
}
