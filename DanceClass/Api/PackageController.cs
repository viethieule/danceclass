using Services.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/package")]
    public class PackageController : ApiBaseController
    {
        private readonly IPackageService _packageService;
        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        [Route("getall")]
        public async Task<IHttpActionResult> GetAll()
        {
            var packages = await _packageService.GetAll(true);
            return ApiJson(packages);
        }
    }
}
