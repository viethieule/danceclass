using Services.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/report")]
    public class ReportController : ApiBaseController
    {
        private readonly IRevenueReportService _revenueReportService;

        public ReportController(IRevenueReportService revenueReportService)
        {
            _revenueReportService = revenueReportService;
        }

        [HttpPost]
        [Route("RevenueReport")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> RevenueReport(RevenueReportRq rq)
        {
            var rs = await _revenueReportService.Run(rq);
            return ApiJson(rs);
        }
    }
}
