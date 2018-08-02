using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace WebIt.Business.UtilityClasses
{
    public class wiExcel
    {

        private static string RegAcctNm = "JDA eHealth Systems, Inc";
        private static string RegAcctKey = "J+73bJstLSkrfovpegRzTjcY22w26unVCs18d32eILo=";

        /// <summary>
        /// Saves the given DataSet to the given Excel path and file name
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="saveAsPathAndFile"></param>
        /// <param name="openAfterSave"></param>
        public static void ExportDataSetToExcel(DataSet ds, string saveAsPathAndFile, bool openAfterSave)
        {
            if (String.IsNullOrEmpty(saveAsPathAndFile))
                throw new Exception("Missing saveAsPathAndFile for ExportDataSetToExcel()!");

            Spire.DataExport.XLS.CellExport cellExport = new Spire.DataExport.XLS.CellExport();
            cellExport.Register(RegAcctNm, RegAcctKey);

            foreach (DataTable table in ds.Tables)
            {
                Spire.DataExport.XLS.WorkSheet worksheet1 = new Spire.DataExport.XLS.WorkSheet();

                worksheet1.DataSource = Spire.DataExport.Common.ExportSource.DataTable;
                worksheet1.DataTable = table;
                worksheet1.StartDataCol = ((System.Byte)(0));
                worksheet1.AutoFitColWidth = true;
                cellExport.Sheets.Add(worksheet1);
            }

            if (openAfterSave)
                cellExport.ActionAfterExport = Spire.DataExport.Common.ActionType.OpenView;

            cellExport.SaveToFile(saveAsPathAndFile);

            Console.WriteLine(saveAsPathAndFile + " was created.");
        }

        /// <summary>
        /// Saves the given DataTable to the given Excel path and file name
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="saveAsPathAndFile"></param>
        /// <param name="openAfterSave"></param>
        public static void ExportDataTableToExcel(DataTable dt, string saveAsPathAndFile, bool openAfterSave)
        {
            if (String.IsNullOrEmpty(saveAsPathAndFile))
                throw new Exception("Missing saveAsPathAndFile for ExportDataSetToExcel()!");

            Spire.DataExport.XLS.CellExport cellExport = new Spire.DataExport.XLS.CellExport();
            cellExport.Register(RegAcctNm, RegAcctKey);
            
            Spire.DataExport.XLS.WorkSheet worksheet1 = new Spire.DataExport.XLS.WorkSheet();

            worksheet1.DataSource = Spire.DataExport.Common.ExportSource.DataTable;
            worksheet1.DataTable = dt;
            worksheet1.StartDataCol = ((System.Byte)(0));
            worksheet1.AutoFitColWidth = true;
            cellExport.Sheets.Add(worksheet1);

            if (openAfterSave)
                cellExport.ActionAfterExport = Spire.DataExport.Common.ActionType.OpenView;

            cellExport.SaveToFile(saveAsPathAndFile);

            Console.WriteLine(saveAsPathAndFile + " was created.");
        }

    }
}
