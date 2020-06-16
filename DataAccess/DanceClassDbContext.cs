using DataAccess.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace DataAccess
{
    public class DanceClassDbContext : IdentityDbContext<ApplicationUser, Role, int, UserLogin, UserRole, UserClaim>
    {
        public DanceClassDbContext()
            : base("DefaultConnection")
        {
        }

        public static DanceClassDbContext Create()
        {
            return new DanceClassDbContext();
        }

        public DbSet<Class> Classes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ClassMember> ClassMembers { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
    }
}
