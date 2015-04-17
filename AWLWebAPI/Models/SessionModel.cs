using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication7.Models
{
    public class SessionModel
    {
        public CommMode comm { get; set; }
        public string ipAddress { get; set; }
        public DateTime datetime { get; set; }
        public string pk { get; set; }
    }
}