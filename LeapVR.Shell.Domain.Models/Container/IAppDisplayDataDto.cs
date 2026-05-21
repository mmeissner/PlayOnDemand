#region Licence
/****************************************************************
 *  Filename: IAppDisplayDataDto.cs
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
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Contains display releated data of one application.
    /// </summary>
    
    public interface IAppDisplayDataDto
    {
        /// <summary>
        /// Guid of application the display data is connected to.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Get or set application name to be displayed.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Get or set application category identifier.
        /// </summary>
        string Category { get; set; }

        string[] Tags { get; set; }

        /// <summary>
        /// Get or set application description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Points to picture file representing the game.
        /// </summary>
        IDiskEntityDto MainPicture { get;  }
    }
}