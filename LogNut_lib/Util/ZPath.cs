using System;
using System.Text;


// CBL This comes from ZLongPathLib.


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// A utility-class to provide some additional static methods for this project.
    /// </summary>
    public static class ZPath
    {
        #region Combine
        /// <summary>
        /// Return the two filesystem-paths concatenated into one path.
        /// </summary>
        /// <param name="path1">the first path to be combined</param>
        /// <param name="path2">the second path to add to the 1st one</param>
        /// <returns>the 2 paths combined, with the appropriate UNC prefix and path-separators</returns>
        /// <exception cref="ArgumentNullException">The paths must not be null.</exception>
        /// <exception cref="ArgumentException">The 2nd path must not be absolute.</exception>
        /// <remarks>
        /// The purpose of this method is to provide some additional robustness over that of Path.Combine.
        /// 
        /// The result does not end with a back-slash even if the 2nd path argument had one.
        /// 
        /// Any leading or trailing spaces are trimmed.
        /// 
        /// Arguments that are an empty string, are simply ignored -- the other path is returned.
        /// 
        /// All forward-slashes are changed to back-slashes.
        /// 
        /// The 2nd path arguments must not be absolute -- in other words, not begin with a drive-letter.
        /// </remarks>
        public static string Combine( string path1,
                                      string path2 )
        {
            if (path1 == null)
            {
                throw new ArgumentNullException( "path1" );
            }
            if (path2 == null)
            {
                throw new ArgumentNullException( "path2" );
            }
            string path1Trimmed = path1.Trim().Replace( '/', '\\' ).TrimEnd( '\\' );
            string path2Trimmed = path2.Trim().Replace( '/', '\\' ).TrimStart( '\\' ).TrimEnd( '\\' );
            if (FileStringLib.IsAbsolutePath( path2Trimmed ))
            {
                throw new ArgumentException( "The 2nd path argument must not start with a drive.", "path2" );
            }
            if (path1Trimmed.Length == 0)
            {
                return path2Trimmed;
            }
            else if (path2Trimmed.Length == 0)
            {
                return path1Trimmed;
            }
            else
            {
                return path1Trimmed + @"\" + path2Trimmed;
            }
        }

        /// <summary>
        /// Return the three filesystem-paths concatenated into one path.
        /// </summary>
        /// <param name="path1">the path to add the other two parts to</param>
        /// <param name="path2">a path to add to the 1st one</param>
        /// <param name="path3">another path to add to the 1st one</param>
        /// <returns>the 2 paths combined, with the appropriate UNC prefix and path-separators</returns>
        /// <exception cref="ArgumentNullException">The paths must not be null.</exception>
        /// <exception cref="ArgumentException">The 2nd and 3rd paths must not be absolute.</exception>
        /// <remarks>
        /// The purpose of this method is to provide some additional robustness and functionality over that of Path.Combine.
        /// 
        /// The result does not end with a back-slash even if the final path argument had one.
        /// 
        /// Any leading or trailing spaces are trimmed.
        /// 
        /// Arguments that are an empty string, are simply ignored -- the other arguments are combined into the result.
        /// 
        /// All forward-slashes are changed to back-slashes.
        /// 
        /// The 2nd and 3rd path arguments must not be absolute -- in other words, not begin with a drive-letter.
        /// </remarks>
        public static string Combine( string path1,
                                      string path2,
                                      string path3 )
        {
            if (path1 == null)
            {
                throw new ArgumentNullException( "path1" );
            }
            if (path2 == null)
            {
                throw new ArgumentNullException( "path2" );
            }
            if (path3 == null)
            {
                throw new ArgumentNullException( "path3" );
            }
            string path1Trimmed = path1.Trim().Replace( '/', '\\' ).TrimEnd( '\\' );
            string path2Trimmed = path2.Trim().Replace( '/', '\\' ).TrimStart( '\\' ).TrimEnd( '\\' );
            string path3Trimmed = path3.Trim().Replace( '/', '\\' ).TrimStart( '\\' ).TrimEnd( '\\' );
            if (FileStringLib.IsAbsolutePath( path2Trimmed ))
            {
                throw new ArgumentException( "The 2nd path argument must not start with a drive.", "path2" );
            }
            if (FileStringLib.IsAbsolutePath( path3Trimmed ))
            {
                throw new ArgumentException( "The 3rd path argument must not start with a drive.", "path3" );
            }

            if (path1Trimmed.Length == 0)
            {
                return path2Trimmed + @"\" + path3Trimmed;
            }
            else if (path2Trimmed.Length == 0)
            {
                return path1Trimmed + @"\" + path3Trimmed;
            }
            else if (path3Trimmed.Length == 0)
            {
                return path1Trimmed + @"\" + path2Trimmed;
            }
            else
            {
                return path1Trimmed + @"\" + path2Trimmed + @"\" + path3Trimmed;
            }
        }

        /// <summary>
        /// Return the three filesystem-paths concatenated into one path.
        /// </summary>
        /// <param name="path1">the path to add the other two parts to</param>
        /// <param name="path2">a path to add to the 1st one</param>
        /// <param name="path3">another path to add</param>
        /// <param name="path4">another path to add</param>
        /// <returns>the 2 paths combined, with the appropriate UNC prefix and path-separators</returns>
        /// <exception cref="ArgumentNullException">The paths must not be null.</exception>
        /// <exception cref="ArgumentException">The 2nd and 3rd paths must not be absolute.</exception>
        /// <remarks>
        /// The purpose of this method is to provide some additional robustness and functionality over that of Path.Combine.
        /// 
        /// The result does not end with a back-slash even if the final path argument had one.
        /// 
        /// Any leading or trailing spaces are trimmed.
        /// 
        /// Arguments that are an empty string, are simply ignored -- the other arguments are combined into the result.
        /// 
        /// All forward-slashes are changed to back-slashes.
        /// 
        /// The 2nd and 3rd path arguments must not be absolute -- in other words, not begin with a drive-letter.
        /// </remarks>
        public static string Combine( string path1,
                                      string path2,
                                      string path3,
                                      string path4 )
        {
            if (path1 == null)
            {
                throw new ArgumentNullException( "path1" );
            }
            if (path2 == null)
            {
                throw new ArgumentNullException( "path2" );
            }
            if (path3 == null)
            {
                throw new ArgumentNullException( "path3" );
            }
            if (path4 == null)
            {
                throw new ArgumentNullException( "path4" );
            }
            string path1Trimmed = path1.Trim().Replace( '/', '\\' ).TrimEnd( '\\' );
            string path2Trimmed = path2.Trim().Replace( '/', '\\' ).TrimStart( '\\' ).TrimEnd( '\\' );
            string path3Trimmed = path3.Trim().Replace( '/', '\\' ).TrimStart( '\\' ).TrimEnd( '\\' );
            string path4Trimmed = path4.Trim().Replace( '/', '\\' ).TrimStart( '\\' ).TrimEnd( '\\' );
            if (FileStringLib.IsAbsolutePath( path2Trimmed ))
            {
                throw new ArgumentException( "The 2nd path argument must not start with a drive.", "path2" );
            }
            if (FileStringLib.IsAbsolutePath( path3Trimmed ))
            {
                throw new ArgumentException( "The 3rd path argument must not start with a drive.", "path3" );
            }
            if (FileStringLib.IsAbsolutePath( path4Trimmed ))
            {
                throw new ArgumentException( "The 4th path argument must not start with a drive.", "path4" );
            }

            StringBuilder sb = new StringBuilder();
            bool hasSome = false;
            if (path1Trimmed.Length > 0)
            {
                sb.Append(path1Trimmed);
                hasSome = true;
            }
            if (path2Trimmed.Length > 0)
            {
                if (hasSome)
                {
                    sb.Append( @"\" );
                }
                sb.Append( path2Trimmed );
                hasSome = true;
            }
            if (path3Trimmed.Length > 0)
            {
                if (hasSome)
                {
                    sb.Append( @"\" );
                }
                sb.Append( path3Trimmed );
                hasSome = true;
            }
            if (path4Trimmed.Length > 0)
            {
                if (hasSome)
                {
                    sb.Append( @"\" );
                }
                sb.Append( path4Trimmed );
            }
            return sb.ToString();
        }
        #endregion

        #region IsUncPath
        /// <summary>
        /// Return true if the given filesystem-path is in the Universal Naming Convention (UNC)
        /// form,
        /// which is '\\{share-name}' or '\\?\UNC\{share-name}' (as long as share-name is not empty).
        /// </summary>
        /// <param name="path">the given filesystem-path to check</param>
        /// <returns>true if the path is a valid UNC path</returns>
        /// <exception cref="ArgumentNullException">The value provided for the path argument must not be null.</exception>
        /// <remarks>
        /// UNC names are used to identify network resources using a specific notation. UNC names consist of three parts -
        /// a server name, a share name, and an optional file path.
        /// 
        /// The three elements are combined using backslashes thusly:
        /// 
        /// <code>
        /// \\server\share\file_path
        /// </code>
        /// 
        /// In most versions of Windows, the built-in share name <c>admin$</c> refers to the root directory of the operating-system installation
        /// - usually <code>C:\WINNT</code> or <code>C:\WINDOWS</code>.
        /// 
        /// An empty (but non-null) value for the path argument results in a return-value of <c>false</c>.
        /// 
        /// If the path string has forward-slashes, those will be converted to back-slashes.
        /// </remarks>
        public static bool IsUncPath( string path )
        {
            return FileStringLib.IsUncPath( path );
        }
        #endregion
    }
}
