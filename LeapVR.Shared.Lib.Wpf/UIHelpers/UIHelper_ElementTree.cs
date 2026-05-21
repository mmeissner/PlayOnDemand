#region Licence
/****************************************************************
 *  Filename: UIHelper_ElementTree.cs
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
using System.Windows;
using System.Windows.Media;

namespace LeapVR.Shared.Lib.Wpf.UIHelpers
{
    public static partial class UIHelper
    {
        #region Static Methods

        public static IEnumerable<UIElement> GetChildrenElements(UIElement rootElement)
        {
            var count = VisualTreeHelper.GetChildrenCount(rootElement);
            for (var i = 0; i < count; i++)
            {
                if (VisualTreeHelper.GetChild(rootElement, i) is UIElement childElement)
                {
                    foreach (var elementInLaw in GetChildrenElements(childElement))
                    {
                        yield return elementInLaw;
                    }
                    yield return childElement;
                }
            }
        }

        public static T GetChildElementByNameFromVisualTree<T>(FrameworkElement rootElement, string elementName) where T : FrameworkElement
        {
            switch (rootElement)
            {
                case null:
                    return null;
                case T root:
                    return root.Name.Equals(elementName) ? root : null;
            }

            var count = VisualTreeHelper.GetChildrenCount(rootElement);

            for (var i = 0; i < count; i++)
            {
                var childElement = (FrameworkElement)VisualTreeHelper.GetChild(rootElement, i);
                var target = GetChildElementByNameFromVisualTree<T>(childElement, elementName);
                if (target == null)
                {
                    continue;
                }
                return target;
            }
            return null;
        }

        public static FrameworkElement GetParentElementByNameFromVisualTree(FrameworkElement element, string elementName)
        {
            while (true)
            {
                var self = element;
                if (self == null)
                {
                    return null;
                }

                if (self.Name.Equals(elementName))
                {
                    return self;
                }

                var parent = VisualTreeHelper.GetParent(self) as FrameworkElement;
                element = parent;
            }
        }
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T variable)
                    return variable;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
        #endregion

    }
}
