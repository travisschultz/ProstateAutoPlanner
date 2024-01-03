using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;



namespace ProstateAutoPlanner
{
	public class ViewModel : INotifyPropertyChanged
	{
		// OLD ONES (TPS VALIDATION)
		private string _status;
		private ObservableCollection<Machine> _machines;
		
		private List<String> _photonCalcModels;
		private List<String> _acurosCalcModels;
		private String _photonSelectionValidation;
		private Double _photonTolerance = 2;
		private Double _electronTolerance = 2;
		private List<String> _machineList;
		private String _selectedMachine;

		// NEW ONES FOR Prostate AutoPlan

		private ScriptContext _context;

		private ObservableCollection<ApStructure> _apStructures;
		private ApPlan _plan;

		private String _patientName;
		private String _patientId;
		private String _planName;
		private String _planId;
		private String _courseId;
		private Prescription _rx;
		private Patient _patient;


		private ObservableCollection<Prescription> _prescription;
		

		public VMS.TPS.Common.Model.API.Application App { get; set; }
		public string Status { get { return _status; } set { _status = value; OnPropertyChanged("Status"); } }

		public Patient Patient { get { return _patient; } set { _patient = value;OnPropertyChanged("Patient"); } }

		public ObservableCollection<Machine> Machines { get { return _machines; } set { _machines = value; OnPropertyChanged("Machines"); } }

		public ApPlan Plan { get { return _plan; } set { _plan = value;OnPropertyChanged("Plan"); } }
		public ObservableCollection<ApStructure > ApStructures { get { return _apStructures; } set { _apStructures = value; OnPropertyChanged("ApStructures"); } }
		public Prescription Rx { get { return _rx; } set { _rx = value; OnPropertyChanged("Rx"); } }
		public List<String> PhotonCalcModels { get { return _photonCalcModels; } set { _photonCalcModels = value; OnPropertyChanged("PhotonCalcModels"); } }
		public List<String> AcurosCalcModels { get { return _acurosCalcModels; } set { _acurosCalcModels = value; OnPropertyChanged("AcurosCalcModels"); } }
		public String PatientName { get { return _patientName; } set { _patientName = value; OnPropertyChanged("PatientName"); } }
		public String PatientId { get { return _patientId; } set { _patientId = value; OnPropertyChanged("PatientId"); } }
		public String CourseId {  get { return _courseId; } set { _courseId = value; OnPropertyChanged("CourseId"); } }
		public String PlanName { get { return _planName; } set { _planName = value; OnPropertyChanged("PlanName"); } }
		public String PlanId { get { return _planId; } set { _planId = value; OnPropertyChanged("PlanId"); } }
		public List<String> MachineList { get { return _machineList; } set { _machineList = value; OnPropertyChanged("MachineList"); } }
		public String SelectedMachine { get { return _selectedMachine; } set { _selectedMachine = value; OnPropertyChanged("SelectedMachine"); } }

		public ViewModel(ScriptContext context)
		{
			// Data for Prostate Auto Planner
			_context = context;
			ExternalPlanSetup eps = _context.ExternalPlanSetup;
			Patient = _context.Patient;

			PatientName = _context.Patient.Name;
			PatientId = _context.Patient.Id;
			CourseId = _context.Course.Id;
			PlanId = eps.Id;
			PlanName = eps.Name;

			Plan = new ApPlan(_context);

			Plan.DosePerFraction = eps.DosePerFraction.Dose;
			Plan.TotalDose = eps.TotalDose.Dose;
			Plan.Fractions = eps.NumberOfFractions.Value;
			Plan.SetTargetVolumeId(eps.TargetVolumeID,eps);

			ApStructures = new ObservableCollection<ApStructure>();

			foreach (Structure s in eps.StructureSet.Structures)
            {
				if (s.Id == "Prostate" || s.Id=="SeminalVes_Full"|| s.Id == "SeminalVes_Prox")
                {
					if (s.HasSegment)
                    {
						ApStructures.Add(new ApStructure(s.Id, s.Name, s.DicomType));
					}
					
                }
				else if (s.DicomType=="PTV"|| s.DicomType == "CTV" || s.DicomType == "GTV")
                {
					if (s.HasSegment)
                    {
						ApStructures.Add(new ApStructure(s.Id, s.Name, s.DicomType));
					}
					
				}
            }
			
			

			// THE OLD ONE

			//UpdateStatus("Gathering photon algorithms...");
            //PhotonCalcModels = new List<string>(GetPhotonCalcModels());
            //AcurosCalcModels = new List<string>(PhotonCalcModels.Where(x => x.ToLower().Contains("acurosxb")));
            //PhotonCalcModels = new List<string>(PhotonCalcModels.Where(x => !x.ToLower().Contains("acurosxb")));
            //UpdateStatus("");

            //Machines = new ObservableCollection<Machine>();

            //set up dropdown of machines
            //MachineList = new List<string>();
            //MachineList.Add("All Machines");
            //MachineList.Add("Select Machine");
            //SelectedMachine = "Select Machine";

        }

		private List<String> GetPhotonCalcModels()
		{
			List<String> modelList = new List<string>();

			//loop through all of the patients and pull models which are available to them
			foreach (String id in Xml.GetPatientIDs())
			{
				Patient patient = App.OpenPatientById(id);

				//loop through all of the nonelectron courses and plans
				foreach(Course course in patient.Courses.Where(c => c.Id != "Electron"))
				{
					foreach(ExternalPlanSetup plan in course.ExternalPlanSetups)
					{
						modelList = new List<String>(modelList.Union(plan.GetModelsForCalculationType(VMS.TPS.Common.Model.Types.CalculationType.PhotonVolumeDose)));
					}
				}

				App.ClosePatient();
			}

			return modelList;
		}

		public static void RefreshUI()
		{
			DispatcherFrame frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
			{
				frame.Continue = false;
				return null;
			}), null);

			Dispatcher.PushFrame(frame);
			//EDIT:
			System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
		}

		// Button Command Section




		public void UpdateStatus(string status)
		{
			Status = status;

			RefreshUI();
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
