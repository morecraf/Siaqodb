using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dotissi.Meta;
using System.IO;

#if ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Dotissi.Core
{
    class SerializerFactory
    {

        static Dictionary<string, ObjectSerializer> serializers = new Dictionary<string, ObjectSerializer>();
        static readonly object _syncRoot = new object();
        public static ObjectSerializer GetSerializer(string folderPath, string typeName,bool useElevatedTrust)
        {
            return GetSerializer(folderPath, typeName, useElevatedTrust, "sqo");
        }
        public static ObjectSerializer GetSerializer(string folderPath, string typeName, bool useElevatedTrust, string fileExtension)
        {
            string fileFull = null;
            if (Sqo.SiaqodbConfigurator.EncryptedDatabase)
            {
                fileFull = folderPath + Path.DirectorySeparatorChar + typeName + ".e"+fileExtension;
            }
            else
            {
                fileFull = folderPath + Path.DirectorySeparatorChar + typeName + "."+fileExtension;
            }
            lock (_syncRoot)
            {
                if (serializers.ContainsKey(fileFull))
                {
                    if (serializers[fileFull].IsClosed)
                    {
                        serializers[fileFull].Open(useElevatedTrust);
                    }

                    return serializers[fileFull];
                }
                else
                {
                    ObjectSerializer ser = new ObjectSerializer(fileFull, useElevatedTrust);
                    serializers[fileFull] = ser;
                    return ser;
                }
            }
        }
#if WinRT

#endif
        public static void CloseAll()
        {
            lock (_syncRoot)
            {
                foreach (string key in serializers.Keys)
                {
                    serializers[key].Close();
                }
                serializers.Clear();
            }

        }
#if ASYNC_LMDB
        public static async Task CloseAllAsync()
        {

            foreach (string key in serializers.Keys)
            {
                await serializers[key].CloseAsync().ConfigureAwait(false);
            }


        }

#endif
		public static void FlushAll()
		{
            lock (_syncRoot)
            {
                foreach (string key in serializers.Keys)
                {
                    serializers[key].Flush();
                }
            }

		}
#if ASYNC_LMDB
        public static async Task FlushAllAsync()
        {

            foreach (string key in serializers.Keys)
            {
                await serializers[key].FlushAsync().ConfigureAwait(false);
            }

        }

#endif
        public static void ClearCache(string folderName)
        {
            lock (_syncRoot)
            {
                List<string> keysToBeRemoved = new List<string>();
                foreach (string key in serializers.Keys)
                {
                    if (Path.GetDirectoryName(key).TrimEnd(Path.DirectorySeparatorChar) == folderName.TrimEnd(Path.DirectorySeparatorChar))
                    {
                        serializers[key].Close();
                        keysToBeRemoved.Add(key);
                    }
                }
                foreach (string k in keysToBeRemoved)
                {
                    serializers.Remove(k);
                }
            }
        }
    }
}
