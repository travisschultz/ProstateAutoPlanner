using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Input;
using ProstateAutoPlanner;

// Lines below need to be uncommmented to write
[assembly: ESAPIScript(IsWriteable = true)]
//[assembly: AssemblyVersion("1.2.0.0")]

namespace VMS.TPS
{
    public class Script
    {
		public void Execute(ScriptContext context)
		{
			var window = new MainWindow();
			window.KeyDown += (object sender, KeyEventArgs e) => { if (e.Key == Key.Escape) window.Close(); };

			if (context.PlanSetup == null)
			{
				MessageBox.Show("Please open a single plan (not a plan sum) before running this script", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				throw new ApplicationException("Please open a single plan (not a plan sum) before running this script", new NullReferenceException());
			}


			ViewModel viewModel = new ViewModel(context);
			window.DataContext = viewModel;

			window.ShowDialog();
		}

	}
}
