using System;
using System.Net;

using Microsoft.Synchronization.ClientServices;
using Sqo;
using Sqo.Attributes;
using System.ComponentModel;
#if SILVERLIGHT
//using System.ComponentModel.DataAnnotations;
#endif

namespace SiaqodbSyncProvider
{
     [System.Reflection.Obfuscation(Exclude = true)]
    public class SiaqodbOfflineEntity : SqoDataObject, IOfflineEntity
    {
        #region IOfflineEntity Members

        private bool isTombstone;
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
        internal string _idMeta;
        [Sqo.Attributes.MaxLength(250)]
        internal string _idMeta2;
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
                    _entityMetadata.Id = this.IdMeta2;
                    _entityMetadata.IsTombstone = this.isTombstone;
                }
                return _entityMetadata;
            }
            set
            {
                this._entityMetadata = value;
                this._etag = this._entityMetadata.ETag;
                this._idMeta2 = this._entityMetadata.Id;
                {
                    this._idMetaHash = this._idMeta2.GetHashCode();
                }
                else
                {
                    this._idMetaHash = 0;
                }
                this.isTombstone = this._entityMetadata.IsTombstone;
                _idMeta = null;
            }
        }
        internal string IdMeta2
        {
            get
            {
                {
                    _idMeta2 = _idMeta;
                    _idMeta = null;
                }
                return _idMeta2;
            }
            
        }
        private bool MetaOldIsValid(string idmeta)
        {
            if (!string.IsNullOrEmpty(idmeta))
            {
                Uri uriResult;
                bool result = Uri.TryCreate(idmeta, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                return result;
            }
            return false;
        }
        
    }
}
