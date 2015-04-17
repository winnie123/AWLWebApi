using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication7.Models
{
    public class DataModel
    {
        public string sequence { get; set; }
        public string time { get; set; }
        public List<MeterModel> meter { get; set; }
    }
}