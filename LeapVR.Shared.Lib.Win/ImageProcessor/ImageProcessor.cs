#region Licence
/****************************************************************
 *  Filename: ImageProcessor.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using LeapVR.Shared.Lib.Win.WinApi;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using NLog;
using Brush = System.Drawing.Brush;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace LeapVR.Shared.Lib.Win.ImagreProcessor
{
    public class ImageProcessor
    {
        //private static bool _dxScreenCaptureInitialized = false;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static List<Rectangle> FindImageOnPrimaryScreen(Bitmap searchImage, int colorTolerance, byte faultTolerance)
        {
            var screen = Gdi32.GetPrimaryScreen();
            return Gdi32.DetectImageArgb(screen, searchImage, colorTolerance, faultTolerance);
        }
        public static List<Rectangle> FindImageOnPicture(Bitmap source, Bitmap searchImage, int colorTolerance, byte faultTolerance)
        {
            return Gdi32.DetectImageArgb(source, searchImage, colorTolerance, faultTolerance);
        }
        public static List<Rectangle> FindImageOnScreenArea(Rectangle rectangle, Bitmap searchImage, int colorTolerance, byte faultTolerance)
        {
            var screen = GetScreenArea(rectangle);
            return Gdi32.DetectImageArgb(screen, searchImage, colorTolerance, faultTolerance);
        }
        public static List<Rectangle> OffsetPositions(Rectangle searchArea, List<Rectangle> foundRectangles)
        {
            var retval = new List<Rectangle>();
            foreach (var rectangle in foundRectangles)
            {
                retval.Add(OffSetPosition(searchArea, rectangle));
            }
            return retval;
        }
        public static Rectangle OffSetPosition(Rectangle searchArea, Rectangle foundRectangle)
        {
            _logger.Debug(String.Format("OffSetPosition called with SearchArea: x={0}, y={1}, OffsetRectangle x={2}, y={3}", searchArea.X, searchArea.Y, foundRectangle.X, foundRectangle.Y));
            var retval = new Rectangle(foundRectangle.Location.X + searchArea.Location.X, foundRectangle.Location.Y + searchArea.Location.Y, foundRectangle.Width, foundRectangle.Height);
            _logger.Debug("Return OffsetPosition x={0}, y={1}", retval.X, retval.Y);
            return retval;
        }
        public static Point OffSetPosition(Rectangle searchArea, Point foundPoint)
        {
            return new Point(foundPoint.X + searchArea.Location.X, foundPoint.Y + searchArea.Location.Y);
        }
        public static Point GetCenter(Rectangle rectangle)
        {
            _logger.Debug("GetCenter called with Rectangle: x={0}, y={1}", rectangle.X, rectangle.Y);
            var retval = new Point(rectangle.Width / 2 + rectangle.X, rectangle.Height / 2 + rectangle.Y);
            _logger.Debug("Return Center as Point: x={0}, y={1}", retval.X, retval.Y);
            return retval;
        }
        //http://tech.pro/tutorial/660/csharp-tutorial-convert-a-color-image-to-grayscale
        public static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
        public static BitmapSource ConvertBitmap(Bitmap source)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }
        /// <summary>
        /// Get a Screencopy of a specific rectangle 
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>A Bitmap with a Screencopy of the specified area</returns>
        public static Bitmap GetScreenArea(Rectangle rect)
        {
            return GetScreenArea(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        /// <summary>
        /// Get a Screencopy of a specific area
        /// </summary>
        /// <param name="x">Left Pixel of Rectangle</param>
        /// <param name="y">Top Pixel of Rectangle</param>
        /// <param name="dx">Rectangle Width</param>
        /// <param name="dy">Rectangle Height</param>
        /// <returns>A Bitmap with a Screencopy of the specified area</returns>
        public static Bitmap GetScreenArea(int x, int y, int dx, int dy)
        {
            _logger.Debug(String.Format("GetScreenArea called with x={0}, y={1}, dx={2}, dy={3}", x, y, dx, dy));
            //Method throwed once without visible reason and was successfull on next try
            Bitmap screenCopy = null;
            //Try maximum 5 Times to get a Screenshot
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (User32.DwmIsCompositionEnabled())
                    {
                        screenCopy = Gdi32.ScreenCopy(x, y, dx, dy);
                    }
                    else
                    {
                        screenCopy = Gdi32.ScreenCopyAeroOffClipboard(x, y, dx, dy);
                    }
                    break;
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Exception during {0} run of GetScreenArea", i + 1);
                }
            }
            return screenCopy;
        }
    }


}
