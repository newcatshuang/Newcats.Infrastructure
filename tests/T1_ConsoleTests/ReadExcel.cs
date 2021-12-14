/***************************************************************************
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T1_ConsoleTests
{
    internal class ReadExcel
    {
        public static void Read()
        {
            const string fullFileName = @"C:\Users\Newcats\Desktop\Result_108.xlsx";
            var list = Newcats.Office.Excel.ReadExcelToList<DepartmentInfo>(fullFileName);

            Newcats.Office.Excel.ReadListToExcel(list, Newcats.Office.Excel.ExcelFormatEnum.xls);
            foreach (var item in list.Where(r => r.IsOnline != 0))
            {
                Console.WriteLine($"-- {item.DepartmentName}");
                Console.WriteLine($"update departmentinfo set OnlineOffline={item.IsOnline} where DepartmentID='{item.DepartmentID}';\r\n");
            }
        }
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