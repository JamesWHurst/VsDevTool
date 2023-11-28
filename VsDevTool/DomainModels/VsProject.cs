using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using Hurst.LogNut;
using Hurst.LogNut.Util;
using static Hurst.LogNut.Util.StringLib;
using Hurst.XamlDevLib;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;


namespace VsDevTool.DomainModels
{
    public class VsProject : IComparable
    {
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public VsProject()
        {
        }

        public VsProject( string pathnameOfProjectFile, bool isToCheckForExistence = true )
        {
            if (isToCheckForExistence)
            {
                if (pathnameOfProjectFile == null)
                {
                    throw new ArgumentNullException(nameof(pathnameOfProjectFile));
                }
            }
            if (pathnameOfProjectFile.EndsWith(".csproj"))
            {
                this.Pathname = pathnameOfProjectFile;
            }
            else // the given pathname does not end with ".csproj"
            {
                string tentativePathname = pathnameOfProjectFile + ".csproj";
                if (isToCheckForExistence)
                {
                    if (!File.Exists(tentativePathname))
                    {
                        throw new FileNotFoundException(message: "Neither the project-pathname as given, nor with .csproj appended, are found on-disk", fileName: pathnameOfProjectFile);
                    }
                }
                this.Pathname = tentativePathname;
            }
        }
        #endregion

        #region public properties

        #region AssemblyName
        public string AssemblyName
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    throw new InvalidOperationException("You need to call GetInformationFromTheProjectFile before you can get the AssemblyName.");
                }
                return _assemblyName;
            }
            set { _assemblyName = value; }
        }
        #endregion

        #region AssemblyVersion
        public string AssemblyVersion
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    throw new InvalidOperationException("You need to call GetInformationFromTheProjectFile before you can get the AssemblyName.");
                }
                return _assemblyVersion;
            }
            set { _assemblyVersion = value; }
        }
        #endregion

        public List<VsAssembly> DependentAssemblies { get; set; }

        #region Description
        /// <summary>
        /// Get or set the description-text that is set for this VS-Project.
        /// </summary>
        /// <remarks>
        /// This is not saved within application-graph snapshots, because it is not one of the properties
        /// that are regarded as having a material impact on the program operation.
        /// </remarks>
        [XmlIgnore]
        public string Description
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    throw new InvalidOperationException("You need to call GetInformationFromTheProjectFile before you can get the AssemblyName.");
                }
                return _description;
            }
            set { _description = value; }
        }
        #endregion

        #region FileVersion
        public string FileVersion
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    throw new InvalidOperationException("You need to call GetInformationFromTheProjectFile before you can get the AssemblyName.");
                }
                return _assemblyFileVersion;
            }
            set { _assemblyFileVersion = value; }
        }
        #endregion

        #region Folder
        public string Folder
        {
            get { return FileStringLib.GetDirectoryOfPath(Pathname); }
        }
        #endregion

        #region Guid
        /// <summary>
        /// Get or set the GUID (Globally Unique IDentifier) of VS-Project.
        /// </summary>
        /// <remarks>
        /// This is not saved within application-graph snapshots, because it is not one of the properties
        /// that are regarded as having a material impact on the program operation.
        /// </remarks>
        [XmlIgnore]
        public string Guid
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    GetInformationFromTheProjectFile();
                }
                return _guid;
            }
            set { _guid = value; }
        }
        #endregion

        #region NugetPackages
        /// <summary>
        /// Get or set the list of the NuGet packages that this VS-project
        /// contains references to.
        /// </summary>
        public List<NugetPackageReference> NugetPackages
        {
            get
            {
                if (_nugetPackages == null)
                {
                    _nugetPackages = new List<NugetPackageReference>();
                }
                return _nugetPackages;
            }
            set { _nugetPackages = value; }
        }
        #endregion

        #region Pathname
        /// <summary>
        /// Get or set the filesystem-pathname of the Visual Studio project-file
        /// that underlies this VsProject object.
        /// </summary>
        public string Pathname { get; set; }

        #endregion

        #region Product
        /// <summary>
        /// Get or set the text that represents the "Product" for this VS-Project.
        /// </summary>
        /// <remarks>
        /// This is not saved within application-graph snapshots, because it is not one of the properties
        /// that are regarded as having a material impact on the program operation.
        /// </remarks>
        [XmlIgnore]
        public string Product
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    throw new InvalidOperationException("You need to call GetInformationFromTheProjectFile before you can get the AssemblyName.");
                }
                return _product;
            }
            set { _product = value; }
        }
        #endregion

        #region ReferencedProjects
        /// <summary>
        /// Get or set the list of other projects that this project has a direct reference to,
        /// NOT recursively. To get the complete list - recurse down into this list.
        /// </summary>
        public List<VsProject> ReferencedProjects
        {
            get
            {
                if (_referencedProjects == null)
                {
                    _referencedProjects = new List<VsProject>();
                }
                return _referencedProjects;
            }
            set { _referencedProjects = value; }
        }
        #endregion

        #region SourceFiles
        /// <summary>
        /// Get or set the SortedSet of VSSourceFile objects that are the source files
        /// of this project.
        /// </summary>
        public SortedSet<VsSourceFile> SourceFiles
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    throw new InvalidOperationException("You need to call GetInformationFromTheProjectFile before you can get the AssemblyName.");
                }
                return _sourceFiles;
            }
            set { _sourceFiles = value; }
        }
        #endregion

        #region Title
        /// <summary>
        /// Get or set the text that represents the "Title" for this VS-Project.
        /// </summary>
        /// <remarks>
        /// This is not saved within application-graph snapshots, because it is not one of the properties
        /// that are regarded as having a material impact on the program operation.
        /// </remarks>
        [XmlIgnore]
        public string Title
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    GetInformationFromTheProjectFile();
                }
                return _title;
            }
            set { _title = value; }
        }
        #endregion

        #region WhenLastWritten
        /// <summary>
        /// Get or set when this Visual Studio project
        /// last had any changes made to it (the project itself, not it's files).
        /// </summary>
        public DateTime WhenLastWritten
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    GetInformationFromTheProjectFile();
                }
                return _whenLastWritten;
            }
            set { _whenLastWritten = value; }
        }
        #endregion

        #endregion public properties

        #region operator ==

        public static bool operator ==( VsProject a, VsProject b )
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
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
            return a.Pathname.Equals(b.Pathname, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator !=( VsProject a, VsProject b )
        {
            return !(a == b);
        }
        #endregion

        #region CompareTo
        /// <summary>
        /// Given another VsProject object, return 0 if they are equal, 1 if greater and 0 if lessor (in terms of the pathname properties).
        /// </summary>
        /// <param name="otherObject">the other Object to compare this to</param>
        /// <returns>the result of calling CompareTo on their pathnames</returns>
        public int CompareTo( object otherObject )
        {
            if ((object)otherObject == null)
            {
                return 1;
            }
            VsProject otherProject = otherObject as VsProject;
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherProject != null)
            {
                return this.Pathname.CompareTo(otherProject.Pathname);
            }
            else
            {
                throw new ArgumentException("otherObject is not a VsProject.");
            }
        }
        #endregion

        #region Equals
        /// <summary>
        /// Determines whether the specified object is equal to the current object,
        /// in terms of the pathnames that they contain
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
            VsProject otherProject = obj as VsProject;
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherProject == null)
            {
                return false;
            }
            return this.Pathname.Equals(otherProject.Pathname, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified VsProject is equal to the current one, in terms of the pathnames they contain.
        /// </summary>
        /// <returns>
        /// true if the specified VsProject  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="otherProject">The VsProject to compare with the current one. </param>
        public bool Equals( VsProject otherProject )
        {
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherProject == null)
            {
                return false;
            }
            return this.Pathname.Equals(otherProject.Pathname, StringComparison.InvariantCultureIgnoreCase);
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
            return this.Pathname.GetHashCode();
        }
        #endregion

        #region GetAllXamlFiles
        /// <summary>
        /// This simply uses XamlScanner to retrieve all of the XAML files from this project
        /// and return them as an array of ZFileInfo objects.
        /// </summary>
        /// <returns>an array of ZFileInfo objects representing the XAML files</returns>
        public ZFileInfo[] GetAllXamlFiles()
        {
            string rootDir = FileStringLib.GetDirectoryOfPath(this.Pathname);
            var xamlFiles = XamlScanner.GetAllXamlFiles(rootDirectoryPath: rootDir);
            return xamlFiles;
        }

        /// <summary>
        /// This uses XamlScanner to retrieve all of the XAML files from this project
        /// and returns as an array of ZFileInfo objects those which contain the given text-pattern.
        /// </summary>
        /// <returns>an array of ZFileInfo objects representing the XAML files that contain the given pattern</returns>
        public ZFileInfo[] GetAllXamlFiles( string searchPattern )
        {
            List<ZFileInfo> result = new List<ZFileInfo>();
            string rootDir = FileStringLib.GetDirectoryOfPath(this.Pathname);
            var xamlFiles = XamlScanner.GetAllXamlFiles(rootDirectoryPath: rootDir);
            foreach (var file in xamlFiles)
            {
                string filePath = file.FullName;
                using (StreamReader r = new StreamReader(filePath))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line.Contains(searchPattern))
                        {
                            result.Add(file);
                            break;
                        }
                    }
                }
            }
            return result.ToArray();
        }
        #endregion GetAllXamlFiles

        #region DeleteLineFromAllXamlFilesThatContainDesignCulture
        public ZFileInfo[] DeleteLineFromAllXamlFilesThatContainDesignCulture( bool isToKeepOriginal = true )
        {
            // For safety, we will
            // 1: Make a copy of the file, with the extension ".xaml_temp" with the given line deleted.
            // 2. If that works with no exception - delete the original .xaml file (or, rename the original file to .xaml_orig).
            // 3. Rename the .xaml_temp file to .xaml
            //
            //CBL  TODO:
            // 1. Avoid creating the _temp file if no pattern is found in the original.
            List<ZFileInfo> result = new List<ZFileInfo>();
            const string searchPattern = @"lex:LocalizeDictionary.DesignCulture=";
            string rootDir = FileStringLib.GetDirectoryOfPath(this.Pathname);
            var xamlFiles = XamlScanner.GetAllXamlFiles(rootDirectoryPath: rootDir);
            foreach (var file in xamlFiles)
            {
                bool wasPatternFound = false;
                string filePath = file.FullName;
                string tempFilePath = filePath + "_temp";
                using (StreamReader r = new StreamReader(filePath))
                {
                    using (StreamWriter w = new StreamWriter(tempFilePath))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            if (line.Contains(searchPattern))
                            {
                                // Skip copying this line of text, but do add this file to the list.
                                if (!wasPatternFound)
                                {
                                    result.Add(file);
                                    wasPatternFound = true;
                                }
                            }
                            else // no pattern-match on this line.
                            {
                                w.WriteLine(line);
                            }
                        }
                    }
                }

                if (wasPatternFound)
                {
                    string filePathOriginal = filePath + "_orig";
                    if (File.Exists(filePathOriginal))
                    {
                        File.Delete(filePathOriginal);
                    }
                    if (isToKeepOriginal)
                    {
                        File.Move(sourceFileName: filePath, destFileName: filePathOriginal);
                    }

                    File.Move(sourceFileName: tempFilePath, destFileName: filePath);
                }
                else // no pattern found in this file, so nothing more is needed.
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
            } // end loop thru the XAML files.
            return result.ToArray();
        }
        #endregion DeleteLineFromAllXamlFilesThatContainDesignCulture

        #region RefactorLexForAllXamlFiles
        public ZFileInfo[] RefactorLexForAllXamlFiles( string newResourceDictionaryValue, bool isToKeepOriginal = true )
        {
            // For safety, we will
            // 1: Make a copy of the file, with the extension ".xaml_temp" with the given line deleted.
            // 2. If that works with no exception - delete the original .xaml file (or, rename the original file to .xaml_orig).
            // 3. Rename the .xaml_temp file to .xaml
            //
            //CBL  TODO:
            // 1. Avoid creating the _temp file if no pattern is found in the original.
            List<ZFileInfo> result = new List<ZFileInfo>();
            const string searchPattern = @"lex:ResxLocalizationProvider.DefaultAssembly=";
            string rootDir = FileStringLib.GetDirectoryOfPath(this.Pathname);
            var xamlFiles = XamlScanner.GetAllXamlFiles(rootDirectoryPath: rootDir);
            foreach (var file in xamlFiles)
            {
                bool isPatternPresent = false;
                string filePath = file.FullName;

                // First, see if this file contains that pattern at all (to avoid any file-copying when not needed)..
                using (StreamReader r = new StreamReader(filePath))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line.Contains(searchPattern))
                        {
                            if (!isPatternPresent)
                            {
                                result.Add(file);
                                isPatternPresent = true;
                            }
                            // No need to read anymore lines of the file, until we start making the change.
                            break;
                        }
                    }
                }

                if (isPatternPresent)
                {
                    string tempFilePath = filePath + "_temp";
                    bool wasPatternFound = false;
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        using (StreamWriter w = new StreamWriter(tempFilePath))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                if (line.Contains(searchPattern))
                                {
                                    // Skip copying this line of text.
                                    wasPatternFound = true;
                                    // Change this line to match the new pattern..
                                    int indexOfValue = line.IndexOf(searchPattern) + searchPattern.Length;
                                    string originaLinePartToKeep = line.Substring(0, indexOfValue);
                                    string newLine = originaLinePartToKeep + @"""" + newResourceDictionaryValue + @"""";
                                    w.WriteLine(newLine);
                                }
                                else // no pattern-match on this line.
                                {
                                    w.WriteLine(line);
                                }
                            }
                        }
                    }

                    if (wasPatternFound)
                    {
                        string filePathOriginal = filePath + "_orig";
                        if (File.Exists(filePathOriginal))
                        {
                            File.Delete(filePathOriginal);
                        }
                        if (isToKeepOriginal)
                        {
                            File.Move(sourceFileName: filePath, destFileName: filePathOriginal);
                        }
                        else
                        {
                            File.Delete(path: filePath);
                        }
                        File.Move(sourceFileName: tempFilePath, destFileName: filePath);
                    }
                    else // no pattern found in this file, so nothing more is needed.
                    {
                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                    }
                }
            } // end loop thru the XAML files.
            return result.ToArray();
        }
        #endregion RefactorLexForAllXamlFiles

        #region GetPackageReferencesFromTheProjectFile
        public List<NugetPackageReference> GetPackageReferencesFromTheProjectFile()
        {
            if (!_hasBeenChecked)
            {
                GetInformationFromTheProjectFile();
            }

            var result = new List<NugetPackageReference>();

            string rootFolder = FileStringLib.GetDirectoryOfPath(Pathname);
            // Change the 'CurrentDirectory' to the folder of this solution-file,
            // so that relative paths may be correctly resolved.
            string originalValueOfCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(rootFolder);

            int i = 0;
            var lines = FilesystemLib.ReadLines(Pathname).ToList();
            for (i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                //    if (line.Contains("<HintPath>"))
                //    {
                //        string hintPath = line.PartBetween("<HintPath>", "</HintPath>");
                //        string actualPath = Path.GetFullPath(hintPath);
                //        n++;
                //        VsSourceFile file = new VsSourceFile(actualPath);
                //        _sourceFiles.Add(file);
                //}
                string relativePath = null;

                if (line.Contains( "Baseline" ))
                {
                    Debug.WriteLine($"Here is the line: {line}");
                }

                if (line.Contains("<Reference Include=") && !line.Contains("/>"))
                {
                    //string referenceName = line.PartBetween("<Reference Include=", ">").WithoutDoubleQuotes();
                    //bool isLookingForReferencedAssbly = true;
                    // Check the next line..
                    string line2 = lines[i + 1];
                    if (line2.Contains("<HintPath>") && line2.Contains("packages"))
                    {
                        string hintPath = line2.PartBetween("<HintPath>", "</HintPath>");
                        string actualPath = Path.GetFullPath(hintPath);
                        Debug.WriteLine($"hintPath={hintPath}, actual Path={actualPath}");

                        // Get the name from between Include=" and the next comma..
                        int indexOfName = line.IndexOf( "Reference Include=" ) + 19;
                        int indexOfComma = line.IndexOf( ',', indexOfName + 1 );
                        int lengthOfName = indexOfComma - indexOfName;
                        string name = line.Substring( indexOfName, lengthOfName );
                        Debug.WriteLine($"name is {AsQuoted(name)}");

                        // and the version-declared which is after the name on the same line..
                        int indexOfVersionDeclared = line.IndexOf("Version=", indexOfComma) + 8;
                        int indexOfComma2 = line.IndexOf(',', indexOfVersionDeclared + 1);
                        int lengthOfVerDeclared = indexOfComma2 - indexOfVersionDeclared;
                        string versionDeclared = line.Substring(indexOfVersionDeclared, lengthOfVerDeclared);
                        Debug.WriteLine($"versionDeclared is {AsQuoted(versionDeclared)}");

                        // Add the version-text that is within the package path..
                        int indexOfNameInPath = line2.IndexOf( name );
                        int indexOfStartOfVersionInPath = indexOfNameInPath + name.Length + 1;
                        int indexOfEndOfVersionInPath = line2.IndexOf( @"\", indexOfNameInPath + name.Length );
                        int lengthOfVersionInPath = indexOfEndOfVersionInPath - indexOfNameInPath - name.Length - 1;
                        string versionInPackagePath = line2.Substring( indexOfStartOfVersionInPath, lengthOfVersionInPath );
                        Debug.WriteLine($"versionInPackagePath is {AsQuoted(versionInPackagePath)}");

                        var packageReference = new NugetPackageReference( name, versionDeclared);
                        packageReference.HintPath = hintPath;
                        packageReference.VersionInPackagePath = versionInPackagePath;
                        NugetPackages.Add(packageReference);
                        result.Add(packageReference);
                    }
                    //else if (line.Contains("<ProjectReference Include="))
                    //{
                    //    relativePath = line.PartBetween("Include=", ">").WithoutDoubleQuotes();
                    //    string actualPath = Path.GetFullPath(relativePath);
                    //    var project = new VsProject(actualPath);
                    //    ReferencedProjects.Add(project);
                    //}
                    else
                    {
                        continue;
                    }
                }
            }

            // Restore the original setting of the 'Current Directory'.
            Directory.SetCurrentDirectory(originalValueOfCurrentDirectory);

            return result;
        }
        #endregion GetPackageReferencesFromTheProjectFile

        public void GetInformationFromTheProjectFile()
        {
            _hasBeenChecked = true;
            if (File.Exists(Pathname))
            {
                WhenLastWritten = FilesystemLib.GetFileLastWriteTime(Pathname);
            }
            else
            {
                throw new FileNotFoundException(message: "Unable to find project file " + Pathname);
            }
            string rootFolder = FileStringLib.GetDirectoryOfPath(Pathname);

            // Get information from the project-definition file.
            _sourceFiles = new SortedSet<VsSourceFile>();
            bool isLookingForReferencedAssbly = false;
            string referenceName = null;

            // Change the 'CurrentDirectory' to the folder of this solution-file,
            // so that relative paths may be correctly resolved.
            string originalValueOfCurrentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(rootFolder);
            int n = 0;

            _sourceFiles.Add(new VsSourceFile(Pathname));
            foreach (string line in FilesystemLib.ReadLines(Pathname))
            {
                if (isLookingForReferencedAssbly)
                {
                    isLookingForReferencedAssbly = false;
                    if (line.Contains("<HintPath>"))
                    {
                        string hintPath = line.PartBetween("<HintPath>", "</HintPath>");
                        string actualPath = Path.GetFullPath(hintPath);
                        n++;
                        VsSourceFile file = new VsSourceFile(actualPath);
                        _sourceFiles.Add(file);
                    }
                }

                string relativePath = null;
                if (line.Contains("<AssemblyName>"))
                {
                    _assemblyName = line.PartBetween("<AssemblyName>", "</AssemblyName>");
                }
                else if (line.Contains("<ProjectGuid>"))
                {
                    Guid = line.PartBetween("<ProjectGuid>{", "}</ProjectGuid>");
                }
                // Get any referenced assemblies (not projects)..
                else if (line.Contains("<Reference Include=") && !line.Contains("/>"))
                {
                    referenceName = line.PartBetween("<Reference Include=", ">").WithoutDoubleQuotes();
                    isLookingForReferencedAssbly = true;
                }
                else if (line.Contains("<ProjectReference Include="))
                {
                    relativePath = line.PartBetween("Include=", ">").WithoutDoubleQuotes();
                    string actualPath = Path.GetFullPath(relativePath);
                    var project = new VsProject(actualPath);
                    ReferencedProjects.Add(project);
                }

                // Get the source-files.
                if (line.Contains("<Compile Include") && line.EndsWith("/>"))
                {
                    relativePath = line.PartBetween("Include=", "/>").WithoutDoubleQuotes();
                }
                else if (line.Contains("<Compile Include") && line.EndsWith(">"))
                {
                    relativePath = line.PartBetween("Include=", ">").WithoutDoubleQuotes();
                }
                else if (line.Contains("<Resource Include") && line.EndsWith("/>"))
                {
                    relativePath = line.PartBetween("Include=", "/>").WithoutDoubleQuotes();
                }
                else if (line.Contains("<Page Include") && line.EndsWith(">"))
                {
                    relativePath = line.PartBetween("Include=", ">").WithoutDoubleQuotes();
                }
                else if (line.Contains("<EmbeddedResource Include=") && line.EndsWith("/>"))
                {
                    relativePath = line.PartBetween("Include=", "/>").WithoutDoubleQuotes();
                }
                else if (line.Contains("<EmbeddedResource Include=") && line.EndsWith(">"))
                {
                    relativePath = line.PartBetween("Include=", ">").WithoutDoubleQuotes();
                }
                else if (line.Contains("<ApplicationDefinition Include=") && line.EndsWith(">"))
                {
                    relativePath = line.PartBetween("Include=", ">").WithoutDoubleQuotes();
                }
                else if (line.Contains("<None Include=") && line.EndsWith("/>"))
                {
                    relativePath = line.PartBetween("Include=", "/>").WithoutDoubleQuotes();
                }
                else if (line.Contains("<None Include=") && line.EndsWith(">"))
                {
                    relativePath = line.PartBetween("Include=", ">").WithoutDoubleQuotes();
                }
                else
                {
                    continue;
                }
                // relativePath = line.PartBetween( "Include=", ">" ).WithoutDoubleQuotes();
                if (!String.IsNullOrWhiteSpace(relativePath))
                {
                    try
                    {
                        string actualPath = Path.GetFullPath(relativePath);
                        VsSourceFile file = new VsSourceFile(actualPath);
                        _sourceFiles.Add(file);
                        //Debug.WriteLine( "Added sourcefile " + file.Pathname );
                    }
                    catch (Exception x)
                    {
                        string msg = "Project = " + this.Pathname + ", line = " + line + ", relativePath = " + StringLib.CharacterDescriptions(relativePath);
                        Logger.LogError(x, msg);
                    }
                }
            }

            // Get the source-files.
            //ZDirectoryInfo rootDir = new ZDirectoryInfo( rootFolder );
            //var files = rootDir.GetFiles( SearchOption.AllDirectories );
            //var filteredFiles = from f in files where (!f.DirectoryName.Contains( "bin" ) && !f.DirectoryName.Contains( "obj" ) && !f.DirectoryName.Contains( ".vs" )) select f;

            //foreach (var f in filteredFiles)
            //{
            //    string extension = FileStringLib.GetExtension( f.Name );
            //    //if (!extension.Equals( "vspscc" ) && !extension.Equals( "user" ) && !extension.Equals( "suo" ))
            //    if (!extension.Matches( "vspscc", "user", "suo" ))
            //    {
            //        n++;
            //        VsSourceFile file = new VsSourceFile( f.FullName );
            //        _sourceFiles.Add( file );
            //    }
            //}

            // Get some stuff from the AssemblyInfo.cs file.
            string folder = FileStringLib.GetDirectoryOfPath(Pathname);
            string pathOfAssemblyInfo = Path.Combine(folder, "Properties", "AssemblyInfo.cs");
            if (!File.Exists(pathOfAssemblyInfo))
            {
                throw new InvalidOperationException(message: "The AssemblyInfo.cs file does not exist.");
            }
            foreach (string line in FilesystemLib.ReadLines(pathOfAssemblyInfo))
            {
                if (line.Contains("AssemblyTitle"))
                {
                    _title = line.PartBetween("AssemblyTitle(", ")]").WithoutDoubleQuotes();
                }
                else if (line.Contains("AssemblyDescription"))
                {
                    _description = line.PartBetween("AssemblyDescription(", ")]").WithoutDoubleQuotes();
                }
                else if (line.Contains("AssemblyProduct"))
                {
                    _product = line.PartBetween("AssemblyProduct(", ")]").WithoutDoubleQuotes();
                }
                else if (line.Contains("AssemblyVersion"))
                {
                    _assemblyVersion = line.PartBetween("AssemblyVersion(", ")]").WithoutDoubleQuotes();
                }
                else if (line.Contains("AssemblyFileVersion"))
                {
                    _assemblyFileVersion = line.PartBetween("AssemblyFileVersion(", ")]").WithoutDoubleQuotes();
                }
            }

            // Enter defaults for the values not set.
            if (String.IsNullOrWhiteSpace(_assemblyName))
            {
                _assemblyName = "Not set";
            }
            if (String.IsNullOrWhiteSpace(_assemblyVersion))
            {
                _assemblyVersion = "Not set";
            }
            if (String.IsNullOrWhiteSpace(_assemblyFileVersion))
            {
                _assemblyFileVersion = "Not set";
            }
            if (String.IsNullOrWhiteSpace(_description))
            {
                _description = "Not set";
            }
            if (String.IsNullOrWhiteSpace(_product))
            {
                _product = "Not set";
            }
            if (String.IsNullOrWhiteSpace(_title))
            {
                _title = "Not set";
            }
            if (String.IsNullOrWhiteSpace(_guid))
            {
                _guid = "Not set";
            }
            // Restore the original setting of the 'Current Directory'.
            Directory.SetCurrentDirectory(originalValueOfCurrentDirectory);
        }

        public Dictionary<string, string> GetStringsFromResourceFile( string resourceFilename )
        {
            string pathname;
            if (File.Exists(resourceFilename))
            {
                pathname = resourceFilename;
            }
            else
            {
                // Try appending it to the project-directory path.
                string dir = FileStringLib.GetDirectoryOfPath(this.Pathname);
                pathname = Path.Combine(dir, resourceFilename);
                if (!File.Exists(pathname))
                {
                    throw new FileNotFoundException(message: "The given resourceFilename (" + pathname + ") is not found", fileName: resourceFilename);
                }
            }

            var resourceFile = new VsResourceFile(pathname);
            return resourceFile.GetStrings();
        }

        public Dictionary<string, string> GetGlobalStrings()
        {
            string rootDirectoryPath = this.Pathname;
            var files = XamlScanner.GetAllXamlFiles(rootDirectoryPath);
            var result = new Dictionary<string, string>();

            foreach (var file in files)
            {
                string thisFilename = file.FullName;
                Console.WriteLine("Looking into file {0}", thisFilename);
                var stringsFromThisFile = XamlScanner.GetGlobalStrings(pathnameOfXamlFile: thisFilename);
                result.AddRange(collection: stringsFromThisFile);
            }
            return result;
        }

        public bool SetCompany( string companyName, out string reason )
        {
            if (companyName == null)
            {
                throw new ArgumentNullException(nameof(companyName));
            }

            bool wasFound = SetAssemblyInfoProperty(propertyName: "AssemblyCompany", propertyValue: companyName, reason: out reason);

            return wasFound;
        }

        public bool SetCopyright( string copyrightNotice, out string reason )
        {
            if (copyrightNotice == null)
            {
                throw new ArgumentNullException(nameof(copyrightNotice));
            }

            bool wasFound = SetAssemblyInfoProperty(propertyName: "AssemblyCopyright", propertyValue: copyrightNotice, reason: out reason);

            return wasFound;
        }

        #region SerializeToXML
        /// <summary>
        /// You must override this in your subclass to accomplish the serialization of your class to the XML file.
        /// </summary>
        public void SerializeToXML( string pathname )
        {
            XmlSerializer serializer = new XmlSerializer(typeof(VsProject), new Type[] { });
            using (var textWriter = new StreamWriter(pathname))
            {
                serializer.Serialize(textWriter, this);
            }
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to yield a useful indication of this object's state.
        /// </summary>
        /// <returns>a string denoting some of this object's properties</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("VsProject(");
            sb.Append("Pathname=").Append(Pathname);
            if (!_hasBeenChecked)
            {
                sb.Append(" (this has not been loaded from it's project-file)");
            }
            else
            {
                // Only include the last-written time, if that has already been gotten.
                if (_whenLastWritten != default(DateTime))
                {
                    sb.Append(", WhenLastWritten=").Append(TimeLib.AsStandardDateTimeString(_whenLastWritten));
                }
                if (_assemblyName != null)
                {
                    sb.Append(", AssemblyName=").Append(_assemblyName);
                }
                sb.Append(")");
            }
            return sb.ToString();
        }
        #endregion

        #region internal implementation

        #region Logger
        /// <summary>
        /// Get the logger for this class to use.
        /// </summary>
        private static Logger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetCurrentClassLogger();
                }
                return _logger;
            }
            set { _logger = value; }
        }
        private static Logger _logger;
        #endregion

        #region SetAssemblyInfoProperty
        private bool SetAssemblyInfoProperty( string propertyName, string propertyValue, out string reason )
        {
            reason = String.Empty;
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (propertyValue == null)
            {
                throw new ArgumentNullException("propertyValue");
            }
            bool wasFound = false;
            if (File.Exists(Pathname))
            {
                bool isNewPropertyEmpty = String.IsNullOrWhiteSpace(propertyValue);

                string rootFolder = FileStringLib.GetDirectoryOfPath(Pathname);
                // Get the path of the AssemblyInfo.cs file.
                string pathOfAssemblyInfo = Path.Combine(rootFolder, "Properties", "AssemblyInfo.cs");
                if (File.Exists(pathOfAssemblyInfo))
                {
                    ZFileInfo zAssemblyInfo = new ZFileInfo(pathOfAssemblyInfo);

                    // Remove the read-only attributes, if necessary.
                    bool wasReadOnly = zAssemblyInfo.IsReadOnly;
                    if (wasReadOnly)
                    {
                        //Logger.LogDebug( "wasReadOnly" );
                        zAssemblyInfo.IsReadOnly = false;
                    }

                    // Prepare for a copy of this file.
                    string pathnameOfCopy = zAssemblyInfo.MakeTempCopy();

                    // Read lines from the config-file, and write them to the temporary file app_TEMP.config...
                    using (var reader = new StreamReader(pathOfAssemblyInfo))
                    using (var writer = new StreamWriter(pathnameOfCopy))
                    {
                        String lineOfText, text;
                        while ((lineOfText = reader.ReadLine()) != null)
                        {
                            if (lineOfText.Contains(propertyName))
                            {
                                if (isNewPropertyEmpty)
                                {
                                    text = "[assembly: " + propertyName + "( \"\" )]";
                                }
                                else
                                {
                                    text = "[assembly: " + propertyName + "( \"" + propertyValue + "\" )]";
                                }
                                wasFound = true;
                            }
                            else
                            {
                                text = lineOfText;
                            }
                            writer.WriteLine(text);
                        }
                    }

                    if (wasFound)
                    {
                        // Delete the original and rename the copy to the same name as the original.
                        FilesystemLib.DeleteFile(pathOfAssemblyInfo);
                        FilesystemLib.MoveFile(pathnameOfCopy, pathOfAssemblyInfo);
                        if (isNewPropertyEmpty)
                        {
                            reason = "Successfully removed the content of property " + propertyName + ".";
                        }
                        else
                        {
                            reason = "Successfully set " + propertyName + "to " + propertyValue.WithinDoubleQuotes() + " .";
                        }
                    }
                    else
                    {
                        reason = "No line was found within AssemblyInfo.cs that contained the property " + propertyName + ".";
                    }

                    // Restore the readonly-attribute if the file originally had that.
                    if (wasReadOnly)
                    {
                        zAssemblyInfo.IsReadOnly = true;
                    }
                    // Remove that temp-file that we craeted.
                    FilesystemLib.DeleteFile(pathnameOfCopy);
                }
                else
                {
                    reason = "The AssemblyInfo.cs file does not exist.";
                }
            }
            else
            {
                throw new FileNotFoundException(message: "Unable to find project file " + Pathname);
            }
            return wasFound;
        }
        #endregion SetAssemblyInfoProperty

        private string _assemblyName;
        private string _assemblyVersion;
        private string _assemblyFileVersion;
        private string _description;
        private string _guid;
        private bool _hasBeenChecked;
        private string _product;

        /// <summary>
        /// This is the list of other projects that this project has a direct reference to,
        /// NOT recursively. To get the complete list - recurse down into this list.
        /// </summary>
        private List<VsProject> _referencedProjects;

        private SortedSet<VsSourceFile> _sourceFiles;
        private List<NugetPackageReference> _nugetPackages;
        private string _title;
        private DateTime _whenLastWritten;

        #endregion internal implementation
    }
}
