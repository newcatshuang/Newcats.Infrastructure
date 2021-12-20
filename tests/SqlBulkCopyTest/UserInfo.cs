/***************************************************************************
 *GUID: f99fc662-0e0a-45a2-bef5-4f39a7e61016
 *CLR Version: 4.0.30319.42000
 *DateCreated：2021-12-20 21:33:09
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBulkCopyTest
{
    [Table("UserInfo")]
    internal class UserInfo
    {
        [Key]
        public long Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("CreateTime")]
        public DateTime CreateTime { get; set; }
    }
}