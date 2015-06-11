using System;
using System.Collections.Generic;
using System.Text;

namespace Dotissi.Meta
{
	class ObjectInfo
	{
		SqoTypeInfo ti;
		public ObjectInfo(SqoTypeInfo ti,object backendObj)
		{
			this.ti = ti;
            this.backendObj = backendObj;
		}
		
		public SqoTypeInfo SqoTypeInfo
		{
			get { return ti; }
		}
		private Dictionary<FieldSqoInfo,object> atInfo=new Dictionary<FieldSqoInfo,object>();
        public Dictionary<FieldSqoInfo, object> AtInfo
		{
			get { return atInfo; }
		}
		private int oid;

		public int Oid
		{
			get { return oid; }
			set { oid = value; }
		}
        private bool inserted;
        public bool Inserted
        {
            get { return inserted; }
            set { inserted = value; }
        }
        private ulong tickCount;
        public ulong TickCount
        {
            get { return tickCount; }
            set { tickCount = value; }
        }
        private object backendObj;
        public object BackendObject
        {
            get { return backendObj; }
            set { backendObj = value; }
        }
	
	}
}
