using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Members
{
    public class MemberDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthdate { get; set; }
        public string Email { get; set; }
        public string DefaultPassword { get; set; }
        public string Username { get; set; }
        public int? PackageId { get; set; }
    }
}
