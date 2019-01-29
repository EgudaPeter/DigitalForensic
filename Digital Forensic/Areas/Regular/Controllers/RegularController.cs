using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Digital_Forensic.Areas.Regular.Controllers
{
    [Authorize(Roles = "RE")]
    public class RegularController : Controller
    {
        // GET: Regular/Regular
        public ActionResult RegularView()
        {
            return View();
        }
    }
}