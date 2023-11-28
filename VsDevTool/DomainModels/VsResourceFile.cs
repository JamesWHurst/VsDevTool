using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Text;
using Hurst.LogNut;
using Hurst.LogNut.Util;


namespace VsDevTool.DomainModels
{
    public class VsResourceFile : VsSourceFile
    {
        /// <summary>
        /// Create a new VsResourceFile object given it's filesystem-path,
        /// that is associated with the given Visual-Studio project.
        /// </summary>
        /// <param name="visualStudioProject">leave this null if not associated with any project</param>
        /// <param name="pathname">the filesystem-path of the given resource-file</param>
        public VsResourceFile( VsProject visualStudioProject, string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( paramName: nameof( pathname ) );
            }
            _associatedVsProject = visualStudioProject;

            string fullPathname;
            if (File.Exists( pathname ))
            {
                fullPathname = pathname;
            }
            else
            {
                if (visualStudioProject == null)
                {
                    throw new FileNotFoundException( message: "The vs-project argument is null and the given pathname (" + pathname + ") is not found", fileName: pathname );
                }
                else
                {
                    // Try appending it to the project-directory path.
                    string dir = FileStringLib.GetDirectoryOfPath( visualStudioProject.Pathname );
                    fullPathname = Path.Combine( dir, pathname );
                    if (!File.Exists( fullPathname ))
                    {
                        throw new FileNotFoundException( message: "Neither the given pathname (" + pathname + ") nor combined with that of the given VsProject (" + fullPathname + ") are found", fileName: pathname );
                    }
                }
            }
            this.Pathname = fullPathname;
            this.ContentSummary = "69 strings";
            this.SetName = "A";
        }

        /// <summary>
        /// Create a new VsResourceFile instance with the given filesystem-pathname denoting the file that this represents.
        /// This does NOT check for the existence of the file.
        /// </summary>
        /// <param name="pathname">the filesystem-path of the given resource-file</param>
        public VsResourceFile( string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( paramName: nameof( pathname ) );
            }
            this.Pathname = pathname;
        }

        public string SetName { get; set; }

        public string ContentSummary
        {
            get { return _contentSummary; }
            set
            {
                if (value != _contentSummary)
                {
                    _contentSummary = value;
                }
            }
        }

        public Dictionary<string, string> GetStrings()
        {
            var result = new Dictionary<string, string>();

            using (ResXResourceReader resxReader = new ResXResourceReader( this.Pathname ))
            {
                foreach (DictionaryEntry entry in resxReader)
                {
                    string key = entry.Key.ToString();
                    string value = entry.Value.ToString();
                    result.Add( key: key, value: value );
                }
            }

            return result;
        }

        #region WriteResxFile
        /// <summary>
        /// Given a list of LanguageResources, create a RESX file that contains those resources
        /// and return the number of items written to it.
        /// </summary>
        /// <param name="collectionOfLanguageResources">the collection of LanguageResource objects to write to the file.</param>
        /// <returns>the number of resource-strings that were written to the file</returns>
        public int WriteResxFile( IList<LanguageResource> collectionOfLanguageResources )
        {
            int countWritten = 0;
            int n = collectionOfLanguageResources.Count;
            if (n > 0)
            {
                using (ResXResourceWriter resx = new ResXResourceWriter( Pathname ))
                {
                    for (int i = 0; i < n; i++)
                    {
                        var item = collectionOfLanguageResources[i];
                        if (!String.IsNullOrWhiteSpace( item.Key ))
                        {
                            resx.AddResource( name: item.Key, value: item.OtherLanguageValue );
                            countWritten++;
                        }
                        else
                        {
                            LogManager.Warn( "In VsResourceFile.WriteResxFile, skipping item " + i + " because it has no key assigned." );
                        }
                    }
                }
            }
            return countWritten;
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to yield a useful indication of this object's state.
        /// </summary>
        /// <returns>a string denoting some of this object's properties</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "VsSourceFile(" );
            sb.Append( ", Pathname = " ).Append( Pathname );
            sb.Append( ")" );
            return sb.ToString();
        }
        #endregion

        private VsProject _associatedVsProject;
        private string _contentSummary;
    }
}
