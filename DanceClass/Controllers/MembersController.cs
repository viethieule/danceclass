using DanceClass.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{
    [Authorize]
    public class MembersController : BaseController
    {
        // GET: Members
        public ActionResult Index(string username)
        {
            return View();
        }

        public ActionResult Create()
        {
            this.LayoutViewModel.SelectedLeftMenuItem = SelectedLeftMenuItem.Create;
            this.LayoutViewModel.SelectedLeftMenuSubItem = SelectedLeftMenuSubItem.Create_Member;
            return View(this.LayoutViewModel);
        }

        [Authorize(Roles = "Admin, Collaborator, Receptionist")]
        public ActionResult Search()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Collaborator")]
        public ActionResult All()
        {
            this.LayoutViewModel.SelectedLeftMenuItem = SelectedLeftMenuItem.Members;
            return View(this.LayoutViewModel);
        }
    }
}