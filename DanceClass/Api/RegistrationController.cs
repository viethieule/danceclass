using Services.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/registration")]
    public class RegistrationController : ApiController
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> Create(CreateRegistrationRq rq)
        {
            CreateRegistrationRs rs = await _registrationService.Create(rq);
            return Json(rs);
        }

        [HttpPost]
        [Route("cancel")]
        public async Task<IHttpActionResult> Cancel([FromBody]int registrationId)
        {
            int result = await _registrationService.Cancel(registrationId);
            return Json(result);
        }

        [HttpPost]
        [Route("getbyscheduledetail")]
        public async Task<IHttpActionResult> GetByScheduleDetail([FromBody]int scheduleDetailId)
        {
            var registrations = await _registrationService.GetByScheduleDetail(scheduleDetailId);
            return Json(registrations);
        }

        [HttpPut]
        [Route("confirmAttendance/:scheduleDetailId")]
        [Authorize(Roles = "Admin")]
        public async Task ConfirmAttendance(int scheduleDetailId)
        {
            await _registrationService.ConfirmAttendance(scheduleDetailId);
        }
    }
}