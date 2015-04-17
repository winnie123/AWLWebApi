using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using MvcApplication7.Models;


namespace MvcApplication7.InterFace.Filter
{
    public class AuthorizationGateFilter : System.Web.Http.AuthorizeAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            //获取的xml进行解析
            Stream stream = actionContext.Request.Content.ReadAsStreamAsync().Result;
            Encoding encoding = Encoding.UTF8;
            stream.Position = 0;
            string responseData = "";
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                responseData = reader.ReadToEnd().ToString();
            }
            //解析获取Common内容
            string building_id = string.Empty;
            string gateway_id = string.Empty;
            string requestData = responseData;
            XmlDocument docuement = new XmlDocument();
            docuement.LoadXml(requestData);
            XmlElement root = docuement.DocumentElement;
            XmlNodeList buildingidNodes = root.GetElementsByTagName("building_id");
            if (buildingidNodes != null && buildingidNodes.Count > 0)
            {
                building_id = buildingidNodes[0].InnerXml;
            }
            XmlNodeList gatewayidNodes = root.GetElementsByTagName("gateway_id");
            if (gatewayidNodes != null && gatewayidNodes.Count > 0)
            {
                gateway_id = gatewayidNodes[0].InnerXml;
            }

            if (string.IsNullOrEmpty(building_id) || string.IsNullOrEmpty(gateway_id))
            {
                actionContext.Response.StatusCode = HttpStatusCode.MethodNotAllowed;
                return;
            }
            //判断session中是否已经有记录
            if (Comm.Common.session.ContainsKey(building_id+gateway_id) == false)
            {
                actionContext.Response = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.MethodNotAllowed
                };  
            }
            else
            {
                //有记录判断时间是否过期
                SessionModel session = Comm.Common.session[gateway_id];
                DateTime expiresDate = session.datetime;
                DateTime now = new DateTime();
                if ((now.Ticks - expiresDate.Ticks) / 10000000.0 >= Comm.Common.Expires * 60)
                {
                    base.OnAuthorization(actionContext);
                }
                else
                {
                    //HttpResponseMessage response=new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
                    actionContext.Response = new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.RequestTimeout
                    };
                }
            }


        }
    }
}