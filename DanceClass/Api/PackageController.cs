using DanceClass.Utils;
using Services.DefaultPackage;
using Services.Package;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/package")]
    public class PackageController : ApiBaseController
    {
        private readonly IPackageService _packageService;
        private readonly IDefaultPackageService _defaultPackageService;

        public PackageController(IPackageService packageService, IDefaultPackageService defaultPackageService)
        {
            _packageService = packageService;
            _defaultPackageService = defaultPackageService;
        }

        [HttpGet]
        [Route("getDefaults")]
        public async Task<IHttpActionResult> GetDefaults()
        {
            var packages = await _defaultPackageService.GetAll(true);
            return ApiJson(packages);
        }

        [HttpPost]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.ReceptionistAndHigher)]
        [Route("add")]
        public async Task<IHttpActionResult> AddForMember(CreatePackageRq rq)
        {
            var rs = await _packageService.AddForMember(rq);
            return ApiJson(rs);
        }

        [HttpPost]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.CollaboratorAndHigher)]
        [Route("edit")]
        public async Task<IHttpActionResult> Edit(EditPackageRq rq)
        {
            var rs = await _packageService.Edit(rq);
            return ApiJson(rs);
        }

        [HttpPost]
        [HierarchicalAuthorize(AuthorizationLevel = AuthorizationLevel.CollaboratorAndHigher)]
        [Route("editPrivate")]
        public async Task<IHttpActionResult> EditPrivate(EditPackageRq rq)
        {
            var rs = await _packageService.EditPrivate(rq);
            return ApiJson(rs);
        }

        [HttpPost]
        [Authorize]
        [Route("getByUserId")]
        public async Task<IHttpActionResult> GetByUserId(GetPackagesRq rq)
        {
            var rs = await _packageService.GetByUserId(rq);
            return ApiJson(rs);
        }
    }
}
