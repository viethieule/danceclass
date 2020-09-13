using Services.Schedule;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/schedule")]
    public class ScheduleController : ApiBaseController
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
            return ApiJson(rs);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("create")]
        public async Task<IHttpActionResult> Create(CreateScheduleRq rq)
        {
            var rs = await _scheduleService.Create(rq);
            return ApiJson(rs);
        }

        [HttpPost]
        [Route("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Update(UpdateScheduleRq rq)
        {
            var rs = await _scheduleService.Update(rq);
            return ApiJson(rs);
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var rs = await _scheduleService.Delete(id);
            return ApiJson(rs);
        }
    }
}
