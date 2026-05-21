#region Licence
/****************************************************************
 *  Filename: BehaviorConfig.cs
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
using System.Linq;
using System.Reflection;
using LeapVR.Shell.Controllers.Behavior;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Input.OpenVR;

namespace LeapVR.Shell.Controllers.FileConfig
{
    
    public class BehaviorConfig : ConfigObject
    {
        #region Fields & Properties
        public bool IsHmdInactivityLogoutEnabled { get; set; } = true;
        public TimeSpan HmdInactivityToLogout { get; set; } = TimeSpan.FromMinutes(5);

        public bool IsVrResetEnabled { get; set; } = true;
        public TimeSpan VrResetWaitBeforeStart { get; set; } = TimeSpan.FromSeconds(5);
        public bool IsQuitGameShortcutsEnabled { get; set; } = true;

        public ControllerShortcutCondition[] VrResetConditions { get; set; }
        public ControllerShortcutCondition[] QuitGameShortcuts { get; set; }

        #endregion

        #region Constructors

        public BehaviorConfig()
        {
            VrResetConditions = GetDefaultVrResetConditions();
            QuitGameShortcuts = GetDefaultQuitGameShortcuts();
        }
        #endregion

        #region Methods

        public override void Initialize()
        {
            if (!CheckWhetherConfigConditionsAreVaild(VrResetConditions))
            {
                VrResetConditions = GetDefaultVrResetConditions();
            }

            if (!CheckWhetherConfigConditionsAreVaild(QuitGameShortcuts))
            {
                QuitGameShortcuts = GetDefaultQuitGameShortcuts();
            }
        }

        private bool CheckWhetherConfigConditionsAreVaild(ControllerShortcutCondition[] conditions)
        {
            if (conditions == null)
            {
                return false;
            }

            if (!conditions.Any())
            {
                return false;
            }

            if (conditions.Any(c => c.KeyActions == null || !c.KeyActions.Any()))
            {
                return false;
            }

            return true;
        }

        private ControllerShortcutCondition[] GetDefaultVrResetConditions()
        {
            return new[]
            {
                new ControllerShortcutCondition
            {
                Scope = ConditionScope.NoSession | ConditionScope.SessionNoGame,
                KeyActions = new []
                {
                    new ControllerKeyAction
                    {
                        ControlerRole = 0, // 0 = Unknown, 1 = LeftHand, 2 = RightHand
                        ButtonId = 2, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                        State = ButtonState.Pressed,
                        TriggerTime = new TimeSpan(00,00,02),
                    },
                    new ControllerKeyAction
                    {
                        ControlerRole = 0, // 0 = Unknown, 1 = LeftHand, 2 = RightHand
                        ButtonId = 33, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                        State = ButtonState.Pressed,
                        TriggerTime = new TimeSpan(00,00,02),
                    },
                },
            },
                new ControllerShortcutCondition
            {
                Scope = ConditionScope.NoSession | ConditionScope.SessionNoGame,
                KeyActions = new []
                {
                    new ControllerKeyAction
                    {
                        ControlerRole = 1, // 0 = Unknown, 1 = LeftHand, 2 = RightHand
                        ButtonId = 2, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                        State = ButtonState.Pressed,
                        TriggerTime = new TimeSpan(00,00,02),
                    },
                    new ControllerKeyAction
                    {
                        ControlerRole = 1, // 0 = Unknown, 1 = LeftHand, 2 = RightHand
                        ButtonId = 33, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                        State = ButtonState.Pressed,
                        TriggerTime = new TimeSpan(00,00,02),
                    },
                },
            },
                new ControllerShortcutCondition
            {
                Scope = ConditionScope.NoSession | ConditionScope.SessionNoGame,
                KeyActions = new []
                {
                    new ControllerKeyAction
                    {
                        ControlerRole = 2, // 0 = Unknown, 1 = LeftHand, 2 = RightHand
                        ButtonId = 2, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                        State = ButtonState.Pressed,
                        TriggerTime = new TimeSpan(00,00,02),
                    },
                    new ControllerKeyAction
                    {
                        ControlerRole = 2, // 0 = Unknown, 1 = LeftHand, 2 = RightHand
                        ButtonId = 33, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                        State = ButtonState.Pressed,
                        TriggerTime = new TimeSpan(00,00,02),
                    },
                },
            },
            };
        }

        private ControllerShortcutCondition[] GetDefaultQuitGameShortcuts()
        {
            return new[]
            {
                new ControllerShortcutCondition
                {
                    Scope = ConditionScope.SessionGame,
                    KeyActions = new []
                    {
                        new ControllerKeyAction
                        {
                            ControlerRole = 1, // 1 = LeftHand, 2 = RightHand
                            ButtonId = 2, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                            State = ButtonState.Pressed,
                            TriggerTime = new TimeSpan(00,00,02),
                        },
                        new ControllerKeyAction
                        {
                            ControlerRole = 1, // 1 = LeftHand, 2 = RightHand
                            ButtonId = 1, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                            State = ButtonState.Pressed,
                            TriggerTime = new TimeSpan(00,00,02),
                        }
                    },
                },
                new ControllerShortcutCondition
                {
                    Scope = ConditionScope.SessionGame,
                    KeyActions = new []
                    {
                        new ControllerKeyAction
                        {
                            ControlerRole = 2, // 1 = LeftHand, 2 = RightHand
                            ButtonId = 2, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                            State = ButtonState.Pressed,
                            TriggerTime = new TimeSpan(00,00,02),
                        },
                        new ControllerKeyAction
                        {
                            ControlerRole = 2, // 1 = LeftHand, 2 = RightHand
                            ButtonId = 1, // 1 = ApplicationMenu, 2 = Grip, 32 = Touchpad, 33 = Trigger
                            State = ButtonState.Pressed,
                            TriggerTime = new TimeSpan(00,00,02),
                        }
                    },
                },
            };
        }

        #endregion

    }
}
