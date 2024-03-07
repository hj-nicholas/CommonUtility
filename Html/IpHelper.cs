using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Jane.Common.Utility.Html
{
    public class IpHelper
    {
        #region 获得用户IP
        /// <summary>
        /// 获得用户IP
        /// </summary>
        public static string GetUserIp(HttpContext context)
        {
            string key = "X-Forwarded-For";
            if (context.Request.Headers.ContainsKey(key))
            {
                //nginx反向代理通过这种方式获取IP
                var ips = context.Request.Headers[key].ToString().Split(',');//多层Nginx 取第一个
                return ips.Any() ? ips[0] : string.Empty;
            }
            var address = context.Connection.RemoteIpAddress.ToString();
            return address == "::1" ? "127.0.0.1" : address.Substring(7);
        }
        #endregion

        #region 检查是否为IP地址
        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
        #endregion
    }
}
