using Services.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/weeklyschedulereport")]
    public class WeeklyScheduleReportController : ApiController
    {
        private readonly IWeeklyScheduleReportService _weeklyScheduleReportService;

        public WeeklyScheduleReportController(IWeeklyScheduleReportService weeklyScheduleReportService)
        {
            _weeklyScheduleReportService = weeklyScheduleReportService;
        }

        [Route("run")]
        public async Task<HttpResponseMessage> Run(WeeklyScheduleReportRq rq)
        {
            var rs = await _weeklyScheduleReportService.Run(rq);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(rs.ByteArray)
            };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = rs.FileName
            };

            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
