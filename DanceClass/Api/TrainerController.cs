using Services.Trainer;
using System.Threading.Tasks;
using System.Web.Http;

namespace DanceClass.Api
{
    [RoutePrefix("api/trainer")]
    public class TrainerController : ApiBaseController
    {
        private readonly ITrainerService _trainerService;

        public TrainerController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        [HttpGet]
        [Route("getAll")]
        public async Task<IHttpActionResult> GetAll()
        {
            var rs = await _trainerService.GetAll();
            return ApiJson(rs);
        }
    }
}