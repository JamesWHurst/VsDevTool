The functions of this program encompass these basic areas:


1) Given a particular Visual Studio project, solution, or set of solutions -- generate a dependency-graph of the Visual Studio projects
that this one depends upon, and from that - create a textual report of every source-file, or of every software-artiface, used.

2) Given the above information - copy the source-code, or the software artifacts, to a given target location.

3) Clean all intermediate and output files and folders, for every project within the currently-targeted
Visual Studio solution.

4) Set the program-version of the currently-targeted program.
   A) Executables get a special 'program-version', consisting of a (configurable) name.year.month.day. etc.
   B) Any file of a library project that is changed, added, or deleted - results in that library having it's file-version incremented.
   C) Any project that sees any project upon which it depends have it's version incremented, is also incremented.
   D) Any solution that sees any of it's projects incremented, gets incremented.

5) Analyze the application for strings that may need to be globalized, and produce reports and tables of information to facilitate this effort.

6) For a given application, apply settings to the projects across all contained projects.
   Examples would include setting which .NET framework to target, or a C#-pragma to define,
   or the copyright information.

7) Run the unit-tests on a category (that's the only lacking functionality that I can see).

8) Produce a formatted report of the unit-tests that were run.

9) Save the file-tree, and then create a new one and compare it against the previous one - highlighting
   files that have changed last-written-times, files that were added, or moved, or deleted.


A particular application consists of:

1. An application name.
2. Set of Visual Studio solutions that are part of that 'application'
3. For each VS solution, a set of VS projects and assemblies (ie, .DLLs that are simply copied as-is, w/o projects)
4. For each VS project, a list of source-code files (their locations), and assembly/file versions
5. For each referenced assembly or other file, a location and version


To-Do:

* Include within the list of files that make up a project - files referenced which are outside of the project's folder,

* and file-links

* assemblies which are referenced, as opposed to included as projects
