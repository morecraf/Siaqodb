using System;
using SiaqodbManager.ViewModel;
using MonoMac.Foundation;
using System.Collections.Generic;
using System.Linq;

namespace SiaqodbManager.DataSourcesAdapters
{
	public class MainViewModelAdapter:AbstractViewModelAdapter
	{
		MainViewModel viewModel;

		public MainViewModelAdapter (MainViewModel viewModel):base(viewModel)
		{
			this.viewModel = viewModel;
		}

		[Export("SelectedPath")]
		public string SelectedPath{
			get{
				return viewModel.SelectedPath.Item;
			}
			set{
				viewModel.SelectedPath.Item = value;
				OnPropertyChange ("SelectedPath");
			}
		}

		public object Siaqodb {
			get{
				return viewModel.Siaqodb;
			}
		}
	
		public List<MetaTypeViewModelAdapter> TypesList {
			get{
				return viewModel.TypesList
					.Select(type=>new MetaTypeViewModelAdapter(type))
					.ToList();
			}
		}

		public ObjectViewModelAdapter CreateObjectsView (MetaTypeViewModelAdapter metaType)
		{
			var objectsViewModel = viewModel.CreateObjectesView (metaType.viewModel);
			var objectAdapter = new ObjectViewModelAdapter (objectsViewModel);
			return objectAdapter;
		}

		[Export("ConnectCommand")]
		void OnConnect(NSObject obj){
			viewModel.ConnectCommand.Execute (obj);
		}
	}
}

