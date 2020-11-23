using DanceClass.Utils;
using Services.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/class")]
    public class ClassController : ApiBaseController
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpGet]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.CollaboratorAndHigher)]
        [Route("getAll")]
        public async Task<IHttpActionResult> GetAll()
        {
            var rs = await _classService.GetAll();
            return ApiJson(rs);
        }
    }
}
