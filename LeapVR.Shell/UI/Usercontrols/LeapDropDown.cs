#region Licence
/****************************************************************
 *  Filename: LeapDropDown.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-10-11
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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LeapVR.Shell.UI.Usercontrols
{
    public class LeapDropDown : ComboBox
    {
        #region Fields & Properties
        public static readonly DependencyProperty DropDownBackgroundProperty =
            DependencyProperty.Register("DropDownBackground", typeof(Brush), typeof(LeapDropDown));

        public static readonly DependencyProperty ButtonHoverBackgroundProperty =
            DependencyProperty.Register("ButtonHoverBackground", typeof(Brush), typeof(LeapDropDown));

        public static readonly DependencyProperty ItemCheckedBackgroundProperty =
            DependencyProperty.Register("ItemCheckedBackground", typeof(Brush), typeof(LeapDropDown));

        public static readonly DependencyProperty PopupPlacementProperty =
            DependencyProperty.Register("PopupPlacement", typeof(PlacementMode), typeof(LeapDropDown));

        /// <summary>
        /// Get or set background of the dropdown.
        /// </summary>
        public Brush DropDownBackground
        {
            get => (Brush)GetValue(DropDownBackgroundProperty);
            set => SetValue(DropDownBackgroundProperty, value);
        }

        /// <summary>
        /// Get or set the hover background.
        /// </summary>
        public Brush ButtonHoverBackground
        {
            get => (Brush)GetValue(ButtonHoverBackgroundProperty);
            set => SetValue(ButtonHoverBackgroundProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public Brush ItemCheckedBackground
        {
            get => (Brush)GetValue(ItemCheckedBackgroundProperty);
            set => SetValue(ItemCheckedBackgroundProperty, value);
        }
        /// <summary>
        /// 
        /// </summary>
        public PlacementMode PopupPlacement
        {
            get => (PlacementMode)GetValue(PopupPlacementProperty);
            set => SetValue(PopupPlacementProperty, value);
        }

        #endregion

        #region Constructors
        static LeapDropDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LeapDropDown), new FrameworkPropertyMetadata(typeof(LeapDropDown)));
        }

        #endregion

        #region Methods

        #endregion

    }

}
