using DanceClass.Utils;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Mvc;
using Services.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Controllers
{

    public class ReportController : Controller
    {
        private readonly IRevenueReportService _revenueReportService;
        private static UserCredential _credential;

        public ReportController(IRevenueReportService revenueReportService)
        {
            _revenueReportService = revenueReportService;
        }

        [Authorize(Roles = "Admin")]
        // GET: Report
        public async Task<ActionResult> Revenue()
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(CancellationToken.None);
            if (result.Credential == null)
            {
                return new RedirectResult(result.RedirectUri);
            }
            else
            {
                _credential = result.Credential;
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Revenue(RevenueReportRq rq)
        {
            rq.Credential = _credential;
            var rs = await _revenueReportService.Run(rq);
            return Json(rs);
        }
    }
}