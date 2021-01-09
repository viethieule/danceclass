using DanceClass.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{
    [Authorize]
    public class ScheduleController : BaseController
    {
        // GET: Schedule
        public ActionResult Index()
        {
            this.LayoutViewModel.SelectedLeftMenuItem = SelectedLeftMenuItem.Schedule;
            return View(this.LayoutViewModel);
        }
    }
}