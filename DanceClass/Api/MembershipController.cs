using DanceClass.Utils;
using Services.Membership;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/membership")]
    [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.CollaboratorAndHigher)]
    public class MembershipController : ApiBaseController
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpPost]
        [Route("update")]
        public async Task<IHttpActionResult> Update(UpdateMembershipRq rq)
        {
            var rs = await _membershipService.Update(rq);
            return ApiJson(rs);
        }
    }
}
