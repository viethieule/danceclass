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
            //this.Configuration.LazyLoadingEnabled = false;
            //this.Configuration.ProxyCreationEnabled = false;
        }

        public static DanceClassDbContext Create()
        {
            return new DanceClassDbContext();
        }

        public DbSet<Class> Classes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleDetail> ScheduleDetails { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<DefaultPackage> DefaultPackages { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Membership> Memberships { get; set; }
    }
}
