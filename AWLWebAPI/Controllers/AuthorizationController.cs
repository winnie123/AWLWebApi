using MvcApplication7.InterFace.Filter;
using MvcApplication7.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml;
using System.Web.Mvc.Properties;
using System.Web.Http;
using System.Xml.Serialization;
using log4net;
using AWLWebAPI.Models.Entity;



namespace MvcApplication7.Controllers
{

    public class AuthorizationController : ApiController
    {
        //定义日志对象
        private ILog log = log4net.LogManager.GetLogger("awlApp.Logging");


        [HttpPost]
        public void Login([FromBody] root model)
        {
            ////没有记录在数据库中进行查询
            ////如果有记录记录在session中
            ////获取的xml进行解析
            ////解析获取Common内容
            XmlDocument xmlDoc;
            //获取操作类型，只有当操作类型为request或md5时执行接下来的逻辑,否则设置响应码为400,设置common和id_validate后返回s
            string operationType = null;
            try
            {
                operationType = model.common.type.ToString();//获取操作类型
            }
            catch (NullReferenceException)
            {
                HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);
                HttpContext.Current.Response.Write("Request Error");

                log.Debug("验证请求类型不是符合要求的指定类型");
                HttpContext.Current.Response.End();
                return;
            }
            HttpRequest request = HttpContext.Current.Request;
            string currentdatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");//获取当前时间
            #region 操作类型不是request和md5

            if (operationType != "request" && (operationType != "md5"))
            {//身份验证类型
                HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);
                HttpContext.Current.Response.Write("Opertation Error");
                log.Debug("验证请求类型不是符合要求的指定类型");
                HttpContext.Current.Response.End();
                return;
            }
            #endregion

            #region 操作类型是request
            //如果请求操作类型为request
            //取出报文的building_id和gateway_id
            //在数据库中查找对应的building_id和gateway_id的采集器，如果没有则设置响应码为400并设置返回的common和id_validate报文后返回
            //如果有则生成随机字符串并设置common和id_validate后返回
            if (operationType == "request")
            {
                string building_id = model.common.building_id;//楼栋编号
                string gateway_id = model.common.gateway_id;//设备编号
                string type = model.common.type.ToString();//请求类型


                //此处省略从数据库验证buliding_id和gateway_id过程(todo).....

                bool collectorExistFlag = true;
                CBEMSDGXXEntities db = new CBEMSDGXXEntities();
                List<PROC_COLLECTOR_REQUEST_Result> list = db.PROC_COLLECTOR_REQUEST(building_id, gateway_id).ToList();
                if (list == null || (list.Count == 0))
                {
                    collectorExistFlag = false;
                }


                //采集器信息在数据库中有对应的buliding_id和gateway_id,生成随机字符串
                #region 数据库验证成功
                if (collectorExistFlag)
                {

                    string sequencestr = getRandomNum();//生成9位随机数sequence
                    //生成response数据对象
                    root returnModel = new root()
                    {
                        common = model.common,
                        //设置返回的id_validate
                        id_validate = new ValidateDataModel()
                        {
                            sequence = sequencestr
                        }
                    };
                    returnModel.common.type = (OperationModel)Enum.Parse(typeof(OperationModel), "sequence");

                    string returnXmlStr = Serialize(returnModel);
                    //序列化returnModel,设置operation属性节点
                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(returnXmlStr);
                    XmlElement root = xmlDoc.DocumentElement;
                    XmlNode idValidateNode = root.SelectSingleNode("id_validate");
                    XmlAttribute attr = null;
                    attr = xmlDoc.CreateAttribute("operation");
                    attr.Value = "sequence";
                    idValidateNode.Attributes.SetNamedItem(attr);

                    //去除id_validate 下的子节点operation,占时不需要
                    XmlNode oper = idValidateNode.SelectSingleNode("operation");
                    idValidateNode.RemoveChild(oper);
                    string returnModelstr = Serialize(xmlDoc);
                    //response写入returnXmlStr
                    HttpContext.Current.Response.Write(returnModelstr);




                    //记录session以便和采集器第二次要发过来的md5验证码做比对
                    //此时sessionModel的操作类型为sequence,验证成功后的操作类型session操作类型为result,据此区别不同阶段的session
                    SessionModel sessionModel = new SessionModel()
                    {
                        comm = new CommMode()
                        {
                            building_id = model.common.building_id,
                            gateway_id = model.common.gateway_id,
                            type = (OperationModel)Enum.Parse(typeof(OperationModel), "sequence"),
                        },
                        //设备可能连接同一个路由器，那么对外的ip可能相同，所以ip不一定有用
                        //HTTP_VIA可以获得用户内部的ip
                        ipAddress = (request.ServerVariables["HTTP_VIA"] != null
                        ? request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString().Split(',')[0].Trim()
                        : request.UserHostAddress),
                        //将随机字符串与服务器对应该采集器的密钥连接,然后再md5加密后存服务器session的pk;
                        //这里先将对应的密钥设为定值,后续开发功能从服务器数据库取对应密钥(todo........)
                        pk = strToMd5(Comm.Common.collectorPk + sequencestr),
                        datetime = Convert.ToDateTime(currentdatetime)
                    };

                    //将session存入Common的session字典
                    if (Comm.Common.session.ContainsKey(sessionModel.comm.building_id + sessionModel.comm.gateway_id) == true)
                    {
                        Comm.Common.session.Remove(sessionModel.comm.building_id + sessionModel.comm.gateway_id);
                    }
                    Comm.Common.session.Add(sessionModel.comm.building_id + sessionModel.comm.gateway_id, sessionModel);

                    //返回response
                    HttpContext.Current.Response.End();
                    return;

                }
                #endregion
                #region 数据库验证失败
                //采集器信息在数据库中没有对应的buliding_id和gateway_id
                else
                {
                    HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);
                    HttpContext.Current.Response.Write("No matching data!");
                    log.Debug("采集器身份验证失败");
                    HttpContext.Current.Response.End();
                    return;
                }
                #endregion

            }
            #endregion

            #region 操作类型是md5
            //如果请求操作类型为md5,先检查session字典中有没有对应的session,如果没有，则返回报错信息，如果有，检查操作状态，上一步该session的操作状态应该是sequence
            //如果不是sequence,则判断是不是result,如果是，则取出pk值与请求的md5值进行比对，如比对成功，则验证成功，生成相应报文并返回，如比对不成功，则验证失败
            //如果既不是sequence,也不是result,则验证失败
            //如果是sequence,则取出pk值与请求的md5值进行比对，如果比对正确则验证成功，如果不正确则验证失败
            if (operationType == "md5")
            {
                string building_id = model.common.building_id;//楼栋编号
                string gateway_id = model.common.gateway_id;//设备编号
                string type = model.common.type.ToString();//请求类型

                #region 在session中没有记录验证失败
                if (Comm.Common.session.ContainsKey(building_id + gateway_id) == false)
                {
                    //HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);
                    //初始化返回的报文model
                    root returnModel = new root()
                    {
                        common = model.common,
                        //设置返回的id_validate
                        id_validate = new ValidateDataModel()
                        {
                            //sequence = string.Empty,
                            //md5 = string.Empty,
                            result = "fail"
                            //time = currentdatetime,
                            //operation = (OperationModel)Enum.Parse(typeof(OperationModel), "result")
                        }
                    };

                    returnModel.common.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");
                    //序列化returnModel
                    string returnXmlStr = Serialize(returnModel);



                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(returnXmlStr);
                    XmlElement root = xmlDoc.DocumentElement;
                    XmlNode idValidateNode = root.SelectSingleNode("id_validate");
                    XmlAttribute attr = null;
                    attr = xmlDoc.CreateAttribute("operation");
                    attr.Value = "result";
                    idValidateNode.Attributes.SetNamedItem(attr);

                    //去除id_validate 下的子节点operation,占时不需要
                    XmlNode oper = idValidateNode.SelectSingleNode("operation");
                    idValidateNode.RemoveChild(oper);
                    string returnModelstr = Serialize(xmlDoc);
                    //response写入returnXmlStr
                    HttpContext.Current.Response.Write(returnModelstr);
                    log.Debug("采集器未经过身份验证，验证失败");
                    HttpContext.Current.Response.End();

                    return;

                }
                #endregion

                SessionModel sessionModel = Comm.Common.session[building_id + gateway_id];

                //不是sequence
                if (sessionModel.comm.type.ToString() != "sequence")
                {
                    //不是sequence也不是result
                    if (sessionModel.comm.type.ToString() != "result")
                    {
                        //HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);

                        //初始化返回的报文model
                        root returnModel = new root()
                        {
                            common = model.common,
                            id_validate = new ValidateDataModel()
                            { //设置返回的id_validate
                                result = "fail"
                            }

                        };
                        returnModel.common.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");
                        //序列化returnModel
                        string returnXmlStr = Serialize(returnModel);

                        xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(returnXmlStr);
                        XmlElement root = xmlDoc.DocumentElement;
                        XmlNode idValidateNode = root.SelectSingleNode("id_validate");
                        XmlAttribute attr = null;
                        attr = xmlDoc.CreateAttribute("operation");
                        attr.Value = "result";
                        idValidateNode.Attributes.SetNamedItem(attr);

                        //去除id_validate 下的子节点operation,占时不需要
                        XmlNode oper = idValidateNode.SelectSingleNode("operation");
                        idValidateNode.RemoveChild(oper);
                        string returnModelstr = Serialize(xmlDoc);
                        //response写入returnXmlStr
                        HttpContext.Current.Response.Write(returnModelstr);
                        log.Debug("session信息异常");
                        HttpContext.Current.Response.End();
                        return;

                    }
                    //不是sequence但是result
                    else
                    {
                        bool validateFlag = (sessionModel.pk == model.id_validate.md5 ? true : false);
                        //比对成功
                        if (validateFlag)
                        {
                            ////记录session

                            sessionModel.datetime = Convert.ToDateTime(currentdatetime);//session中已存在，只更新记录
                            sessionModel.comm.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");

                            //生成success报文并返回
                            HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.OK);

                            //生成response数据对象
                            root returnModel = new root()
                            {
                                common = model.common,
                                //设置返回的id_validate
                                id_validate = new ValidateDataModel()
                                {
                                    //md5 = string.Empty,
                                    //sequence = string.Empty,
                                    result = "pass",
                                    time = currentdatetime,
                                    //operation = (OperationModel)Enum.Parse(typeof(OperationModel), "result")
                                }
                            };
                            returnModel.common.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");
                            //序列化returnModel
                            string returnXmlStr = Serialize(returnModel);

                            xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(returnXmlStr);
                            XmlElement root = xmlDoc.DocumentElement;
                            XmlNode idValidateNode = root.SelectSingleNode("id_validate");
                            XmlAttribute attr = null;
                            attr = xmlDoc.CreateAttribute("operation");
                            attr.Value = "result";
                            idValidateNode.Attributes.SetNamedItem(attr);

                            //去除id_validate 下的子节点operation,占时不需要
                            XmlNode oper = idValidateNode.SelectSingleNode("operation");
                            idValidateNode.RemoveChild(oper);
                            string returnModelstr = Serialize(xmlDoc);
                            //response写入returnXmlStr
                            HttpContext.Current.Response.Write(returnModelstr);
                            HttpContext.Current.Response.End();
                            return;
                        }
                        //比对失败
                        else
                        {
                            // HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);
                            //初始化返回的报文model
                            root returnModel = new root()
                            {
                                common = model.common,
                                id_validate = new ValidateDataModel()
                                { //设置返回的id_validate
                                    result = "fail"
                                }

                            };
                            returnModel.common.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");
                            //序列化returnModel
                            string returnXmlStr = Serialize(returnModel);

                            xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(returnXmlStr);
                            XmlElement root = xmlDoc.DocumentElement;
                            XmlNode idValidateNode = root.SelectSingleNode("id_validate");
                            XmlAttribute attr = null;
                            attr = xmlDoc.CreateAttribute("operation");
                            attr.Value = "result";
                            idValidateNode.Attributes.SetNamedItem(attr);

                            //去除id_validate 下的子节点operation,占时不需要
                            XmlNode oper = idValidateNode.SelectSingleNode("operation");
                            idValidateNode.RemoveChild(oper);
                            string returnModelstr = Serialize(xmlDoc);
                            //response写入returnXmlStr
                            HttpContext.Current.Response.Write(returnModelstr);
                            log.Debug("加密信息比对错误，验证失败");
                            HttpContext.Current.Response.End();
                            return;
                        }
                    }
                }
                //是sequence
                #region sequence类型
                else
                {
                    bool validateFlag = (sessionModel.pk == model.id_validate.md5 ? true : false);
                    //比对成功
                    if (validateFlag)
                    {

                        sessionModel.datetime = Convert.ToDateTime(currentdatetime);//session中已存在，只更新记录
                        sessionModel.comm.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");

                        //生成success报文并返回
                        HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.OK);

                        //生成response数据对象
                        root returnModel = new root()
                        {
                            common = model.common,//返回的common为请求的common不变以便客户端查找错误
                            //设置返回的id_validate
                            id_validate = new ValidateDataModel()
                            {
                                result = "pass",
                                time = currentdatetime,
                            }
                        };

                        returnModel.common.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");
                        //序列化returnModel
                        string returnXmlStr = Serialize(returnModel);

                        xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(returnXmlStr);
                        XmlElement root = xmlDoc.DocumentElement;
                        XmlNode idValidateNode = root.SelectSingleNode("id_validate");
                        XmlAttribute attr = null;
                        attr = xmlDoc.CreateAttribute("operation");
                        attr.Value = "result";
                        idValidateNode.Attributes.SetNamedItem(attr);

                        //去除id_validate 下的子节点operation,占时不需要
                        XmlNode oper = idValidateNode.SelectSingleNode("operation");
                        idValidateNode.RemoveChild(oper);
                        string returnModelstr = Serialize(xmlDoc);
                        //response写入returnXmlStr
                        HttpContext.Current.Response.Write(returnModelstr);
                        HttpContext.Current.Response.End();
                        return;
                    }
                    //比对失败
                    else
                    {
                        //HttpContext.Current.Response.StatusCode = Convert.ToInt32(HttpStatusCode.BadRequest);

                        //初始化返回的报文model
                        root returnModel = new root()
                        {
                            common = model.common,//返回的common为请求的common不变以便客户端查找错误
                            id_validate = new ValidateDataModel()
                            { //设置返回的id_validate
                                result = "fail"
                            }

                        };
                        returnModel.common.type = (OperationModel)Enum.Parse(typeof(OperationModel), "result");

                        //序列化returnModel
                        string returnXmlStr = Serialize(returnModel);

                        xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(returnXmlStr);
                        XmlElement root = xmlDoc.DocumentElement;
                        XmlNode idValidateNode = root.SelectSingleNode("id_validate");
                        XmlAttribute attr = null;
                        attr = xmlDoc.CreateAttribute("operation");
                        attr.Value = "result";
                        idValidateNode.Attributes.SetNamedItem(attr);

                        //去除id_validate 下的子节点operation,占时不需要
                        XmlNode oper = idValidateNode.SelectSingleNode("operation");
                        idValidateNode.RemoveChild(oper);
                        string returnModelstr = Serialize(xmlDoc);
                        //response写入returnXmlStr
                        HttpContext.Current.Response.Write(returnModelstr);
                        log.Debug("加密信息比对错误，验证失败");
                        HttpContext.Current.Response.End();
                        return;
                    }
                }
                #endregion
            }

            #endregion

        }


        // 序列化对象 
        // </summary> 
        // <typeparam name=\"T\">对象类型</typeparam> 
        // <param name=\"t\">对象</param> 
        // <returns></returns> 
        public static string Serialize<T>(T t)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer xz = new XmlSerializer(t.GetType());
                xz.Serialize(sw, t);
                return sw.ToString();
            }
        }


        // 反序列化为对象 
        // </summary> 
        // <param name=\"type\">对象类型</param> 
        // <param name=\"s\">对象序列化后的Xml字符串</param> 
        // <returns></returns> 
        public static object Deserialize(Type type, string s)
        {
            using (StringReader sr = new StringReader(s))
            {
                XmlSerializer xz = new XmlSerializer(type);
                return xz.Deserialize(sr);
            }
        }

        //md5加密算法
        public static string strToMd5(string sequence)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5Cng();
            byte[] bytResult = md5.ComputeHash(System.Text.Encoding.GetEncoding("UTF-8").GetBytes(sequence));

            //转换成字符串，32位
            string strResult = BitConverter.ToString(bytResult).Replace("-", "");

            return strResult;
        }

        //生成9位随机数sequence
        public static string getRandomNum()
        {
            Random mRandom = new Random();
            string randomstr = string.Empty;
            for (int i = 0; i < 9; i++)
            {
                int a = mRandom.Next(1, 9);
                randomstr += a.ToString();
            }
            return randomstr;
        }
    }
}
