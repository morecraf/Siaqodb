using System;
using System.Runtime.InteropServices;

namespace LightningDB.Native
{
    internal static class NativeMethods
    {
        private static readonly LightningVersionInfo _libraryVersion;

        private static readonly INativeLibraryFacade _libraryFacade;

        public static INativeLibraryFacade Library { get { return _libraryFacade; } }

        public static LightningVersionInfo LibraryVersion { get { return _libraryVersion; } }

        static NativeMethods()
        {
			#if UNITY3D || MONODROID || WinRT || MONOMAC
            _libraryFacade=new FallbackLibraryFacade();
#else



            _libraryFacade = IntPtr.Size == 8
                ? new Native64BitLibraryFacade()
                : (INativeLibraryFacade) new Native32BitLibraryFacade();
#endif

            Exception archSpecificException;
            _libraryVersion = GetVersionInfo(_libraryFacade, out archSpecificException);

            if (archSpecificException != null)
            {
                Exception fallbackException;

                var fallbackLibrary = new FallbackLibraryFacade();
                _libraryVersion = GetVersionInfo(fallbackLibrary, out fallbackException);

                if (fallbackException != null)
                    throw archSpecificException;

                _libraryFacade = fallbackLibrary;
            }
        }

        private static LightningVersionInfo GetVersionInfo(INativeLibraryFacade lib, out Exception exception)
        {
            exception = null;

            LightningVersionInfo versionInfo = null;

            try
            {
                versionInfo = LightningVersionInfo.Create(lib);
            }
            catch (DllNotFoundException dllNotFoundException)
            {
                exception = dllNotFoundException;
            }
            catch (BadImageFormatException badImageFormatException)
            {
                exception = badImageFormatException;
            }

            return versionInfo;
        }

        #region Constants

        /// <summary>
        /// Txn has too many dirty pages
        /// </summary>
        public const int MDB_TXN_FULL = -30788;

        /// <summary>
        /// Environment mapsize reached
        /// </summary>
        public const int MDB_MAP_FULL = -30792;

        /// <summary>
        /// File is not a valid MDB file.
        /// </summary>
        public const int MDB_INVALID = -30793;

        /// <summary>
        /// Environment version mismatch.
        /// </summary>
        public const int MDB_VERSION_MISMATCH = -30794;

        /// <summary>
        /// Update of meta page failed, probably I/O error
        /// </summary>
        public const int MDB_PANIC = -30795;

        /// <summary>
        /// Database contents grew beyond environment mapsize
        /// </summary>
        public const int MDB_MAP_RESIZED = -30785;

        /// <summary>
        /// Environment maxreaders reached
        /// </summary>
        public const int MDB_READERS_FULL = -30790;

        /// <summary>
        /// Environment maxdbs reached
        /// </summary>
        public const int MDB_DBS_FULL = -30791;

        /// <summary>
        /// key/data pair not found (EOF)
        /// </summary>
        public const int MDB_NOTFOUND = -30798;

        #endregion Constants

        #region Helpers

        public static int Execute(Func<INativeLibraryFacade, int> action)
        {
            return ExecuteHelper(action, err => true);
        }

        public static int Read(Func<INativeLibraryFacade, int> action)
        {
            return ExecuteHelper(action, err => err != MDB_NOTFOUND);
        }

        public static bool TryRead(Func<INativeLibraryFacade, int> action)
        {
            return Read(action) != MDB_NOTFOUND;
        }

        private static int ExecuteHelper(Func<INativeLibraryFacade, int> action, Func<int, bool> shouldThrow)
        {
            var res = action.Invoke(_libraryFacade);
            if (res != 0 && shouldThrow(res))
                throw new LightningException(res);
            return res;
        }

        #endregion Helpers
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MDBStat
    {
        /// <summary>
        /// Size of a database page. This is currently the same for all databases.
        /// </summary>
        public uint ms_psize;

        /// <summary>
        /// Depth (height) of the B-tree
        /// </summary>
        public uint ms_depth;

        /// <summary>
        /// Number of internal (non-leaf) pages
        /// </summary>
        public IntPtr ms_branch_pages;

        /// <summary>
        /// Number of leaf pages
        /// </summary>
        public IntPtr ms_leaf_pages;

        /// <summary>
        /// Number of overflow pages
        /// </summary>
        public IntPtr ms_overflow_pages;

        /// <summary>
        /// Number of data items
        /// </summary>
        public IntPtr ms_entries;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MDBEnvInfo
    {
        /// <summary>
        /// Address of map, if fixed
        /// </summary>
        public IntPtr me_mapaddr;

        /// <summary>
        /// Size of the data memory map
        /// </summary>
        public IntPtr me_mapsize;

        /// <summary>
        /// ID of the last used page
        /// </summary>
        public IntPtr me_last_pgno;

        /// <summary>
        /// ID of the last committed transaction
        /// </summary>
        public IntPtr me_last_txnid;

        /// <summary>
        /// max reader slots in the environment
        /// </summary>
        public uint me_maxreaders;

        /// <summary>
        /// max reader slots used in the environment
        /// </summary>
        public uint me_numreaders;
    }
}
