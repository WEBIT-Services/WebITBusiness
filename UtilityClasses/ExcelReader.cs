using System;
//using Microsoft.Office.Interop.Excel;

namespace WebIt.Business.UtilityClasses
{
    public class ExcelReader
    {
        public ExcelReader()
        {
        }
        
        ///
        /// This class uses Excel COM classes
        /// Those assemblies are a pain to distribute to web servers
        /// so don't use it unless we really need to
        /// 
        //public void ReadExcel(string excelFile)
        //{
        //    string Path = excelFile;

        //    // initialize the Excel Application class
        //    ApplicationClass app = new ApplicationClass();
        //    // create the workbook object by opening the excel file.
        //    Workbook workBook = app.Workbooks.Open(Path,
        //                                                 0,
        //                                                 true,
        //                                                 5,
        //                                                 "",
        //                                                 "",
        //                                                 true,
        //                                                 XlPlatform.xlWindows,
        //                                                 "\t",
        //                                                 false,
        //                                                 false,
        //                                                 0,
        //                                                 true,
        //                                                 1,
        //                                                 0);
        //    // get the active worksheet using sheet name or active sheet
        //    Worksheet workSheet = (Worksheet)workBook.ActiveSheet;
        //    int index = 0;
        //    // This row,column index should be changed as per your need.
        //    // i.e. which cell in the excel you are interesting to read.
        //    object rowIndex = 2;
        //    object colIndex1 = 1;
        //    object colIndex2 = 2;
        //    try
        //    {
        //        while (((Range)workSheet.Cells[rowIndex, colIndex1]).Value2 != null)
        //        {
        //            rowIndex = 2 + index;
        //            string firstName = ((Range)workSheet.Cells[rowIndex, colIndex1]).Value2.ToString();
        //            string lastName = ((Range)workSheet.Cells[rowIndex, colIndex2]).Value2.ToString();
        //            Console.WriteLine("Name : {0},{1} ", firstName, lastName);
        //            index++;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        app.Quit();
        //        Console.WriteLine(ex.Message);
        //    }
        //}
    }
}