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
        private readonly IPackageService _packageService;
        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var packages = await _packageService.GetAll(true);
            return Json(packages, JsonRequestBehavior.AllowGet);
        }
    }
}