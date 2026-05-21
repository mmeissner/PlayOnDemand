#region Licence
/****************************************************************
 *  Filename: DisplayContainer.cs
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
using System.Drawing;
using System.Windows;

namespace LeapVR.Shell.UI.Core {
    internal class DisplayContainer
    {
        public DisplayContainer(UIElement container, Window containerWindow)
        {
            ContainerHeight = container.RenderSize.Height;
            ContainerWidth = container.RenderSize.Width;
            PosInWindow = container.TranslatePoint(new System.Windows.Point(0, 0), containerWindow);
            PosOnScreen = containerWindow.PointToScreen(PosInWindow);
        }
        public double ContainerHeight { get; }
        public double ContainerWidth { get; }
        public System.Windows.Point PosInWindow { get; }
        public System.Windows.Point PosOnScreen { get; }

        public Rectangle GetWindowRectangle => new Rectangle(
                new System.Drawing.Point(Convert.ToInt32(PosInWindow.X), Convert.ToInt32(PosInWindow.Y)),
                new System.Drawing.Size(Convert.ToInt32(ContainerWidth), Convert.ToInt32(ContainerHeight)));
        public Rectangle GetScreenRectangle => new Rectangle(
                new System.Drawing.Point(Convert.ToInt32(PosOnScreen.X), Convert.ToInt32(PosOnScreen.Y)),
                new System.Drawing.Size(Convert.ToInt32(ContainerWidth), Convert.ToInt32(ContainerHeight)));
    }
}