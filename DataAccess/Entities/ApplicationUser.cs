using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class UserRole : IdentityUserRole<int>
    {
    }

    public class UserClaim : IdentityUserClaim<int>
    {
    }

    public class UserLogin : IdentityUserLogin<int>
    {
    }

    public class Role : IdentityRole<int, UserRole>
    {
        public Role() { }
        public Role(string name) { Name = name; }
    }

    public class UserStore : UserStore<ApplicationUser, Role, int,
        UserLogin, UserRole, UserClaim>
    {
        public UserStore(DanceClassDbContext context) : base(context)
        {
        }
    }

    public class RoleStore : RoleStore<Role, int, UserRole>
    {
        public RoleStore(DanceClassDbContext context) : base(context)
        {
        }
    }

    public class ApplicationUser : IdentityUser<int, UserLogin, UserRole, UserClaim>
    {
        public string FullName { get; set; }
        public DateTime? Birthdate { get; set; }
        public int? PackageId { get; set; }

        [ForeignKey("PackageId")]
        public Package Package { get; set; }
        public int IdentityNo { get; set; }

        public virtual IEnumerable<ScheduleMember> ScheduleMembers { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
