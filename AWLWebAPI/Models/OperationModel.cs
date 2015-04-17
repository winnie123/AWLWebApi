using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication7.Models
{
    public enum OperationModel
    {
        request,//采集器请求身份验证（该数据包为采集器发送给服务器）
        sequence,//服务器发送一串随机序列，sequence子元素有效（该数据包为服务器发送给采集器）
        md5,//采集器发送计算的MD5，md5子元素有效（该数据包为采集器发送给服务器）
        result,//服务器发送验证结果，result子元素有效（该数据包为服务器发送给采集器）
        notify,//采集器定期给服务器发送存活通知
        time,//服务器在收到存活通知后发送授时信息，此时子元素time有效
        query,//服务器查询数据采集器，不需要子元素
        reply,//采集器对服务器查询的应答
        report,//采集器定时上报的能耗数据
        continuous,//采集器断点续传的能耗数据
        continuous_ack,//全部续传数据包接收完成后，服务器对断点续传的应答，不需要子元素
        period,//表示服务器对采集器采集周期的配置,period子元素有效
        period_ack,//表示采集器对服务器采集周期配置信息的应答
        revert//服务器response客户端请求数据

    }

    
}