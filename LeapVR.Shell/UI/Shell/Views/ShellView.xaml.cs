#region Licence
/****************************************************************
 *  Filename: ShellView.xaml.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-11-16
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Windows;
using System.Windows.Input;
using LeapVR.Shared.Lib.Win;
using NLog;
using Pod.Data.Infrastructure;

namespace LeapVR.Shell.UI.Shell.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ShellView()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Related to UC-52 tries to prevent crash of Shell on Drag due to unknown reasons that cause an exception to throw
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            }
            catch (System.Exception exception)
            {
                Logger.Warn(exception, $"Exception during Window_MouseDown Drag Move. {e.ToJson()}");
                throw;
            }
        }
    }
}
