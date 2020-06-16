using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Mvc;
using Services.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Areas.Services.Controllers
{
    public class MembersController : Controller
    {
        [HttpPost]
        public async Task<ActionResult> Create(CreateMemberRq rq)
        {
            MemberService service = new MemberService(
                HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(),
                HttpContext.GetOwinContext().GetUserManager<ApplicationSignInManager>());

            CreateMemberRs rs = await service.Create(rq);
            return Json(rs);
        }
    }
}