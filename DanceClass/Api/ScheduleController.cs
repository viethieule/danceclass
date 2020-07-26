using Services.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/schedule")]
    public class ScheduleController : ApiController
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost]
        [Route("getdetail")]
        public async Task<IHttpActionResult> GetDetail(GetDetailedScheduleRq rq)
        {
            var rs = await _scheduleService.GetDetail(rq);
            return Json(rs);
        }

        [HttpPost]
        [Route("getregisteredsessions")]
        public async Task<IHttpActionResult> GetRegisteredSessions(GetRegisteredSessionRq rq)
        {
            var rs = await _scheduleService.GetRegisteredSessions(rq);
            return Json(rs);
        }
    }
}
