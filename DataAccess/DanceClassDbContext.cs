using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

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

        public override Task<int> SaveChangesAsync()
        {
            var modifieds = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

            foreach (DbEntityEntry item in modifieds)
            {
                var changedOrAddedItem = item.Entity as IAuditable;
                if (changedOrAddedItem != null)
                {
                    if (item.State == EntityState.Added)
                    {
                        changedOrAddedItem.CreatedDate = DateTime.Now;
                    }
                    changedOrAddedItem.UpdatedDate = DateTime.Now;
                }
            }

            return base.SaveChangesAsync();
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
