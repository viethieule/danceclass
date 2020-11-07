using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{
    public class ReceptionistController : Controller
    {
        // GET: Receptionist
        public ActionResult Create()
        {
            return View();
        }
    }
}