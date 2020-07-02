using Services.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Areas.Services.Controllers
{
    public class PackageController : Controller
    {
        private readonly PackageService _packageService;
        public PackageController()
        {
            _packageService = new PackageService();
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var packages = await _packageService.GetAll();
            return Json(packages, JsonRequestBehavior.AllowGet);
        }
    }
}