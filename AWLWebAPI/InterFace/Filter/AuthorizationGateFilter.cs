using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml;
using MvcApplication7.Models;
using System.Web.Http.Filters;
using System.Security.Cryptography;


namespace MvcApplication7.InterFace.Filter
{
    public class AuthorizationGateFilter : ActionFilterAttribute
    {
            
       // private static byte[] _key1 = { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38 };
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            //获取的xml进行解析
            Stream stream = actionContext.Request.Content.ReadAsStreamAsync().Result;
            //long xxxd = stream.Length;
            Encoding encoding = Encoding.Default;
            stream.Position = 0;
            byte[] a = new byte[stream.Length];           
            //AES CBC解密
            string responseData = "";

            string keys = "1234567890123456";
            byte[] s = dddecrypt(stream, keys);
            responseData = Encoding.Default.GetString(s);
           
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
            if (Comm.Common.session.ContainsKey(building_id + gateway_id) == false)
            {
                actionContext.Response = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.MethodNotAllowed
                };
            }
            else
            {
                try
                {
                    //有记录判断时间是否过期
                    SessionModel session = Comm.Common.session[building_id + gateway_id];
                    DateTime expiresDate = session.datetime;
                    DateTime now = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    //if ((now.Ticks - expiresDate.Ticks) >= Comm.Common.Expires) //*60 / 10000000.0 
                    long i = (now.Ticks - expiresDate.Ticks) / 10000000;
                    long xxx = i;
                    if ((now.Ticks - expiresDate.Ticks) / 10000000 <= Comm.Common.Expires)
                    {
                        //expiresDate = now;
                        //base.OnAuthorization(actionContext);
                        actionContext.Request.Content = new StringContent(responseData);
                        base.OnActionExecuting(actionContext);
                    }
                    else
                    {

                        //HttpResponseMessage response=new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
                        actionContext.Response = new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.RequestTimeout
                        };
                    }
                }catch(Exception){
                    HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    HttpContext.Current.Response.Write("authorization Errror!");
                    HttpContext.Current.Response.End();
                }
            }
          
        }

        public static string AESEncrypt(string plainText , string strKey )  
        {  
            //分组加密算法  
            SymmetricAlgorithm des = Rijndael .Create () ;                
            byte[] inputByteArray =Encoding .UTF8  .GetBytes (plainText ) ;//得到需要加密的字节数组      
                            //设置密钥及密钥向量  
            des.Key =Encoding.UTF8.GetBytes (strKey );  
            des.IV = Encoding.UTF8.GetBytes("1234567812345678") ;
            des.Mode = CipherMode.CBC;
            //des.Padding = PaddingMode.PKCS7;
            //des.KeySize = 128;
            MemoryStream ms = new MemoryStream();  
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);  
            cs.Write(inputByteArray, 0, inputByteArray.Length);  
            cs.FlushFinalBlock();  
            byte[] cipherBytes = ms .ToArray () ;//得到加密后的字节数组  
            cs.Close();  
            ms.Close();
            string a =null;
            string t = null;
            for (int i = 0; i < cipherBytes.Length; i++)

                        {
                            t = t + cipherBytes[i].ToString();
                            a =a + cipherBytes[i].ToString("X2");
                        }

            return a;
                     
                    }
            //return cipherBytes ;              
       
          
    //     <summary>  
    //     AES字符转码解密  
    //     </summary>  
    //     <param name="cipherText">密文字节数组</param>  
    //     <param name="strKey">密钥</param>  
    //     <returns>返回解密后的字符串</returns>  
    //    public static byte[] AESDecrypt(byte[] cipherText, string strKey)
    //    {
    //        SymmetricAlgorithm des = Rijndael.Create();
    //        des.Key = Encoding.UTF8.GetBytes(strKey);
    //        des.IV = Encoding.UTF8.GetBytes("1234567812345678");
    //        des.Mode = CipherMode.CBC;
    //        des.Padding = PaddingMode.PKCS7;
    //        des.KeySize = 128;
    //        byte[] decryptBytes = new byte[cipherText.Length];
    //        MemoryStream ms = new MemoryStream(cipherText);
    //        CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
    //        cs.Read(decryptBytes, 0, decryptBytes.Length);
    //        cs.Close();
    //        ms.Close();
    //        return decryptBytes;
    //    }
    //}  

     //<summary>  
        // AES流转码解密  
        // </summary>  
        // <param name="cipherText">密文字节数组</param>  
        // <param name="strKey">密钥</param>  
        // <returns>返回解密后的字符串</returns>  
        public static byte[] dddecrypt(Stream stream, string strKey)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            string hex2 = null;
            for (int i = 0; i < bytes.Length; i++)
            {

                hex2 = hex2 + Convert.ToString(bytes[i], 16).ToUpper();
            }
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(strKey);
            des.IV = Encoding.UTF8.GetBytes("1234567812345678");
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;
            //des.KeySize = 128;

            byte[] decryptBytes = new byte[stream.Length];
            MemoryStream ms = new MemoryStream(bytes);
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(des.Key, des.IV), CryptoStreamMode.Read);
            cs.Read(decryptBytes, 0, decryptBytes.Length);
            cs.Close();
            ms.Close();
            return decryptBytes;
        }
    }  

    }