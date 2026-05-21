#region Licence
/****************************************************************
 *  Filename: MediaInitializingEventArgs.cs
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
namespace Unosquare.FFME.Events
{
    using Shared;
    using System;
    using System.Windows;

    /// <summary>
    /// Represents the event arguments of the MediaInitializing routed event.
    /// </summary>
    /// <seealso cref="RoutedEventArgs" />
    public class MediaInitializingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaInitializingEventArgs" /> class.
        /// </summary>
        /// <param name="config">The container configuration options.</param>
        /// <param name="url">The URL.</param>
        public MediaInitializingEventArgs(ContainerConfiguration config, string url)
        {
            Configuration = config;
            Url = url;
        }

        /// <summary>
        /// Set or change the container configuration options before the media is opened.
        /// </summary>
        public ContainerConfiguration Configuration { get; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        public string Url { get; }
    }
}
