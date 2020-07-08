using Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Members.Get
{
    public class GetMemberRq : GetEntityRequestBase
    {
        public string UserName { get; set; }
    }
}
