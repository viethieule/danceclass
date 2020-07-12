using DataAccess.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IdentityAccessor
{
    public class ApplicationUserStore : UserStore<ApplicationUser, Role, int, UserLogin, UserRole, UserClaim>
    {
        public ApplicationUserStore(DanceClassDbContext context) : base(context)
        {
        }
    }
}
