#region Licence
/****************************************************************
 *  Filename: EmailContentTemplate.cs
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
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;
using Pod.Enums;

namespace Pod.Data.Models.Mail
{
    /// <summary>
    /// Template for sending emails to users
    /// </summary>
    public class EmailContentTemplate
    {
        private HashSet<EmailVariable> _variables;
        private HashSet<EMailAccountDataEMailContentTemplate> _emailAccounts;
        private EmailContentTemplate() { }
        private EmailContentTemplate(
                string displayName,
                EMailTemplateIdentifier identifier,
                HashSet<EmailVariable> variables,
                string subjectText,
                string contentText,
                string contentHtml,
                char variableControlChar)
        {
            DisplayName = displayName;
            Identifier = identifier;
            SubjectText = subjectText;
            ContentText = contentText;
            ContentHtml = contentHtml;
            VariableControlChar = variableControlChar;
            _variables = variables;
        }
        
        /// <summary>
        /// The Id of this instance
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// A display name for this template
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// A string identifier for an purpose as registration, forgotPassword and so on
        /// </summary>
        public EMailTemplateIdentifier Identifier { get; private set; }

        /// <summary>
        /// The Subject for the Mail
        /// </summary>
        public string SubjectText { get; private set; }

        /// <summary>
        /// Escape character for Variables that is set at start and end 
        /// </summary>
        public char VariableControlChar { get; private set; }
        
        /// <summary>
        /// The Variables identified in the Template
        /// </summary>
        public IReadOnlyCollection<EmailVariable> Variables => _variables;

        /// <summary>
        /// The Email Accounts this Template is linked to
        /// </summary>
        public IReadOnlyCollection<EMailAccountDataEMailContentTemplate> EmailAccounts => _emailAccounts;

        /// <summary>
        /// The Text Body for the Mail
        /// </summary>
        public string ContentText { get; private set; }

        /// <summary>
        /// The Html Body for the Mail
        /// </summary>
        public string ContentHtml { get; private set; }

        /// <summary>
        /// Creates an new Template
        /// </summary>
        /// <param name="variableParser">Parser for Variables</param>
        /// <param name="displayName">Name for Template</param>
        /// <param name="identifier">Identifier for usage</param>
        /// <param name="variableControlChar">Control character for variables</param>
        /// <param name="subject">Subject Text</param>
        /// <param name="content">Email Text</param>
        /// <param name="contentHtml">Email Html Body</param>
        /// <returns>result</returns>
        public static IResult<EmailContentTemplate> Create(
                IVariableParser variableParser,
                string displayName,
                EMailTemplateIdentifier identifier,
                char variableControlChar,
                string subject,
                string content,
                string contentHtml)
        {
            var retval = new Result<EmailContentTemplate>();
            bool hasContent = false;
            bool hasContentHtml = false;
            retval.ArgNotNullOrWhitespace(displayName, nameof(displayName), UserError.EMailTemplateDataInvalid);
            retval.ArgNotNull(variableParser, nameof(variableParser), UserError.EMailTemplateDataInvalid);
            retval.ArgNotNull(subject, nameof(subject), UserError.EMailTemplateDataInvalid);
            if(!string.IsNullOrWhiteSpace(content)) hasContent = true;
            if(!string.IsNullOrWhiteSpace(contentHtml)) hasContentHtml = true;
            retval.ArgTrue(hasContent || hasContentHtml, "Email Content", UserError.EMailTemplateDataInvalid);
            if (retval.HasError()) return retval;

            HashSet<EmailVariable> hashVariables = null;
            retval.Add(ProcessVars(EmailVariableType.Subject, ref hashVariables, variableParser.Parse(subject, variableControlChar)));
            if(hasContent)retval.Add(ProcessVars(EmailVariableType.Content, ref hashVariables, variableParser.Parse(content, variableControlChar)));
            if(hasContentHtml)retval.Add(ProcessVars(EmailVariableType.ContentHtml, ref hashVariables, variableParser.Parse(contentHtml, variableControlChar)));
            if(retval.HasError()) return retval;

            var template = new EmailContentTemplate(
                    displayName,
                    identifier,
                    hashVariables,
                    subject,
                    content,
                    contentHtml,
                    variableControlChar);
            if(retval.HasError()) return retval;
            return retval.Add(template);
        }
        
        private static IResult ProcessVars(EmailVariableType type, ref HashSet<EmailVariable> hashVariables,ICollection<IContentTemplateVariable> variables)
        {
            var result = new Result();
            if(variables.Any())
            {
                if(hashVariables == null)hashVariables = new HashSet<EmailVariable>();
                foreach(IContentTemplateVariable subjectVar in variables)
                {
                    var resultSubjectVar = EmailVariable.Create(
                            type,
                            subjectVar.VariableKey,
                            subjectVar.StartChar,
                            subjectVar.Length);
                    if(resultSubjectVar.HasError()) result.Add(resultSubjectVar);
                    else
                    {
                        hashVariables.Add(resultSubjectVar.ReturnValue);
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Variable in an Email Template
    /// </summary>
    public class EmailVariable : TemplateVariable
    {
        private EmailVariable(EmailVariableType type, TemplateVariableKey variableKey,
                string variableKeyString,
                int startChar, int length) : base(
                variableKey,
                variableKeyString,
                startChar,
                length)
        {
            Type = type;
        }

        /// <summary>
        /// Location this Variable is Included
        /// </summary>
        public EmailVariableType Type { get; private set; }
        public static IResult<EmailVariable> Create(EmailVariableType type, string variableKey, int startChar, int length)
        {
            var retval = new Result<EmailVariable>();
            retval.ArgNotNullOrWhitespace(variableKey, nameof(variableKey));
            if(!Enum.TryParse<TemplateVariableKey>(variableKey, true, out var templateVariable))
            {
                retval.Add(
                        $"The template variable {variableKey} is not known to the system!",
                        UserError.TemplateInvalidVariable);
            }

            retval.ArgNotLowerThen(length, nameof(length), 3, "minimum length of a variable with control chars");
            if(retval.HasError()) return retval;
            return retval.Add(new EmailVariable(type, templateVariable, variableKey, startChar, length));
        }
    }

    /// <summary>
    /// A Variable that can be replaced
    /// </summary>
    public class TemplateVariable
    {
        protected TemplateVariable() { }
        protected TemplateVariable(TemplateVariableKey variableKey, string variableKeyString,
                int startChar, int length)
        {
            VariableKey = variableKey;
            VariableKeyString = variableKeyString;
            StartChar = startChar;
            Length = length;
        }
        public Guid Id { get; private set; }
        public Guid EmailContentTemplateId { get; private set; }
        /// <summary>
        /// The Variable Key as Enum
        /// </summary>
        public TemplateVariableKey VariableKey { get; private set; }
        /// <summary>
        /// The Template Variable as String at the Time the template gets persisted
        /// This allows us to rename the enums without breaking older templates to work
        /// </summary>
        public string VariableKeyString { get; private set; }
        /// <summary>
        /// Position of the first character in text
        /// </summary>
        public int StartChar { get; private set; }

        /// <summary>
        /// Length of the Variable 
        /// </summary>
        public int Length { get; private set; }
    }
}