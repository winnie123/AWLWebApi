using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication7.Models
{

    public class CommMode
    {
        /// <summary>
        /// 楼栋编号
        /// </summary>
        public string building_id { get; set; }
        /// <summary>
        /// 采集器编号
        /// </summary>
        public string gateway_id { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public OperationModel type { get; set; }
    }
}