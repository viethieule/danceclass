using DataAccess.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    public class ApplicationUser : IdentityUser<int, UserLogin, UserRole, UserClaim>, IAuditable
    {
        [Index]
        [MaxLength(100)]
        public string FullName { get; set; }
        public DateTime? Birthdate { get; set; }
        public Membership Membership { get; set; }
        public ICollection<Registration> Registrations { get; set; }
        public ICollection<Package> Packages { get; set; }
        public bool IsNeedToChangePassword { get; set; }
        public int? RegisteredBranchId { get; set; }
        [ForeignKey("RegisteredBranchId")]
        public Branch RegisteredBranch { get; set; }

        [Index]
        [MaxLength(100)]
        public string NormalizedFullName { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
