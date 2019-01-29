using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DevExpress.Web.Mvc;

namespace Digital_Forensic
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DevExpressHelper.Theme = "MetropolisBlue";
            DevExpressHelper.Theme = "Glass";
            DevExpressHelper.Theme = "Mulberry";
            DevExpressHelper.Theme = "RedWine";
            DevExpressHelper.Theme = "Aqua";
            ModelBinders.Binders.DefaultBinder = new DevExpressEditorsBinder();
        }
    }
}
