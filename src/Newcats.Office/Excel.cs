/***************************************************************************
 *GUID: de5fda94-94a4-40de-94b5-16844cde186d
 *CLR Version: 4.0.30319.42000
 *DateCreated：2020-05-04 11:33:45
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright © NewcatsHuang All rights reserved.
*****************************************************************************/
using System.ComponentModel;
using System.Data;
using Microsoft.AspNetCore.Http;
using Newcats.Utils.Extensions;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Newcats.Office
{
    /// <summary>
    /// Excel文件帮助类，通过NPOI进行操作
    /// </summary>
    public static class Excel
    {
        /// <summary>
        /// Excel格式
        /// </summary>
        public enum ExcelFormatEnum
        {
            /// <summary>
            /// office2003的旧版格式.xls
            /// </summary>
            [Description("xls")]
            xls = 2003,

            /// <summary>
            /// office2007的新版格式.xlsx
            /// </summary>
            [Description("xlsx")]
            xlsx = 2007
        }

        /// <summary>
        /// 重写Npoi流方法
        /// </summary>
        public class NpoiMemoryStream : MemoryStream
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public NpoiMemoryStream()
            {
                AllowClose = true;
            }

            /// <summary>
            /// 是否运行关闭
            /// </summary>
            public bool AllowClose { get; set; }

            /// <summary>
            /// 关闭流
            /// </summary>
            public override void Close()
            {
                if (AllowClose)
                    base.Close();
            }
        }

        /// <summary>
        /// 读取Excel到DataTable(NPOI方式)(默认获取第一个Sheet)(第一行为表头,不记录数据)
        /// </summary>
        /// <param name="fullFilePath">文件的完整物理路径</param>
        /// <returns>DataTable</returns>
        public static DataTable ReadExcelToTable(string fullFilePath)
        {
            IWorkbook? wk = null;
            using (FileStream file = new(fullFilePath, FileMode.Open, FileAccess.Read))
            {
                file.Position = 0;
                string fileExtension = Path.GetExtension(file.Name).ToLower();
                switch (fileExtension)
                {
                    case ".xls":
                        wk = new HSSFWorkbook(file);
                        break;
                    case ".xlsx":
                        wk = new XSSFWorkbook(file);
                        break;
                }
                return GetDataTableFromWorkbook(wk);
            }
        }

        /// <summary>
        /// 读取Excel到DataTable(NPOI方式)(默认获取第一个Sheet)(第一行为表头,不记录数据)
        /// </summary>
        /// <param name="file">含有Excel的文件流</param>
        /// <param name="format">文件扩展名（.xls,.xlsx）</param>
        /// <returns>DataTable</returns>
        public static DataTable ReadExcelToTable(Stream file, ExcelFormatEnum format)
        {
            file.Position = 0;
            IWorkbook? wk = null;
            switch (format)
            {
                case ExcelFormatEnum.xls:
                    wk = new HSSFWorkbook(file);
                    break;
                case ExcelFormatEnum.xlsx:
                    wk = new XSSFWorkbook(file);
                    break;
            }
            return GetDataTableFromWorkbook(wk);
        }

        /// <summary>
        /// 读取Excel到DataTable(NPOI方式)(默认获取第一个Sheet)(第一行为表头,不记录数据)
        /// </summary>
        /// <param name="file">含有Excel的文件</param>
        /// <returns>DataTable</returns>
        public static DataTable ReadExcelToTable(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new FileNotFoundException("The excel file do not exist!");
            IWorkbook? wk = null;
            using (MemoryStream stream = new())
            {
                file.CopyTo(stream);
                stream.Position = 0;
                switch (file.ContentType.ToLower())
                {
                    case "application/vnd.ms-excel":
                        wk = new HSSFWorkbook(stream);
                        break;
                    case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                        wk = new XSSFWorkbook(stream);
                        break;
                }
                return GetDataTableFromWorkbook(wk);
            }
        }

        /// <summary>
        /// 读取DataTable数据源到Excel内存流(NPOI方式)
        /// 1.写入到Excel文件，2.输出到响应
        /// 3.用完之后记得释放（using{}）
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="format">文件扩展名（.xls,.xlsx）</param>
        /// <returns>数据流</returns>
        public static MemoryStream ReadDataTableToExcel(DataTable source, ExcelFormatEnum format = ExcelFormatEnum.xls)
        {
            if (source == null || source.Rows.Count == 0)
                throw new ArgumentNullException(nameof(source));

            IWorkbook? workbook = null;
            switch (format)
            {
                case ExcelFormatEnum.xls:
                    workbook = new HSSFWorkbook();
                    break;
                case ExcelFormatEnum.xlsx:
                    workbook = new XSSFWorkbook();
                    break;
                default:
                    workbook = new HSSFWorkbook();
                    break;
            }

            ISheet sheet = workbook.CreateSheet("Sheet0");//创建一个名称为Sheet0的表  
            int rowCount = source.Rows.Count;//行数  
            int columnCount = source.Columns.Count;//列数  

            //设置列头  
            IRow row = sheet.CreateRow(0);//excel第一行设为列头  
            for (int c = 0; c < columnCount; c++)
            {
                ICell cell = row.CreateCell(c);
                cell.SetCellValue(source.Columns[c].ColumnName);
            }

            //设置每行每列的单元格,  
            for (int i = 0; i < rowCount; i++)
            {
                row = sheet.CreateRow(i + 1);
                for (int j = 0; j < columnCount; j++)
                {
                    ICell cell = row.CreateCell(j);//excel第二行开始写入数据
                    string defaultValue = source.Rows[i][j].ToString();//默认为字符串
                    switch (source.Columns[j].DataType.ToString())
                    {
                        case "System.String"://字符串类型
                            cell.SetCellValue(defaultValue);
                            break;
                        case "System.DateTime"://日期类型
                            //System.DateTime dateV;
                            //System.DateTime.TryParse(defaultValue, out dateV);
                            cell.SetCellValue(defaultValue);
                            break;
                        case "System.Boolean"://布尔型
                            bool boolV = false;
                            bool.TryParse(defaultValue, out boolV);
                            cell.SetCellValue(boolV);
                            break;
                        case "System.Int16"://整型
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Byte":
                            int intV = 0;
                            int.TryParse(defaultValue, out intV);
                            cell.SetCellValue(intV);
                            break;
                        case "System.Decimal"://浮点型
                        case "System.Double":
                            double doubV = 0;
                            double.TryParse(defaultValue, out doubV);
                            cell.SetCellValue(doubV);
                            break;
                        case "System.DBNull"://空值处理
                            cell.SetCellValue(string.Empty);
                            break;
                        default:
                            cell.SetCellValue(defaultValue);
                            break;
                    }
                }
            }

            var ms = new NpoiMemoryStream();
            ms.AllowClose = false;
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            ms.AllowClose = false;
            return ms;
        }

        /// <summary>
        /// 读取Excel到List(NPOI方式)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="fullFilePath">文件的完整物理路径</param>
        /// <returns>List</returns>
        public static List<T> ReadExcelToList<T>(string fullFilePath) where T : class, new()
        {
            return ReadExcelToTable(fullFilePath).ToList<T>();
        }

        /// <summary>
        /// 读取Excel到List(NPOI方式)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="file">含有Excel的文件流</param>
        /// <param name="format">文件扩展名（.xls,.xlsx）</param>
        /// <returns>List</returns>
        public static List<T> ReadExcelToList<T>(Stream file, ExcelFormatEnum format) where T : class, new()
        {
            return ReadExcelToTable(file, format).ToList<T>();
        }

        /// <summary>
        /// 读取Excel到List(NPOI方式)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="file">含有Excel的文件</param>
        /// <returns>DataTable</returns>
        public static List<T> ReadExcelToList<T>(IFormFile file) where T : class, new()
        {
            return ReadExcelToTable(file).ToList<T>();
        }

        /// <summary>
        /// 读取List数据源到Excel内存流(NPOI方式)
        /// 1.写入到Excel文件，2.输出到响应
        /// 3.用完之后记得释放 using{}
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="format">文件扩展名（.xls,.xlsx）</param>
        /// <returns>数据流</returns>
        public static MemoryStream ReadListToExcel<T>(List<T> source, ExcelFormatEnum format = ExcelFormatEnum.xls) where T : class, new()
        {
            return ReadDataTableToExcel(source.ToDataTable(), format);
        }

        /// <summary>
        /// 从Excel的一个工作簿获取DataTable(默认获取第一个Sheet)
        /// </summary>
        /// <param name="workbook">工作簿</param>
        /// <param name="sheetIndex">Sheet索引</param>
        /// <returns>DataTable</returns>
        private static DataTable GetDataTableFromWorkbook(IWorkbook workbook, int sheetIndex = 0)
        {
            if (workbook == null)
                throw new FileNotFoundException("The excel file do not exist!");

            return GetDataTableFromSheet(workbook.GetSheetAt(sheetIndex));
        }

        /// <summary>
        /// 从Excel的一个Sheet获取DataTable(第一行为表头,不记录数据)
        /// </summary>
        /// <param name="sheet">Excel的Sheet</param>
        /// <returns>DataTable</returns>
        private static DataTable GetDataTableFromSheet(ISheet sheet)
        {
            if (sheet == null)
                throw new FileNotFoundException("The excel file do not exist!");

            DataTable dt = new();

            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;

            for (int j = 0; j < cellCount; j++)
            {
                ICell cell = headerRow.GetCell(j);
                dt.Columns.Add(cell?.ToString());
            }

            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null || row.Cells.Count == 0)//跳过整行为空的情况
                    continue;
                DataRow dataRow = dt.NewRow();

                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    var cell = row.GetCell(j);
                    if (cell == null)
                        continue;

                    #region 旧方法
                    //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
                    //switch (cell.CellType)
                    //{
                    //    case CellType.Blank:
                    //        dataRow[j] = string.Empty;
                    //        break;
                    //    case CellType.Numeric:
                    //        short cellFormat = cell.CellStyle.DataFormat;
                    //        //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                    //        if (cellFormat == 14 || cellFormat == 31 || cellFormat == 57 || cellFormat == 58)
                    //            dataRow[j] = cell.DateCellValue;
                    //        else
                    //            dataRow[j] = cell.NumericCellValue;
                    //        break;
                    //    case CellType.String:
                    //        dataRow[j] = cell.StringCellValue;
                    //        break;
                    //    default:
                    //        dataRow[j] = cell.ToString();
                    //        break;
                    //} 
                    #endregion

                    switch (cell.CellType)
                    {
                        case CellType.Unknown:
                            dataRow[j] = cell.ToString();
                            break;
                        case CellType.Numeric:
                            dataRow[j] = DateUtil.IsCellDateFormatted(cell) ? cell.DateCellValue.ToString() : cell.NumericCellValue.ToString();
                            break;
                        case CellType.String:
                            dataRow[j] = cell.StringCellValue;
                            break;
                        case CellType.Formula:
                            try
                            {
                                HSSFFormulaEvaluator e = new(cell.Sheet.Workbook);
                                e.EvaluateInCell(cell);
                                dataRow[j] = cell.ToString();
                            }
                            catch
                            {
                                dataRow[j] = cell.NumericCellValue.ToString();
                            }
                            break;
                        case CellType.Blank:
                            dataRow[j] = string.Empty;
                            break;
                        case CellType.Boolean:
                            dataRow[j] = cell.BooleanCellValue.ToString();
                            break;
                        case CellType.Error:
                            dataRow[j] = cell.ErrorCellValue.ToString();
                            break;
                        default:
                            dataRow[j] = cell.ToString();
                            break;
                    }
                }
                dt.Rows.Add(dataRow);
            }
            return dt;
        }
    }
}