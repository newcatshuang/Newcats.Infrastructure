/***************************************************************************
 *GUID: 683aa574-4622-4a95-b4b1-71fc712476eb
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-03 17:49:14
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/

namespace Newcats.Utils.Models
{
    /// <summary>
    /// Base64格式的RsaKey
    /// </summary>
    public struct RsaKey
    {
        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// 私钥
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// 密钥格式
        /// </summary>
        public RsaKeyFormatEnum KeyFormat { get; set; }
    }
}