using DanceClass.Utils;
using Services.Branch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/branch")]
    public class BranchController : ApiBaseController
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [HttpGet]
        [Route("getAll")]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.ReceptionistAndHigher)]
        public async Task<IHttpActionResult> GetAll()
        {
            var rs = await _branchService.GetAll();
            return ApiJson(rs);
        }
    }
}
