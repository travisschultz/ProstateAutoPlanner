using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using VMS.TPS.Common.Model.API;


namespace ProstateAutoPlanner
{
    public class ApPlan:INotifyPropertyChanged
    {
		private ExternalPlanSetup _externalPlanSetup;
		private ScriptContext _context;

		private double _totalDose;
		private double _dosePerFraction;
		private int _fractions;

		private string _name;
		private string _id;
		private string _comment;
	

		private string _targetVolumeId;


		public String Id {  get { return _id; } set { _id = value; OnPropertyChanged("Id"); } }
		public String Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }
		public String Comment { get { return _comment; } set { _comment = value;OnPropertyChanged("Comment"); } }

		// For the public setters putting logic in to calculate to make sure the relation is good

		public ExternalPlanSetup ExternalPlanSetup { get { return _externalPlanSetup; } set { _externalPlanSetup = value; OnPropertyChanged("ExternalPlanSetup"); } }
		public String TargetVolumeId 
		{ 
			get { return _targetVolumeId; }
			set
			{
				if (_externalPlanSetup.StructureSet.Structures.Where(x => x.Id == value).Count()==1)
                {
					_targetVolumeId = value;
					OnPropertyChanged("TargetVolumeId");
				}

				else
                {
					DialogResult dr = System.Windows.Forms.MessageBox.Show($"{value} is not in Structs, Should new structure be created when expanding margins?", "Struct Not There Dialog", MessageBoxButtons.YesNo);
					if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
						
						// add the structure and update the variable and also change the plan target structure
						System.Windows.MessageBox.Show($"{value} and in the loop");
						_context.Patient.BeginModifications();
						System.Windows.MessageBox.Show($"mods begun");
						_externalPlanSetup.StructureSet.AddStructure("PTV", value);
						bool wasTargetUpdated = _externalPlanSetup.SetTargetStructureIfNoDose(_externalPlanSetup.StructureSet.Structures.Where(x => x.Id == value).First(), new StringBuilder("Oops, Target not updated"));
						System.Windows.MessageBox.Show($"past the add");
						
						if (wasTargetUpdated)
                        {
							_targetVolumeId = value;
							OnPropertyChanged("TargetVolumeId");
						}
						else
                        {
							System.Windows.MessageBox.Show($"For some reason {value} couldn't be assigned as plan target");
                        }
						
						//System.Windows.MessageBox.Show($"{value} added as PTV strucutre");
												
                    }
					else if (dr == System.Windows.Forms.DialogResult.No)
                    {
						//Don't do anything just yet, maybe a message
                    }
                }
            }
		}
		public bool SetTargetVolumeId(String targId, PlanSetup ps)
        {
						
			if (ps.StructureSet.Structures.Where(x => x.Id == targId).Any()) // checks to make sure the target id string is in there
            {
				_targetVolumeId = targId;
				OnPropertyChanged("TargetVolumeId");
				return true;
            }
			else
            {
				return false;
            }
		}
		public bool SetRapidPlanModel(string rpm, string ps)
        {
			return false;
        }
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
					if (_totalDose / _dosePerFraction == Math.Round(_totalDose / _dosePerFraction, 0))
					{
						Fractions = Convert.ToInt32(_totalDose / _dosePerFraction);
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
				if (_dosePerFraction != 0) // dose per fraction has a value so calculate total dose
				{
					TotalDose = _dosePerFraction * _fractions;
				}

				else if (_totalDose != 0)
				{
					DosePerFraction = _totalDose / _fractions;
				}
				OnPropertyChanged("Fractions");
			}
		}

		public ApPlan(ScriptContext context)
        {
			_context = context;
			_externalPlanSetup = context.ExternalPlanSetup;

			Id = _externalPlanSetup.Id;
			Name = _externalPlanSetup.Name;
			TargetVolumeId = _externalPlanSetup.TargetVolumeID;

			

        }

		public bool CreateProsatateOptiRings()
        {
			if (_externalPlanSetup.StructureSet.Structures.First(x => x.Id == _targetVolumeId).IsEmpty) // check here to see if the target has contours
            {
				System.Windows.MessageBox.Show($"Target: {_targetVolumeId} is feeling empty. Hard to expand an empty structure");
				return false;
			}
			else
            {
				System.Windows.MessageBox.Show("I'm in the loop where I would create structures if I could write");



				return true;
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
