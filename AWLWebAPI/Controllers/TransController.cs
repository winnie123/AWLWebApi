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
using System.IO;
using System.Text;
using System.Collections;
using log4net;
using AWLWebAPI.Models.Entity;


namespace MvcApplication7.Controllers
{
    [AuthorizationGateFilter]
    public class TransController : ApiController
    {

        //定义日志对象
        private ILog log = log4net.LogManager.GetLogger("awlApp.Logging");
        static object locker = new object();
        [HttpPost]
        public void DataTransmit([FromBody] string model)
        {
            Stream stream = Request.Content.ReadAsStreamAsync().Result;
            //Encoding encoding = Encoding.Default;
            using (StreamReader reader = new StreamReader(stream))
            {
                model = reader.ReadToEnd().ToString();
            }

            string result = string.Empty;
            XmlDocument docuement = new XmlDocument();
            docuement.LoadXml(model);
            XmlElement root = docuement.DocumentElement;

            XmlNode buildingidNode = root.GetElementsByTagName("building_id")[0];//common节点下的building_id节点
            XmlNode typeNode = root.GetElementsByTagName("type")[0];//common节点下的type节点
            //xml数据不符合格式
            if (typeNode == null || string.IsNullOrEmpty(typeNode.InnerText)
                || buildingidNode == null || string.IsNullOrEmpty(buildingidNode.InnerText))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                HttpContext.Current.Response.Write("BadRequest");
                log.Debug("数据传输格式不正确");
                HttpContext.Current.Response.End();
                return;
            }

            string buildingid = buildingidNode.InnerText;//buildingid或Divisionid
            OperationModel operation = (OperationModel)Enum.Parse(typeof(OperationModel), typeNode.InnerText);//获取type类型
            int cou = 0;
            int err = 0;
            int cou2 = 0;
            switch (operation)
            {
                //客户端请求数据
                case OperationModel.report:
                    {
                        //string collectionTime = timeNode.InnerText;//获取采集时间
                        XmlNodeList meterNodes = root.GetElementsByTagName("meter");
                        if (meterNodes != null)
                        {

                            List<TransDataModel> list = SerializeMeterData(meterNodes, buildingid);
                            string info = "建筑:" + buildingid + "共有" + meterNodes.Count + "个采集点上传";
                            log.Info(info);

                            if (list != null && list.Count > 0)
                            {
                                CBEMSDGXXEntities db = new CBEMSDGXXEntities();
                                for (int i = 0; i < list.Count; i++)
                                {
                                    try
                                    {
                                        db.PROC_COLLECTORDATA_INSERT(list[i].primary, list[i].building_id, list[i].collectiontime, list[i].equipmentid, list[i].quantity, list[i].id, list[i].data2, list[i].cou);
                                        cou++;
                                    }
                                    catch (Exception)
                                    {
                                        err++;
                                    }

                                }
                                log.Info("建筑" + buildingid + "共有:" + cou + "条传输成功");
                                log.Info("建筑" + buildingid + "共有:" + err + "条错误");

                            }


                            HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.OK;
                            //HttpContext.Current.Response.Write("BadRequest");
                            HttpContext.Current.Response.End();
                            //}
                        }
                        //解析采集数据
                        break;
                    }
                //其他类型，response 错误信息
                default:
                    {
                        HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        HttpContext.Current.Response.Write("BadRequest");
                        log.Debug("数据传输类型不是指定的类型");
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
        /// 
        //修改之后的插入数据库的逻辑
        private List<TransDataModel> SerializeMeterData(XmlNodeList meterNodes, string buildingid)
        {
            List<TransDataModel> trans = new List<TransDataModel>();

            for (int i = 0, len = meterNodes.Count; i < len; i++)
            {
                //string meterid = meterNode.Attributes["id"].Value;//meterid
                string equipmentid = meterNodes[i].Attributes["id"].Value;//equipmentid
                XmlNodeList funNodes = meterNodes[i].SelectNodes("function");
                XmlNodeList timeNodes = meterNodes[i].SelectNodes("time");
                string time = timeNodes[0].InnerText;
                if (funNodes == null)
                    continue;
                string id = null;
                string data = null;

                for (int j = 0, nodesLen = funNodes.Count; j < nodesLen; j++)
                {
                    string id2 = funNodes[j].Attributes["id"].Value;
                    string data2 = funNodes[j].InnerText;
                    string primary = funNodes[j].Attributes["primary"].Value;
                    if (j == 0)
                    {
                        id = id2;
                        data = data2;
                    }
                    else
                    {
                        id = id + "," + id2;
                        data = data + "," + data2;
                    }
                    if (j == nodesLen - 1 || primary == "1")
                    {
                        trans.Add(new TransDataModel()
                        {
                            primary = primary,
                            collectiontime = time,
                            quantity = data,
                            building_id = buildingid,
                            equipmentid = equipmentid,
                            id = id,
                            data2 = data,
                            cou = funNodes.Count.ToString()
                        });
                    }
                }

            }

            return trans;
        }
    }
}
