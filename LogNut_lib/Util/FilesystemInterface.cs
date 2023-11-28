using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This is similar to FilesystemLib but is intended to provide methods that can be mocked
    /// for unit-testing.
    /// </summary>
    public class FilesystemInterface : IFilesystemInterface
    {
        //TODO:  This is a WIP

        public FilesystemInterface()
        {
        }

        #region GetRemovableDriveContainingFile
        /// <summary>
        /// Look for any removable-drive that contains the given file and, if found,
        /// return the directory-path in the form "D:\" .
        /// </summary>
        /// <param name="identifyingFilename">a file which serves to identify the removable-drive of interest</param>
        /// <returns>the path of the drive found, or null if none is present at this moment</returns>
        public string GetRemovableDriveContainingFile( string identifyingFilename )
        {
            string rootDirOfDrive = null;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    string thisDriveRoot = drive.RootDirectory.FullName;
                    string pathnameOfIdentifyingFile = Path.Combine(thisDriveRoot, identifyingFilename);
                    if (File.Exists( pathnameOfIdentifyingFile ))
                    {
                        // Found the file that identifies this drive as the one that is of interest.
                        rootDirOfDrive = thisDriveRoot;
                        break;
                    }
                }
            }
            return rootDirOfDrive;
        }
        #endregion
    }
}
