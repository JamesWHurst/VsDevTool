This is a "WPF User Control Library" project.

I created this simply to have two projects, for testing the capacity of VsDevTool to handle multiple projects and not count duplicate files that they may share.

File Class1.cs is a LINK from the other TestProject.

I also added a reference to GalaSoft.MvvmLight from VendorLibs, since the other TestProject links that same file, in order to prove that it does not get counted twice.
