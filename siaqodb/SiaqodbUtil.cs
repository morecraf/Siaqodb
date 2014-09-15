using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.MetaObjects;
using Sqo.Core;
using System.IO;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;
#endif
#if WinRT
using Windows.Storage;
#endif
namespace Sqo
{
#if !WinRT
    /// <summary>
    /// Database utilities
    /// </summary>
    public class SiaqodbUtil
    {
        /// <summary>
        /// Rebuild and defragment indexes
        /// </summary>
        /// <param name="siaqodb"></param>
        public static void ReIndex(string databasePath)
        {
            Siaqodb siaqodb = new Siaqodb(databasePath);
            ReIndex(siaqodb);
        }
#if ASYNC
        public static async Task ReIndexAsync(string databasePath)
        {
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);
            await ReIndexAsync(siaqodb);
        }
#endif
        #if SL4
        /// <summary>
        /// Rebuild and defragment indexes
        /// </summary>
        /// <param name="siaqodb"></param>
        public static void ReIndex(string databasePath, Environment.SpecialFolder specialFolder)
        {
            Siaqodb siaqodb = new Siaqodb(databasePath,specialFolder);
            ReIndex(siaqodb);
        }
        #endif

        private static void ReIndex(Siaqodb siaqodb)
        {
            
            siaqodb.ReIndexAll(true);
            siaqodb.Close();
        }
#if ASYNC
        private static async Task ReIndexAsync(Siaqodb siaqodb)
        {

            await siaqodb.ReIndexAllAsync(true);
            await siaqodb.CloseAsync();
        }
#endif
        /// <summary>
        /// Shrink database files including rawdata.sqr and indexes 
        /// </summary>
        public static void Shrink(string databasePath, ShrinkType shrinkType)
        {
            Siaqodb siaqodb = new Siaqodb(databasePath);
            Shrink(siaqodb, shrinkType);
        }

#if SL4
        /// <summary>
        /// Shrink database files including rawdata.sqr and indexes 
        /// </summary>
        public static void Shrink(string databasePath, Environment.SpecialFolder specialFolder, ShrinkType shrinkType)
        {
            Siaqodb siaqodb = new Siaqodb(databasePath, specialFolder);
            Shrink(siaqodb, shrinkType);
        }

#endif
#if ASYNC
        /// <summary>
        /// Shrink database files including rawdata.sqr and indexes 
        /// </summary>
        public static async Task ShrinkAsync(string databasePath, ShrinkType shrinkType)
        {
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);
            await ShrinkAsync(siaqodb, shrinkType);
        }
#endif
        private static void Shrink(Siaqodb siaqodb, ShrinkType shrinkType)
        {
            if (shrinkType == ShrinkType.Normal)
            {
                ShrinkNormal(siaqodb);
            }
            else if (shrinkType == ShrinkType.ForceClaimSpace)
            {
                ClaimSpace(siaqodb);
                ShrinkNormal(siaqodb);
            }
            else if (shrinkType == ShrinkType.Total)
            {
                ShrinkTotal(siaqodb);
                ClaimSpace(siaqodb);
                ShrinkNormal(siaqodb);
                
            }
            siaqodb.Close();
        }
#if ASYNC
        private static async Task ShrinkAsync(Siaqodb siaqodb, ShrinkType shrinkType)
        {
            if (shrinkType == ShrinkType.Normal)
            {
                await ShrinkNormalAsync(siaqodb);
            }
            else if (shrinkType == ShrinkType.ForceClaimSpace)
            {
                await ClaimSpaceAsync(siaqodb);
                await ShrinkNormalAsync(siaqodb);
            }
            else if (shrinkType == ShrinkType.Total)
            {
                await ShrinkTotalAsync(siaqodb);
                await ClaimSpaceAsync(siaqodb);
                await ShrinkNormalAsync(siaqodb);

            }
            await siaqodb.CloseAsync();
        }
#endif
        private static void ShrinkTotal(Siaqodb siaqodb)
        {
            siaqodb.ShrinkAllTypes();
        }
#if ASYNC
        private static async Task ShrinkTotalAsync(Siaqodb siaqodb)
        {
            await siaqodb.ShrinkAllTypesAsync();
        }
#endif

        private static void ClaimSpace(Siaqodb siaqodb)
        {
            List<int> existingOids = siaqodb.GetUsedRawdataInfoOIDS();
            Expression<Func<RawdataInfo, bool>> predicate = ri => ri.IsFree == false;
            List<int> existingOIDsOccupied= siaqodb.LoadOids<RawdataInfo>(predicate);

            //intersection
            existingOids.Sort();
            List<int> oidsToBeFreed = new List<int>();
            foreach (int oid in existingOIDsOccupied)
            {
                int index = existingOids.BinarySearch(oid);
                if (index < 0)
                {
                    oidsToBeFreed.Add(oid);
                }
            }
            siaqodb.MarkRawInfoAsFree(oidsToBeFreed);
        
        }
#if ASYNC
        private static async Task ClaimSpaceAsync(Siaqodb siaqodb)
        {
            List<int> existingOids = await siaqodb.GetUsedRawdataInfoOIDSAsync();
            Expression<Func<RawdataInfo, bool>> predicate = ri => ri.IsFree == false;
            List<int> existingOIDsOccupied = await siaqodb.LoadOidsAsync<RawdataInfo>(predicate);

            //intersection
            existingOids.Sort();
            List<int> oidsToBeFreed = new List<int>();
            foreach (int oid in existingOIDsOccupied)
            {
                int index = existingOids.BinarySearch(oid);
                if (index < 0)
                {
                    oidsToBeFreed.Add(oid);
                }
            }
            await siaqodb.MarkRawInfoAsFreeAsync(oidsToBeFreed);

        }
#endif
        private static void ShrinkNormal(Siaqodb siaqodb)
        {
             var allOrderByPos = (from RawdataInfo ri in siaqodb
                                 where ri.IsFree == false
                                 orderby ri.Position
                                 select ri).ToList();

            ISqoFile file = siaqodb.GetRawFile();
            MemoryStream memStream = new MemoryStream();
            MemoryStream memStreamNew = new MemoryStream();
            byte[] fullFile = new byte[file.Length];

            file.Read(0, fullFile);
            memStream.Write(fullFile, 0, fullFile.Length);

            for (int i = 0; i < allOrderByPos.Count; i++)
            {
                RawdataInfo rawi = allOrderByPos[i];
                byte[] actualBytes = new byte[rawi.Length];
                memStream.Seek(rawi.Position, SeekOrigin.Begin);
                memStream.Read(actualBytes, 0, actualBytes.Length);
                if (i == 0)
                {

                    rawi.Position = 0;
                    memStreamNew.Write(actualBytes, 0, actualBytes.Length);
                }
                else
                {
                    RawdataInfo prev = allOrderByPos[i - 1];
                    rawi.Position = prev.Position + prev.Length;
                    memStreamNew.Seek(rawi.Position, SeekOrigin.Begin);
                    memStreamNew.Write(actualBytes, 0, actualBytes.Length);
                }
            }
            for (int i = 0; i < allOrderByPos.Count; i++)
            {
                siaqodb.StoreObject(allOrderByPos[i]);
            }
            file.Length = memStreamNew.Length;
            file.Write(0, memStreamNew.ToArray());

                             
        }
#if ASYNC
        private static async Task ShrinkNormalAsync(Siaqodb siaqodb)
        {

			var allOrderByPos = (await (from RawdataInfo ri in siaqodb
                                 where ri.IsFree == false
                                 select ri).ToListAsync()).OrderBy(a=>a.Position).ToList();

            ISqoFile file = siaqodb.GetRawFile();
            MemoryStream memStream = new MemoryStream();
            MemoryStream memStreamNew = new MemoryStream();
            byte[] fullFile = new byte[file.Length];

            await file.ReadAsync(0, fullFile);
            memStream.Write(fullFile, 0, fullFile.Length);

            for (int i = 0; i < allOrderByPos.Count; i++)
            {
                RawdataInfo rawi = allOrderByPos[i];
                byte[] actualBytes = new byte[rawi.Length];
                memStream.Seek(rawi.Position, SeekOrigin.Begin);
                memStream.Read(actualBytes, 0, actualBytes.Length);
                if (i == 0)
                {

                    rawi.Position = 0;
                    memStreamNew.Write(actualBytes, 0, actualBytes.Length);
                }
                else
                {
                    RawdataInfo prev = allOrderByPos[i - 1];
                    rawi.Position = prev.Position + prev.Length;
                    memStreamNew.Seek(rawi.Position, SeekOrigin.Begin);
                    memStreamNew.Write(actualBytes, 0, actualBytes.Length);
                }
            }
            for (int i = 0; i < allOrderByPos.Count; i++)
            {
                await siaqodb.StoreObjectAsync(allOrderByPos[i]);
            }
            file.Length = memStreamNew.Length;
            await file.WriteAsync(0, memStreamNew.ToArray());


        }
#endif
       
        /// <summary>
        /// Repair database files by fixing corrupted objects bytes
        /// </summary>
        /// <param name="siaqodb"></param>
        private static void Repair(Siaqodb siaqodb)
        {
            IsRepairMode = true;
            siaqodb.RepairAllTypes();
            siaqodb.Close();
            IsRepairMode = false;
        }
#if ASYNC
        /// <summary>
        /// Repair database files by fixing corrupted objects bytes
        /// </summary>
        /// <param name="siaqodb"></param>
        private static async Task RepairAsync(Siaqodb siaqodb)
        {
            IsRepairMode = true;
            await siaqodb.RepairAllTypesAsync();
            await siaqodb.CloseAsync();
            IsRepairMode = false;
        }
#endif
        /// <summary>
        /// Repair database files by fixing corrupted objects bytes
        /// </summary>
        /// <param name="siaqodb"></param>
        public static void Repair(string databasePath)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb(databasePath);
            Repair(siaqodb);
        }
#if ASYNC
        /// <summary>
        /// Repair database files by fixing corrupted objects bytes
        /// </summary>
        /// <param name="siaqodb"></param>
        public static async Task RepairAsync(string databasePath)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);
            await RepairAsync(siaqodb);
        }
#endif
        #if SL4
        /// <summary>
        /// Repair database files by fixing corrupted objects bytes
        /// </summary>
        /// <param name="siaqodb"></param>
        public static void Repair(string databasePath, Environment.SpecialFolder specialFolder)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb(databasePath, specialFolder);
            Repair(siaqodb);
        }
#endif
        /// <summary>
        /// Repair database file of Type provided, the corrupted objects will be recuperated or deleted
        /// </summary>
        /// <param name="siaqodb"></param>
        public static void RepairType<T>(string databasePath)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb(databasePath);
            RepairType<T>(siaqodb);
        }
#if ASYNC
        /// <summary>
        /// Repair database file of Type provided, the corrupted objects will be recuperated or deleted
        /// </summary>
        /// <param name="siaqodb"></param>
        public static async Task RepairTypeAsync<T>(string databasePath)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);
            await RepairTypeAsync<T>(siaqodb);
        }
#endif
#if SL4
        /// <summary>
        /// Repair database file of Type provided, the corrupted objects will be recuperated or deleted
        /// </summary>
        /// <param name="siaqodb"></param>
        public static void RepairType<T>(string databasePath, Environment.SpecialFolder specialFolder)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb(databasePath,specialFolder);
            RepairType<T>(siaqodb);
        }
#endif
        private static void RepairType<T>(Siaqodb siaqodb)
        {
            IsRepairMode = true;
          
            IList<T> all = siaqodb.LoadAll<T>();
            siaqodb.Close();
            IsRepairMode = false;
        }
       
#if ASYNC
        private static async Task RepairTypeAsync<T>(Siaqodb siaqodb)
        {
            IsRepairMode = true;

            IList<T> all = await siaqodb.LoadAllAsync<T>();
            await siaqodb.CloseAsync();
            IsRepairMode = false;
        }
#endif
       
       

        internal static bool IsRepairMode;
       
    }
#else //WinRT

    /// <summary>
    /// Database utilities
    /// </summary>
    public class SiaqodbUtil
    {
        /// <summary>
        /// Rebuild and defragment indexes
        /// </summary>
        /// <param name="siaqodb"></param>
        public static async Task ReIndexAsync(StorageFolder databasePath)
        {
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);
            await ReIndexAsync(siaqodb);
        }


        private static async Task ReIndexAsync(Siaqodb siaqodb)
        {

            await siaqodb.ReIndexAllAsync(true);
            await siaqodb.FlushAsync();
            siaqodb.Close();
        }
        /// <summary>
        /// Shrink database files including rawdata.sqr and indexes 
        /// </summary>
        public static async Task ShrinkAsync(StorageFolder databasePath, ShrinkType shrinkType)
        {
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);
            await ShrinkAsync(siaqodb, shrinkType);
        }

        private static async Task ShrinkAsync(Siaqodb siaqodb, ShrinkType shrinkType)
        {
            if (shrinkType == ShrinkType.Normal)
            {
                await ShrinkNormal(siaqodb);
            }
            else if (shrinkType == ShrinkType.ForceClaimSpace)
            {
                await ClaimSpace(siaqodb);
                await ShrinkNormal(siaqodb);
            }
            else if (shrinkType == ShrinkType.Total)
            {
                await ShrinkTotal(siaqodb);
                await ClaimSpace(siaqodb);
                await ShrinkNormal(siaqodb);

            }

            await siaqodb.FlushAsync();
            siaqodb.Close();
        }

        private static async Task ShrinkTotal(Siaqodb siaqodb)
        {
            await siaqodb.ShrinkAllTypesAsync();
        }
        private static async Task ClaimSpace(Siaqodb siaqodb)
        {
            List<int> existingOids = await siaqodb.GetUsedRawdataInfoOIDSAsync();
            Expression<Func<RawdataInfo, bool>> predicate = ri => ri.IsFree == false;
            List<int> existingOIDsOccupied = await siaqodb.LoadOidsAsync<RawdataInfo>(predicate);

            //intersection
            existingOids.Sort();
            List<int> oidsToBeFreed = new List<int>();
            foreach (int oid in existingOIDsOccupied)
            {
                int index = existingOids.BinarySearch(oid);
                if (index < 0)
                {
                    oidsToBeFreed.Add(oid);
                }
            }
            await siaqodb.MarkRawInfoAsFreeAsync(oidsToBeFreed);

        }
        private static async Task ShrinkNormal(Siaqodb siaqodb)
        {
            var allOrderByPos = await (from RawdataInfo ri in siaqodb
                                       where ri.IsFree == false
                                       orderby ri.Position
                                       select ri).ToListAsync();

            ISqoFile file =  siaqodb.GetRawFile();
            MemoryStream memStream = new MemoryStream();
            MemoryStream memStreamNew = new MemoryStream();
            byte[] fullFile = new byte[file.Length];

            await file.ReadAsync(0, fullFile);
            memStream.Write(fullFile, 0, fullFile.Length);

            for (int i = 0; i < allOrderByPos.Count; i++)
            {
                RawdataInfo rawi = allOrderByPos[i];
                byte[] actualBytes = new byte[rawi.Length];
                memStream.Seek(rawi.Position, SeekOrigin.Begin);
                memStream.Read(actualBytes, 0, actualBytes.Length);
                if (i == 0)
                {

                    rawi.Position = 0;
                    memStreamNew.Write(actualBytes, 0, actualBytes.Length);
                }
                else
                {
                    RawdataInfo prev = allOrderByPos[i - 1];
                    rawi.Position = prev.Position + prev.Length;
                    memStreamNew.Seek(rawi.Position, SeekOrigin.Begin);
                    memStreamNew.Write(actualBytes, 0, actualBytes.Length);
                }
            }
            for (int i = 0; i < allOrderByPos.Count; i++)
            {
                await siaqodb.StoreObjectAsync(allOrderByPos[i]);
            }
            file.Length = memStreamNew.Length;
            await file.WriteAsync(0, memStreamNew.ToArray());


        }


        /// <summary>
        /// Repair database files by fixing corrupted objects bytes
        /// </summary>
        /// <param name="siaqodb"></param>
        private static async Task Repair(Siaqodb siaqodb)
        {
            IsRepairMode = true;
            await siaqodb.RepairAllTypesAsync();
            await siaqodb.FlushAsync();
            siaqodb.Close();
            IsRepairMode = false;
        }
        /// <summary>
        /// Repair database files by fixing corrupted objects bytes
        /// </summary>
        /// <param name="siaqodb"></param>
        public static async Task RepairAsync(StorageFolder databasePath)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);
            await Repair(siaqodb);
        }

        /// <summary>
        /// Repair database file of Type provided, the corrupted objects will be recuperated or deleted
        /// </summary>
        /// <param name="siaqodb"></param>
        public static async Task RepairTypeAsync<T>(StorageFolder databasePath)
        {
            IsRepairMode = true;
            Siaqodb siaqodb = new Siaqodb();
            await siaqodb.OpenAsync(databasePath);

            await RepairType<T>(siaqodb);
        }

        private static async Task RepairType<T>(Siaqodb siaqodb)
        {
            IsRepairMode = true;

            IList<T> all = await siaqodb.LoadAllAsync<T>();
            await siaqodb.FlushAsync();
            siaqodb.Close();
            IsRepairMode = false;
        }





        internal static bool IsRepairMode;

    }
#endif


    public enum ShrinkType {
        /// <summary>
        ///Normal shrink, all blocks marked as free will be supressed 
        /// </summary>
        Normal=0,
        /// <summary>
        /// All database files will be parsed and check if a block can be marked as free then supress free blocks;
        /// This operation can be slow if your database is big.
        /// </summary>
        ForceClaimSpace=1,
        /// <summary>
        /// This includes Normal + ForceClaimSpace but also shrink of every db file; after this operation OID values of the stored objects may change.
        /// </summary>
        Total
        
    
    }
    public class ShrinkResult
    {
        public int OID { get; set; }
        public int Old_OID;
        public int New_OID;
        public int TID;
        public object GetValue(System.Reflection.FieldInfo field)
        {
            return field.GetValue(this);
        }
        public void SetValue(System.Reflection.FieldInfo field, object value)
        {
            field.SetValue(this, value);
        }
    }

    public static class SqoStringExtensions
    {
        /// <summary>
        ///  Returns a value indicating whether the specified System.String object occurs
        ///    within this string.A parameter specifies the type of search
        ///     to use for the specified string.
        /// </summary>
        /// <param name="stringObj">Input string</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType"> One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>true if the value parameter occurs within this string, or if value is the
        ///     empty string (""); otherwise, false.</returns>
        public static bool Contains(this string stringObj, string value, StringComparison comparisonType)
        {
            return stringObj.IndexOf(value, comparisonType) != -1;
        }
      
    }
}
