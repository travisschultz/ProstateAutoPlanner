using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ProstateAutoPlanner
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			//DataContext = new ViewModel();
		}

		private void Button_Click_CreateProstateOptiRings(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Prostate Opti Rings button was clicked.");
			ViewModel vm = DataContext as ViewModel;
			vm.Patient.BeginModifications();
			vm.Plan.ExternalPlanSetup.StructureSet.AddStructure("Control", "OptiStruct");
			
		}

		private void Button_Click_CreatePTV(object sender, RoutedEventArgs e)
		{
			try
            {
				MessageBox.Show("Create Struct Clicked");

				bool isBaseStructureGood = false;
				bool isPTVGood = false;
				bool areExpansionsGood = false;
				string failMessage = "";
				double symExpansionMargin;
				

				ViewModel vm = DataContext as ViewModel;

				ExternalPlanSetup plan = vm.Plan.ExternalPlanSetup;
				
				// Check to see if base structure(s) are present and contain contours

				if(baseTargetList.SelectedItems.Count >= 1)
                {
					isBaseStructureGood = true;
                }
				else
                {
					failMessage += "Base Structure not selected\n";
                }

				// Check to see if expansions are present and good

				if(cbAsymExpand.IsChecked == true && cbSymExpand.IsChecked==true)
                {
					failMessage += "Must select only one expansion method\n";
                }
				else if (cbAsymExpand.IsChecked == true)
				{
					// Check for all
                }
				else if (cbSymExpand.IsChecked == true)
				{
					bool isDouble = Double.TryParse(tbSymm.Text, out symExpansionMargin);
					if (isDouble)
                    {
						areExpansionsGood = true;
                    }
				}
				else
                {
					failMessage += "Must select an expantion method\n";
                }

				// Check to see if PTV target is there and good

				// If all checks out then create the structures
				if (isBaseStructureGood && isPTVGood && areExpansionsGood)
                {

					StructureSet ss = vm.Plan.ExternalPlanSetup.StructureSet;
					vm.Patient.BeginModifications();
					
					if (baseTargetList.SelectedItems.Count == 1)
                    {
						ApStructure selectedStructure = baseTargetList.SelectedItem as ApStructure;

						if (cbSymExpand.IsChecked == true)
                        {
							Structure baseStruct = ss.Structures.Where(x => x.Id == selectedStructure.Id).FirstOrDefault();
							ss.Structures.Where(x=> x.Id == tbTargetVolumeId.Text).FirstOrDefault().SegmentVolume = baseStruct.Margin(Double.Parse(tbSymm.Text));
						}
                    }
                }
            }
			catch (Exception ex)
            {
				System.Windows.MessageBox.Show(ex.Message);
            }
			
		
		}

		private void Button_Click_RunEvaluation(object sender, RoutedEventArgs e)
		{
			ViewModel vm = DataContext as ViewModel;

			

			if (vm.SelectedMachine == "Select Machine")
			{
				MessageBox.Show("Please select a machine from the dropdown before running", "No Machine Selected", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}


			Globals.Instance.ClearLog();

			vm.Machines.Clear();

            foreach (String id in Xml.GetPatientIDs())
            {
                Patient patient = vm.App.OpenPatientById(id);

				//only run if we are on the selected machine or we want to run for all machines
				if (vm.SelectedMachine == "All Machines" || patient.FirstName == vm.SelectedMachine)
				{
					vm.UpdateStatus($"Running Evaluation on {patient.Name}...");
					vm.Machines.Add(new Machine(patient));
				}

                vm.App.ClosePatient();
            }
            
            vm.UpdateStatus("");

			DisplayLogWindow();
		}

		private void Button_Click_RunAll(object sender, RoutedEventArgs e)
		{
		}

		//loop through photon plans in patients and display somewhere any plans that the algorithm isn't available for and will skip if you continue to use this selection
		private void ComboBox_ValidatePhotonSelection(object sender, RoutedEventArgs e)
		{
		}

		private void DisplayLogWindow()
		{
			if (Globals.Instance.GetLog() != "")
				MessageBox.Show(Globals.Instance.GetLog(), "Error Log", MessageBoxButton.OK, MessageBoxImage.Hand);
		}

		private void Button_Click_PrintToCSV(object sender, RoutedEventArgs e)
		{
			//validate that evaluation has been run

			// Displays a SaveFileDialog so the user can save the Image  
			// assigned to Button2.  
			System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog
			{
				Filter = "Text Document (*.txt)|*.txt",
				Title = "Save Comma Separated Values (CSV) File of TPS Validation",
				FileName = "TPS Validation",
				RestoreDirectory = true

			};
			saveFileDialog1.ShowDialog();

			// If the file name is not an empty string open it for saving.  
			if (saveFileDialog1.FileName != "")
			{
				ViewModel vm = DataContext as ViewModel;

				string text = "MachineID,CourseID,PlanID - Field Name,Reference Point,Baseline Dose,Validation Dose,Percent Difference,Result" + Environment.NewLine;

				foreach (Machine m in vm.Machines)
				{
					foreach (ValidationGroup vg in m.Groups)
					{
						foreach (ValidationCase vc in vg.Cases)
						{
							foreach (ValidationTest vt in vc.ValidationTests)
							{
								text += m.MachineID + "," + vg.Name + "," + vc.Name + "," + vt.TestName + "," + vt.OldDoseText + "," + vt.NewDoseText + "," + vt.PercentDifferenceText + "," + vt.Result.ToString() + Environment.NewLine;
							}
						}
					}
				}

				File.WriteAllText(saveFileDialog1.FileName, text);
			}
		}

		private void Button_Click_ErrorLog(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(Globals.Instance.GetLog(), "Error Log", MessageBoxButton.OK, MessageBoxImage.Hand);
		}

		void Button_Click_PrintToPDF(object sender, RoutedEventArgs e)
		{
			ViewModel vm = DataContext as ViewModel;

			if (vm.Machines.Count == 0)
			{
				MessageBox.Show("Please run evaluation first", "No Evaluation", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			
			var reportData = CreateReportData();

			System.Windows.Forms.PrintDialog printDlg = new System.Windows.Forms.PrintDialog();
			MigraDocPrintDocument printDoc = new MigraDocPrintDocument();
			printDoc.Renderer = new MigraDoc.Rendering.DocumentRenderer(CreateReportData());
			printDoc.Renderer.PrepareDocument();

			printDoc.DocumentName = Window.GetWindow(this).Title;
			//printDoc.PrintPage += new PrintPageEventHandler(printDoc_PrintPage);
			printDlg.Document = printDoc;
			printDlg.AllowSelection = true;
			printDlg.AllowSomePages = true;
			//Call ShowDialog
			if (printDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				printDoc.Print();
		}

		private Document CreateReportData()
		{
			ViewModel vm = DataContext as ViewModel;

			Document doc = new Document();
			Internal.CustomStyles.Define(doc);
			Section section = new Section();

			// Set up page
			section.PageSetup.PageFormat = PageFormat.Letter;

			section.PageSetup.LeftMargin = Internal.Size.LeftRightPageMargin;
			section.PageSetup.TopMargin = Internal.Size.TopBottomPageMargin;
			section.PageSetup.RightMargin = Internal.Size.LeftRightPageMargin;
			section.PageSetup.BottomMargin = Internal.Size.TopBottomPageMargin;

			section.PageSetup.HeaderDistance = Internal.Size.HeaderFooterMargin;
			section.PageSetup.FooterDistance = Internal.Size.HeaderFooterMargin;

			// Add heder and footer
			new Internal.HeaderAndFooter().Add(section, vm);

			// Add contents
			//new Internal.MachineInfo().Add(section);
			new Internal.ValidationTableContent().Add(section, vm);

			doc.Add(section);

			return doc;
		}

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void tbTargetVolumeId_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
