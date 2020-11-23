using DanceClass.Utils;
using Services.Receptionist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.CollaboratorAndHigher)]
    [RoutePrefix("api/receptionist")]
    public class ReceptionistController : ApiBaseController
    {
        private readonly IReceptionistService _receptionistService;

        public ReceptionistController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> Create(CreateReceptionistRq rq)
        {
            var rs = await _receptionistService.Create(rq);
            return ApiJson(rs);
        }
    }
}
