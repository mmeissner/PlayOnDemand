#region Licence
/****************************************************************
 *  Filename: DirectCommandBase.cs
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
namespace Unosquare.FFME.Commands
{
    /// <summary>
    /// Represents a promise-style media command that is executed directly on the
    /// media command manager.
    /// </summary>
    /// <seealso cref="CommandBase" />
    internal abstract class DirectCommandBase : CommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectCommandBase"/> class.
        /// </summary>
        /// <param name="mediaCore">The media core.</param>
        public DirectCommandBase(MediaEngine mediaCore)
            : base(mediaCore)
        {
            Category = CommandCategory.Direct;
        }

        /// <summary>
        /// Gets the command category.
        /// </summary>
        public override CommandCategory Category { get; }

        /// <summary>
        /// Performs actions when the command has been executed.
        /// This is useful to notify exceptions or update the state of the media.
        /// </summary>
        public abstract void PostProcess();
    }
}
