#region Licence
/****************************************************************
 *  Filename: FirewallController.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Controllers;
using NetFwTypeLib;
using NLog;

// ReSharper disable ExplicitCallerInfoArgument

namespace LeapVR.Shell.Controllers.Firewall
{
    public class FirewallController : IFirewallController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _firewallLock = new object();

        private readonly IDiskController _diskController;
        private readonly INetFwPolicy2 _firewallPolicy;
        private INetFwRule[] _rules = null;

        public FirewallController(IDiskController diskController)
        {
            QuickLeap.AssertNotNull(diskController);
            _diskController = diskController;
            _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
        }


        public async Task<FirewallState> GetFirewallStateAsync(Guid applicationGuid)
        {
            return await Task.Factory.StartNew(() => GetFirewallStateInternal(applicationGuid));
        }

        public void SetFirewallState(Guid applicationGuid, FirewallState newState)
        {
            try
            {
                var appBaseDirLower =
                        _diskController.GetApplicationStorageDirectory(applicationGuid).ToLowerInvariant();

                lock(_firewallLock)
                {
                    var currentRules = GetApplicationRules(appBaseDirLower).ToArray();
                    Logger.Debug(
                            $"Got rules for applicationGuid = `{applicationGuid}`, appBaseDirLower = `{appBaseDirLower}`",currentRules);
                    RemoveRules(currentRules);

                    var allAppExecutable = GetApplicationExecutables(appBaseDirLower).ToArray();
                    Logger.Debug(
                            $"Got allAppExecutables for applicationGuid = `{applicationGuid}`, appBaseDirLower = `{appBaseDirLower}`",allAppExecutable);
                    foreach(var executablePath in allAppExecutable)
                    {
                        var inboundRule = CreateExecutableRule(
                                applicationGuid,
                                executablePath,
                                FirewallDirection.Inbound,
                                newState);
                        var outboundRule = CreateExecutableRule(
                                applicationGuid,
                                executablePath,
                                FirewallDirection.Outbound,
                                newState);

                        _firewallPolicy.Rules.Add(inboundRule);
                        _firewallPolicy.Rules.Add(outboundRule);
                        Logger.Debug(
                                $"Setting of new rules (Inbound & Outbound) for executablePath = `{executablePath}`: newState = `{newState}` in DONE.");
                    }

                    _rules = GetApplicationRules().ToArray();
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, $"applicationGuid={applicationGuid};newState={newState}");
            }
        }

        public void RemoveAllRules(Guid applicationGuid)
        {
            try
            {
                var appBaseDirLower =
                        _diskController.GetApplicationStorageDirectory(applicationGuid);
                if(appBaseDirLower == null)
                {
                    Logger.Warn("Could not receive application base directory");
                    return;
                }
                appBaseDirLower = appBaseDirLower.ToLowerInvariant();

                lock(_firewallLock)
                {
                    var currentRules = GetApplicationRules(appBaseDirLower).ToArray();
                    Logger.Debug(
                            $"Got rules for applicationGuid = `{applicationGuid}`, appBaseDirLower = `{appBaseDirLower}`",currentRules);
                    RemoveRules(currentRules);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, $"applicationGuid={applicationGuid};");
            }
        }
        private FirewallState GetFirewallStateInternal(Guid applicationGuid)
        {
            try
            {
                if(_rules == null)
                {
                    lock(_firewallLock)
                    {
                        if(_rules == null) _rules = GetApplicationRules().ToArray();
                    }
                }

                var appBaseDirLower =
                        _diskController.GetApplicationStorageDirectory(applicationGuid).ToLowerInvariant();
                int ruleCount = 0;
                int allowTrafficRules = 0;
                int allowMaxTrafficRules = 0;
                int blockTrafficRules = 0;


                foreach(INetFwRule rule in _rules)
                {
                    if(rule.ApplicationName == null ||
                       !rule.ApplicationName.ToLowerInvariant().StartsWith(appBaseDirLower)) continue;
                    ruleCount++;
                    switch(rule.Action)
                    {
                        case NET_FW_ACTION_.NET_FW_ACTION_BLOCK:
                            blockTrafficRules++;
                            break;
                        case NET_FW_ACTION_.NET_FW_ACTION_ALLOW:
                            allowTrafficRules++;
                            break;
                        case NET_FW_ACTION_.NET_FW_ACTION_MAX:
                            allowMaxTrafficRules++;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if(ruleCount == 0)
                {
                    // When no rules found report that app is allowed.
                    Logger.Warn(
                            $"No rules found for applicationGuid = `{applicationGuid}`; Returning `AllTraficAllowed` response.");
                    return FirewallState.AllTrafficAllowed;
                }

                if(allowTrafficRules == ruleCount)
                {
                    return FirewallState.AllTrafficAllowed;
                }

                if(blockTrafficRules == ruleCount)
                {
                    return FirewallState.NoTrafficAllowed;
                }

                // If both allow & block rules are present for this specific applicationGuid we are not so smart to figure out what is going on, so return unknown.
                Logger.Warn(
                        $"Mixed rules found for applicationGuid = `{applicationGuid}`; Returning `Mixed` response.");
                return FirewallState.Mixed;
            }
            catch(Exception e)
            {
                Logger.Error( e, $"ApplicationGUID : {applicationGuid}");
                return FirewallState.Unknown;
            }
        }

        private void RemoveRules(IEnumerable<INetFwRule> rules)
        {
            // must be executed in lock(_firewallLock) context

            foreach(var currentRule in rules)
            {
                Logger.Debug("Removing currentRule",currentRule);
                _firewallPolicy.Rules.Remove(currentRule.Name);
            }
        }

        private IEnumerable<INetFwRule> GetApplicationRules(string appBaseDirLower = null)
        {
            // must be executed in lock(_firewalllock) context
            if(string.IsNullOrEmpty(appBaseDirLower))
            {
                return _firewallPolicy.Rules.OfType<INetFwRule>();
            }

            return _firewallPolicy.Rules.OfType<INetFwRule>().
                                   Where(
                                           q => q.ApplicationName != null &&
                                                q.ApplicationName.ToLowerInvariant().StartsWith(appBaseDirLower));
        }

        private INetFwRule CreateExecutableRule(
                Guid applicationGuid,
                string executablePath,
                FirewallDirection direction,
                FirewallState state)
        {
            // must be executed in lock(_firewalllock) context

            INetFwRule2 newRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            newRule.Action = FirewallStateToAction(state);
            newRule.ApplicationName = executablePath;
            newRule.Description = $"LeapVR - {applicationGuid} - {executablePath} - {(int)direction} - {(int)state}";
            newRule.Direction = FirewallDirectionToDirection(direction);
            newRule.Enabled = true;
            newRule.Grouping = GlobalConfig.FirewallGroupName;
            newRule.InterfaceTypes = "All";
            newRule.LocalAddresses = "*";
            //newRule.LocalPorts = null;
            newRule.Name =
                    $"{GlobalConfig.FirewallGroupName} - {applicationGuid} - {executablePath} - {(int)direction} - {(int)state}";

            return newRule;
        }

        private IEnumerable<string> GetApplicationExecutables(string appBaseDirLower)
        {
            return Directory.GetFiles(appBaseDirLower, "*.exe", SearchOption.AllDirectories);
        }

        private NET_FW_ACTION_ FirewallStateToAction(FirewallState state)
        {
            switch(state)
            {
                case FirewallState.AllTrafficAllowed:
                    return NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                case FirewallState.NoTrafficAllowed:
                    return NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                default:
                    throw new ArgumentException($"Unsupported value of state = `{state}`.");
            }
        }

        private NET_FW_RULE_DIRECTION_ FirewallDirectionToDirection(FirewallDirection direction)
        {
            switch(direction)
            {
                case FirewallDirection.Inbound:
                    return NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                case FirewallDirection.Outbound:
                    return NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                default:
                    throw new ArgumentException($"Unsupported value of direction = `{direction}`.");
            }
        }
    }
}