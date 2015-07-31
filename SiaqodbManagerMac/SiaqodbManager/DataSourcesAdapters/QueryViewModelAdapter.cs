using System;
using SiaqodbManager.DataSourcesAdapters;
using SiaqodbManager.ViewModel;
using MonoMac.Foundation;
using SiaqodbManager.Entities;

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
			
		[Export("SaveCommand")]
		public void SaveCommand(NSObject obj)
		{
			viewModel.SaveAs ();
		}

		public EventHandler<LinqEventArgs> LinqExecuted {
		   get{
				return viewModel.LinqExecuted;
			}
			set{
				viewModel.LinqExecuted = value;
			}
		}
	}
}

