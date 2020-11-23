using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        // GET: Members
        public ActionResult Index(string username)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Collaborator, Receptionist")]
        public ActionResult Search()
        {
            return View();
        }
    }
}