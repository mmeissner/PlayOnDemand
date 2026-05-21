#region Licence
/****************************************************************
 *  Filename: ContainerEdit.cs
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
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Content.Creator.Logic
{
    /// <summary>
    /// View-model bag the WPF edit wizard binds against
    /// (cf. EditAppDetailInfoView.xaml). Constructed by
    /// LeapVrContainerEditor; aggregates the editor's mutable state into the
    /// property shape the XAML expects.
    ///
    /// DisplayData / PlatformData are exposed as the raw DTOs (not via
    /// notifying wrappers): WPF TwoWay binding reads + writes through their
    /// public setters directly, so edits land in the DTOs and persist on the
    /// next LeapVrContainerEditor.Save().
    /// </summary>
    public sealed class ContainerInfo
    {
        public Guid ContainerApplicationGuid { get; }
        public int ContainerVersion { get; }
        public int TotalFilesCount { get; }
        public long TotalFilesSize { get; }
        public ImageSource ThumbnailAsImage { get; }
        public byte[] ThumbnailAsBytes { get; }

        public AppDisplayDataDto DisplayData { get; }
        public AppPlatformDataDto PlatformData { get; }

        internal ContainerInfo(LeapVrContainerEditor editor)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));
            ContainerApplicationGuid = editor.ApplicationGuid;
            ContainerVersion = editor.Version;
            ThumbnailAsBytes = editor.ThumbnailAsBytes;
            ThumbnailAsImage = LoadImage(editor.ThumbnailAsBytes);
            DisplayData = editor.DisplayData;
            PlatformData = editor.PlatformData;

            // Roll up file counts/sizes across all packages so the wizard's
            // "Disk size required" field reflects what's in the container.
            if (editor.Packages != null)
            {
                TotalFilesCount = editor.Packages.Sum(p => p.TotalFilesCount);
                TotalFilesSize = editor.Packages.Sum(p => p.TotalFilesSize);
            }
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            try
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}
