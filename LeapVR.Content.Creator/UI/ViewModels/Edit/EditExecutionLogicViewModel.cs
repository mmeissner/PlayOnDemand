#region Licence
/****************************************************************
 *  Filename: EditExecutionLogicViewModel.cs
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
using LeapVR.Content.Creator.Logic;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    /// <summary>
    /// Placeholder view-model for the (currently empty) Execution Logic edit
    /// view. Not instantiated by ShellViewModel today; kept so the matching
    /// XAML still has a code-behind it can resolve. When the edit wizard
    /// grows a per-execution-instruction editor, this is where it lands -
    /// source the data from
    /// <see cref="LeapVrContainerEditor.PlatformData"/>.ExecutionLogicInstructions.
    /// </summary>
    public class EditExecutionLogicViewModel : ValidatingScreen
    {
        public LeapVrContainerEditor ContainerEditor { get; }

        public EditExecutionLogicViewModel(LeapVrContainerEditor containerEditor)
        {
            ContainerEditor = containerEditor;
        }
    }
}
