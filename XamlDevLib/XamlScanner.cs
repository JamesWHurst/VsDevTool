using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Hurst.LogNut.Util;


namespace Hurst.XamlDevLib
{
    public static class XamlScanner
    {
        public static SortedSet<string> GetAllKeys( string rootDirectoryPath, string patterns )
        {
            SortedSet<string> result = new SortedSet<string>();
            var files = XamlScanner.GetAllXamlFiles( rootDirectoryPath );
            Console.WriteLine( "I see {0} XAML files.", files.Length );
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    int lineCount = 0;
                    Console.WriteLine( "Looking into file {0}", file.FullName );
                    using (StreamReader r = new StreamReader( file.FullName ))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            Regex g = new Regex( @"(lex:Loc) (\w+)", RegexOptions.ECMAScript );
                            var m = g.Match( line );
                            if (m.Success)
                            {
                                Console.WriteLine( "    line: " + line );
                                lineCount++;
                                //Console.WriteLine("Match on line: {0}", line);
                                //Console.WriteLine("  Match.Value is {0}", m.Value);
                                //Console.WriteLine("  Groups.Count = {0}", m.Groups.Count);
                                //for (int i = 0; i < 3; i++)
                                //{
                                //    Console.WriteLine(@"    group {0} = ""{1}""", i, m.Groups[i].Value);
                                //}
                                string key = m.Groups[2].Value;
                                result.Add( key );

                                var nextMatch = m.NextMatch();
                                while (nextMatch.Success)
                                {
                                    string nextKey = nextMatch.Groups[2].Value;
                                    result.Add( nextKey );
                                    nextMatch = nextMatch.NextMatch();
                                }
                            }
                        }
                        Console.WriteLine( "Matched {0} lines.", lineCount );
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, string> GetGlobalStrings( string pathnameOfXamlFile )
        {
            var result = new Dictionary<string, string>();
            int lineCount = 0;
            Console.WriteLine( "Looking into file {0}", pathnameOfXamlFile );
            using (StreamReader r = new StreamReader( pathnameOfXamlFile ))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    Regex g = new Regex( @"(lex:Loc) (\w+)", RegexOptions.ECMAScript );
                    var m = g.Match( line );
                    if (m.Success)
                    {
                        Console.WriteLine( "    line: " + line );
                        lineCount++;
                        //Console.WriteLine("Match on line: {0}", line);
                        //Console.WriteLine("  Match.Value is {0}", m.Value);
                        //Console.WriteLine("  Groups.Count = {0}", m.Groups.Count);
                        //for (int i = 0; i < 3; i++)
                        //{
                        //    Console.WriteLine(@"    group {0} = ""{1}""", i, m.Groups[i].Value);
                        //}
                        string key = m.Groups[2].Value;
                        result.Add( key, "" );

                        var nextMatch = m.NextMatch();
                        while (nextMatch.Success)
                        {
                            string nextKey = nextMatch.Groups[2].Value;
                            result.Add( nextKey, "" );
                            nextMatch = nextMatch.NextMatch();
                        }
                    }
                }
                Console.WriteLine( "Matched {0} lines.", lineCount );
            }
            return result;
        }

        public static ZFileInfo[] GetAllXamlFiles( string rootDirectoryPath )
        {
            return FilesystemLib.GetFiles( directoryPath: rootDirectoryPath, fileSpec: "*.xaml", searchOption: SearchOption.AllDirectories );
        }

        public static void AddRange<T, S>( this Dictionary<T, S> source, Dictionary<T, S> collection )
        {
            if (collection == null)
            {
                throw new ArgumentNullException( nameof( collection ) );
            }
            foreach (var item in collection)
            {
                if (!source.ContainsKey( item.Key ))
                {
                    source.Add( item.Key, item.Value );
                }
            }
        }
    }
}
