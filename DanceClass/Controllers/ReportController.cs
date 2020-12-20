using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{

    public class ReportController : Controller
    {
        [Authorize(Roles = "Admin")]
        // GET: Report
        public ActionResult Revenue()
        {
            return View();
        }
    }
}