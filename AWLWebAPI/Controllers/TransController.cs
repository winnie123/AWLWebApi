using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using MvcApplication7.InterFace.Filter;
using MvcApplication7.Models;

namespace MvcApplication7.Controllers
{
    [AuthorizationGateFilter]
    public class TransController : ApiController
    {
        [HttpPost]
        public void DataTransmit([FromBody] string model)
        {
            string result = string.Empty;
            XmlDocument docuement = new XmlDocument();
            docuement.LoadXml(model);
            XmlElement root = docuement.DocumentElement;

            XmlNode buildingidNode = root.SelectSingleNode("building_id");//common节点下的building_id节点
            XmlNode typeNode = root.SelectSingleNode("type");//common节点下的type节点
            //xml数据不符合格式
            if (typeNode == null || string.IsNullOrEmpty(typeNode.Value)
                || buildingidNode == null || string.IsNullOrEmpty(buildingidNode.Value))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                HttpContext.Current.Response.Write("BadRequest");
                HttpContext.Current.Response.End();
                return;
            }

            string buildingid = buildingidNode.Value;//buildingid或Divisionid
            OperationModel operation = (OperationModel)Enum.Parse(typeof(OperationModel), typeNode.Value);//获取type类型
            switch (operation)
            {
                //客户端请求数据
                case OperationModel.report:
                    {
                        XmlNode timeNode = root.SelectSingleNode("time");
                        if (timeNode == null || string.IsNullOrEmpty(timeNode.Value))
                        {
                            HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            HttpContext.Current.Response.Write("BadRequest");
                            HttpContext.Current.Response.End();
                        }
                        else
                        {
                            string collectionTime = timeNode.Value;//获取采集时间
                            XmlNodeList meterNodes = root.SelectNodes("meter");
                            if (meterNodes != null)
                            {
                                List<TransDataModel> list = SerializeMeterData(meterNodes, buildingid);
                                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                                //HttpContext.Current.Response.Write("BadRequest");
                                HttpContext.Current.Response.End();
                            }
                        }
                        //解析采集数据
                        break;
                    }
                //其他类型，response 错误信息
                default:
                    {
                        HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        HttpContext.Current.Response.Write("BadRequest");
                        HttpContext.Current.Response.End();
                        break;
                    }
            }
        }

        /// <summary>
        /// 解析meter节点
        /// </summary>
        /// <param name="meterNodes">meter节点集合</param>
        /// <param name="buildingid">building编号</param>
        /// <returns></returns>
        private List<TransDataModel> SerializeMeterData(XmlNodeList meterNodes, string buildingid)
        {
            List<TransDataModel> trans = new List<TransDataModel>();
            for (int i = 0, len = meterNodes.Count; i < len; i++)
            {
                //string meterid = meterNode.Attributes["id"].Value;//meterid
                string equipmentid = meterNodes[i].Attributes["name"].Value;//equipmentid
                XmlNodeList funNodes = meterNodes[i].SelectNodes("function");
                if (funNodes == null)
                    continue;
                for (int j = 0, nodesLen = funNodes.Count; j < nodesLen; j++)
                {
                    string id = funNodes[j].Attributes["id"].Value;
                    string error = funNodes[j].Attributes["error"].Value;
                    if (id != "w" || error != "0")
                        continue;
                    string data = funNodes[j].Value;
                    trans.Add(new TransDataModel()
                    {
                        data = data,
                        divisionid = buildingid,
                        equipmentid = equipmentid,
                    });
                }

            }
            return trans;
        }
    }
}
