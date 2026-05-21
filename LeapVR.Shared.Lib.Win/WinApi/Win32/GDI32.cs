#region Licence
/****************************************************************
 *  Filename: GDI32.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-15
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LeapVR.Shared.Lib.Win.ImagreProcessor;
using LeapVR.Shared.Lib.Win.Structs;
using LeapVR.Shared.Lib.Win.VirtualKeyboard;
using NLog;
using Point = System.Drawing.Point;

namespace LeapVR.Shared.Lib.Win.WinApi.Win32
{
    public static class Gdi32
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        #region Native Functions
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
        #endregion

        #region Operations

        public static Bitmap ScreenCopy(int x, int y, int dx, int dy)
        {
            Bitmap screenCopy = new Bitmap(dx, dy);
            using (Graphics gdest = Graphics.FromImage(screenCopy))
            using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hSrcDc = gsrc.GetHdc();
                IntPtr hDc = gdest.GetHdc();
                bool retval = BitBlt(hDc, 0, 0, dx, dy, hSrcDc, x, y, TernaryRasterOperations.Srccopy);

                gdest.ReleaseHdc();
                gsrc.ReleaseHdc();
            }
            return screenCopy;
        }


        public static Bitmap ScreenCopyAeroOffClipboard(int x, int y, int dx, int dy)
        {
            //Very Dirty
            int xlowest = 0;
            int ylowest = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Bounds.X < xlowest) xlowest = screen.Bounds.X;
                if (screen.Bounds.Y < ylowest) ylowest = screen.Bounds.Y;
            }
            x = x + Math.Abs(xlowest);
            y = y + Math.Abs(ylowest);

            Bitmap screenCopy;
            var screenAreaBitmap = new Bitmap(dx, dy);
            long lastTimeStamp = ClipboardData.GetClipboardBitmap(out screenCopy);
            InputProcessor.KeyboardInput().KeyPress(VirtualKeyCode.Snapshot);

            int maxRound = 20;
            bool loop = true;
            do
            {
                Thread.Sleep(500);
                if (ClipboardData.GetClipboardBitmap(out screenCopy) <= lastTimeStamp)
                {
                    maxRound--;
                    if (maxRound <= 0) return screenAreaBitmap;
                }
                else loop = false;
            } while (loop);

            // Create the new bitmap and associated graphics object
            Graphics g = Graphics.FromImage(screenAreaBitmap);

            // Draw the specified section of the source bitmap to the new one
            g.DrawImage(screenCopy, 0, 0, new Rectangle(x, y, dx, dy), GraphicsUnit.Pixel);

            // Clean up
            g.Dispose();

            // Return the bitmap
            return screenAreaBitmap;
        }

        public static Bitmap GetPrimaryScreen()
        {
            if (User32.DwmIsCompositionEnabled())
            {
                return GetPrimaryScreenAero();
            }
            return GetPrimaryScreenNoAero();
        }
        public static Bitmap GetPrimaryScreenAero()
        {
            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format24bppRgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }

        public static Bitmap GetPrimaryScreenNoAero()
        {
            return ScreenCopyAeroOffClipboard(0, 0, Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
        }

        public static List<Point> SearchPixelWithColor(Rectangle area, string hexColor, int tolerance = 0)
        {
            //Get a Bitmap of this area
            var screenCopy = ImageProcessor.GetScreenArea(area);
            //Get all Pixels in that area that match the color
            var pointList = DetectColorHex(screenCopy, hexColor, tolerance);
            //Recalculate all Pixels back to the Screen
            List<Point> newPoint = pointList.Select(point => new Point(point.X + area.Left, point.Y + area.Top)).ToList();
            return newPoint;
        }

        private static List<Point> DetectColorHex(Bitmap image, string hexColor, int tolerance)
        {
            var tt = Convert.ToInt32(hexColor, 16);
            var color = Color.FromArgb(tt);
            return DetectColorRgb(image, color.R, color.G, color.B, tolerance);
        }
        static unsafe List<Point> DetectColorRgb(Bitmap image, byte searchedR, byte searchedG, int searchedB, int tolerance)
        {
            BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width,
              image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            const int bytesPerPixel = 3;

            //if casted to int the rgba order inverses 
            byte* scan0 = (byte*)imageData.Scan0.ToPointer();
            int stride = imageData.Stride;

            List<Point> foundPoints = new List<Point>();
            int toleranceSquared = tolerance * tolerance;

            for (int y = 0; y < imageData.Height; y++)
            {
                byte* row = scan0 + (y * stride);

                for (int x = 0; x < imageData.Width; x++)
                {
                    // Watch out for actual order (BGR)!
                    int bIndex = x * bytesPerPixel;
                    int gIndex = bIndex + 1;
                    int rIndex = bIndex + 2;

                    byte pixelR = row[rIndex];
                    byte pixelG = row[gIndex];
                    byte pixelB = row[bIndex];

                    int diffR = pixelR - searchedR;
                    int diffG = pixelG - searchedG;
                    int diffB = pixelB - searchedB;

                    int distance = diffR * diffR + diffG * diffG + diffB * diffB;

                    //Original that changes the Pixels to white if found and black if not
                    //row[rIndex] = row[bIndex] = row[gIndex] = distance > toleranceSquared ? unmatchingValue : matchingValue;

                    //Changed that adds found Pixel to a list
                    if (distance <= toleranceSquared)
                    {
                        foundPoints.Add(new Point(x, y));
                        //row[rIndex] = row[bIndex] = row[gIndex] = matchingValue;
                    }
                    else
                    {
                        //row[rIndex] = row[bIndex] = row[gIndex] = unmatchingValue;
                    }
                }
            }
            image.UnlockBits(imageData);
            //image.Save("C:\\Test.bmp");
            return foundPoints;
        }

        /// <summary>
        /// Dectets a Image with transparancy information (PNG) in an Source image(BMP)
        /// Pixels that are not full opace will not be evaluated
        /// </summary>
        /// <param name="sourceImage">The RGB Bitmap</param>
        /// <param name="searchImage">The ARGB Bitmap or PNG</param>
        /// <param name="colorTolerance">RGB color tolerance for valid colors</param>
        /// <param name="faultTolerance">Maximum wrong pixels in percent(very slow)</param>
        /// <returns></returns>
        public static unsafe List<Rectangle> DetectImageArgb(Bitmap sourceImage, Bitmap searchImage, int colorTolerance = 0,byte faultTolerance = 0)
        {
            //A constant
            const int bytesPerPixelRgb = 3;
            const int bytesPerPixelArgb = 4;

            //Use Alpha
            bool searchImageIsAlpha = searchImage.PixelFormat == PixelFormat.Format32bppArgb;

            //Fault Tolerance Values
            if (faultTolerance > 100) faultTolerance = 100;
            uint searchImagePixels = 0;

            //The return value
            var retval = new List<Rectangle>();

            //The Image in that we search
            BitmapData sourceImageData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width,sourceImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //The Image that we search
            BitmapData searchImageData = searchImage.LockBits(new Rectangle(0, 0, searchImage.Width, searchImage.Height), ImageLockMode.ReadOnly, searchImageIsAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);

            //Get's the pointer to the first pixel in the picture
            //* is a dereferenced operator that has something to do with pointer
            byte* scanSource0 = (byte*)sourceImageData.Scan0.ToPointer();
            byte* scanSearch0 = (byte*)searchImageData.Scan0.ToPointer();


            //A Stride is the scan width
            int strideSource = sourceImageData.Stride;
            int strideSearch = searchImageData.Stride;

            int toleranceSquared = colorTolerance * colorTolerance;

            //Points that mark the bounderies of the detection area in the search picture
            //Only full opace pixels are valid
            var firstTopColorPixel = new Point(0, 0); 
            var mostLeftColorPixel = new Point(0,0);
            var mostRightColorPixel = new Point(0, 0); 
            var mostButtomColorPixel = new Point(0, 0);

            //Get the first pixel in the search image
            bool firstPixelIdentified = false;
            byte searchedR = 0;
            byte searchedG = 0;
            int searchedB = 0;


            for (int y = 0; y < searchImageData.Height; y++)
            {
                //Gets the data for the Row
                // Pointer-To-Byte-Of-First-Pixel-In-Bitmap(scan0) + (Bytes in one Line * needed Line) = Pointer to Byte of first pixel in desired line 
                byte* row = scanSearch0 + (y * strideSearch);

                //Runs for every Pixel in the row of the source picture
                for (int x = 0; x < searchImageData.Width; x++)
                {
                    // We calculate the index of the pixel in the row
                    // X is the current pixel we inspect, bytesPerPixel is the number of Bytes for a single pixel (3 for RGB,4 for ARGB)
                    // Watch out for actual order (BGR)!
                    int bIndex;

                    if (searchImageIsAlpha)bIndex = x*bytesPerPixelArgb;
                    else bIndex = x * bytesPerPixelRgb;

                    int gIndex = bIndex + 1;
                    int rIndex = bIndex + 2;
                    
                             

                    //Now get the Byte value of every single channel for this pixel
                    byte pixelR = row[rIndex];
                    byte pixelG = row[gIndex];
                    byte pixelB = row[bIndex];
                    byte pixelA;

                    //Alpha pixel
                    if (searchImageIsAlpha)
                    {
                        int aIndex = bIndex + 3;
                        pixelA = row[aIndex];
                    }
                    else pixelA = 255;

                    if (!firstPixelIdentified && pixelA == 255)
                    {
                        firstTopColorPixel = new Point(x, y);
                        firstPixelIdentified = true;
                        searchedB = pixelB;
                        searchedG = pixelG;
                        searchedR = pixelR;
                        mostLeftColorPixel = new Point(x, y);
                        mostRightColorPixel = new Point(x, y);
                        mostButtomColorPixel = new Point(x, y);
                        searchImagePixels++;
                    }
                    else if (firstPixelIdentified && pixelA == 255)
                    {
                        if (mostLeftColorPixel.X > x) mostLeftColorPixel =new Point(x,y);
                        if (mostRightColorPixel.X < x) mostRightColorPixel = new Point(x, y);
                        if (mostButtomColorPixel.Y < y) mostButtomColorPixel = new Point(x, y);
                        searchImagePixels++;
                    }
                }
            }

            //Calculate max wrong pixels
            uint maxWrongPixels = faultTolerance == 0 ? 0 : Convert.ToUInt32(searchImagePixels * ((decimal)faultTolerance / 100));

            //The Rectangle in the search Picture that defines non transparent pixels 
            var colorInformationArea = new Rectangle(mostLeftColorPixel.X,
                firstTopColorPixel.Y,
                mostRightColorPixel.X-mostLeftColorPixel.X,
                mostButtomColorPixel.Y- firstTopColorPixel.Y);

            //Runs for every Row in the source picture
            for (int y = 0; y < sourceImageData.Height; y++)
            {
                //Gets the data for the Row
                // Pointer-To-Byte-Of-First-Pixel-In-Bitmap(scan0) + (Bytes in one Line * needed Line) = Pointer to Byte of first pixel in desired line 
                byte* row = scanSource0 + (y * strideSource);

                //Runs for every Pixel in the row of the source picture
                for (int x = 0; x < sourceImageData.Width; x++)
                {
                    // We calculate the index of the pixel in the row
                    // X is the current pixel we inspect, bytesPerPixel is the number of Bytes for a single pixel (3 for RGB,4 for ARGB)
                    // Watch out for actual order (BGR)!
                    int bIndex = x * bytesPerPixelRgb;
                    int gIndex = bIndex + 1;
                    int rIndex = bIndex + 2;

                    //Now get the Byte value of every single channel for this pixel
                    byte pixelR = row[rIndex];
                    byte pixelG = row[gIndex];
                    byte pixelB = row[bIndex];

                    //Calculate the difference between the found and the searched pixel
                    int diffR = pixelR - searchedR;
                    int diffG = pixelG - searchedG;
                    int diffB = pixelB - searchedB;

                    //Calculate from the single differences a total one
                    int distance = diffR * diffR + diffG * diffG + diffB * diffB;

                    //Used for fault tolerance
                    int faultyPixels = 0;

                    //If the distance is lower or equal the tolerance we found a proper pixel in the source
                    if (distance > toleranceSquared)
                    {
                        if (maxWrongPixels == 0) continue;
                        faultyPixels++;
                    }

                    //Check if our search rectangle fit's in the remaining image area of the source
                    //Check first from the current point(top/left) if the height is sufficent
                    int remainingLinesInHeight = sourceImageData.Height - y;
                    if (remainingLinesInHeight < colorInformationArea.Height)
                    {
                        //Dont fit by height
                        //Continue to search for the next pixel in the source
                        continue;
                    }

                    //Check from point(top/left) if the space to the left is sufficent
                    int neededlinesToLeft = firstTopColorPixel.X - mostLeftColorPixel.X;
                    if ((x - neededlinesToLeft) < 0)
                    {
                        //Dont fit to the left
                        //Continue to search for the next pixel in the source
                        continue;
                    }
                    var searchAreaStartPoint = new Point(x - neededlinesToLeft, y);

                    //Check from point(top/left) if the space to the right is sufficent
                    int neededlinesToRight = mostRightColorPixel.X - firstTopColorPixel.X;
                    if ((x + neededlinesToRight) > sourceImageData.Width)
                    {
                        //Dont fit to the right
                        //Continue to search for the next pixel in the source
                        continue;
                    }

                    //The space is enough to hold our searched image
                    //Define the search area in the source picture
                    var sourceSearchArea = new Rectangle(searchAreaStartPoint,colorInformationArea.Size);

                    //Define a control variable
                    bool noMatch = false;

                    //Compare the source search area pixel by pixel for a match of the searched image 
                    for (int ysrc = sourceSearchArea.Top; ysrc < sourceSearchArea.Bottom; ysrc++)
                    {
                        //Jump out of loop as informations dont match
                        if(noMatch)break;

                        // Get the pointer to the line in the source image
                        byte* rowsrc = scanSource0 + (ysrc * strideSource);
                        // Get the pointer to the line in the search image
                        byte* rowsearch = scanSearch0 + ((colorInformationArea.Top + (ysrc - sourceSearchArea.Top)) * strideSearch);

                        //Runs for every Pixel in the rows we compare with each other
                        for (int xsrc = sourceSearchArea.Left; xsrc < sourceSearchArea.Right; xsrc++)
                        {
                            //Search Image
                            int bIndexSearch;
                            if (searchImageIsAlpha) bIndexSearch = (xsrc + colorInformationArea.Left - sourceSearchArea.Left) * bytesPerPixelArgb;
                            else bIndexSearch = (xsrc + colorInformationArea.Left - sourceSearchArea.Left) * bytesPerPixelRgb;
                            int gIndexSearch = bIndexSearch + 1;
                            int rIndexSearch = bIndexSearch + 2;

                            //Now get the Byte value of every single channel for this pixel
                            byte pixelRSearch = rowsearch[rIndexSearch];
                            byte pixelGSearch = rowsearch[gIndexSearch];
                            byte pixelBSearch = rowsearch[bIndexSearch];
                            byte pixelASearch;

                            //Alpha pixel
                            if (searchImageIsAlpha)
                            {
                                int aIndexSearch = bIndexSearch + 3;
                                pixelASearch = rowsearch[aIndexSearch];
                            }
                            else pixelASearch = 255;

                            //If it's a transparent pixel we can skip it as we don't compare them
                            if(pixelASearch != 255) continue;

                            //Source Image
                            int bIndexSrc = xsrc * bytesPerPixelRgb;
                            int gIndexSrc = bIndexSrc + 1;
                            int rIndexSrc = bIndexSrc + 2;

                            //Now get the Byte value of every single channel for this pixel
                            byte pixelRSrc = rowsrc[rIndexSrc];
                            byte pixelGSrc = rowsrc[gIndexSrc];
                            byte pixelBSrc = rowsrc[bIndexSrc];

                            //Calculate the difference between the found and the searched pixel
                            int diffRSrc = pixelRSrc - pixelRSearch;
                            int diffGSrc = pixelGSrc - pixelGSearch;
                            int diffBSrc = pixelBSrc - pixelBSearch;

                            //Calculate from the single differences a total one
                            int distanceB = diffRSrc * diffRSrc + diffGSrc * diffGSrc + diffBSrc * diffBSrc;

                            //Set a faulty pixel
                            if (!(distanceB <= toleranceSquared))
                            {
                                faultyPixels++;
                            }
                            //Set no Match if we have to many pixels not in tolerance
                            if (faultyPixels > maxWrongPixels)
                            {
                                noMatch = true;
                            }
                        }
                    }

                    //We compared all areas with success
                    if (!noMatch)
                    {
                        //We identified our picture!
                        retval.Add(sourceSearchArea);
                    }
                }
            }

            //Unlock the Bits
            sourceImage.UnlockBits(sourceImageData);
            searchImage.UnlockBits(searchImageData);

            //Return the found areas in that we found the picture
            return retval;
        }

        public static List<Point> FilterPixelPoints(List<Point> points, int pixelTolerance)
        {
            //TODO: Change to an average mechanism
            var retval = new List<Point>();
            //Points that belong together have to follow one after the next and need to be in tolerance
            Point averagePoint = new Point();
            foreach (var point in points)
            {
                //Relates to the previous point and is in tolerance
                if (point.X >= averagePoint.X - pixelTolerance &&
                    point.X <= averagePoint.X + pixelTolerance &&
                    point.Y >= averagePoint.Y - pixelTolerance &&
                    point.Y <= averagePoint.Y + pixelTolerance)
                {
                    averagePoint = new Point((point.X + averagePoint.X) / 2, (point.Y + averagePoint.Y) / 2);
                }
                //Is a new point
                else
                {
                    //Add previous point
                    if (!averagePoint.IsEmpty) retval.Add(averagePoint);
                    averagePoint = point;
                }
            }
            //Add last point
            if (!averagePoint.IsEmpty) retval.Add(averagePoint);
            return retval;
        } 
        #endregion

        #region Enum & Structs
        /// <summary>
        ///     Specifies a raster-operation code. These codes define how the color data for the
        ///     source rectangle is to be combined with the color data for the destination
        ///     rectangle to achieve the final color.
        /// </summary>
        public enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            Srccopy = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            Srcpaint = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            Srcand = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            Srcinvert = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            Srcerase = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            Notsrccopy = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            Notsrcerase = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            Mergecopy = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            Mergepaint = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            Patcopy = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            Patpaint = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            Patinvert = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            Dstinvert = 0x00550009,
            /// <summary>dest = BLACK</summary>
            Blackness = 0x00000042,
            /// <summary>dest = WHITE</summary>
            Whiteness = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            Captureblt = 0x40000000
        }
        #endregion
    }
}
