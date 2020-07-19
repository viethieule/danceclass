using Services.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Areas.Services.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateRegistrationRq rq)
        {
            CreateRegistrationRs rs = await _registrationService.Create(rq);
            return Json(rs);
        }

        [HttpPost]
        public async Task<ActionResult> Cancel(int id)
        {
            int result = await _registrationService.Cancel(id);
            return Json(result);
        }
    }
}