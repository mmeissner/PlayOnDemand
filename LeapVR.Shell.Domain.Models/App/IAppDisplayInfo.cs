#region Licence
/****************************************************************
 *  Filename: IAppDisplayInfo.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-6
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
using System.ComponentModel;
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.App
{

    /// <summary>
    /// Represent a collection of information for an application can be displayed mainly in the UI.
    /// </summary>
    public interface IAppDisplayInfo: INotifyPropertyChanged
    {
        /// <summary>
        /// Guid of application the display data is connected to.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Get application name to be displayed.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the category the application belongs to.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        IAppCategory Category { get; }

        /// <summary>
        /// Get application tags
        /// </summary>
        string[] Tags { get; }

        /// <summary>
        /// Get or set application description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Get the application thumbnail
        /// </summary>
        byte[] Thumbnail { get; }

        bool IsSupportScreen { get; }
        bool IsSupportVirtualReality { get; }
        IAppDisplayUpdate GetAppDisplayUpdate();
    }
}
