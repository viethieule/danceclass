﻿using DanceClass.Utils;
using Services.Registration;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/registration")]
    public class RegistrationController : ApiBaseController
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.ReceptionistAndHigher)]
        [Route("create")]
        public async Task<IHttpActionResult> Create(CreateRegistrationRq rq)
        {
            CreateRegistrationRs rs = await _registrationService.Create(rq);
            return ApiJson(rs);
        }

        [HttpPost]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.ReceptionistAndHigher)]
        [Route("cancel")]
        public async Task<IHttpActionResult> Cancel(CancelRegistrationRq rq)
        {
            int result = await _registrationService.Cancel(rq);
            return ApiJson(result);
        }

        [HttpPost]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.ReceptionistAndHigher)]
        [Route("getbyscheduledetail")]
        public async Task<IHttpActionResult> GetByScheduleDetail([FromBody]int scheduleDetailId)
        {
            var registrations = await _registrationService.GetByScheduleDetail(scheduleDetailId);
            return ApiJson(registrations);
        }

        [HttpPut]
        [Route("confirmAttendance/{registrationId}")]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.ReceptionistAndHigher)]
        public async Task ConfirmAttendance(int registrationId)
        {
            await _registrationService.ConfirmAttendance(registrationId);
        }

        [HttpPost]
        [Route("getByUser")]
        [Authorize]
        public async Task<IHttpActionResult> GetByUser(GetRegistrationsRq rq)
        {
            var rs = await _registrationService.GetByUser(rq);
            return ApiJson(rs);
        }
    }
}