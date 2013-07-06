using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo
{
	public class SavingEventsArgs:EventArgs
	{
		public bool Cancel { get; set; }
		internal SavingEventsArgs(Type t, Object o)
		{
			this.oType = t;
			this.o = o;
            
		}
		private Type oType;
		private Object o;
		public Type ObjectType { get{return oType;}}
		public object Object { get{return o;} }
	}
	public class SavedEventsArgs : EventArgs
	{
		internal SavedEventsArgs(Type t, Object o)
		{
			this.oType = t;
			this.o = o;
		}
		private Type oType;
		private Object o;
		public Type ObjectType { get{return oType;}}
		public object Object { get{return o;} }
        bool inserted;
        public bool Inserted
        {
            get { return inserted; }
            internal set { inserted = value; }
        }
	}
	public class DeletingEventsArgs : EventArgs
	{
		public bool Cancel { get; set; }
		internal DeletingEventsArgs(Type t, int oid)
		{
			this.oType = t;
			this.oid = oid;
		}
		private Type oType;
		private int oid;
		public Type ObjectType { get { return oType; } }
		public int Object { get { return oid; } }
	}
	public class DeletedEventsArgs : EventArgs
	{
		internal DeletedEventsArgs(Type t, int oid)
		{
			this.oType = t;
			this.oid = oid;
		}
		private Type oType;
		private int oid;
		public Type ObjectType { get { return oType; } }
		public int OID { get { return oid; } }
	}
    public class LoadingObjectEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        private Type oType;
        private int oid;
        public Type ObjectType { get { return oType; } }
        public int OID { get { return oid; } }
        public Object Replace { get; set; }
        internal LoadingObjectEventArgs(int oid, Type objectType)
        {
            this.oid = oid;
            this.oType = objectType;
        }
    }
    public class LoadedObjectEventArgs : EventArgs
    {
        private int oid;
        private object obj;
        public Object Object { get { return obj; } }
        public int OID { get { return oid; } }

        internal LoadedObjectEventArgs(int oid, object obj)
        {
            this.oid = oid;
            this.obj = obj;
        }
    }
    public class IndexesSaveAsyncFinishedArgs:EventArgs
    {
        public Exception Error { get; set; }
        public bool Succeeded { get; set; }
    }
}
