using DanceClass.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{
    public class BaseController : Controller
    {
        protected LayoutViewModel LayoutViewModel;
        public BaseController()
        {
            LayoutViewModel = new LayoutViewModel();
        }
    }
}