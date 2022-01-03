/***************************************************************************
 *GUID: ad4ee703-2f9c-4c67-b14f-42f19a3a88cf
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-03 18:55:58
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

namespace Newcats.Utils.Models
{
    /// <summary>
    /// Rsa密钥格式
    /// </summary>
    public enum RsaKeyFormatEnum
    {
        /// <summary>
        /// Pkcs8
        /// </summary>
        Pkcs8 = 0,

        /// <summary>
        /// Pkcs1
        /// </summary>
        Pkcs1 = 1,

        /// <summary>
        /// Xml
        /// </summary>
        Xml = 2
    }
}