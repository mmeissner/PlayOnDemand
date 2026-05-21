#region Licence
/****************************************************************
 *  Filename: TransparancyArea.cs
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
using System.Drawing.Printing;
using System.Windows.Media;
using LeapVR.Shared.Lib.Win.Structs;
using LeapVR.Shared.Lib.Win.WinApi;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.UI.Core {
    internal class TransparancyArea : ITransparencyArea
    {
        private readonly DisplayContainer _positioningContainer;
        private readonly int _width;
        private readonly int _height;
        public TransparancyArea(
                DisplayContainer positioningContainer,
                double height,
                double width,
                Margins margin,
                AlignmentX horizontalAlignment,
                AlignmentY vertialAlignment)
        {
            _positioningContainer = positioningContainer;
            Height = height;
            Width = width;
            _width = Convert.ToInt32(width);
            _height = Convert.ToInt32(height);
            Margin = margin;
            HorizontalAlignment = horizontalAlignment;
            VertialAlignment = vertialAlignment;
        }

        public double Height { get; }
        public double Width { get; }
        public Margins Margin { get; }
        public AlignmentX HorizontalAlignment { get; }
        public AlignmentY VertialAlignment { get; }
        public Rectangle CalcScreenPos()
        {
            double relativeElementWidth = 0;
            double relativeElementHeight = 0;
            relativeElementWidth = _positioningContainer.GetScreenRectangle.Width;
            relativeElementHeight = _positioningContainer.GetScreenRectangle.Height;

            int xTransparencyPos;
            int yTransparencyPos;
            switch(HorizontalAlignment)
            {
                case AlignmentX.Left:
                    xTransparencyPos = 0 + Margin.Left;
                    break;
                case AlignmentX.Center:
                    xTransparencyPos = Convert.ToInt32(
                            relativeElementWidth / 2 - Width / 2);
                    break;
                case AlignmentX.Right:
                    xTransparencyPos = Convert.ToInt32(
                            relativeElementWidth -
                            Width -
                            Margin.Right);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch(VertialAlignment)
            {
                case AlignmentY.Top:
                    yTransparencyPos = 0 + Margin.Top;
                    break;
                case AlignmentY.Center:
                    yTransparencyPos = Convert.ToInt32(
                            relativeElementHeight / 2 - Height / 2);
                    break;
                case AlignmentY.Bottom:
                    yTransparencyPos = Convert.ToInt32(
                            relativeElementHeight -
                            Height -
                            Margin.Bottom);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Rectangle(
                    xTransparencyPos + _positioningContainer.GetScreenRectangle.X,
                    yTransparencyPos + _positioningContainer.GetScreenRectangle.Y,
                    _width,
                    _height);
        }
        public bool GetIntersection(DisplayContainer container, out Rectangle intersection)
        {
            var containerScreenRec = container.GetScreenRectangle;
            var transparentAreaScreenPos = CalcScreenPos();
            var screenIntersection = Rectangle.Intersect(transparentAreaScreenPos, containerScreenRec);
            if(screenIntersection.IsEmpty)
            {
                intersection = Rectangle.Empty;
                return false;
            }

            intersection = new Rectangle(
                    screenIntersection.Location.X - containerScreenRec.Location.X,
                    screenIntersection.Location.Y - containerScreenRec.Location.Y,
                    _width,
                    _height);
            return true;
        }
        public DpiInfo GetDpiInfo()=>DpiUtil.GetPrimaryScreenDpiSafeResolution();
    }
}