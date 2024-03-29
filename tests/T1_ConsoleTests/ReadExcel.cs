﻿/***************************************************************************
 *GUID: b4c8c0d2-54e1-44b8-92af-601d680513cf
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-14 10:47:40
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright © NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T1_ConsoleTests
{
    internal class ReadExcel
    {
        //public static void Read()
        //{
        //    const string fullFileName = @"C:\Users\Newcats\Desktop\Result_108.xlsx";
        //    DataTable r1 = Newcats.Office.Excel.ReadExcelToTable(fullFileName);

        //    using (FileStream fs = new(fullFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        DataTable r2 = Newcats.Office.Excel.ReadExcelToTable(fs, Newcats.Office.Excel.ExcelFormatEnum.xlsx);
        //    }

        //    using (MemoryStream r4 = Newcats.Office.Excel.ReadDataTableToExcel(r1))
        //    {

        //    }

        //    using (FileStream fs = new(fullFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        List<DepartmentInfo> r6 = Newcats.Office.Excel.ReadExcelToList<DepartmentInfo>(fs, Newcats.Office.Excel.ExcelFormatEnum.xlsx);
        //    }

        //    List<DepartmentInfo> r5 = Newcats.Office.Excel.ReadExcelToList<DepartmentInfo>(fullFileName);

        //    using (MemoryStream r8 = Newcats.Office.Excel.ReadListToExcel<DepartmentInfo>(r5, Newcats.Office.Excel.ExcelFormatEnum.xlsx))
        //    {

        //    }

        //    Newcats.Office.Excel.ReadListToExcel(r5, Newcats.Office.Excel.ExcelFormatEnum.xls);
        //    foreach (var item in r5.Where(r => r.IsOnline != 0))
        //    {
        //        Console.WriteLine($"-- {item.DepartmentName}");
        //        Console.WriteLine($"update departmentinfo set OnlineOffline={item.IsOnline} where DepartmentID='{item.DepartmentID}';\r\n");
        //    }
        //}
    }

    public class DepartmentInfo
    {
        public string DepartmentID { get; set; }

        public string DepartmentName { get; set; }

        public string IsOnlineStr { get; set; }

        public int IsOnline
        {
            get
            {
                if (IsOnlineStr == "线下")
                    return 10;
                else if (IsOnlineStr == "线上")
                    return 20;
                else
                    return 0;
            }
        }
    }
}