using System;
using SiaqodbManager.DataSourcesAdapters;
using SiaqodbManager.ViewModel;
using MonoMac.Foundation;

namespace SiaqodbManager
{
	public class QueryViewModelAdapter:AbstractViewModelAdapter
	{
		QueryViewModel viewModel;

		public QueryViewModelAdapter (QueryViewModel viewModel):base(viewModel)
		{
			this.viewModel = viewModel;
		}

		[Export("Linq")]
		public NSAttributedString Linq
		{
			get{
				var linq = viewModel.Linq;
				var attributedString = new NSAttributedString(linq.Trim(('\0')));
				return attributedString;
			}
			set{
				if(value != null){
					viewModel.Linq = value.Value;
				}
			}
		}

		[Export("ExecuteCommand")]
		public void ExecuteCommand(NSObject obj)
		{
			viewModel.Execute ();
		}
	}
}

