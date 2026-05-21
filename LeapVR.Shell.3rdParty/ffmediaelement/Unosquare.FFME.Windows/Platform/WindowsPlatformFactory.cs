#region Licence
/****************************************************************
 *  Filename: WindowsPlatformFactory.cs
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
using System.Windows.Threading;
using Unosquare.FFME.Shared;

namespace Unosquare.FFME.Platform
{
    internal class WindowsPlatformFactory : IPlatformFactory
    {
        public IDispatcherTimer CreateDispatcherTimer(ActionPriority priority)
        {
            return new WindowsDispatcherTimer((DispatcherPriority)priority);
        }

        public IMediaEventConnector CreateEventConnector()
        {
            return new WindowsEventConnector(this);
        }

        public INativeMethodsProvider CreateNativeMethodsProvider()
        {
            throw new System.NotImplementedException();
        }

        public IMediaRenderer CreateRenderer(MediaType mediaType, MediaEngine mediaEngine)
        {
            throw new System.NotImplementedException();
        }
    }
}
