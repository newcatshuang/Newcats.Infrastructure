/***************************************************************************
 *GUID: de5fda94-94a4-40de-94b5-16844cde186d
 *CLR Version: 4.0.30319.42000
 *DateCreated：2020-05-04 11:33:45
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright © NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Data;
using MiniExcelLibs;

namespace Newcats.Office
{
    /// <summary>
    /// Excel文件帮助类
    /// </summary>
    public static class Excel
    {
        /// <summary>
        /// 读取Excel到DataTable(默认获取第一个Sheet)(第一行为表头,不记录数据)
        /// </summary>
        /// <param name="fullFilePath">文件的完整物理路径</param>
        /// <returns>DataTable</returns>
        public static DataTable ReadExcelToTable(string fullFilePath)
        {
            return MiniExcel.QueryAsDataTable(fullFilePath);
        }

        /// <summary>
        /// 读取Excel到DataTable(默认获取第一个Sheet)(第一行为表头,不记录数据)
        /// </summary>
        /// <param name="file">含有Excel的文件流</param>
        /// <returns>DataTable</returns>
        public static DataTable ReadExcelToTable(Stream file)
        {
            return MiniExcel.QueryAsDataTable(file);
        }

        /// <summary>
        /// 读取DataTable数据源到Excel内存流
        /// 1.写入到Excel文件，2.输出到响应
        /// 3.用完之后记得释放 using{}
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns>数据流</returns>
        public static MemoryStream ReadDataTableToExcel(DataTable source)
        {
            var stream = new MemoryStream();
            stream.SaveAs(source);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;

        }

        /// <summary>
        /// 读取Excel到List
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="fullFilePath">文件的完整物理路径</param>
        /// <returns>List</returns>
        public static IEnumerable<T> ReadExcelToList<T>(string fullFilePath) where T : class, new()
        {
            var result = MiniExcel.Query<T>(fullFilePath);
            return result;
        }

        /// <summary>
        /// 读取Excel到List
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="file">含有Excel的文件流</param>
        /// <returns>List</returns>
        public static IEnumerable<T> ReadExcelToList<T>(Stream file) where T : class, new()
        {
            using (file)
            {
                var result = file.Query<T>();
                return result;
            }
        }

        /// <summary>
        /// 读取List数据源到Excel内存流
        /// 1.写入到Excel文件，2.输出到响应
        /// 3.用完之后记得释放 using{}
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns>数据流</returns>
        public static MemoryStream ReadListToExcel<T>(IEnumerable<T> source) where T : class, new()
        {
            var stream = new MemoryStream();
            stream.SaveAs(source);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// 读取List数据源到Excel文件
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="fullFilePath">保存数据的excel文件完整路径</param>
        public static void ReadListToExcel<T>(IEnumerable<T> source, string fullFilePath) where T : class, new()
        {
            MiniExcel.SaveAs(fullFilePath, source);
        }

        /// <summary>
        /// 读取Excel到List
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="path">文件的完整物理路径</param>
        /// <param name="sheetName">sheetName</param>
        /// <param name="excelType">excel文件类型</param>
        /// <param name="startCell">起始单元格</param>
        /// <param name="configuration">配置</param>
        /// <returns>List</returns>
        public static IEnumerable<T> Query<T>(string path, string sheetName = null, ExcelType excelType = ExcelType.UNKNOWN, string startCell = "A1", IConfiguration configuration = null) where T : class, new()
        {
            return MiniExcel.Query<T>(path, sheetName, excelType, startCell, configuration);
        }

        /// <summary>
        /// 读取Excel到List
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="stream">包含excel的流</param>
        /// <param name="sheetName">sheetName</param>
        /// <param name="excelType">excel文件类型</param>
        /// <param name="startCell">起始单元格</param>
        /// <param name="configuration">配置</param>
        /// <returns>List</returns>
        public static IEnumerable<T> Query<T>(this Stream stream, string sheetName = null, ExcelType excelType = ExcelType.UNKNOWN, string startCell = "A1", IConfiguration configuration = null) where T : class, new()
        {
            using (stream)
            {
                return stream.Query<T>(sheetName, excelType, startCell, configuration);
            }
        }

        /// <summary>
        /// 读取Excel到List
        /// </summary>
        /// <param name="path">文件的完整物理路径</param>
        /// <param name="useHeaderRow">启用第一行</param>
        /// <param name="sheetName">sheetName</param>
        /// <param name="excelType">excel文件类型</param>
        /// <param name="startCell">起始单元格</param>
        /// <param name="configuration">配置</param>
        /// <returns>List</returns>
        public static IEnumerable<dynamic> Query(string path, bool useHeaderRow = false, string sheetName = null, ExcelType excelType = ExcelType.UNKNOWN, string startCell = "A1", IConfiguration configuration = null)
        {
            return MiniExcel.Query(path, useHeaderRow, sheetName, excelType, startCell, configuration);
        }

        /// <summary>
        /// 读取Excel到List
        /// </summary>
        /// <param name="stream">包含excel的流</param>
        /// <param name="useHeaderRow">启用第一行</param>
        /// <param name="sheetName">sheetName</param>
        /// <param name="excelType">excel文件类型</param>
        /// <param name="startCell">起始单元格</param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static IEnumerable<dynamic> Query(this Stream stream, bool useHeaderRow = false, string sheetName = null, ExcelType excelType = ExcelType.UNKNOWN, string startCell = "A1", IConfiguration configuration = null)
        {
            using (stream)
            {
                return stream.Query(useHeaderRow, sheetName, excelType, startCell, configuration);
            }
        }

        /// <summary>
        /// 保存数据到excel
        /// </summary>
        /// <param name="path">保存excel文件的完整路径</param>
        /// <param name="value">数据源</param>
        /// <param name="printHeader">是否打印表头</param>
        /// <param name="sheetName">sheetName</param>
        /// <param name="excelType">excel文件类型</param>
        /// <param name="configuration">配置</param>
        /// <param name="overwriteFile">是否覆写文件</param>
        public static void SaveAs(string path, object value, bool printHeader = true, string sheetName = "Sheet1", ExcelType excelType = ExcelType.UNKNOWN, IConfiguration configuration = null, bool overwriteFile = false)
        {
            MiniExcel.SaveAs(path, value, printHeader, sheetName, excelType, configuration, overwriteFile);
        }

        /// <summary>
        /// 保存数据到excel流
        /// </summary>
        /// <param name="stream">excel流</param>
        /// <param name="value">数据源</param>
        /// <param name="printHeader">是否打印表头</param>
        /// <param name="sheetName">sheetName</param>
        /// <param name="excelType">excel文件类型</param>
        /// <param name="configuration">配置</param>
        public static void SaveAs(this Stream stream, object value, bool printHeader = true, string sheetName = "Sheet1", ExcelType excelType = ExcelType.XLSX, IConfiguration configuration = null)
        {
            using (stream)
            {
                stream.SaveAs(value, printHeader, sheetName, excelType, configuration);
            }
        }
    }
}