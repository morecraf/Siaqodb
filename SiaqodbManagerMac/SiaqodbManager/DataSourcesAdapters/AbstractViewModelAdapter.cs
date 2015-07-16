using System;
using MonoMac.Foundation;
using System.ComponentModel;

namespace SiaqodbManager.DataSourcesAdapters
{
	public abstract class AbstractViewModelAdapter:NSObject
	{
		protected AbstractViewModelAdapter (INotifyPropertyChanged viewModel)
		{
			if (viewModel == null)
				return;
			viewModel.PropertyChanged+=OnPropertyChange;
		}
		protected void OnPropertyChange (object sender, PropertyChangedEventArgs e)
		{
			MainWindowController.BindHandler.Invoke (this,e);
		}
		protected void OnPropertyChange (string propertyName)
		{
			OnPropertyChange (this,new PropertyChangedEventArgs(propertyName));
		}
	}
}

