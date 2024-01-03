﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ProstateAutoPlanner
{
	public class ValidationTest : INotifyPropertyChanged
	{
        private string _testName;
        private ValidationCase _case;
		private DoseValue _oldDose;
        private double _oldDoseDouble; // probably don't need
		private DoseValue _newDose;
		private double _percentDifference;
        private double _tolerance;
        private bool _result;
		
		//public string OldDoseText { get { return _oldDose.ToString(); } }
        public string OldDoseText { get { return $"{String.Format(CultureInfo.InvariantCulture, "{0:N1}", _oldDose.Dose)} { _oldDose.UnitAsString}"; } }
        //public string NewDoseText { get { return _newDose.ToString(); } }
        public string NewDoseText { get { return $"{String.Format(CultureInfo.InvariantCulture, "{0:N1}", _newDose.Dose)} { _newDose.UnitAsString}"; } }
        public string ToleranceText { get { return String.Format("{0:0.00}%", _tolerance); } }
        public string TestName { get { return _testName; } }
		public string PercentDifferenceText { get { return String.Format("{0:0.00}%", _percentDifference); } }
        public double PercentDifference { get { return _percentDifference; } set { _percentDifference = value; } }
        public bool Result
        {
            get { return _result; }
            set { _result = value; }
        }
        public string ResultText
        {
            get
            {
                if (_result)
                {
                    return "Pass";
                }
                else
                {
                    return "Warning";
                }
            }
        }

		public ValidationTest(ValidationCase valCase, Beam beam)
		{
			_case = valCase;

			foreach (ReferencePoint refPoint in _case.ReferencePlan.ReferencePoints)
			{
			}
		}

        public ValidationTest(string name, DoseValue oldVal, DoseValue newVal, ValidationCase valCase)  // think 
        {
			_case = valCase;
            _testName = name;
            _oldDose = oldVal;
            _newDose = newVal;
			if (_case.Group.Name.ToLower().Contains("electron"))
				_tolerance = Globals.Instance.GetElectronTolerance();
			else
				_tolerance = Globals.Instance.GetPhotonTolerance();
            RunTestEvaluation();
        }

        public void RunTestEvaluation()  // think this is good to go
        {
            PercentDifference = (1 - (_newDose / _oldDose))*100;

            if (!Double.IsNaN(_tolerance))
            {
                if (Math.Abs(PercentDifference)<= _tolerance)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
                }
            }
            else { System.Windows.MessageBox.Show("No Tolerance Value Set"); }
        }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
