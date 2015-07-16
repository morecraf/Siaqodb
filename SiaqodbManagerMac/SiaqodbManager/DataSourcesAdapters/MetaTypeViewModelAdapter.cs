using System;
using SiaqodbManager.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace SiaqodbManager.DataSourcesAdapters
{
	public class MetaTypeViewModelAdapter:AbstractViewModelAdapter
	{
		public MetaTypeViewModel viewModel;

		public MetaTypeViewModelAdapter (MetaTypeViewModel viewModel) :base(viewModel)
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

		public List<MetaFieldViewModelAdapter> Fields{
			get{
				return viewModel.Fields
					.Select(field => new MetaFieldViewModelAdapter(field))
					.ToList();
			}
		}
	}
}

