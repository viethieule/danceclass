using Services.MemberPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/memberPackage")]
    public class MemberPackageController : ApiBaseController
    {
        private readonly IMemberPackageService _memberPackageService;

        public MemberPackageController(IMemberPackageService memberPackageService)
        {
            _memberPackageService = memberPackageService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("create")]
        public async Task<IHttpActionResult> Create(CreateMemberPackageRq rq)
        {
            var rs = await _memberPackageService.Create(rq);
            return ApiJson(rs);
        }
    }
}
