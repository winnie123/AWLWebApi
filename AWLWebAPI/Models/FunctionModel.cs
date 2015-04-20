using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication7.Models
{
    /// <summary>
    /// 每个计量装置的具体采集功能
    /// </summary>
    public class FunctionModel
    {
        /// <summary>
        /// 计量装置的数据采集功能编号
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 能耗数据分类/分项编码
        /// </summary>
        public string coding { get; set; }
        /// <summary>
        /// 该功能出现错误的状态码，0表示没有错误
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// 采集数据
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// 辨别插入方式
        /// </summary>
        public string primary { get; set; }
    }
}