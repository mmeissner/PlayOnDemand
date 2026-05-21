#region Licence
/****************************************************************
 *  Filename: UIHelper_ResourceConverter.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace LeapVR.Shared.Lib.Wpf.UIHelpers
{
    public static partial class UIHelper
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Seek(0, SeekOrigin.Begin);

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memory;
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            using (bitmap)
            {
                IntPtr hBitmap = bitmap.GetHbitmap();
                try
                {
                   return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        public static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            //convert image format
            var src = new FormatConvertedBitmap();
            src.BeginInit();
            src.Source = bitmapSource;
            src.DestinationFormat = PixelFormats.Bgra32;
            src.EndInit();

            //copy to bitmap
            Bitmap bitmap = new Bitmap(src.PixelWidth, src.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new System.Drawing.Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            src.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bitmap.UnlockBits(data);

            return bitmap;
        }


        public static ImageSource ToImageSource(this byte[] buffer)
        {
            return BytesToImageSource(buffer);
        }
        public static ImageSource BytesToImageSource(byte[] buffer)
        {
            if (buffer == null)
            {
                return null;
            }

            using (var memory = new MemoryStream(buffer))
            {
                memory.Seek(0, SeekOrigin.Begin);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = memory;
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                if(bitmap.CanFreeze)bitmap.Freeze();
                return bitmap;
            }
        }

        public static ImageSource FilePathToImageSource(string fileAbsolutePath)
        {
            using (var memory = new FileStream(fileAbsolutePath, FileMode.Open))
            {
                memory.Seek(0, SeekOrigin.Begin);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = memory;
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                return bitmap;
            }
        }
        public static ImageSource MakeImageWithArea(BitmapImage imageSource, int width, int height, int posX = 0, int posY = 0)
        {
            var sourceBmp = BitmapSourceToBitmap(imageSource);
            var newBitmap = MakeImageWithArea(sourceBmp, width, height, posX, posY);
            var newImageSource = BitmapToBitmapImage(newBitmap);
            return newImageSource;
        }

        public static ImageSource MakeImageWithRectangleAlphaArea(BitmapImage imageSource, int width, int height, int posX = 0, int posY = 0, int opacityPercent = 1)
        {
            var sourceBmp = BitmapSourceToBitmap(imageSource);
            var newBitmap = MakeImageWithRectangleAlphaArea(sourceBmp, width, height, posX, posY, opacityPercent);
            var newImageSource = BitmapToBitmapImage(newBitmap);
            return newImageSource;
        }

        public static ImageSource MakeImageWithScaledRectangleAlphaArea(BitmapImage imageSource, int width, int height, int posX = 0, int posY = 0, int opacityPercent = 0)
        {
            // TODO [FH] get Bitmap from BitmapSource here will always throw exceptions. Find a better way to convert.
            var sourceBmp = BitmapSourceToBitmap(imageSource);

            var scaledSourceBmp = new Bitmap((int)imageSource.Width, (int)imageSource.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            scaledSourceBmp.SetResolution(sourceBmp.HorizontalResolution, sourceBmp.VerticalResolution);

            using (var graphics = Graphics.FromImage(scaledSourceBmp))
            {

                graphics.DrawImage(sourceBmp, 0, 0, scaledSourceBmp.Width, scaledSourceBmp.Height);

                var alphaArea = new Bitmap(width, height);
                var color = Color.FromArgb((255 * opacityPercent) / 100, Color.Black);
                for (var columnIndex = 0; columnIndex < width; columnIndex++)
                {
                    for (var rowIndex = 0; rowIndex < height; rowIndex++)
                    {
                        alphaArea.SetPixel(columnIndex, rowIndex, color);
                    }
                }
                graphics.SetClip(new System.Drawing.Rectangle(posX, posY, width, height));
                graphics.Clear(Color.Transparent);
                graphics.DrawImage(alphaArea, new Point(posX, posY));
                graphics.ResetClip();

            }

            var newImageSource = BitmapToBitmapImage(scaledSourceBmp);
            return newImageSource;
        }

        // Make an image that includes only the selected area.
        public static Bitmap MakeImageWithArea(Bitmap sourceBmp, int width, int height, int posX = 0, int posY = 0)
        {
            // Copy the image.
            Bitmap bmp = new Bitmap(sourceBmp.Width, sourceBmp.Height);

            // Clear the selected area.
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.Clear(Color.Transparent);

                // Make a brush that contains the original image.
                using (Brush brush = new TextureBrush(sourceBmp))
                {
                    var points = GenerateBorderPointsFromArea(width, height, posX, posY);
                    // Fill the selected area.
                    gr.FillPolygon(brush, points.ToArray());
                }
            }
            return bmp;
        }
        public static Bitmap MakeImageWithRectangleAlphaArea(Bitmap sourceBmp, int width, int height, int posX = 0, int posY = 0, int opacityPercent = 0)
        {
            var color = Color.FromArgb((255 * opacityPercent) / 100, Color.Black);
            var bmp = new Bitmap(sourceBmp);
            for (var columnIndex = 0; columnIndex < width; columnIndex++)
            {
                for (var rowIndex = 0; rowIndex < height; rowIndex++)
                {
                    if (columnIndex + posX > bmp.Width || rowIndex + posY > bmp.Height)
                    {
                        continue;
                    }

                    try
                    {
                        int resultPosx = columnIndex + posX;
                        int resultPosY = rowIndex + posY;
                        if(resultPosx == bmp.Width) resultPosx = bmp.Width-1;
                        else if(resultPosx < 0) resultPosx = 0;
                        if(resultPosY == bmp.Height) resultPosY = bmp.Height-1;
                        else if(resultPosY < 0) resultPosY = 0;
                        bmp.SetPixel(resultPosx, resultPosY, color);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            return bmp;
        }


        private static IEnumerable<Point> GeneratePointsFromArea(int width, int height, int posX, int posY)
        {
            for (int columnIndex = 0; columnIndex <= width; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex <= height; rowIndex++)
                {
                    yield return new Point(posX + columnIndex, posY + rowIndex);
                }
            }
        }

        private static IEnumerable<Point> GenerateBorderPointsFromArea(int width, int height, int posX, int posY)
        {
            return from point in GeneratePointsFromArea(width, height, posX, posY)
                   where point.X == posX || point.X == posX + width || point.Y == posY || point.Y == posY + height
                   select point;
        }




    }
}
