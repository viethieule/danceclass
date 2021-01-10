using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Services.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DanceClass.Utils
{
    public interface IAppFlowMetadata
    {
    }

    public class AppFlowMetadata : FlowMetadata, IAppFlowMetadata
    {
        private readonly IMemberService _memberService;

        public AppFlowMetadata(IMemberService memberService) : base()
        {
            _memberService = memberService;
        }

        private static IAuthorizationCodeFlow flow = null;

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.
            var user = controller.Session["user"];
            if (user == null)
            {
                user = Guid.NewGuid();
                controller.Session["user"] = user;
            }
            return user.ToString();

        }

        public override IAuthorizationCodeFlow Flow
        {
            get
            {
                if (flow == null)
                {
                    var user = Task.Run(async () => await _memberService.GetAll(new GetAllMemberRq())).Result;
                    flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = "1060276698310-el1ldkjpbeamioj8psru786isp60u3gp.apps.googleusercontent.com",
                            ClientSecret = "EKGK8KniA4GcO9Run2-PHqCE"
                        },
                        Scopes = new[] { SheetsService.Scope.Spreadsheets },
                        DataStore = new FileDataStore(@"C:\datastore", true)
                    });
                }

                return flow;
            }
        }
    }
}