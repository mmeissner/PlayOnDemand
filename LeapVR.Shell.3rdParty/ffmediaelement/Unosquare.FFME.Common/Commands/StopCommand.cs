#region Licence
/****************************************************************
 *  Filename: StopCommand.cs
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
    using System;
    using Unosquare.FFME.Shared;

    /// <summary>
    /// The Stop Command Implementation
    /// </summary>
    /// <seealso cref="CommandBase" />
    internal sealed class StopCommand : CommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StopCommand"/> class.
        /// </summary>
        /// <param name="mediaCore">The media core.</param>
        public StopCommand(MediaEngine mediaCore)
            : base(mediaCore)
        {
            CommandType = CommandType.Stop;
            Category = CommandCategory.Priority;
        }

        /// <summary>
        /// Gets the command type identifier.
        /// </summary>
        public override CommandType CommandType { get; }

        /// <summary>
        /// Gets the command category.
        /// </summary>
        public override CommandCategory Category { get; }

        /// <summary>
        /// Performs the actions represented by this deferred task.
        /// </summary>
        protected override void PerformActions()
        {
            var m = MediaCore;
            m.Clock.Reset();
            m.State.UpdateMediaState(PlaybackStatus.Manual);
            var seek = new SeekCommand(m, TimeSpan.Zero);
            seek.Execute();
            m.State.UpdateMediaState(PlaybackStatus.Stop, m.WallClock);

            foreach (var renderer in m.Renderers.Values)
                renderer.Stop();
        }
    }
}
