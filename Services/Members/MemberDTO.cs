using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Members
{
    public class MemberDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime Birthdate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public ICollection<UserRoleDTO> Roles { get; set; }
        public List<string> RoleNames { get; set; }
    }
}
