using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using MvcApplication7.InterFace.Filter;
using MvcApplication7.Models;

namespace MvcApplication7.Controllers
{
    [AuthorizationGateFilter]
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5

        public string Get(int id)
        {
            return "value";
        }


        //public string Post([FromBody] DataModel model)
        //{
        //    return string.Format("id:{0},name:{1},value:{2}", model.id, model.name, model.value);
        //}

        //public string Post([FromBody] string model)
        //{
        //    return null;
        //    //return string.Format("id:{0},name:{1},value:{2}", model.id, model.name, model.value);
        //}



        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}