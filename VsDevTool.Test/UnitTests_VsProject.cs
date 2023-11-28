using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VsDevTool.DomainModels;


namespace VsDevTool.Test
{
    [TestFixture]
    public class UnitTests_VsProject
    {
        public static string TestProject = @"C:\dev\GT\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject.csproj";

        [Test]
        public void VsSourceFile_2DistinctItemsAddOneToList_ListDoesNotContainsTheOther()
        {
            VsSourceFile file1 = new VsSourceFile( "Path1.txt", isToIgnoreFilesystem: true );
            VsSourceFile file2 = new VsSourceFile( "Path2.txt", isToIgnoreFilesystem: true );
            List<VsSourceFile> list = new List<VsSourceFile>();
            list.Add( file1 );
            Assert.IsFalse( list.Contains( file2 ) );
        }

        [Test]
        public void VsSourceFile_2IdenticalItemsAddOneToList_ListContainsTheOther()
        {
            VsSourceFile file1 = new VsSourceFile( "Path1.txt", isToIgnoreFilesystem: true );
            VsSourceFile file2 = new VsSourceFile( "Path1.txt", isToIgnoreFilesystem: true );
            List<VsSourceFile> list = new List<VsSourceFile>();
            list.Add( file1 );
            Assert.IsTrue( list.Contains( file2 ) );
        }

        [Test]
        public void VsSourceFile_Add2DistinctItemsToSortedSet_TheyAreInCorrectOrder()
        {
            VsSourceFile file1 = new VsSourceFile( "PathA.txt", isToIgnoreFilesystem: true );
            VsSourceFile file2 = new VsSourceFile( "PathB.txt", isToIgnoreFilesystem: true );
            SortedSet<VsSourceFile> set = new SortedSet<VsSourceFile>();
            set.Add( file1 );
            set.Add( file2 );
            Assert.AreEqual( 2, set.Count() );

            VsSourceFile foundFile1 = null, foundFile2 = null;
            int i = 0;
            foreach (var file in set)
            {
                if (i == 0)
                {
                    foundFile1 = file;
                }
                else
                {
                    foundFile2 = file;
                }
                i++;
            }
            Assert.IsTrue( foundFile1.Equals( file1 ) );
            Assert.IsTrue( foundFile2.Equals( file2 ) );
        }

        [Test]
        public void VsSourceFile_Add2DistinctItemsToSortedSetInReverseOrder_TheyAreInCorrectOrder()
        {
            VsSourceFile file1 = new VsSourceFile( "PathA.txt", isToIgnoreFilesystem: true );
            VsSourceFile file2 = new VsSourceFile( "PathB.txt", isToIgnoreFilesystem: true );
            SortedSet<VsSourceFile> set = new SortedSet<VsSourceFile>();
            set.Add( file2 );
            set.Add( file1 );
            Assert.AreEqual( 2, set.Count() );

            VsSourceFile foundFile1 = null, foundFile2 = null;
            int i = 0;
            foreach (var file in set)
            {
                if (i == 0)
                {
                    foundFile1 = file;
                }
                else
                {
                    foundFile2 = file;
                }
                i++;
            }
            Assert.IsTrue( foundFile1.Equals( file1 ) );
            Assert.IsTrue( foundFile2.Equals( file2 ) );
        }

        [Test]
        public void VsSourceFile_Add2OfTheSameItemsToSortedSet_ResultContainsOnlyOneCopy()
        {
            VsSourceFile file1 = new VsSourceFile( "PathX.txt", isToIgnoreFilesystem: true );
            VsSourceFile file2 = new VsSourceFile( "PathX.txt", isToIgnoreFilesystem: true );
            SortedSet<VsSourceFile> set = new SortedSet<VsSourceFile>();
            set.Add( file1 );
            set.Add( file2 );
            Assert.AreEqual( 1, set.Count() );
        }


        [Test]
        public void VsSolution_BeMsSample_CorrectResults()
        {
            string solutionPathname = @"C:\dev\GT\Development\BeMs\BeMs.sln";
            VsSolution solution = new VsSolution( solutionPathname );
            //solution.SolutionPathname = solutionPathname;
            var projects = solution.Projects;

            Assert.IsTrue( projects.Count > 0 );
        }

        [Test]
        public void VsProject_Sample1_CorrectAssemblyName()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.AreEqual( "VsDevTool.TestProject", project.AssemblyName );
        }

        [Test]
        public void VsProject_Sample1_CorrectTitle()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.AreEqual( "The Title", project.Title );
        }

        [Test]
        public void VsProject_Sample1_CorrectDescription()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.AreEqual( "A Description", project.Description );
        }

        [Test]
        public void VsProject_Sample1_CorrectProduct()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.AreEqual( "Product VsDevTool", project.Product );
        }

        [Test]
        public void VsProject_Sample1_CorrectFileVersion()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.AreEqual( "11.12.13.14", project.FileVersion );
        }

        [Test]
        public void VsProject_Sample1_CorrectAssemblyVersion()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.AreEqual( "1.2.3.4", project.AssemblyVersion );
        }

        [Test]
        public void VsProject_Sample1_HasSourceFiles()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.IsTrue( project.SourceFiles.Count > 0 );
        }

        // The test-project should have these source files:
        // 
        // App.config
        // App.xaml
        // App.xaml.cs
        // Check.png
        // Class1.cs
        // MainWindow.xaml
        // MainWindow.xaml.cs
        // USKeyboard.xml
        // VsDevTool.TestProject.csproj
        // Folder1\Class2.cs
        // Fonts\VAGRoundedStd-Bold.rtf
        // Properties\AssemblyInfo.cs
        // Properties\Resources.Designer.cs
        // Properties\Resources.resx
        // Properties\Settings.Designer.cs
        // Properties\Settings.settings
        // 
        // Total: 16
        // 

        [Test]
        public void VsProject_Sample1_CorrectNumberOfSourceFiles()
        {
            string projectPathname = TestProject;
            VsProject project = new VsProject( projectPathname );

            Assert.AreEqual( 16, project.SourceFiles.Count );
        }

    }
}
