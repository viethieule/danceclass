using DataAccess.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IdentityAccessor
{
    public class ApplicationRoleStore : RoleStore<Role, int, UserRole>
    {
        public ApplicationRoleStore(DanceClassDbContext context) : base(context)
        {
        }
    }
}
