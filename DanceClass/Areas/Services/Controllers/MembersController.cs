using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Mvc;
using Services.Members;
using Services.Members.Get;
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
        private readonly IMemberService _memberService;
        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateMemberRq rq)
        {
            CreateMemberRs rs = await _memberService.Create(rq);
            return Json(rs);
        }

        [HttpPost]
        public async Task<ActionResult> Get(GetMemberRq rq)
        {
            GetMemberRs rs = await _memberService.GetById(rq);
            return Json(rs);
        }
    }
}