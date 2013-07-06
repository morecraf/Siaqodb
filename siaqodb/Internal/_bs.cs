using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Sqo.Internal
{
    /// <summary>
    /// Do NOT use it!, it is used only internally
    /// </summary>
    #if KEVAST
    internal
#else
        public
#endif
        static class _bs
    {
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Siaqodb _b(string p)
        {
            return new Siaqodb(p, false);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Siaqodb _ofm(string p,string option)
        {
            return new Siaqodb(p, option);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static void _uf(Siaqodb siaqodb, int oid, MetaType metaType, string field, object value)
        {
            siaqodb.UpdateField(oid, metaType, field, value);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static List<object> _gd(Siaqodb siaqodb, Type type)
        {
            return siaqodb.LoadDirtyObjects(type);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static void _do(Siaqodb siaqodb, int oid, MetaType metaType)
        {
            siaqodb.DeleteObjectByMeta(oid, metaType);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static int _io(Siaqodb siaqodb,  MetaType metaType)
        {
            return siaqodb.InsertObjectByMeta(metaType);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static void _sdbfn(Siaqodb siaqodb, MetaType metaType,string fileName)
        {
             siaqodb.SetDatabaseFileName(fileName,metaType);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static void  _loidtid(Siaqodb siaqodb,int oid, MetaType metaType, string fieldName,ref List<int> listOIDs,ref int TID)
        {
             siaqodb.LoadObjectOIDAndTID(oid, fieldName, metaType,ref listOIDs,ref TID);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static void _ltid(Siaqodb siaqodb, int oid, MetaType metaType, string fieldName, ref int TID,ref bool isArray)
        {
            siaqodb.LoadTIDofComplex(oid, fieldName, metaType, ref TID, ref isArray);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static void _loidby(Siaqodb siaqodb, string fieldName, object obj)
        {
            siaqodb.GetOIDForAMSByField(obj, fieldName);
        }
        /// <summary>
        /// Do NOT use it!, it is used only internally
        /// </summary>
        public static object _lobjby(Siaqodb siaqodb, Type type, int oid)
        {
            return siaqodb.LoadObjectByOID(type, oid);
        }
    }
}
