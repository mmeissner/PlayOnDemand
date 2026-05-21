#region Licence
/****************************************************************
 *  Filename: StepScreen.cs
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
using System.Threading.Tasks;
using Caliburn.Micro;

namespace LeapVR.ContentCreator.UI.ViewModels
{
    public class StepScreen : Screen, IStepScreen
    {
        public virtual IStepScreen Previous { get; set; }
        public virtual IStepScreen Next { get; set; }
        public virtual int StepOrder => 0;
        public virtual PackageCreation PackageCreation { get; } = null;
    }
}
