using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace ProstateAutoPlanner
{
    public class ApStructure : INotifyPropertyChanged
    {
		//
		private string _id;
		private string _name;
		private string _volumeType;
		private string _comment;

		public String Id { get { return _id; } set { _id = value; OnPropertyChanged("Id"); } }
		public String Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }
		public String VolumeType { get { return _volumeType; } set { _volumeType = value; OnPropertyChanged("VolumeType"); } }
		public String Comment { get { return _comment; } set { _comment = value;OnPropertyChanged("Comment"); } }


		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public ApStructure(string id, string name, string volumetype)
        {
			_id = id;
			_name = name;
			_volumeType = volumetype;
        }
	}
}

