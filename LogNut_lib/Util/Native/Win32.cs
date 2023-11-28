using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;


namespace Hurst.LogNut.Util.Native
{
    // See this useful article: http://www.pinvoke.net/default.aspx/kernel32.findfirstfile

    #region class Win32
    /// <summary>
    /// This class contains some useful file-related WIN32 API functions.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public class Win32
    {
        /// <summary>
        /// SEt the program-wide policy for handling crashes.
        /// </summary>
        /// <param name="uMode">Set this to a bitwise-OR of the ErrorModes enumeration values.</param>
        /// <returns></returns>
        // See   https://msdn.microsoft.com/en-us/library/windows/desktop/ms680621(v=vs.85).aspx
        [DllImport( "kernel32.dll" )]
        public static extern Int32 SetErrorMode( Int32 uMode );

        /// <summary>
        /// SEt the process-wide policy for handling crashes.
        /// See  http://pinvoke.net/default.aspx/kernel32/SetThreadErrorMode.html
        /// </summary>
        /// <param name="dwNewMode"></param>
        /// <param name="lpOldMode"></param>
        /// <returns></returns>
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool SetThreadErrorMode( UInt32 dwNewMode, out UInt32 lpOldMode );


        /// <summary>
        /// The maximum length of any single part of a filesystem-path under Windows (this is 253).
        /// </summary>
        internal const int MAX_PATH = 253;  // Apparently 260 did not work. I had failure with 254.

        /// <summary>
        /// The maximum length of the cAlternate field of the WIN32_FIND_DATA struct.
        /// </summary>
        internal const int MAX_ALTERNATE = 14;

        // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx

        /// <summary>
        /// This is 0 "The operation completed successfully."
        /// </summary>
        internal const int ERROR_SUCCESS = 0;
        /// <summary>
        /// This is 2 "The system cannot find the file specified."
        /// </summary>
        internal const int ERROR_FILE_NOT_FOUND = 2;
        /// <summary>
        /// This is 3 "The system cannot find the path specified."
        /// </summary>
        internal const int ERROR_PATH_NOT_FOUND = 3;
        /// <summary>
        /// This is 5 "Access is denied."
        /// </summary>
        internal const int ERROR_ACCESS_DENIED = 5;
        /// <summary>
        /// This is 18 "There are no more files."
        /// </summary>
        internal const int ERROR_NO_MORE_FILES = 18;
        /// <summary>
        /// This is 32 "The process cannot access the file because it is being used by another process."
        /// </summary>
        internal const int ERROR_SHARING_VIOLATION = 32;
        /// <summary>
        /// This is 33 "The process cannot access the file because another process has locked a portion of the file."
        /// </summary>
        internal const int ERROR_LOCK_VIOLATION = 33;
        /// <summary>
        /// This is 1008 "An attempt was made to reference a token that does not exist."
        /// </summary>
        internal const int ERROR_NO_TOKEN = 1008;

        /// <summary>
        /// This struct represents the WIN32 FILETIME structure.
        /// </summary>
        [StructLayout( LayoutKind.Sequential )]
        public struct FILETIME
        {
            /// <summary>
            /// The lower-order 32 bits of the FILETIME
            /// </summary>
            public uint dwLowDateTime;
            /// <summary>
            /// The higher-order 32 bits of the FILETIME
            /// </summary>
            public uint dwHighDateTime;
        };

        /// <summary>
        /// This represents the result that is written to by the WIN32 find-file operation.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
        internal struct WIN32_FIND_DATA
        {
            /// <summary>
            /// The attributes associated with the file
            /// </summary>
            public FileAttributes dwFileAttributes;
            /// <summary>
            /// When the file was created
            /// </summary>
            public FILETIME ftCreationTime;
            /// <summary>
            /// When the file was last accessed
            /// </summary>
            public FILETIME ftLastAccessTime;
            /// <summary>
            /// When the file was last written
            /// </summary>
            public FILETIME ftLastWriteTime;
            /// <summary>
            /// The higher-order 32 bits of the file-size
            /// </summary>
            public uint nFileSizeHigh; //changed all to uint from int, otherwise you run into unexpected overflow
            /// <summary>
            /// The lower-order 32 bits of the file-size
            /// </summary>
            public uint nFileSizeLow;  //| http://www.pinvoke.net/default.aspx/Structures/WIN32_FIND_DATA.html
            /// <summary>
            /// Reserved.
            /// </summary>
            public uint dwReserved0;
            /// <summary>
            /// Reserved.
            /// </summary>
            public uint dwReserved1;

            /// <summary>
            /// The filename
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            /// <summary>
            /// The DOS 8.3-form alternate filename
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
            public string cAlternate;
        }

        // http://www.dotnet247.com/247reference/msgs/21/108780.aspx
        [DllImportAttribute( @"advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode )]
        internal static extern int GetNamedSecurityInfo(
            string pObjectName,
            int objectType,
            int securityInfo,
            out IntPtr ppsidOwner,
            out IntPtr ppsidGroup,
            out IntPtr ppDacl,
            out IntPtr ppSacl,
            out IntPtr ppSecurityDescriptor );

        [DllImport( @"advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
        internal static extern int LookupAccountSid(
            string systemName,
            IntPtr psid,
            StringBuilder accountName,
            ref int cbAccount,
            [Out] StringBuilder domainName,
            ref int cbDomainName,
            out int use );

        /// <summary>
        /// This is used in the call to GetNamedSecurityInfo.
        /// </summary>
        public const int OwnerSecurityInformation = 1;
        /// <summary>
        /// This is used in the call to GetNamedSecurityInfo.
        /// </summary>
        public const int SeFileObject = 1;

        /// <summary>
        /// The .NET equivalent of the Win32 SECURITY_ATTRIBUTES structure.
        /// </summary>
        [StructLayout( LayoutKind.Sequential )]
        public struct SECURITY_ATTRIBUTES
        {
            /// <summary>
            /// This denotes the length in bytes of this structure.
            /// </summary>
            public int nLength;
            /// <summary>
            /// This is 
            /// </summary>
            public IntPtr lpSecurityDescriptor;
            /// <summary>
            /// This is a boolean value that indicates whether to inherit the handle from the parent.
            /// </summary>
            public int bInheritHandle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringSecurityDescriptor"></param>
        /// <param name="stringSDRevision"></param>
        /// <param name="securityDescriptor"></param>
        /// <param name="securityDescriptorSize"></param>
        /// <returns></returns>
        [DllImport( "advapi32.dll", SetLastError = true )]
        public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor( string stringSecurityDescriptor,
                                                                                       uint stringSDRevision,
                                                                                       out IntPtr securityDescriptor,
                                                                                       out UIntPtr securityDescriptorSize );

        [DllImport( @"kernel32.dll",
                  CharSet = CharSet.Unicode,
                  CallingConvention = CallingConvention.StdCall,
                  SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool CopyFile(
                          [MarshalAs( UnmanagedType.LPTStr )] string lpExistingFileName,
                          [MarshalAs( UnmanagedType.LPTStr )] string lpNewFileName,
                          [MarshalAs( UnmanagedType.Bool )] bool bFailIfExists );

        [DllImport( @"kernel32.dll",
                   CharSet = CharSet.Unicode,
                   CallingConvention = CallingConvention.StdCall,
                   SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool MoveFile(
                           [MarshalAs( UnmanagedType.LPTStr )] string lpExistingFileName,
                           [MarshalAs( UnmanagedType.LPTStr )] string lpNewFileName );

        [DllImport( @"kernel32.dll", CharSet = CharSet.Unicode )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool CreateDirectory(
           [MarshalAs( UnmanagedType.LPTStr )]string lpPathName,
          IntPtr lpSecurityAttributes );

        [DllImport( @"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
        internal static extern uint GetFileAttributes(
            [MarshalAs( UnmanagedType.LPTStr )]string lpFileName );

        [DllImport( @"kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false )]
        internal static extern bool GetFileAttributesEx(
            [MarshalAs( UnmanagedType.LPTStr )]string lpFileName,
            int fInfoLevelId,
            ref WIN32_FILE_ATTRIBUTE_DATA fileData );

        /// <summary>
        /// This is the structure that GetFileAttributes writes to.
        /// </summary>
        [StructLayout( LayoutKind.Sequential )]
        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            /// <summary>
            /// This double-word value denotes the file attributes as a bit-field.
            /// </summary>
            public int dwFileAttributes;
            /// <summary>
            /// The timestamp denoting when this file was created.
            /// </summary>
            public FILETIME ftCreationTime;
            /// <summary>
            /// The timestamp denoting when this file was most recently accessed.
            /// </summary>
            public FILETIME ftLastAccessTime;
            /// <summary>
            /// The timestamp denoting when this file was last written to.
            /// </summary>
            public FILETIME ftLastWriteTime;
            /// <summary>
            /// This unsigned-32bit-integer is the high-order half of the 64-bit file-size value.
            /// </summary>
            public uint nFileSizeHigh;
            /// <summary>
            /// This unsigned-32bit-integer is the low-order half of the 64-bit file-size value.
            /// </summary>
            public uint nFileSizeLow;
        }

        [DllImport( @"kernel32.dll", CharSet = CharSet.Unicode )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool SetFileAttributes( [MarshalAs( UnmanagedType.LPTStr )]string lpFileName,
                                                     [MarshalAs( UnmanagedType.U4 )] FileAttributes dwFileAttributes );

        /// <summary>
        /// Remove the given file from the filesystem by calling the WIN32 function of the same name.
        /// </summary>
        /// <param name="lpFileName">the pathname of the file to remove</param>
        /// <returns>true if successful</returns>
        [DllImport( "kernel32.dll", EntryPoint = "DeleteFileW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool DeleteFile( string lpFileName );
        //CBL  Here is the version that ZLongPathLib had.
        //[DllImport(@"kernel32.dll", CharSet = CharSet.Unicode)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool DeleteFile([MarshalAs(UnmanagedType.LPTStr)]string lpFileName);

        /// <summary>
        /// Call the WIN32 FindClose function to close the given file-find operation.
        /// </summary>
        /// <param name="hFindFile">the handle of the file-find operation to close</param>
        /// <returns>true if successful</returns>
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool FindClose( IntPtr hFindFile );
        //CBL  Here is the version that ZLongPathLib had.
        //[DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool FindClose(IntPtr hFindFile);

        /// <summary>
        /// Call the WIN32 FindFirstFile function to fetch the first filesystem-file of a new file-fine operation.
        /// </summary>
        /// <param name="lpFileName">the file-spec to pattern the search upon</param>
        /// <param name="lpFindFileData">the result gets written to this</param>
        /// <returns>the handle of the file-find operation to use for subsequent operations</returns>
        [DllImport( "kernel32", CharSet = CharSet.Unicode )]
        internal static extern IntPtr FindFirstFile( string lpFileName, out WIN32_FIND_DATA lpFindFileData );
        //CBL  Here is the version that ZLongPathLib had.
        //[DllImport(@"kernel32.dll", CharSet = CharSet.Unicode)]
        //internal static extern IntPtr FindFirstFile([MarshalAs(UnmanagedType.LPTStr)]string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        /// <summary>
        /// Call the WIN32 FindNextFile function to fetch the next filesystem-file of the file-fine operation.
        /// denoted
        /// </summary>
        /// <param name="hFindFile">this is the handle of the existing file-find operation</param>
        /// <param name="lpFindFileData">the result gets written to this</param>
        /// <returns>true if a file was found</returns>
        [DllImport( "kernel32", CharSet = CharSet.Unicode )]
        internal static extern bool FindNextFile( IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData );

        /// <summary>
        /// Delete the given directory. This is an interface to the WIN32 API.
        /// </summary>
        /// <param name="lpPathName">the pathname of the directory to delete</param>
        /// <returns>true if successful</returns>
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool RemoveDirectory( string lpPathName );
        // Here is the version that ZLongPathLib had:
        //[DllImport(@"kernel32.dll", CharSet = CharSet.Unicode)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //internal static extern bool RemoveDirectory([MarshalAs(UnmanagedType.LPTStr)]string lpPathName);

        [DllImport( @"kernel32.dll", SetLastError = true, EntryPoint = @"SetFileTime", ExactSpelling = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool SetFileTime1( IntPtr hFile,
                                                 ref long lpCreationTime,
                                                 IntPtr lpLastAccessTime,
                                                 IntPtr lpLastWriteTime );

        [DllImport( @"kernel32.dll", SetLastError = true, EntryPoint = @"SetFileTime", ExactSpelling = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool SetFileTime2( IntPtr hFile,
                                                 IntPtr lpCreationTime,
                                                 ref long lpLastAccessTime,
                                                 IntPtr lpLastWriteTime );

        [DllImport( @"kernel32.dll", SetLastError = true, EntryPoint = @"SetFileTime", ExactSpelling = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool SetFileTime3(
            IntPtr hFile,
            IntPtr lpCreationTime,
            IntPtr lpLastAccessTime,
            ref long lpLastWriteTime );

        [DllImport( "shlwapi.dll", CharSet = CharSet.Unicode )]
        [ResourceExposure( ResourceScope.None )]
        [return: MarshalAsAttribute( UnmanagedType.Bool )]
        internal static extern bool PathIsUNC( [MarshalAsAttribute( UnmanagedType.LPWStr ), In] string pszPath );

        [DllImport( @"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode )]
        internal static extern int GetFullPathName(
            [MarshalAs( UnmanagedType.LPTStr )]string lpFileName,
            int nBufferLength,
            StringBuilder lpBuffer,
            IntPtr mustBeZero );

        internal static int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        internal static uint INVALID_FILE_ATTRIBUTES = 0xFFFFFFFF;

        internal static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        // This is from URL: http://www.pinvoke.net/default.aspx/kernel32.setfilepointer
        [DllImport( "Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        internal static extern int SetFilePointer( IntPtr handle,
                                                  int lDistanceToMove,
                                                  out int lpDistanceToMoveHigh,
                                                  uint dwMoveMethod );

        [DllImport( @"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode )]
        internal static extern SafeFileHandle CreateFile( [MarshalAs( UnmanagedType.LPTStr )]string lpFileName,
                                                         FileAccess dwDesiredAccess,
                                                         FileShare dwShareMode,
                                                         IntPtr lpSecurityAttributes,
                                                         Native.CreationDisposition dwCreationDisposition,
                                                         NativeFileAttributes dwFlagsAndAttributes,
                                                         IntPtr hTemplateFile );

        /// <summary>
        /// 
        /// Assume dirName passed in is already prefixed with \\?\
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static List<string> FindFilesAndDirectories( string directoryPath )
        {
            var results = new List<string>();
            WIN32_FIND_DATA findData;
            var findHandle = FindFirstFile(directoryPath.TrimEnd('\\') + @"\*", out findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                bool found;
                do
                {
                    var currentFileName = findData.cFileName;

                    // if this is a directory, find its contents
                    if (((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
                    {
                        if (currentFileName != @"." && currentFileName != @"..")
                        {
                            var childResults = FindFilesAndDirectories(Path.Combine(directoryPath, currentFileName));
                            // add children and self to results
                            results.AddRange( childResults );
                            results.Add( Path.Combine( directoryPath, currentFileName ) );
                        }
                    }

                    // it's a file; add it to the results
                    else
                    {
                        results.Add( Path.Combine( directoryPath, currentFileName ) );
                    }

                    // find next
                    found = FindNextFile( findHandle, out findData );
                }
                while (found);
            }

            // close the find handle
            FindClose( findHandle );
            return results;
        }

        /// <summary>
        /// Return the scrolled-position of the given control-handle.
        /// </summary>
        /// <param name="hWnd">the WIN32-handle of the visual control to get the scrolled-position of</param>
        /// <param name="nBar">this identifies which scrollbar to get the position of</param>
        /// <returns></returns>
        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        public static extern int GetScrollPos( IntPtr hWnd, System.Windows.Forms.Orientation nBar );

        /// <summary>
        /// SafeFindHandle is a SafeHandleZeroOrMinusOneIsInvalid that by default owns it's own handle,
        /// and closes it's file-find operation upon release.
        /// </summary>
        public sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            // Methods
            [SecurityPermission( SecurityAction.LinkDemand, UnmanagedCode = true )]
            private SafeFindHandle()
                : base( true )
            {
            }

            private SafeFindHandle( IntPtr preExistingHandle, bool ownsHandle )
                : base( ownsHandle )
            {
                base.SetHandle( preExistingHandle );
            }

            /// <summary>
            /// Override ReleaseHandle to ensure any open file-find operation is closed.
            /// </summary>
            /// <returns></returns>
            protected override bool ReleaseHandle()
            {
                if (!(IsInvalid || IsClosed))
                {
                    return Kernel32UsingSafeHandle.FindClose( this );
                }
                return (IsInvalid || IsClosed);
            }

            /// <summary>
            /// Release any contained unmanaged resources - in this case, the file-find operation.
            /// </summary>
            /// <param name="disposing">true if releasing managed resources</param>
            protected override void Dispose( bool disposing )
            {
                if (!(IsInvalid || IsClosed))
                {
                    Kernel32UsingSafeHandle.FindClose( this );
                }
                base.Dispose( disposing );
            }
        }

        // For setting the Windows Desktop background image
        // See  https://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net

        /// <summary>
        /// Set the SystemParameters
        /// </summary>
        /// <param name="uAction"></param>
        /// <param name="uParam"></param>
        /// <param name="lpvParam"></param>
        /// <param name="fWinIni"></param>
        /// <returns></returns>
        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        public static extern bool SystemParametersInfo( uint uAction, uint uParam, string lpvParam, uint fWinIni );

        const int SPI_SETDESKWALLPAPER = 0x14;
        const int SPI_GETDESKWALLPAPER  = 0x73;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        /// <summary>
        /// I don't see where I am using this.
        /// </summary>
        public enum Style : int
        {
            /// <summary>
            /// When the space is larger than the image, render repeated copies of the image.
            /// </summary>
            Tiled,
            /// <summary>
            /// Center the image within it's space.
            /// </summary>
            Centered,
            /// <summary>
            /// Cause the image to stretch to fill it's space.
            /// </summary>
            Stretched
        }

        /// <summary>
        /// Set 2 values in the Windows Registry under Current User
        /// that dictates the Windows Desktop background image.
        /// </summary>
        /// <param name="pathnameOfImage"></param>
        public static void SetWindowsDesktopBackgroundImage( string pathnameOfImage )
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
            rk.SetValue( "WallpaperStyle", "2" ); // Stretched
            rk.SetValue( "TileWallpaper", "0" ); // No
            rk.Close();
            SystemParametersInfo( SPI_SETDESKWALLPAPER, 0, pathnameOfImage, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE );
        }
    }
    #endregion class Win32

    /// <summary>
    /// This class exists to provide a managed-code way to call the WIN32 file-find functions.
    /// </summary>
    public class Kernel32UsingSafeHandle
    {
        /// <summary>
        /// Call the WIN32 function FindFirstFile with the given filename-spec, and return a SafeFindHandle for the resulting file-find operation.
        /// </summary>
        /// <param name="lpFileName">a file-spec specifying what files to find</param>
        /// <param name="lpFindFileData">the WIN32_FIND_DATA is output to this</param>
        /// <returns>a SafeFindHandle representing the file-find operation</returns>
        [DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        internal static extern Win32.SafeFindHandle FindFirstFile( string lpFileName, out Win32.WIN32_FIND_DATA lpFindFileData );


        /// <summary>
        /// Call the WIN32 FindNextFile function to find the next file within a file-find operation.
        /// </summary>
        /// <param name="hFindFile">the handle of the file-find operation</param>
        /// <param name="lpFindFileData">any WIN32_FIND_DATA is written to this</param>
        /// <returns>true if a file is found</returns>
        [DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        internal static extern bool FindNextFile( SafeHandle hFindFile, out Win32.WIN32_FIND_DATA lpFindFileData );

        /// <summary>
        /// Close the file-find operation.
        /// </summary>
        /// <param name="hFindFile">the handle to the file-find operation</param>
        /// <returns>true if successful</returns>
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool FindClose( SafeHandle hFindFile );
    }
}
