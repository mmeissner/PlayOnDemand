#region Licence
/****************************************************************
 *  Filename: CategoryViewModel.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;

namespace LeapVR.Content.Creator.UI.ViewModels.Category
{
    public class CategoryViewModel : Screen
    {
        public ImageSource Icon { get; }
        public string ResourceKey { get; }
        public string TranslationKey { get; }
        public string Translation { get; }
        public CategoryViewModel(ImageSource icon, string key)
        {
            Icon = icon;
            ResourceKey = key;
            TranslationKey = $"Category_{key}";
            Translation = Shell.Categories.Resource.ResourceManager.GetString(TranslationKey);
        }
    }
}
