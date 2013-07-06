using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Meta;

namespace Sqo.Meta
{
    class TypeHeader
    {
        public int headerSize;
        public int typeNameSize;
        public DateTime lastUpdated;
        public int numberOfRecords;
        public int positionFirstRecord;
        public int lengthOfRecord;
        public int version = -35;//version 3.5
        public int NrFields;
        public int TID;
        public int Unused1;
        public int Unused2;
        public int Unused3;
       
    }
    class AttributeHeader
    {
        private int sizeOfName;
        public int SizeOfName
        {
            get { return sizeOfName; }
            set { sizeOfName = value; }
        }
        int length;
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        int position;
        public int PositionInRecord
        {
            get { return position; }
            set { position = value; }
        }
        int realLength;
        public int RealLength
        {
            get { return realLength; }
            set { realLength = value; }
        }

    }
}
