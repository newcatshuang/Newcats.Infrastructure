/***************************************************************************
 *GUID: f4e3c0c4-982a-492d-a85a-e29ae5d8fe69
 *CLR Version: 4.0.30319.42000
 *DateCreated：2022-01-09 11:48:59
 *Author: NewcatsHuang
 *Email: newcats@live.com
 *Github: https://github.com/newcatshuang
 *Copyright NewcatsHuang All rights reserved.
*****************************************************************************/
using System.Text;
using Newcats.Utils.Models;

namespace Newcats.Utils.Helpers
{
    /// <summary>
    /// 中文拼音帮助类
    /// </summary>
    public class PinYinHelper
    {
        //参考 https://github.com/2881099/NPinyin

        /// <summary>
        /// 获取中文文本的拼音首字母(例如:中国=>zg)
        /// </summary>
        /// <param name="chineseText">编码为UTF8的中文文本</param>
        /// <returns>中文文本的拼音首字母</returns>
        public static string GetFirstPinYin(string chineseText)
        {
            chineseText = chineseText.Trim();
            StringBuilder chars = new StringBuilder();
            for (var i = 0; i < chineseText.Length; ++i)
            {
                string py = GetPinyin(chineseText[i], "", false);
                chars.Append(py[0]);
            }

            return chars.ToString();
        }

        /// <summary>
        /// 获取中文文本的拼音
        /// </summary>
        /// <param name="chineseText">编码为UTF8的中文文本</param>
        /// <param name="separator">分隔符</param>
        /// <param name="upperFirst">首字母是否返回大写</param>
        /// <returns>中文文本的拼音</returns>
        public static string GetPinyin(string chineseText, string separator = "", bool upperFirst = true)
        {
            StringBuilder pinyin = new StringBuilder();
            for (var i = 0; i < chineseText.Length; ++i)
            {
                string py = GetPinyin(chineseText[i], separator, upperFirst);
                pinyin.Append(py);
            }

            return pinyin.ToString().Trim();
        }

        /// <summary>
        /// 获取和拼音相同的汉字列表
        /// </summary>
        /// <param name="pinyin">编码为UTF8的拼音</param>
        /// <returns>取拼音相同的汉字列表，如拼音“ai”将会返回“唉爱……”等</returns>
        public static string GetChineseText(string pinyin)
        {
            string key = pinyin.Trim().ToLower();

            foreach (string str in PinYinDictionary.PinYinCode)
            {
                if (str.StartsWith(key + " ") || str.StartsWith(key + ":"))
                    return str.Substring(7);
            }

            return string.Empty;
        }

        /// <summary>
        /// 返回单个字符的汉字拼音
        /// </summary>
        /// <param name="ch">编码为UTF8的中文字符</param>
        /// <param name="separator">分隔符</param>
        /// <param name="upperFirst">首字母是否返回大写</param>
        /// <returns>字符对应的拼音</returns>
        public static string GetPinyin(char ch, string separator = "", bool upperFirst = true)
        {
            byte[] charBytes = Encoding.UTF8.GetBytes(ch.ToString());
            if (charBytes[0] <= 127)
                return ch.ToString();//非中文字符直接返回

            short hash = GetHashIndex(ch);
            for (var i = 0; i < PinYinDictionary.PinYinHash[hash].Length; ++i)
            {
                short index = PinYinDictionary.PinYinHash[hash][i];
                var pos = PinYinDictionary.PinYinCode[index].IndexOf(ch, 7);
                if (pos != -1)
                {
                    var result = $"{PinYinDictionary.PinYinCode[index].Substring(0, 6).Trim()}{separator}";
                    if (!upperFirst)
                        return result.ToString();

                    if (result.Length > 1)
                    {
                        return string.Concat(result[0].ToString().ToUpper(), result.AsSpan(1));
                    }
                    else
                    {
                        return result.ToString().ToUpper();
                    }
                }

            }
            return ch.ToString();
        }

        /// <summary>
        /// 取文本索引值
        /// </summary>
        /// <param name="ch">字符</param>
        /// <returns>文本索引值</returns>
        private static short GetHashIndex(char ch)
        {
            return (short)((uint)ch % PinYinDictionary.PinYinCode.Length);
        }
    }
}