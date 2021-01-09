using DanceClass.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{
    public class ReceptionistController : BaseController
    {
        // GET: Receptionist
        public ActionResult Create()
        {
            this.LayoutViewModel.SelectedLeftMenuItem = SelectedLeftMenuItem.Create;
            this.LayoutViewModel.SelectedLeftMenuSubItem = SelectedLeftMenuSubItem.Create_Receptionist;
            return View(this.LayoutViewModel);
        }
    }
}