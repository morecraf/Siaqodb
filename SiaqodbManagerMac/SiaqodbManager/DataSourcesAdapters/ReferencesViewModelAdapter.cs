using System;
using SiaqodbManager.ViewModel;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace SiaqodbManager.DataSourcesAdapters
{
	public class ReferencesViewModelAdapter:AbstractViewModelAdapter
	{
		ReferencesViewModel viewModel;

		public ReferencesViewModelAdapter (ReferencesViewModel viewModel):base(viewModel)
		{
			this.viewModel = viewModel;
		}

		[Export("References")]
		public string[] References{
			get{
				return viewModel.References.Select (r => r.Item).ToArray();
			}
		}
		[Export("Namespaces")]
		public string Namespaces{
			get{
				return viewModel.Namespaces;
			}
			set{
				viewModel.Namespaces = value;
			}
		}


		[Export("AddDefaultCommand")]
		public void AddDefaultCommand(NSObject obj){
			viewModel.References.Add(new ReferenceItem{ Item = "System.dll"});
			viewModel.References.Add(new ReferenceItem { Item = "System.Core.dll" });
			viewModel.References.Add(new ReferenceItem { Item = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "Siaqodb.dll"});
			viewModel.References.Add(new ReferenceItem { Item = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "SiaqodbPortable.dll" });
			OnPropertyChange ("References");
		}
		[Export("AddCommand")]
		public void AddCommand(NSObject obj){
			viewModel.AddCommand.Execute (null);
		}
		[Export("RemoveCommand")]
		public void RemoveCommand(NSObject obj){
			viewModel.RemoveCommand.Execute (null);
		}
		[Export("LoadReferencesCommand")]
		public void LoadReferencesCommand(NSObject obj){
			viewModel.LoadReferencesCommand.Execute (null);
		}

		[Export("observeValueForKeyPath:ofObject:change:context:")]
		private void observeValueForKeyPath(NSString keyPath, NSArrayController ofObject, NSDictionary change, IntPtr context)
		{
			if (ofObject.SelectedObjects.Count()==0)
				return;
			if (context.Equals (IntPtr.Zero)) {
				var reference = ofObject.SelectedObjects [0].ToString();
				viewModel.SelectedRef = new ReferenceItem{ Item = reference };
				return;
			}
		}
	}
}

