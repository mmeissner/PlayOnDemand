#region Licence
/****************************************************************
 *  Filename: ListViewWithSelectionChangedOnMouseUp.cs
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
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation
{
    public class ListViewWithSelectionChangedOnMouseUp : ListView
    {
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var item = ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
                if (item != null && Items.Contains(item.DataContext))
                {
                    SelectedItem = item.DataContext;
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            // TODO [FH] find other way to differenciate the folder and application.
            var element = e.OriginalSource as FrameworkElement;

            if (element?.DataContext is InstallationFolderViewModel)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }

        }
    }
}
