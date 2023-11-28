using System;
using System.IO;
using System.Text;
using Hurst.LogNut;
using Hurst.LogNut.Util;


namespace VsDevTool.DomainModels
{
    public class FileSourceLocation : IComparable
    {
        public FileSourceLocation( string pathname, string sourceRootFolder )
        {
            SourcePathname = pathname;
            SourceRootFolder = sourceRootFolder;
        }

        public string SourcePathname { get; set; }

        public string SourceRootFolder { get; set; }

        public string GetDestinationPathname(string commonRootOfSource, string destinationRootFolder )
        {
            //CBL Must NOT copy source-files that originate from ABOVE the level of the source-project.
            // Probably I should warn of this from the start.

            // If SourcePathname is C:\dev\Apps\ProjectA\file.txt,
            // and the commonRootOfSource is C:\dev, then destination = D:\DestinFolder\dev\Apps\ProjectA\file.txt
            //

            // For the example, this = \Apps\ProjectA\file.txt
            string relativePath = SourcePathname.WithoutAtStart( commonRootOfSource );

            string drive, dirPartOfCommonSourceRoot;
            bool ok = FileStringLib.GetDriveAndDirectory(commonRootOfSource, drive: out drive, directory: out dirPartOfCommonSourceRoot );
            if (!ok)
            {
                LogManager.LogError("GetDriveAndDirectory failed in GetDestinationPathname");
            }

            // This = D:\DestinFolder + C:\dev
            string destinationPathname = ZPath.Combine( destinationRootFolder, dirPartOfCommonSourceRoot, relativePath );

            return destinationPathname;
        }

        #region CompareTo
        /// <summary>
        /// Given another FileSourceLocation object, return 0 if they are equal, 1 if greater and 0 if lessor (in terms of the pathname properties).
        /// </summary>
        /// <param name="otherObject">the other Object to compare this to</param>
        /// <returns>the result of calling CompareTo on their SourcePathnames</returns>
        public int CompareTo( object otherObject )
        {
            if ((object)otherObject == null)
            {
                return 1;
            }
            FileSourceLocation otherFileSourceLocation = otherObject as FileSourceLocation;
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherFileSourceLocation != null)
            {
                return this.SourcePathname.CompareTo( otherFileSourceLocation.SourcePathname );
            }
            else
            {
                throw new ArgumentException( "otherObject is not a FileSourceLocation." );
            }
        }
        #endregion

        #region Equals
        /// <summary>
        /// Determines whether the specified object is equal to the current object,
        /// in terms of the SourcePathname that they contain
        /// </summary>
        /// <returns>
        /// true if the specified object is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals( object obj )
        {
            if (obj == null)
            {
                return false;
            }
            FileSourceLocation otherFileSourceLocation = obj as FileSourceLocation;
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherFileSourceLocation == null)
            {
                return false;
            }
            return this.SourcePathname.Equals( otherFileSourceLocation.SourcePathname, StringComparison.InvariantCultureIgnoreCase );
        }

        /// <summary>
        /// Determines whether the specified FileSourceLocation is equal to the current one, in terms of the SourcePathname they contain.
        /// </summary>
        /// <returns>
        /// true if the specified FileSourceLocation  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="otherFileSourceLocation">The FileSourceLocation to compare with the current one. </param>
        public bool Equals( FileSourceLocation otherFileSourceLocation )
        {
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherFileSourceLocation == null)
            {
                return false;
            }
            return this.SourcePathname.Equals( otherFileSourceLocation.SourcePathname, StringComparison.InvariantCultureIgnoreCase );
        }
        #endregion

        #region GetHashCode
        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return this.SourcePathname.GetHashCode();
        }
        #endregion

        #region operator ==

        public static bool operator ==( FileSourceLocation a, FileSourceLocation b )
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals( a, b ))
            {
                return true;
            }

            // If one is null, but not both, return false.
            // Here, I cast a and b to Object first, in order to NOT result in a call back to this same operator and thus cause an infinite loop.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the pathnames match.
            return a.SourcePathname.Equals( b.SourcePathname, StringComparison.InvariantCultureIgnoreCase );
        }

        public static bool operator !=( FileSourceLocation a, FileSourceLocation b )
        {
            return !(a == b);
        }
        #endregion

        #region ToString
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder("FileSourceLocation(SourcePathname=");
            sb.Append(SourcePathname).Append(", SourceRootFolder=").Append(SourceRootFolder).Append(")");
            return sb.ToString();
        }
        #endregion
    }
}
