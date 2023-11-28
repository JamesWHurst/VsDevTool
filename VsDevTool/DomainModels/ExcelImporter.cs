using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;


namespace VsDevTool.DomainModels
{
    public class ExcelImporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="spreadsheetPathname"></param>
        /// <param name="worksheetIndex">make this 1 for CytologyStrings, 2 for ReportResources, and 3 for Resources, </param>
        /// <returns></returns>
        public static IList<LanguageResource> ImportStringsFromExcelSpreadsheet( string spreadsheetPathname, int worksheetIndex )
        {
            var result = new List<LanguageResource>();
            // Excel.Application xlApp = new Excel.Application();
            //string pathname = this.SpreadsheetFilePathname;
            string pathname = spreadsheetPathname;
            if (!String.IsNullOrWhiteSpace( pathname ))
            {
                if (File.Exists( pathname ))
                {
                    // Using EPPlus
                    FileInfo file = new FileInfo( pathname );
                    using (ExcelPackage package = new ExcelPackage( file ))
                    {
                        // get the first worksheet in the workbook
                        int numberOfSheets = package.Workbook.Worksheets.Count;
                        //Console.WriteLine( "numberOfSheets is " + numberOfSheets );
                        //int iStart = 0;
                        //int iEnd = numberOfSheets - 1;
                        //if (package.Compatibility.IsWorksheets1Based)
                        //{
                        //    iStart = 1;
                        //    iEnd = numberOfSheets;
                        //}
                        //for (int i = iStart; i <= iEnd; i++)
                        //{
                        //    ExcelWorksheet worksheet = package.Workbook.Worksheets[i];
                        //    Console.WriteLine( "For " + i + ": worksheet.Name = " + worksheet.Name );
                        //}

                        ExcelWorksheet worksheetResources = package.Workbook.Worksheets[worksheetIndex];
                        //ExcelColumn firstColumn = worksheetResources.Column( 1 );

                        string englishVersion = "?";
                        string chineseVersion = "?";
                        string descriptionInEnglish = "?";
                        string descriptionInChinese;
                        bool lastWasEnglish = true;

                        // This does iterate down column A.
                        var dim = worksheetResources.Dimension;
                        int nRows = dim.Rows;
                        int nCols = dim.Columns;
                        int n = 2;
                        do
                        {
                            var x = worksheetResources.Cells[n, 1].Value;
                            string s = x as String;
                            //Console.Write( "for n = " + n + ", x = " + x );
                            if (x != null)
                            {
                                var r = new LanguageResource();
                                // If this is English
                                if (!IsChinese( s ))
                                {
                                    // Console.WriteLine( "   is English" );
                                    englishVersion = s;
                                    descriptionInEnglish = worksheetResources.Cells[n, 2].Value as String;
                                    if (!String.IsNullOrWhiteSpace( descriptionInEnglish ))
                                    {
                                        r.DescriptionInEnglish = descriptionInEnglish;
                                        if (descriptionInEnglish.Contains( "DO NOT TRANSLATE" ))
                                        {
                                            r.IsToBeTranslated = false;
                                        }
                                        else
                                        {
                                            r.IsToBeTranslated = true;
                                        }
                                    }
                                    lastWasEnglish = true;
                                    n++;
                                    var x2 = worksheetResources.Cells[n, 1].Value;
                                    descriptionInChinese = worksheetResources.Cells[n, 2].Value as String;
                                    if (x2 != null)
                                    {
                                        string s2 = x2 as String;
                                       // if (IsChinese( s2 ))
                                        {
                                            chineseVersion = s2;
                                            r.EnglishValue = englishVersion;
                                            r.OtherLanguageValue = chineseVersion;
                                            //r.IsToBeTranslated = true;
                                            r.DescriptionInOtherLanguage = descriptionInChinese;
                                            result.Add( r );
                                        }
                                        //else // not Chinese
                                        //{
                                        //    if (s2 == "<-")
                                        //    {
                                        //        chineseVersion = s2;
                                        //        r.EnglishValue = englishVersion;
                                        //        r.OtherLanguageValue = chineseVersion;
                                        //        r.IsToBeTranslated = false;
                                        //        r.DescriptionInEnglish = descriptionInEnglish;
                                        //        r.DescriptionInOtherLanguage = descriptionInChinese;
                                        //        result.Add( r );
                                        //    }
                                        //    else
                                        //    {
                                        //        throw new FormatException( message: "Not expecting English at line " + n );
                                        //    }
                                        //}
                                    }
                                    else // empty
                                    {
                                        r.EnglishValue = englishVersion;
                                        r.IsToBeTranslated = false;
                                        r.DescriptionInOtherLanguage = descriptionInChinese;
                                        result.Add( r );
                                    }
                                }
                                else // Chinese
                                {
                                    //Console.WriteLine( "    is Chinese" );
                                    throw new FormatException( message: "Not expecting Chinese at line " + n );
                                    //chineseVersion = s;
                                    //lastWasEnglish = false;
                                    //var r = new LanguageResource();
                                    //r.EnglishValue = englishVersion;
                                    //r.OtherLanguageValue = chineseVersion;
                                    //r.IsToBeTranslated = true;
                                    //result.Add(r);
                                    //Console.WriteLine( "for n " + n + ": English = " + englishVersion + ", Chinese = " + chineseVersion + ", description = " + descriptionInEnglish );
                                }
                            }
                            //else // blank
                            //{
                            //    if (lastWasEnglish)
                            //    {
                            //        chineseVersion = "None";
                            //        var r = new LanguageResource();
                            //        r.EnglishValue = englishVersion;
                            //        r.IsToBeTranslated = false;
                            //        result.Add( r );
                            //        //Console.WriteLine( "for n " + n + ": English = " + englishVersion + ", Chinese = " + chineseVersion + ", description = " + descriptionInEnglish );
                            //    }
                            //    else
                            //    {
                            //        //Console.WriteLine( "for n = " + n + " - blank line." );
                            //    }
                            //    lastWasEnglish = false;
                            //}

                            n++;

                        } while (n <= nRows + 1);

                        // Strategy:

                        // Get the next row value:
                        //   If this is English,
                        //     accept this as the English version (I do not see keys here?)
                        //     Get the enxt row value
                        //     If this is Chinese
                        //       accept this as a translation.
                        //     else if this is blank
                        //       add that English term to the list as having no translation (it appears in the UX as-is)
                        //   If this is blank
                        //     next


                        //int col = 2; //The item description
                        //// output the data in column 2
                        //for (int row = 2; row < 5; row++)
                        //    Console.WriteLine( "\tCell({0},{1}).Value={2}", row, col, worksheet.Cells[row, col].Value );

                        //// output the formula in row 5
                        //Console.WriteLine( "\tCell({0},{1}).Formula={2}", 3, 5, worksheet.Cells[3, 5].Formula );
                        //Console.WriteLine( "\tCell({0},{1}).FormulaR1C1={2}", 3, 5, worksheet.Cells[3, 5].FormulaR1C1 );

                        //// output the formula in row 5
                        //Console.WriteLine( "\tCell({0},{1}).Formula={2}", 5, 3, worksheet.Cells[5, 3].Formula );
                        //Console.WriteLine( "\tCell({0},{1}).FormulaR1C1={2}", 5, 3, worksheet.Cells[5, 3].FormulaR1C1 );
                    }



                }
                else // wrong file?
                {
                    throw new FileNotFoundException( message: "I don't see that file.", fileName: spreadsheetPathname );
                }
            }
            else // no file!
            {
                throw new ArgumentException( message: "Please specify what Excel spreadsheet file to import from", paramName: nameof( spreadsheetPathname ) );
            }
            return result;
        }

        public static bool IsChinese( string text )
        {
            if (text == null)
            {
                throw new ArgumentNullException( paramName: nameof( text ) );
            }
            bool isChinese = false;
            for (int i = 0; i < text.Length; i++)
            {
                char thisChar = text[i];
                UnicodeCategory category = Char.GetUnicodeCategory( thisChar );
                if (category == UnicodeCategory.OtherLetter)
                {
                    isChinese = true;
                    break;
                }
            }
            return isChinese;
        }
    }
}
