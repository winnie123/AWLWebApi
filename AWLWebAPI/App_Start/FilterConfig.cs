using System.Web;
using System.Web.Mvc;
using MvcApplication7.InterFace.Filter;

namespace MvcApplication7
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new AuthorizationGateFilter());
            filters.Add(new HandleErrorAttribute());
           
        }
    }
}