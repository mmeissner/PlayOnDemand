#region Licence
/****************************************************************
 *  Filename: WpfDirectionalNavigation.cs
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Caliburn.Micro;
using LeapVR.Shell.UI.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Core
{
    public class WpfDirectionalNavigation<T> where T: class,IScreen
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Selector _itemSelector;
        private bool isFunctional = false;
        private static readonly Dictionary<NavigationDirection, FocusNavigationDirection> DirectionalMapping = new Dictionary<NavigationDirection, FocusNavigationDirection>
        {
            {NavigationDirection.Up,FocusNavigationDirection.Up},
            {NavigationDirection.Down,FocusNavigationDirection.Down},
            {NavigationDirection.Left,FocusNavigationDirection.Left},
            {NavigationDirection.Right,FocusNavigationDirection.Right}
        };

        public WpfDirectionalNavigation(Selector itemSelector)
        {
            if (itemSelector != null)
            {
                isFunctional = true;
                _itemSelector = itemSelector;
            }
            else Logger.Error("WpfDirectionalNavigator was initialized with a itemSelector that is null, Navigator will be not functional!");
        }

        public void Navigate(NavigationDirection direction,T selectedItem, Action<T> selectItem)
        {
            if(!isFunctional)return;

            FocusNavigationDirection navigationDirection = DirectionalMapping[direction];

            Logger.Trace($"Try to navigate in direction '{direction} to select {nameof(T)}.");

            //Get the ItemContainerGenerator that is responsible to create Items in WPF Controlls that have an ItemsCollection
            var selectorItemGenerator = _itemSelector?.ItemContainerGenerator;
            if (selectorItemGenerator == null)
            {
                Logger.Trace($"Failed to navigate in direction: '{direction}'. Can not get {nameof(_itemSelector)}.{nameof(ItemContainerGenerator)}.");
                return;
            }
            //Get the View that relates to the ViewModel
            var selectedItemContainer = selectorItemGenerator.ContainerFromItem(selectedItem);
            if (!(selectedItemContainer is UIElement focusMovableElemet))
            {
                Logger.Trace($"Failed to navigate in direction: '{direction}'. Can not cast from {nameof(selectedItemContainer)} to {nameof(focusMovableElemet)}.");
                return;
            }
            //Get the Item that would be focused if we move into this direction
            var predictedItem = focusMovableElemet.PredictFocus(navigationDirection);
            //We cant move into this direction if we receive null
            if (predictedItem == null)
            {
                Logger.Trace($"Failed to navigate through to direction: '{direction}'. Returned 'null' on execute {nameof(focusMovableElemet)}.{nameof(UIElement.PredictFocus)}({navigationDirection}).");
                return;
            }
            var request = new TraversalRequest(navigationDirection);
            //Moves just the Focus
            if (!focusMovableElemet.MoveFocus(request))
            {
                Logger.Trace($"Failed to navigate to direction: '{direction}'. Returned 'False' on execute {nameof(focusMovableElemet)}.{nameof(UIElement.MoveFocus)}");
                return;
            }
            //Select the Item that has the Focus
            if (!(selectorItemGenerator.ItemFromContainer(predictedItem) is T targetItem))
            {
                Logger.Trace($"Failed to navigate to direction: '{direction}'. Can not cast SelectedItem to {nameof(T)}.");
                return;
            }
            Logger.Trace($"Navigate from [{selectItem}] to [{targetItem}] by direction: '{direction}'.");
            selectItem.Invoke(targetItem);
        }

        public void BringIntoView(T itemToBrintIntoView)
        {
            if(!isFunctional)return;
            if (_itemSelector.ItemContainerGenerator.ContainerFromItem(itemToBrintIntoView) is FrameworkElement item)
            {
                item.BringIntoView();
                Logger.Debug($"Bring selected item into viewable area {itemToBrintIntoView} to view.");
            }
        }
    }
}