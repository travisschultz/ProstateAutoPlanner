using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace ProstateAutoPlanner
{
    public class Prescription : INotifyPropertyChanged
    {
		private double _totalDose;
		private double _dosePerFraction;
		private int _fractions;

		// For the public setters putting logic in to calculate to make sure the relation is good
		public double TotalDose 
		{ 
			get { return _totalDose; } 
			set 
			{ 
				_totalDose = value;

				if (_fractions > 0) 
				{
					if (_fractions * _dosePerFraction != _totalDose)
                    {
						DosePerFraction = _totalDose / _fractions;
                    }
				}

				OnPropertyChanged("TotalDose"); 
			} 
		}
		public double DosePerFraction 
		{ 
			get { return _dosePerFraction; } 
			set 
			{
				_dosePerFraction = value;
				if (_fractions != 0)
                {
					TotalDose = _dosePerFraction * _fractions;
				}


				// Need these to match up
				else if (_totalDose != 0)
				{
					if (_totalDose/_dosePerFraction == Math.Round(_totalDose / _dosePerFraction, 0))
					{
						Fractions =  Convert.ToInt32(_totalDose / _dosePerFraction);
					}
					
				}
				OnPropertyChanged("DosePerFraction");

			} 
		}
		public int Fractions 
		{ 
			get { return _fractions; } 
			set 
			{
				_fractions = value;
				if (_dosePerFraction !=0 ) // dose per fraction has a value so calculate total dose
                {
					TotalDose = _dosePerFraction * _fractions;
                }

				else if (_totalDose !=0)
                {
					DosePerFraction = _totalDose / _fractions;
                }
				OnPropertyChanged("Fractions"); 
			} 
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
