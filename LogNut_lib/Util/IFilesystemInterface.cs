using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// The facilities that this specifies is similar to FilesystemLib but is intended to provide methods that can be mocked
    /// for unit-testing.
    /// </summary>
    public interface IFilesystemInterface
    {
        /// <summary>
        /// Look for any removable-drive that contains the given file and, if found,
        /// return the directory-path in the form "D:\" .
        /// </summary>
        /// <param name="identifyingFilename">a file which serves to identify the removable-drive of interest</param>
        /// <returns>the path of the drive found, or null if none is present at this moment</returns>
        string GetRemovableDriveContainingFile( string identifyingFilename );
    }
}
