using System;
using System.IO;
using Hurst.LogNut.Util;
using Hurst.XamlDevLib;
using NUnit.Framework;
using VsDevTool.DomainModels;


namespace VsDevTool.Test
{
    [TestFixture]
    public class SubsystemTests_Glob
    {
        public string MainTestFolder
        {
            get { return @"C:\Tests\LuVivaMain"; }
        }

        public string TestFolder
        {
            get { return Path.Combine( MainTestFolder, "GetUserManualPathname" ); }
        }

        [Test]
        public void CreateTestProjectObject_CheckAssemblyName_CorrectResults()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            project.GetInformationFromTheProjectFile();
            Assert.AreEqual( expected: "VsDevTool.TestProject", actual: project.AssemblyName );
        }

        [Test]
        public void CreateTestProjectObject_GetGlobalStringsOfFirstResourceFile_CorrectCount()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            project.GetInformationFromTheProjectFile();
            var stringResources = project.GetStringsFromResourceFile( @"Resources\Strings\Resources1.resx" );
            Assert.AreEqual( expected: 2, actual: stringResources.Count );
        }

        [Test]
        public void CreateVsSourceFile_GetGlobalStrings_CorrectCount()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            project.GetInformationFromTheProjectFile();
            var stringResources = project.GetStringsFromResourceFile( @"Resources\Strings\Resources1.resx" );
            Assert.AreEqual( expected: 2, actual: stringResources.Count );
        }

        [Test]
        public void CreateVsProjectAndSourceFile_GetGlobalStrings_CorrectCount()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            var resourceFile = new VsResourceFile( visualStudioProject: project, pathname: @"Resources\Strings\Resources1.resx" );
            var stringResources = resourceFile.GetStrings();
            Assert.AreEqual( expected: 2, actual: stringResources.Count );
        }

        [Test]
        public void CreateVsProjectObject_XamlScannerGetAllXamlFiles_CorrectNumberOfThem()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            string rootDir = FileStringLib.GetDirectoryOfPath( project.Pathname );
            var xamlFiles = XamlScanner.GetAllXamlFiles( rootDirectoryPath: rootDir );
            //foreach (var file in xamlFiles)
            //{
            //    Console.WriteLine( "XAML file: " + file.FullName );
            //}
            Assert.AreEqual( expected: 3, actual: xamlFiles.Length );
        }

        [Test]
        public void CreateVsProjectObject_GetAllXamlFiles_CorrectNumberOfThem()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            var xamlFiles = project.GetAllXamlFiles();
            //foreach (var file in xamlFiles)
            //{
            //    Console.WriteLine( "XAML file: " + file.FullName );
            //}
            Assert.AreEqual( expected: 3, actual: xamlFiles.Length );
        }

        [Test]
        public void CreateVsProjectObject_GetAllXamlFilesThatContainLex_CorrectNumberOfThem()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            var xamlFiles = project.GetAllXamlFiles( searchPattern: "lex:" );
            foreach (var file in xamlFiles)
            {
                Console.WriteLine( "XAML file: " + file.FullName );
            }
            Assert.AreEqual( expected: 2, actual: xamlFiles.Length );
        }

        [Test]
        public void CreateVsProjectObject_DeleteLineFromAllXamlFilesThatContainDesignCulture_CorrectResults()
        {
            var project = new VsProject( @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject" );
            var xamlFiles = project.DeleteLineFromAllXamlFilesThatContainDesignCulture( isToKeepOriginal: true );
            Console.WriteLine( "This deleted lines from " + xamlFiles.Length + " XAML files." );
            foreach (var file in xamlFiles)
            {
                Console.WriteLine( "XAML file: " + file.FullName );
            }
            Assert.AreEqual( expected: 2, actual: xamlFiles.Length );
        }

        // [Test]
        public void CheckTheXaml()
        {
            //Regex g = new Regex(@"Lex", RegexOptions.ECMAScript);
            //string text = "BlueLexus";
            //Match n = g.Match(text);
            //Console.WriteLine("n.Success is {0}, ", n.Success);
            //Console.WriteLine("n.Groups is {0}, ", n.Groups);
            const string rootPathToTargetProject = @"C:\CNDS-0 (2015)\Dev\Development\Luviva\Luviva\Views";

            var setOfKeys = XamlScanner.GetAllKeys( rootPathToTargetProject, "lex:Loc" );
            Console.WriteLine( "{0} distinct keys found.", setOfKeys.Count );
            foreach (var key in setOfKeys)
            {
                Console.WriteLine( "  Key: {0}", key );
            }

            //var files = XamlScanner.GetPathsOfAllXamlFiles();
            //Console.WriteLine("I see {0} XAML files.", files.Count);
            //if (files.Count > 0)
            //{
            //    foreach (var pathname in files)
            //    {
            //        if (pathname.Contains("StateSetupView.xaml"))
            //        {
            //            int lineCount = 0;
            //            Console.WriteLine("Starting with {0}", pathname);
            //            using (StreamReader r = new StreamReader(pathname))
            //            {
            //                string line;
            //                while ((line = r.ReadLine()) != null)
            //                {
            //                    Regex g = new Regex(@"(lex:Loc) (\w+)", RegexOptions.ECMAScript);
            //                    Match m = g.Match(line);
            //                    if (m.Success)
            //                    {
            //                        lineCount++;
            //                        Console.WriteLine("Match on line: {0}", line);
            //                        Console.WriteLine("  Match.Value is {0}", m.Value);
            //                        Console.WriteLine("  Groups.Count = {0}", m.Groups.Count);
            //                        int groupIndex = 1;
            //                        foreach (var group in m.Groups)
            //                        {
            //                            Console.WriteLine(@"    group {0} = ""{1}""", groupIndex, group.ToString());
            //                            groupIndex++;
            //                        }
            //                    }
            //                }
            //            }
            //            Console.WriteLine("Matched {0} lines.", lineCount);
            //        }
            //    }
            //}
        }

        //[Test]
        //public void ResourceSets_RetrieveCompleteSets_AreConsistentWithEachOther()
        //{
        //    //string cultureCodeEnglish = "en-US";
        //    string cultureCodeReference = "en-TT";
        //    LuVivaResourceManager.The.Initialize( cultureCodeReference );

        //    var resourcesReference = LuVivaResourceManager.The.GetAllStrings( cultureCodeReference );
        //    int countOfReferenceResources = resourcesReference.Count;

        //    Console.WriteLine( @"The reference (""en-TT"") ResourceSet has {0} things in it.", countOfReferenceResources );
        //    Assert.IsTrue( countOfReferenceResources > 0 );


        //    Dictionary<string, int> sizes = new Dictionary<string, int>();
        //    sizes.Add( "en-TT", countOfReferenceResources );

        //    foreach (var cultureCode in LuVivaResourceManager.AllCultureCodes)
        //    {
        //        if (cultureCode != cultureCodeReference)
        //        {
        //            LuVivaResourceManager.The.Initialize( cultureCode );
        //            var resources = LuVivaResourceManager.The.GetAllStrings( cultureCode );

        //            // Compare the counts.
        //            if (resources.Count != countOfReferenceResources)
        //            {
        //                Console.WriteLine( @"Culture ""{0}"" does not have the same number of strings ( {1} ) as en-TT.", cultureCode, resources.Count );
        //            }
        //            sizes.Add( cultureCode, resources.Count );

        //            // See whether this culture has everything that the Reference culture does..
        //            foreach (var entry in resourcesReference)
        //            {
        //                if (!resources.ContainsKey( entry.Key ))
        //                {
        //                    Console.WriteLine( @"Culture ""{0}"" does not have key ""{1}"" which ""en-TT"" does.", cultureCode, entry.Key );
        //                }
        //            }
        //            // See whether the Reference has everything that this culture does..
        //            foreach (var entry in resources)
        //            {
        //                if (!resourcesReference.ContainsKey( entry.Key ))
        //                {
        //                    Console.WriteLine( @"Culture ""{0}"" has key ""{1}"" which is not available in ""en-TT"".", cultureCode, entry.Key );
        //                }
        //            }
        //        }
        //    }

        //    // Show a list of counts...
        //    Console.WriteLine( "" );
        //    Console.WriteLine( "Culture Item Counts: " );
        //    foreach (var item in sizes)
        //    {
        //        Console.WriteLine( "{0} {1}", item.Key, item.Value );
        //    }
        //}

        //[Test]
        //public void CytologyResourceSets_RetrieveCompleteSets_AreConsistentWithEachOther()
        //{
        //    string cultureCodeReference = "en-TT";
        //    LuVivaResourceManager.The.Initialize( cultureCodeReference );

        //    var resourcesReference = LuVivaResourceManager.The.GetAllCytologyStrings( cultureCodeReference );
        //    int countOfReferenceResources = resourcesReference.Count;

        //    Console.WriteLine( "" );
        //    Console.WriteLine( "For Cytology:" );
        //    Console.WriteLine( @"The reference (""en-TT"") ResourceSet has {0} things in it.", countOfReferenceResources );
        //    Assert.IsTrue( countOfReferenceResources > 0 );


        //    Dictionary<string, int> sizes = new Dictionary<string, int>();
        //    sizes.Add( "en-TT", countOfReferenceResources );

        //    foreach (var cultureCode in LuVivaResourceManager.AllCultureCodes)
        //    {
        //        if (cultureCode != cultureCodeReference)
        //        {
        //            LuVivaResourceManager.The.Initialize( cultureCode );
        //            var resources = LuVivaResourceManager.The.GetAllCytologyStrings( cultureCode );

        //            // Compare the counts.
        //            if (resources.Count != countOfReferenceResources)
        //            {
        //                Console.WriteLine( @"Culture ""{0}"" does not have the same number of strings ( {1} ) as en-TT.", cultureCode, resources.Count );
        //            }
        //            sizes.Add( cultureCode, resources.Count );

        //            // See whether this culture has everything that the Reference culture does..
        //            foreach (var entry in resourcesReference)
        //            {
        //                if (!resources.ContainsKey( entry.Key ))
        //                {
        //                    Console.WriteLine( @"Culture ""{0}"" does not have key ""{1}"" which ""en-TT"" does.", cultureCode, entry.Key );
        //                }
        //            }
        //            // See whether the Reference has everything that this culture does..
        //            foreach (var entry in resources)
        //            {
        //                if (!resourcesReference.ContainsKey( entry.Key ))
        //                {
        //                    Console.WriteLine( @"Culture ""{0}"" has key ""{1}"" which is not available in ""en-TT"".", cultureCode, entry.Key );
        //                }
        //            }
        //        }
        //    }
        //    // Show a list of counts...
        //    Console.WriteLine( "" );
        //    Console.WriteLine( "Culture Item Counts: " );
        //    foreach (var item in sizes)
        //    {
        //        Console.WriteLine( "{0} {1}", item.Key, item.Value );
        //    }
        //}

        //[Test]
        //public void ReportResourceSets_RetrieveCompleteSets_AreConsistentWithEachOther()
        //{
        //    string cultureCodeReference = "en-TT";
        //    LuVivaResourceManager.The.Initialize( cultureCodeReference );

        //    var resourcesReference = LuVivaResourceManager.The.GetAllReportStrings( cultureCodeReference );
        //    int countOfReferenceResources = resourcesReference.Count;

        //    Console.WriteLine( "" );
        //    Console.WriteLine( "For Reports:" );
        //    Console.WriteLine( @"The reference (""en-TT"") ResourceSet has {0} things in it.", countOfReferenceResources );
        //    Assert.IsTrue( countOfReferenceResources > 0 );


        //    Dictionary<string, int> sizes = new Dictionary<string, int>();
        //    sizes.Add( "en-TT", countOfReferenceResources );

        //    foreach (var cultureCode in LuVivaResourceManager.AllCultureCodes)
        //    {
        //        if (cultureCode != cultureCodeReference)
        //        {
        //            LuVivaResourceManager.The.Initialize( cultureCode );
        //            var resources = LuVivaResourceManager.The.GetAllReportStrings( cultureCode );

        //            // Compare the counts.
        //            if (resources.Count != countOfReferenceResources)
        //            {
        //                Console.WriteLine( @"Culture ""{0}"" does not have the same number of strings ( {1} ) as en-TT.", cultureCode, resources.Count );
        //            }
        //            sizes.Add( cultureCode, resources.Count );

        //            // See whether this culture has everything that the Reference culture does..
        //            foreach (var entry in resourcesReference)
        //            {
        //                if (!resources.ContainsKey( entry.Key ))
        //                {
        //                    Console.WriteLine( @"Culture ""{0}"" does not have key ""{1}"" which ""en-TT"" does.", cultureCode, entry.Key );
        //                }
        //            }
        //            // See whether the Reference has everything that this culture does..
        //            foreach (var entry in resources)
        //            {
        //                if (!resourcesReference.ContainsKey( entry.Key ))
        //                {
        //                    Console.WriteLine( @"Culture ""{0}"" has key ""{1}"" which is not available in ""en-TT"".", cultureCode, entry.Key );
        //                }
        //            }
        //        }
        //    }
        //    // Show a list of counts...
        //    Console.WriteLine( "" );
        //    Console.WriteLine( "Culture Item Counts: " );
        //    foreach (var item in sizes)
        //    {
        //        Console.WriteLine( "{0} {1}", item.Key, item.Value );
        //    }
        //}

        //[Test]
        //public void AppSetCurrentCulture_SetToEnglishThenGerman_CorrectResults()
        //{
        //    string cultureCode = "en-US";
        //    App.SetCurrentCulture( cultureCode );
        //    string powerOffEnglish = LuVivaResourceManager.The.GetString( "PowerOff" );
        //    Assert.AreEqual( "Power", powerOffEnglish );
        //    App.SetCurrentCulture( "de-DE" );
        //    string powerOffGerman = LuVivaResourceManager.The.GetString( "PowerOff" );
        //    Assert.AreEqual( "Ausschalten", powerOffGerman );
        //}

    }
}
