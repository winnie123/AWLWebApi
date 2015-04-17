using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication7.Models
{
    public class ValidateDataModel
    {
        public string sequence { get; set; }
        public string md5 { get; set; }
        public string result { get; set; }
        public OperationModel operation { get; set; }
        public string time { get; set; }
    }
}