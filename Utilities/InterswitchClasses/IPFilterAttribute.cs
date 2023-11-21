using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CPEA.Utilities.InterswitchClasses
{
    public class IPFilterAttribute : ActionFilterAttribute
    {
        public HttpRequestMessage Request { get; private set; }

        //IP filter is still WIP as @ 21th-Setp.,2018
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                string IP = actionContext.Request.RequestUri.Authority;
                if (General.WhitelistedIps(IP))
                {
                    //take an action here
                }
                //  string log = string.Format("Action Method {0} executing at {1}", actionContext.ActionDescriptor.ActionName, DateTime.Now.ToShortDateString(), "Interswitch Request got here");
                //  General.LogToFileLenght(log);
            }
            catch (Exception e)
            {
                General.LogToFile(e);
            }


        }


        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                // string log = string.Format("Action Method {0} executed at {1}", actionExecutedContext.ActionContext.ActionDescriptor.ActionName, DateTime.Now.ToShortDateString(), "Response was Returned");
                // General.LogToFileLenght(log);
            }
            catch (Exception e)
            {
                General.LogToFile(e);
            }
        }
    }
}


