using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
            AuditEntity();
            return base.SaveChangesAsync();
        }

        public override int SaveChanges()
        {
            AuditEntity();
            return base.SaveChanges();
        }

        private void AuditEntity()
        {
            var modifieds = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            var currentUser = string.Empty;
            if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
            {
                currentUser = HttpContext.Current.User.Identity.Name;
            }

            DateTime now = DateTime.Now;
            foreach (DbEntityEntry item in modifieds)
            {
                if (item.Entity is IAuditable changedOrAddedItem)
                {
                    if (item.State == EntityState.Added)
                    {
                        changedOrAddedItem.CreatedDate = now;

                        // When using user manager Http context is null so not update the current user with string empty
                        if (!string.IsNullOrEmpty(currentUser))
                        {
                            changedOrAddedItem.CreatedBy = currentUser;
                        }
                    }
                    changedOrAddedItem.UpdatedDate = now;

                    // When using user manager Http context is null so not update the current user with string empty
                    if (!string.IsNullOrEmpty(currentUser))
                    {
                        changedOrAddedItem.UpdatedBy = currentUser;
                    }

                    if (item.State == EntityState.Modified && item.Entity is IFieldChangeLog logItem)
                    {
                        ObjectContext objContext = ((IObjectContextAdapter)this).ObjectContext;
                        ObjectStateEntry objState = objContext.ObjectStateManager.GetObjectStateEntry(item.Entity);
                        string primaryKeyName = objState.EntitySet.ElementType.KeyMembers.Select(k => k.Name).FirstOrDefault();
                        string id = !string.IsNullOrEmpty(primaryKeyName) ? item.CurrentValues[primaryKeyName].ToString() : string.Empty;

                        List<FieldChangeLogDetail> changeLogs = new List<FieldChangeLogDetail>();

                        IEnumerable<string> modifiedProperties = objState.GetModifiedProperties();
                        foreach (var propName in modifiedProperties)
                        {
                            changeLogs.Add(new FieldChangeLogDetail
                            {
                                Field = propName,
                                PreviousValue = objState.OriginalValues[propName].ToString(),
                                UpdatedValue = objState.CurrentValues[propName].ToString()
                            });
                        }

                        string changeLog = JsonConvert.SerializeObject(changeLogs);

                        this.FieldChangeLogs.Add(new FieldChangeLog
                        {
                            Entity = ObjectContext.GetObjectType(item.Entity.GetType()).Name,
                            EntityId = id,
                            ChangeLog = changeLog,
                            Action = logItem.LatestAction,
                            CreatedBy = currentUser,
                            CreatedDate = now,
                            UpdatedBy = currentUser,
                            UpdatedDate = now
                        });
                    }
                }
            }
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
        public DbSet<Branch> Branches { get; set; }
        public DbSet<FieldChangeLog> FieldChangeLogs { get; set; }
    }
}
