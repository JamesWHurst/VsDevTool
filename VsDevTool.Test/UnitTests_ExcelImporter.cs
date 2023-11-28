using System;
using NUnit.Framework;
using VsDevTool.DomainModels;


namespace VsDevTool.Test
{
    [TestFixture]
    public class UnitTests_ExcelImporter
    {
        public static string TestFile = @"C:\Users\jhurst\DropBox\GT\Chinese\TestSpreadsheetForLanguages.xlsx";

        [Test]
        public void ExcelImporter_ImportFromSampleFile_CorrectCount()
        {
            var r = ExcelImporter.ImportStringsFromExcelSpreadsheet( TestFile, 3 );
            int n = r.Count;
            Console.WriteLine( "n = " + n );
            if (n > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    var item = r[i];
                    Console.WriteLine( "i = " + i + ": item is " + item );
                }
            }
            Assert.AreEqual( expected: 6, actual: r.Count );
        }
    }
}
