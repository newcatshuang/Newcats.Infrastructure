using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Web;

namespace Newcats.Utils.Helpers
{
    /// <summary>
    /// Http操作帮助类
    /// </summary>
    public static class HttpHelper
    {
        #region IP
        /// <summary>
        /// 获取当前页面客户端的IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP(HttpContext context, bool tryUseXForwardHeader = true)
        {
            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            //
            if (tryUseXForwardHeader)
            {
                ip = GetHeaderValueAs<string>(context, "X-Forwarded-For");//.TrimEnd(',').Split(',').AsEnumerable().Select(s => s.Trim()).ToList().FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(ip))
                    ip = ip.TrimEnd(',').Split(',').AsEnumerable().Select(s => s.Trim()).ToList().FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(ip) && context.Connection?.RemoteIpAddress != null)
                ip = context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeaderValueAs<string>(context, "REMOTE_ADDR");

            if (string.IsNullOrWhiteSpace(ip))
                ip = "0.0.0.0";

            return ip;
        }

        private static T GetHeaderValueAs<T>(HttpContext context, string headerName)
        {
            if (context.Request?.Headers?.TryGetValue(headerName, out StringValues values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrWhiteSpace(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }
        #endregion

        #region Download(把文件流写入客户端响应)
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="response">http响应</param>
        /// <param name="filePath">文件绝对路径</param>
        /// <param name="fileName">文件名,包含扩展名</param>
        public static async Task DownloadFileAsync(HttpResponse response, string filePath, string fileName)
        {
            await DownloadFileAsync(response, filePath, fileName, Encoding.UTF8);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="response">http响应</param>
        /// <param name="filePath">文件绝对路径</param>
        /// <param name="fileName">文件名,包含扩展名</param>
        /// <param name="encoding">字符编码</param>
        public static async Task DownloadFileAsync(HttpResponse response, string filePath, string fileName, Encoding encoding)
        {
            var bytes = FileHelper.Read(filePath);
            await DownloadAsync(response, bytes, fileName, encoding);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="response">http响应</param>
        /// <param name="stream">流</param>
        /// <param name="fileName">文件名,包含扩展名</param>
        public static async Task DownloadAsync(HttpResponse response, Stream stream, string fileName)
        {
            await DownloadAsync(response, stream, fileName, Encoding.UTF8);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="response">http响应</param>
        /// <param name="stream">流</param>
        /// <param name="fileName">文件名,包含扩展名</param>
        /// <param name="encoding">字符编码</param>
        public static async Task DownloadAsync(HttpResponse response, Stream stream, string fileName, Encoding encoding)
        {
            await DownloadAsync(response, FileHelper.ToBytes(stream), fileName, encoding);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="response">http响应</param>
        /// <param name="bytes">字节流</param>
        /// <param name="fileName">文件名,包含扩展名</param>
        public static async Task DownloadAsync(HttpResponse response, byte[] bytes, string fileName)
        {
            await DownloadAsync(response, bytes, fileName, Encoding.UTF8);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="response">http响应</param>
        /// <param name="bytes">字节流</param>
        /// <param name="fileName">文件名,包含扩展名</param>
        /// <param name="encoding">字符编码</param>
        public static async Task DownloadAsync(HttpResponse response, byte[] bytes, string fileName, Encoding encoding)
        {
            if (bytes == null || bytes.Length == 0)
                return;
            fileName = fileName.Replace(" ", "");
            fileName = HttpUtility.UrlEncode(fileName, encoding);
            response.ContentType = "application/octet-stream";
            response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
            response.Headers.Add("Content-Length", bytes.Length.ToString());
            await response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
        #endregion
    }
}