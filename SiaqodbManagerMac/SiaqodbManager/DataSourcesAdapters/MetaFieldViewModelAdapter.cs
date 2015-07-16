using System;
using SiaqodbManager.ViewModel;

namespace SiaqodbManager.DataSourcesAdapters
{
	public class MetaFieldViewModelAdapter:AbstractViewModelAdapter
	{
		MetaFieldViewModel viewModel;
		public MetaFieldViewModelAdapter (MetaFieldViewModel viewModel):base(viewModel)
		{
			this.viewModel = viewModel;
		}

		public string Name {
			get{
				return viewModel.Name;
			}
			set{
				viewModel.Name = value;
			}
		}
	}
}

