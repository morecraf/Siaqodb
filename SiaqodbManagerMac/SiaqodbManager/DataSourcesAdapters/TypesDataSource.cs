using System;
using SiaqodbManager.ViewModel;
using System.Collections.Generic;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace SiaqodbManager.DataSourcesAdapters
{
	public class TypesDataSource:NSOutlineViewDataSource
	{
		public List<MetaTypeViewModelAdapter> Types;

		public TypesDataSource (List<MetaTypeViewModelAdapter> types)
		{
			Types = types;
		}

		public override int GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			// If the item is not null, return the child count of our item
			if(item != null)
				return (item as MetaTypeViewModelAdapter).Fields.Count;
			// Its null, that means its asking for our root element count.
			return Types.Count;
		}

		public override NSObject GetObjectValue (NSOutlineView outlineView, NSTableColumn forTableColumn, NSObject byItem)
		{
			// Is it null? (It really shouldnt be...)
			if (byItem != null) {
				// Jackpot, typecast to our Person object
				var name = new NSString("");
				if(byItem is MetaTypeViewModelAdapter){
					var p = (MetaTypeViewModelAdapter)(byItem);
					name = new NSString(p.Name);
				}else if(byItem is MetaFieldViewModelAdapter){
					var p = (MetaFieldViewModelAdapter)(byItem);
					name = new NSString(p.Name);
				}
				return name;
			}
			// Oh well.. errors dont have to be THAT depressing..
			return (NSString)"Not enough jQuery";
		}

		public override NSObject GetChild (NSOutlineView outlineView, int childIndex, NSObject ofItem)
		{
			// If the item is null, it's asking for a root element. I had serious trouble figuring this out...
			if(ofItem == null)
				return Types[childIndex];
			// Return the child its asking for.
			return (NSObject)((ofItem as MetaTypeViewModelAdapter).Fields[childIndex]);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			// Straight forward - it wants to know if its expandable.
			var result = false;
			if(item == null)
				return result;
			if(item is MetaTypeViewModelAdapter){
				result = (item as MetaTypeViewModelAdapter).Fields.Count > 0;
			}else if(item is MetaFieldViewModelAdapter){
				result = false;
			}
			return result;
		}
	}
}

