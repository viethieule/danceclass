using Autofac;
using DanceClass.Utils;

namespace DanceClass.Controllers
{
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        private readonly AppFlowMetadata _appFlowMetadata;

        public AuthCallbackController(AppFlowMetadata appFlowMetadata) : base()
        {
            _appFlowMetadata = appFlowMetadata;
        }

        protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        {
            get { return _appFlowMetadata; }
        }
    }
}