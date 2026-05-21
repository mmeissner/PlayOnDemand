#region Licence
/****************************************************************
 *  Filename: WindowData.cs
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
using LeapVR.Shell.Modules.Interfaces.Utilities.WinApi;

namespace LeapVR.Shell.Managers.Window
{
    public class WindowData : IWindowData
    {
        #region Properties & Fields

        public string WindowClassName { get; internal set; }

        public int PosX { get; internal set; }
        public int PosY { get; internal set; }

        public int Width { get; internal set; }
        public int Height { get; internal set; }

        #endregion Properties & Fields

        #region Constructors

        internal WindowData()
        {
            //
        }

        #endregion Constructors

        #region Methods

        //

        #endregion Methods
    }
}
