using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Web;

using CarlosAg.ExcelXmlWriter;

namespace WebIt.Business.UtilityClasses
{
    /// <summary>
    /// 
    /// </summary>
    public class ExcelWriter
    {
        /// <summary>
        /// ExcelWriter constructor
        /// </summary>
        public ExcelWriter()
        {
        }

        /// <summary>
        /// Export the given DataSet to an Excel XML object and send to the HTTP output
        /// </summary>
        /// <param name="ds"></param>
        public void ExportDataSetToXMLExcel(DataSet ds)
        {
            Workbook book = new Workbook();
            foreach (DataTable dt in ds.Tables)
            {
                Worksheet sheet = book.Worksheets.Add(dt.TableName);

                // Add Header Row
                WorksheetRow headerRow = sheet.Table.Rows.Add();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    headerRow.Cells.Add(new WorksheetCell(dt.Columns[i].ColumnName));
                }
                // Add DataTable Rows
                foreach (DataRow dr in dt.Rows)
                {
                    WorksheetRow row = sheet.Table.Rows.Add();
                    object[] a = dr.ItemArray;
                    for (int i = 0; i < a.Length; i++)
                    {
                        row.Cells.Add(new WorksheetCell(a[i].ToString(), GetDataType(a[i].GetType().Name)));
                    }
                }
            }
            book.Save(HttpContext.Current.Response.OutputStream);
        }

        /// <summary>
        /// Export the given DataSet to an Excel XML object and send to the HTTP output, with the given Headers
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="gridHeader"></param>
        /// <param name="headings"></param>
        public void ExportDataSetToXMLExcel(DataSet ds, List<string[]> gridHeader, List<string[]> pageHeader)
        {
            if (gridHeader.Count != ds.Tables.Count)
                throw new Exception("Invalid Number of Tables to Headers");
            else
            {
                Workbook book = new Workbook();
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    DataTable dt = ds.Tables[i];
                    Worksheet sheet = book.Worksheets.Add(dt.TableName);
                    WorksheetRow currentRow;

                    // Add Headings Rows
                    for (int a = 0; a < pageHeader[i].Length; a++)
                    {
                        currentRow = sheet.Table.Rows.Add();
                        currentRow.Cells.Add((pageHeader[i])[a]);
                    }

                    // Add Empty row for spacing
                    currentRow = sheet.Table.Rows.Add();
                    currentRow.Cells.Add(new WorksheetCell(string.Empty));

                    // Add Grid Header Row
                    currentRow = sheet.Table.Rows.Add();
                    for (int j = 0; j < gridHeader[i].Length; j++)
                    {
                        currentRow.Cells.Add(new WorksheetCell((gridHeader[i])[j]));
                    }
                    // Add DataTable Rows
                    foreach (DataRow dr in dt.Rows)
                    {
                        currentRow = sheet.Table.Rows.Add();
                        object[] a = dr.ItemArray;
                        for (int k = 0; k < a.Length; k++)
                        {
                            currentRow.Cells.Add(new WorksheetCell(a[k].ToString(), GetDataType(a[k].GetType().Name)));
                        }
                    }

                    currentRow = sheet.Table.Rows.Add();
                    currentRow.Cells.Add(new WorksheetCell(string.Empty));

                    // Add Confidentiality Statement
                    currentRow = sheet.Table.Rows.Add();
                    currentRow.Cells.Add(new WorksheetCell("This Information is Confidential"));
                }
                book.Save(HttpContext.Current.Response.OutputStream);
            }
        }
        public static DataType GetDataType(string type)
        {
            DataType dt = DataType.String;
            switch (type)
            {
                case "Int32":
                    {
                        dt = DataType.Number;
                        break;
                    }
                case "Int64":
                    {
                        dt = DataType.Number;
                        break;
                    }
                case "Decimal":
                    {
                        dt = DataType.Number;
                        break;
                    }
                case "Double":
                    {
                        dt = DataType.Number;
                        break;
                    }
                case "String":
                    {
                        dt = DataType.String;
                        break;
                    }
            }
            return dt;
        }
    }
}
