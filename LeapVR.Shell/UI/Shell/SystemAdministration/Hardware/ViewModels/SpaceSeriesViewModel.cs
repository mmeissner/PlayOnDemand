#region Licence
/****************************************************************
 *  Filename: SpaceSeriesViewModel.cs
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
using Caliburn.Micro;
using LiveCharts;
using LiveCharts.Defaults;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Hardware.ViewModels
{
    public class SpaceSeriesViewModel : Screen
    {
        #region Fields & Properties

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public ChartValues<ObservableValue> ChartValues { get; }

        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                if (ChartValues.Count <= 0)
                {
                    ChartValues.Add(new ObservableValue(_value));
                }
                else
                {
                    ChartValues[0].Value = _value;
                }
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ChartValues));
            }
        }


        public static Func<ChartPoint, string> LabelPoint => (point) => ""; //$"{QuickLeap.ToDiskSize((ulong)point.Y)}";
        #endregion

        #region Constructors

        public SpaceSeriesViewModel(string seriesTitle, double value) : this(seriesTitle)
        {
            Value = value;
        }

        public SpaceSeriesViewModel(string seriesTitle)
        {
            _title = seriesTitle;
            ChartValues = new ChartValues<ObservableValue>();
        }

        #endregion

        #region Methods

        #endregion
    }
}
