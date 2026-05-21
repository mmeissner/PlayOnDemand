#region Licence
/****************************************************************
 *  Filename: SelectorHideSelectedItemBehavior.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-10-12
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
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using NLog;

namespace LeapVR.Shell.UI.Core.Behaviors
{
    /// <summary>
    /// Behavior class to filter out selected item from the lists in <see cref="Selector"/> and its derived controls.
    /// </summary>
    public class SelectorHideSelectedItemBehavior : Behavior<Selector>
    {

        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        #endregion

        #region Methods
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            AssociatedObject.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FilterOutSelectedItemInItems(AssociatedObject);
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            FilterOutSelectedItemInItems(AssociatedObject);
            Logger.Debug($"Renew ItemsSource of {AssociatedObject} on {nameof(ItemContainerGenerator_StatusChanged)}.");
        }

        private void FilterOutSelectedItemInItems(Selector selector)
        {
            foreach (var item in selector.Items)
            {
                var element = selector.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                if (element != null)
                {
                    element.Visibility = item.Equals(selector.SelectedItem) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        #endregion




    }
}
