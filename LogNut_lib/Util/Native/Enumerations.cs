using System;


// See http://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx

namespace Hurst.LogNut.Util.Native
{
    //CBL  This had been internal. Made it public to clear an error with unit-testing. Why was it working before?

    /// <summary>
    /// Values intended for use within Win32.SetErrorMode and Win32.SetThreadErrorMode.
    /// </summary>
    [Flags]
    public enum ErrorModes : uint
    {
        /// <summary>
        /// This is 0.
        /// </summary>
        SYSTEM_DEFAULT = 0x0,
        /// <summary>
        /// This is 0x0001.
        /// </summary>
        SEM_FAILCRITICALERRORS = 0x0001,
        /// <summary>
        /// This is 0x0004.
        /// </summary>
        SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
        /// <summary>
        /// This is 0x0002.
        /// </summary>
        SEM_NOGPFAULTERRORBOX = 0x0002,
        /// <summary>
        /// This is 0x8000.
        /// </summary>
        SEM_NOOPENFILEERRORBOX = 0x8000
    }

    /// <summary>
    /// This type is a UINT value - GenericRead, GenericWrite, GenericExecute, or GenericAll.
    /// </summary>
	[Flags]
    public enum FileAccess : uint
	{
        /// <summary>
        /// Read-access
        /// </summary>
		GenericRead = 0x80000000,
        /// <summary>
        /// Write-access
        /// </summary>
		GenericWrite = 0x40000000,
        /// <summary>
        /// Execute privilege
        /// </summary>
		GenericExecute = 0x20000000,
        /// <summary>
        /// Any kind of access
        /// </summary>
		GenericAll = 0x10000000,
	}

    /// <summary>
    /// This type is a UINT value - None, Read, Write, or Delete.
    /// </summary>
    [Flags]
	public enum FileShare : uint
	{
        /// <summary>
        /// This indicates no sharing privileges are requested.
        /// </summary>
		None = 0x00000000,
        /// <summary>
        /// Read-Sharing.
        /// Enables subsequent open operations on an object to request read access.
        /// Otherwise, other processes cannot open the object if they request read access.
        /// If this flag is not specified, but the object has been opened for read access, the function fails.
        /// </summary>
		Read = 0x00000001,
        /// <summary>
        /// Write-Sharing.
        /// Enables subsequent open operations on an object to request write access.
        /// Otherwise, other processes cannot open the object if they request write access.
        /// If this flag is not specified, but the object has been opened for write access, the function fails.
        /// </summary>
		Write = 0x00000002,
        /// <summary>
        /// Deletion-Sharing.
        /// Enables subsequent open operations on an object to request delete access.
        /// Otherwise, other processes cannot open the object if they request delete access.
        /// If this flag is not specified, but the object has been opened for delete access, the function fails.
        /// </summary>
		Delete = 0x00000004,
	}

    /// <summary>
    /// This flag dictates the behavior upon file creation - New, CreateAlways, OpenExisting, OpenAlways, TruncateExisting.
    /// It corresponds to the dwCreationDisposition parameter to the Win32 CreateFile function.
    /// </summary>
	public enum CreationDisposition : uint
	{
        /// <summary>
        /// Creates a new file, only if it does not already exist.
        /// If the file exists, the function fails and the last-error code is set to ERROR_FILE_EXISTS (80).
        /// </summary>
		New = 1,
        /// <summary>
        /// Creates a new file, always.
        /// If the file exists and is writable, the function overwrites the file and last-error code is set to ERROR_ALREADY_EXISTS (183).
        /// If it does not exist, a new file is created.
        /// </summary>
		CreateAlways = 2,
        /// <summary>
        /// Opens a file only if it exists.
        /// If the file does not exist, the function fails and the last-error code is set to ERROR_FILE_NOT_FOUND (2).
        /// </summary>
		OpenExisting = 3,
        /// <summary>
        /// Opens a file, always.
        /// If the file exists, the function succeeds and last-error code is set to ERROR_ALREADY_EXISTS (183).
        /// If it doesn't exist, the function creates a new file and sets last-error code to zero.
        /// </summary>
		OpenAlways = 4,
        /// <summary>
        /// Opens a file and truncates it so that its size is zero bytes, only if it exists.
        /// If it does not exist, the function fails and last-error code is set to ERROR_FILE_NOT_FOUND (2).
        /// The calling process must open the file with the GENERIC_WRITE bit set as part of the dwDesiredAccess parameter.
        /// </summary>
		TruncateExisting = 5,
	}

    /// <summary>
    /// This flags-enum type denotes various attributes of a file,
    /// such as Readonly, Hidden, or Directory.
    /// Note: This mostly, but not exactly, duplicates System.IO.FileAttributes.
    /// </summary>
	[Flags]
	public enum NativeFileAttributes : uint
	{
        /// <summary>
        /// This file is marked as read-only. Applications can read this file, but not write to or delete it.
        /// This attribute is not honored on directories.
        /// </summary>
		Readonly = 0x00000001,
        /// <summary>
        /// This file is marked as "hidden", and thus is not included in an ordinary directory listing.
        /// </summary>
		Hidden = 0x00000002,
        /// <summary>
        /// This file or directory is marked as a "System" file, that the operating system uses.
        /// </summary>
		System = 0x00000004,
        /// <summary>
        /// This is actually a directory, or folder.
        /// </summary>
		Directory = 0x00000010,
        /// <summary>
        /// This file has the "Archive" bit set.
        /// Applications typically use this attribute to mark files for backup or removal.
        /// </summary>
		Archive = 0x00000020,
        /// <summary>
        /// This attribute is reserved for system use.
        /// </summary>
		Device = 0x00000040,
        /// <summary>
        /// This is just a "normal" file.
        /// </summary>
		Normal = 0x00000080,
        /// <summary>
        /// This attribute denotes a file that is being used for temporary storage.
        /// File systems avoid writing data back to mass storage if sufficient cache memory is available,
        /// because typically, an application deletes a temporary file after the handle is closed.
        /// In that scenario, the system can entirely avoid writing the data.
        /// Otherwise, the data is written after the handle is closed.
        /// </summary>
		Temporary = 0x00000100,
        /// <summary>
        /// This attribute denotes a file that is a sparse file.
        /// </summary>
		SparseFile = 0x00000200,
        /// <summary>
        /// This attribute denotes a file or directory that has an associated reparse point,
        /// or a file that is a symbolic link.
        /// </summary>
		ReparsePoint = 0x00000400,
        /// <summary>
        /// This file or directory is compressed. For a directory, compression is the default for newly created files
        /// and directories.
        /// </summary>
		Compressed = 0x00000800,
        /// <summary>
        /// The data of this file is not available immediately. This attribute indicates that the file data
        /// is physically moved to offline storage. This attribute is used by Remote Storage, which is the hierarchical
        /// storage management software. Applications should not arbitrarily change this attribute.
        /// </summary>
		Offline = 0x00001000,
        /// <summary>
        /// This file or directory is not to be indexed by the content indexing service.
        /// </summary>
		NotContentIndexed = 0x00002000,
        /// <summary>
        /// This file is encrypted. For a file, all data streams in this file are encrypted.
        /// For a directory, encryption is the default for newly created files and subdirectories.
        /// </summary>
		Encrypted = 0x00004000,
        /// <summary>
        /// This file is set as write-through, meaning writes to it are immediately written to disk.
        /// </summary>
		Write_Through = 0x80000000,
        /// <summary>
        /// The file or device is being opened or created for asynchronous I/O.
        /// </summary>
		Overlapped = 0x40000000,
        /// <summary>
        /// There are strict requirements for successfully working with files opened with the NoBuffering flag.
        /// For details see the section on "File Buffering" in the online MSDN documentation.
        /// </summary>
		NoBuffering = 0x20000000,
        /// <summary>
        /// Access is intended to be random. The system can use this as a hint to optimize file caching.
        /// </summary>
		RandomAccess = 0x10000000,
        /// <summary>
        /// Access is intended to be sequential from beginning to end.
        /// The system can use this as a hint to optimize file caching.
        /// </summary>
		SequentialScan = 0x08000000,
        /// <summary>
        /// The file is to be deleted immediately after all of its handles are closed, which includes
        /// the specified handle and any other open or duplicated handles.
        /// If there are existing open handles to a file, the call fails unless they were all opened with the Delete share mode.
        /// Subsequent open requests for the file fail, unless the Delete share mode is specified.
        /// </summary>
		DeleteOnClose = 0x04000000,
        /// <summary>
        /// The file is being opened or created for a backup or restore operation.
        /// The system ensures that the calling process overrides file security checks
        /// when the process has SE_BACKUP_NAME and SE_RESTORE_NAME privileges.
        /// You must set this flag to obtain a handle to a directory.
        /// A directory handle can be passed to some functions instead of a file handle.
        /// </summary>
		BackupSemantics = 0x02000000,
        /// <summary>
        /// Access will occur according to POSIX rules.
        /// This includes allowing multiple files with names, differing only in case,
        /// for file systems that support that naming.
        /// Use care when using this option, because files created with this flag may not
        /// be accessible by applications that are written for MS-DOS or 16-bit Windows.
        /// </summary>
		PosixSemantics = 0x01000000,
        /// <summary>
        /// Normal reparse point processing will not occur; an attempt to open the reparse point will be made.
        /// When a file is opened, a file handle is returned,
        /// whether or not the filter that controls the reparse point is operational.
        /// </summary>
		OpenReparsePoint = 0x00200000,
        /// <summary>
        /// The file data is requested, but it should continue to be located in remote storage.
        /// It should not be transported back to local storage.
        /// This flag is for use by remote storage systems.
        /// </summary>
		OpenNoRecall = 0x00100000,
        /// <summary>
        /// I find no information on this attribute.
        /// </summary>
		FirstPipeInstance = 0x00080000
	}
}