using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplication7.Models;

namespace MvcApplication7.Comm
{
    public static class Common
    {
        /// <summary>
        /// 自定义session
        /// </summary>
        public static Dictionary<string, SessionModel> session = new Dictionary<string, SessionModel>();
        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        public static int Expires = 30;

        public static string collectorPk = "hello123";
    }
}