using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Members.Get
{
    public class GetMemberRs
    {
        public MemberDTO Member { get; set; }
        public bool? IsAuthenticated { get; set; }
    }
}
