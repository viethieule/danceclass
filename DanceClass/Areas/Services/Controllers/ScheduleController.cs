using Services.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Areas.Services.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost]
        public async Task<ActionResult> GetDetail(GetDetailedScheduleRq rq)
        {
            GetDetailedScheduleRs rs = await _scheduleService.GetDetail(rq);
            return Json(rs);
        }
    }
}