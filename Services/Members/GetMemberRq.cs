using Services.Common;

namespace Services.Members
{
    public class GetMemberRq : GetEntityRequestBase
    {
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
