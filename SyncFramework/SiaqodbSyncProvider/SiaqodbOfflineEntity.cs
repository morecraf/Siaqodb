using System;
using System.Net;

using Microsoft.Synchronization.ClientServices;
using Sqo;
using Sqo.Attributes;
using System.ComponentModel;
#if SILVERLIGHT
//using System.ComponentModel.DataAnnotations;
#endif
using Newtonsoft.Json;
namespace SiaqodbSyncProvider
{
     [System.Reflection.Obfuscation(Exclude = true)]
    public class SiaqodbOfflineEntity : SqoDataObject, IOfflineEntity
    {
        #region IOfflineEntity Members

        private bool isTombstone;
         [JsonProperty]
        internal bool IsTombstone
        {
            get
            {
                return isTombstone;
            }
            set
            {
                this.isTombstone = value;
            }
        }

        #endregion

        private bool isDirty;
        public bool IsDirty { get { return this.isDirty; } set { this.isDirty = value; } }

        private ulong tickCount;
#if SILVERLIGHT
        protected override object GetValue(System.Reflection.FieldInfo field)
        {
            if (field.DeclaringType == typeof(SiaqodbOfflineEntity))
                return field.GetValue(this);
            else
                return base.GetValue(field);
        }
        protected override void SetValue(System.Reflection.FieldInfo field, object value)
        {
            if (field.DeclaringType == typeof(SiaqodbOfflineEntity))
                field.SetValue(this, value);
            else
                base.SetValue(field, value);
        }
#endif
        private string _etag;
        [Sqo.Attributes.Text]
        [JsonProperty]
        internal string _idMeta;
         //used only for indexes
         [Index]
        internal int _idMetaHash;

        [Ignore]
        private OfflineEntityMetadata _entityMetadata;


        [EditorBrowsable(EditorBrowsableState.Never)]
#if SILVERLIGHT
        // [Display(AutoGenerateField = false)]
#endif
        public OfflineEntityMetadata ServiceMetadata
        {
            get
            {
                if (_entityMetadata == null)
                {
                    _entityMetadata = new OfflineEntityMetadata();
                    _entityMetadata.ETag = this._etag;
                    _entityMetadata.Id = this._idMeta;
                    _entityMetadata.IsTombstone = this.isTombstone;
                }
                return _entityMetadata;
            }
            set
            {
                this._entityMetadata = value;
                this._etag = this._entityMetadata.ETag;
                this._idMeta = this._entityMetadata.Id;
                if (this._idMeta != null)
                {
                    this._idMetaHash = this._idMeta.GetHashCode();
                }
                else
                {
                    this._idMetaHash = 0;
                }
                this.isTombstone = this._entityMetadata.IsTombstone;
            }
        }

        
    }
     [System.Reflection.Obfuscation(Exclude = true)]
     enum DirtyOperation { Inserted = 1, Updated = 2, Deleted = 3 }
     [System.Reflection.Obfuscation(Exclude = true)]
     class DirtyEntity
     {
         public int OID { get; set; }
         public int EntityOID;
         [MaxLength(200)]
         public string EntityType;
         public DirtyOperation DirtyOp;
         public byte[] TombstoneObj;
        
     }
}
