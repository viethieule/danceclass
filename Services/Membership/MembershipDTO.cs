using Services.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Membership
{
    public class MembershipDTO
    {
        public int? Id { get; set; }

        public int RemainingSessions { get; set; }
        public DateTime ExpiryDate { get; set; }

        public int UserId { get; set; }
        public MemberDTO User { get; set; }
    }
}
