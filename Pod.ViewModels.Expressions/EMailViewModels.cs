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
using System.Linq;
using System.Linq.Expressions;
using Pod.Data.Models.Mail;
using Pod.ViewModels.Mail;

namespace Pod.ViewModels.Expressions
{
    public static class ToEMailAccountVm
    {
        public static readonly Func<EMailAccountData, EMailAccountViewModel> FuncFromEmailAccountData =
                FromEmailAccountData().Compile();
        private static Expression<Func<EMailAccountData, EMailAccountViewModel>> FromEmailAccountData()
        {
            return x => new EMailAccountViewModel
                        {
                                Id = x.Id,
                                DisplayName = x.DisplayName,
                                IsEnabled = x.IsEnabled,
                                SenderName = x.SenderName,
                                EMailAddress = x.EmailAddress,
                                SmtpServer = x.SmtpServer,
                                SmtpPort = x.SmtpPort,
                                UseSsl = x.UseSsl,
                                AuthMethod = x.AuthMethod,
                                Username = x.Username,
                                AssignedTemplates = x.EmailContentTemplates.Any() ?
                                        x.EmailContentTemplates.Select(
                                                  u => ToEmailTemplateVm.FuncToEMailTemplateViewModel(
                                                          u.EmailContentTemplate)) : null
                        };
        }
    }

    public static class ToEmailTemplateVm
    {
        public static readonly Func<EmailContentTemplate, EMailTemplateViewModel> FuncToEMailTemplateViewModel =
                ToEMailTemplateViewModel().Compile();
        private static Expression<Func<EmailContentTemplate, EMailTemplateViewModel>> ToEMailTemplateViewModel()
        {
            return x => new EMailTemplateViewModel
                        {
                                Id = x.Id,
                                DisplayName = x.DisplayName,
                                Identifier = x.Identifier
                        };
        }
    }

    public static class ToEmailTemplateDetailsVm
    {
        public static readonly Func<EmailContentTemplate, EMailTemplateDetailsViewModel>
                FuncToEmailContentTemplateDetailsViewModel = ToEmailContentTemplateDetailsViewModel().Compile();

        private static Expression<Func<EmailContentTemplate, EMailTemplateDetailsViewModel>>
                ToEmailContentTemplateDetailsViewModel()
        {
            return x => new EMailTemplateDetailsViewModel
                        {
                                Id = x.Id,
                                DisplayName = x.DisplayName,
                                Identifier = x.Identifier,
                                VariableControlChar = x.VariableControlChar,
                                SubjectText = x.SubjectText,
                                ContentText = x.ContentText,
                                ContentHtml = x.ContentHtml,
                                Variables = x.Variables.Any() ?
                                        x.Variables.Select(ToEmailVariablesVm.FuncToEmailVariableViewModel) :
                                        null
                        };
        }
    }

    public static class ToEmailVariablesVm
    {
        public static readonly Func<EmailVariable, EMailVariableViewModel> FuncToEmailVariableViewModel =
                ToEmailVariableViewModel().Compile();

        private static Expression<Func<EmailVariable, EMailVariableViewModel>> ToEmailVariableViewModel()
        {
            return x => new EMailVariableViewModel
                        {
                                Type = x.Type,
                                VariableKeyString = x.VariableKeyString,
                                Length = x.Length,
                                StartChar = x.StartChar
                        };
        }
    }

    public static class EMailSendOrderVm
    {
        public static readonly Func<EmailSendOrder, EmailSendOrderViewModel> FuncToEmailSendOrderViewModel = ToEmailSendOrderViewModel().Compile();
        private static Expression<Func<EmailSendOrder, EmailSendOrderViewModel>> ToEmailSendOrderViewModel()
        {
            return x => new EmailSendOrderViewModel
                        {
                                CreatedOnUtc = x.CreatedOnUtc,
                                ErrorMsg = x.ErrorMsg,
                                Id = x.Id,
                                LastSendAttemptUtc = x.LastSendAttemptUtc,
                                Receivers = x.Receivers.Any() ?
                                        x.Receivers.Select(EMailReceiverVm.FuncToEmailReceiverViewModel) : null,
                                SendAttempts = x.SendAttempts,
                                SendState = x.SendState,
                                TemplateIdentifier = x.TemplateIdentifier,
                                Variables = x.Variables.Any() ?
                                        x.Variables.Select(EMailVariableValueVm.FuncToEmailVariableViewModel) : null

                        };
        }
    }

    public static class EMailReceiverVm
    {
        public static readonly Func<EMailReceiver, EMailReceiverViewModel> FuncToEmailReceiverViewModel =
                ToEmailReceiverViewModel().Compile();

        private static Expression<Func<EMailReceiver, EMailReceiverViewModel>> ToEmailReceiverViewModel()
        {
            return x => new EMailReceiverViewModel
                        {
                                EmailAddress = x.EmailAddress,
                                Name = x.Name,
                                Type = x.Type,
                        };
        }
    }

    public static class EMailVariableValueVm
    {
        public static readonly Func<EMailVariableValue, EMailVariableValueViewModel> FuncToEmailVariableViewModel =
                ToEmailVariableValueViewModel().Compile();

        private static Expression<Func<EMailVariableValue, EMailVariableValueViewModel>> ToEmailVariableValueViewModel()
        {
            return x => new EMailVariableValueViewModel
                        {
                                Key = x.Key,
                                Value = x.Value,
                        };
        }
    }
}