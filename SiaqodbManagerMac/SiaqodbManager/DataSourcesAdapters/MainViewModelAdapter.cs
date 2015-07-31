using System;
using SiaqodbManager.ViewModel;
using MonoMac.Foundation;
using System.Collections.Generic;
using System.Linq;
using SiaqodbManager.MacWinInterface;

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

	
		public List<MetaTypeViewModelAdapter> TypesList {
			get{
				return viewModel.TypesList
					.Select(type=>new MetaTypeViewModelAdapter(type))
					.ToList();
			}
		}

		public ObjectViewModelAdapter CreateObjectsView (MetaTypeViewModelAdapter metaType,List<int> oids)
		{
			var objectsViewModel = viewModel.CreateObjectsModel (metaType.viewModel,oids);
			var objectAdapter = new ObjectViewModelAdapter (objectsViewModel);
			return objectAdapter;
		}

		public QueryViewModelAdapter CreateQueryView (IDialogService saveDialog)
		{
			return new QueryViewModelAdapter (new QueryViewModel(saveDialog,viewModel));
		}

		[Export("ConnectCommand")]
		void OnConnect(NSObject obj){
			viewModel.ConnectCommand.Execute (obj);
		}
	}
}

