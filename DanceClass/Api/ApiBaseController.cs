using DanceClass.Utils;
using Newtonsoft.Json.Serialization;
using System.Web.Http;
using System.Web.Http.Results;

namespace DanceClass.Api
{
    public class ApiBaseController : ApiController
    {
        protected JsonResult<T> ApiJson<T>(T content)
        {
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                //ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            serializerSettings.Converters.Add(new EnumToKeyValuePairConverter());
            return Json(content, serializerSettings);
        }
    }
}