#region Licence
/****************************************************************
 *  Filename: DbSetup.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Pod.Data;
using Pod.Data.Config;
using Pod.Data.Models.Servers;
using Pod.Data.Models.Shell;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Email;
using Pod.ViewModels.Mail;

namespace Pod.Web.Center
{
    public class DbSetupUsers : IDbSetupTask
    {
        private readonly PodDbContext _dbContext;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ConfigSuperuser _superuserConfig;
        public DbSetupUsers(
                PodDbContext dbContext,
                RoleManager<ApplicationRole> roleManager,
                UserManager<ApplicationUser> userManager,
                ConfigSuperuser superuserConfig)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _superuserConfig = superuserConfig;
        }
        public int Priority => 1;
        public void Execute()
        {
            //Ensure Roles
            EnsureRoles();

            //Create Administrator
            var admin = CreateAdministrator();

            //Create Station for Admin
            CreateStation(_dbContext, admin);
        }

        private void EnsureRoles()
        {
            //Adding customs roles
            foreach(var roleName in RolesConfig.Roles)
            {
                // creating the roles and seeding them to the database
                var roleExist = _roleManager.RoleExistsAsync(roleName).Result;
                if(!roleExist)
                {
                    var result = _roleManager.CreateAsync(new ApplicationRole(roleName)).Result;
                }
            }
        }

        private ApplicationUser CreateAdministrator()
        {
            var superUser = _userManager.FindByNameAsync(_superuserConfig.Username).Result;
            //There must be a Superuser this is required as otherwise the system would be not manageable
            if(superUser == null)
            {
                superUser = new ApplicationUser
                            {
                                    UserName = _superuserConfig.Username,
                                    Email = _superuserConfig.Email
                            };

                var createAdminResult = _userManager.CreateAsync(superUser, _superuserConfig.Password).Result;
                if(createAdminResult.Succeeded)
                {
                    var addToRoleResult = _userManager.AddToRolesAsync(superUser, RolesConfig.Roles).Result;
                    var token = _userManager.GenerateEmailConfirmationTokenAsync(superUser).Result;
                    var confirmEmailResult = _userManager.ConfirmEmailAsync(superUser, token).Result;
                }
                else
                {
                    throw new Exception(
                            createAdminResult.Errors.Select(x => x.Description).
                                              Aggregate("", (current, s) => current + $"|{s}"));
                }
            }

            return superUser;
        }

        private void CreateStation(PodDbContext context, ApplicationUser superUser)
        {
            //Create a Station when there is none existing for the Superuser but one is configured
            if(!context.Stations.Any(x => x.ApplicationUserId == superUser.Id) &&
               _superuserConfig != null &&
               !string.IsNullOrWhiteSpace(_superuserConfig.StationPassword))
            {
                var stationResult = Station.Create(
                        superUser.Id,
                        "SuperUser Test Station",
                        _superuserConfig.StationPassword,
                        new PasswordHasher());
                if(stationResult.IsSuccess())
                {
                    context.Stations.Add(stationResult.ReturnValue);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception(stationResult.ToErrorString());
                }
            }
        }
    }

    public class DbSetupShellServer : IDbSetupTask
    {
        private readonly PodDbContext _dbContext;
        private readonly ConfigShellServer _shellServerConfig;
        public DbSetupShellServer(PodDbContext dbContext, ConfigShellServer shellServerConfig)
        {
            _dbContext = dbContext;
            _shellServerConfig = shellServerConfig;
        }

        public int Priority => 2;
        public void Execute()
        {
            //Checks if there is at least one Server defined
            var shellServer = _dbContext.Servers.FirstOrDefault();
            if(shellServer != null) return;

            //Create a default Server as configured if there is none 
            //and one was configured
            if(_shellServerConfig != null &&
               !string.IsNullOrWhiteSpace(_shellServerConfig.DisplayName) &&
               !string.IsNullOrWhiteSpace(_shellServerConfig.HostAddress) &&
               _shellServerConfig.Port > 0)
            {
                var serverResult = ShellServer.Create(
                        _shellServerConfig.DisplayName,
                        _shellServerConfig.HostAddress,
                        _shellServerConfig.Port,
                        _shellServerConfig.InterfaceVersion);

                //When a server was configured then we set it as active
                serverResult.ReturnValue.SetActive(true);
                if(serverResult.HasError())
                {
                    throw new Exception(serverResult.ToErrorString());
                }

                _dbContext.Add(serverResult.ReturnValue);
                _dbContext.SaveChanges();
            }
        }
    }

    public class DbSetupEmail : IDbSetupTask
    {
        private readonly EMailService _eMailService;
        public DbSetupEmail(EMailService eMailService) { _eMailService = eMailService; }
        public int Priority => 3;
        public void Execute()
        {
            //Ensure a Template for each identifier
            var templatesRequest = _eMailService.EMailTemplateGetAll();
            if(templatesRequest.HasError()) throw new InvalidOperationException(templatesRequest.ToErrorString());
            List<EMailTemplateDetailsViewModel> existingTemplates = new List<EMailTemplateDetailsViewModel>();
            List<EMailTemplateDetailsViewModel> createdTemplates = new List<EMailTemplateDetailsViewModel>();
            if(templatesRequest.ReturnValue != null) existingTemplates.AddRange(templatesRequest.ReturnValue);

            bool foundAnyTemplate = false;
            foreach(EMailTemplateIdentifier identifier in Enum.GetValues(typeof(EMailTemplateIdentifier)))
            {
                //Template exists
                if(existingTemplates.Any(x => x.Identifier == identifier))
                {
                    foundAnyTemplate = true;
                    continue;
                }
                //We ensure for each Identifier an default template
                var defaultTemplate = GetDefaultTemplate(identifier);
                var createdTemplateResult = _eMailService.EMailTemplateCreate(
                        defaultTemplate.DisplayName,
                        identifier,
                        defaultTemplate.VariableControlChar,
                        defaultTemplate.Subject,
                        defaultTemplate.Content,
                        defaultTemplate.ContentHtml);
                if(createdTemplateResult.HasError()) throw new InvalidOperationException(createdTemplateResult.ToErrorString());
                createdTemplates.Add(createdTemplateResult.ReturnValue);
            }
            //If any template already existed then it's not a new setup and we stop any further operation
            if(foundAnyTemplate)return;

            //Check for an E-Mail, if any exists then it's also not a new setup
            var emailAccounts = _eMailService.EmailAccountGetAll();
            if(emailAccounts.HasError()) throw new InvalidOperationException(emailAccounts.ToErrorString());
            if(emailAccounts.ReturnValue != null && emailAccounts.ReturnValue.Any())return;

            //Create an E-Mail Account and add the templates
            var newAccountResult = _eMailService.EMailAccountCreate("Default E-Mail Account");
            if(newAccountResult.HasError()) throw new InvalidOperationException(newAccountResult.ToErrorString());

            //Add the Templates to this Account
            foreach(EMailTemplateDetailsViewModel template in createdTemplates)
            {
                var assignTemplateResult = _eMailService.EMailAccountAddTemplate(newAccountResult.ReturnValue.Id, template.Id);
                if(assignTemplateResult.HasError()) throw new InvalidOperationException(assignTemplateResult.ToErrorString());
            }
        }


        public (string DisplayName, char VariableControlChar, string Subject, string
                Content, string ContentHtml) GetDefaultTemplate(EMailTemplateIdentifier identifier)
        {
            switch(identifier)
            {
                case EMailTemplateIdentifier.RegisterAccount:
                    return (DisplayName: "Default - Register Account",
                            VariableControlChar: '%',
                            Subject: "Welcome %Username% to Leap Play",
                            Content:
                            $"Before you can use your account you will have to confirm your e-mail address.{Environment.NewLine}" +
                            $"You can use following link:{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"%EMailVerificationTokenLink%{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"As alternative you can use our systems API and confirm you password with the following token:{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"%EMailVerificationToken%{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            "Thank you for joining!",
                            ContentHtml: null);
                case EMailTemplateIdentifier.ResendEMailVerification:
                    return (DisplayName: "Default - Resend EMail Verification",
                            VariableControlChar: '%',
                            Subject: "Leap Play Email Verification",
                            Content:
                            $"Someone requested to resend a Email verification for the account: %Username%.{Environment.NewLine}" +
                            $"Please ignore this message if this wasn't you, otherwise use our systems API{Environment.NewLine}" +
                            $"to verify your e-mail with the token:{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"%EMailVerificationToken%{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            "Have a nice day!",
                            ContentHtml: null);
                case EMailTemplateIdentifier.ResetPassword:
                    return (DisplayName: "Default - Forgot Password",
                            VariableControlChar: '%',
                            Subject: "Password Reset Request for Leap Play",
                            Content:
                            $"Someone requested a password reset for your account: %Username%.{Environment.NewLine}" +
                            $"Please ignore this message if this wasn't you. You can use following link to reset your password{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"%PasswordResetTokenLink%{Environment.NewLine}" +
                            $"{Environment.NewLine}" +
                            $"As alternative you can use our systems API to reset your password with the following token:" +
                            $"{Environment.NewLine}" +
                            $"{Environment.NewLine}%PasswordResetToken%" +
                            $"{Environment.NewLine}" +
                            "Have a nice day!",
                            ContentHtml: null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(identifier), identifier, null);
            }
        }
    }
}