using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Collections.Generic;
using SiaqodbManager.ViewModel;

namespace SiaqodbManager
{
	public class ObjectsDataSource:NSTableViewDataSource
	{

	
		// Dict key is section name, value is a sorted list of section element (result it comes from and the index in it) with key being the url (to avoid duplicates)
		//Dictionary<string, SortedList<string, Tuple<Result, int>>> sections = new Dictionary<string, SortedList<string, Tuple<Result, int>>> ();
		List<int> data = new List<int> ();

		NSTextFieldCell normalCell;
		NSTableHeaderCell headerCell;
		ObjectViewModelAdapter viewModel;

		public ObjectsDataSource (ObjectViewModelAdapter viewModel)
		{
			this.viewModel = viewModel;

			normalCell = new NSTextFieldCell ();

			headerCell = new NSTableHeaderCell ();
			headerCell.LineBreakMode = NSLineBreakMode.TruncatingMiddle;
			headerCell.FocusRingType = NSFocusRingType.None;
			headerCell.Editable = false;
			headerCell.Selectable = false;
		}

//		public void AddResultSet (Result result)
//		{
//			for (int i = 0; i < result.Count; i++) {
//				string fullTitle = result.GetFullTitle (i);
//				string section = string.IsNullOrWhiteSpace (fullTitle) ? "Other" : fullTitle.Split (':')[0];
//				SortedList<string, Tuple<Result, int>> sectionContent;
//				var newItem = Tuple.Create (result, i);
//				var url = result.GetUrl (i);
//
//				if (!sections.TryGetValue (section, out sectionContent))
//					sections[section] = new SortedList<string, Tuple<Result, int>> () { { url, newItem } };
//				else
//					sectionContent[url] = newItem;
//			}
//			// Flatten everything back to a list
//			data.Clear ();
//			foreach (var kvp in sections) {
//				data.Add (new ResultDataEntry { SectionName = kvp.Key });
//				foreach (var item in kvp.Value)
//					data.Add (new ResultDataEntry { ResultSet = item.Value.Item1, Index = item.Value.Item2 });
//			}
//		}

//		public void ClearResultSet ()
//		{
//			sections.Clear ();
//		}

		public override int GetRowCount (NSTableView tableView)
		{
			return viewModel.GetNrOfObjects();
		}
			

//		public override NSCell GetCell (NSTableView tableView, NSTableColumn tableColumn, int row)
//		{
//			if (tableView == null)
//				return null;
//			var value = data[row];
//			return !string.IsNullOrEmpty (value) ? headerCell : normalCell;
//		}
			

		public override void SetObjectValue (NSTableView tableView, NSObject theObject, NSTableColumn tableColumn, int row)
		{
			var valueKey = (string)(NSString)tableColumn.Identifier;
			//var dataRow = _app.villains[rowIndex];



//			switch((string)valueKey)
//			{
//			case "name":
//				dataRow.Name = (string)(NSString)theObject;
//				break;
//			case "mugshot":
//				dataRow.Mugshot = (NSImage)theObject;
//				break;
//			case "lastSeenDate":
//				dataRow.LastSeenDate = (NSDate)theObject;
//				break;
//			}

			//_app.UpdateDetailViews();
		}


		public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
		{
			var valueKey = tableColumn.HeaderCell.Identifier;
			if(valueKey != null){
				var value = viewModel.GetValue(valueKey,row);
				return NSObject.FromObject(value);
			}
			return new NSString("0");
		}

		// Keep the search term in memory so that heavy search can check if its result are still fresh enough
		public string LatestSearchTerm { get; set; }
	}
}

