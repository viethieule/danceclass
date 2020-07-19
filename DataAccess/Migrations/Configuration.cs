namespace DataAccess.Migrations
{
    using DataAccess.Entities;
    using DataAccess.IdentityAccessor;
    using Microsoft.AspNet.Identity;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class Configuration : DbMigrationsConfiguration<DataAccess.DanceClassDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DataAccess.DanceClassDbContext context)
        {
            if (!context.Classes.Any())
            {
                context.Classes.AddRange(new List<Class>
                {
                    new Class { Name = "Múa đương đại" },
                    new Class { Name = "Dance Workout" },
                    new Class { Name = "Sexy Beginner" },
                    new Class { Name = "Choreography" },
                    new Class { Name = "Urban Choreography" },
                    new Class { Name = "Dance Cover" },
                });

                context.SaveChanges();
            }

            if (!context.Trainers.Any())
            {
                context.Trainers.AddRange(new List<Trainer>
                {
                    new Trainer { Name = "Linh" },
                    new Trainer { Name = "Thảo" }
                });

                context.SaveChanges();
            }

            if (!context.Schedules.Any())
            {
                List<Class> classes = context.Classes.ToList();
                Class muaDuongDai = classes.FirstOrDefault(x => x.Name == "Múa đương đại");
                Class danceWorkout = classes.FirstOrDefault(x => x.Name == "Dance Workout");
                Class sexyBeginner = classes.FirstOrDefault(x => x.Name == "Sexy Beginner");
                Class choreography = classes.FirstOrDefault(x => x.Name == "Choreography");
                Class urbanChoreography = classes.FirstOrDefault(x => x.Name == "Urban Choreography");
                Class danceCover = classes.FirstOrDefault(x => x.Name == "Dance Cover");

                List<Trainer> trainers = context.Trainers.ToList();
                Trainer linh = trainers.FirstOrDefault(x => x.Name == "Linh");
                Trainer thao = trainers.FirstOrDefault(x => x.Name == "Thảo");

                context.Schedules.AddRange(new List<Schedule>
                {
                    new Schedule { Song = "Tay trái chỉ trăng", OpeningDate = new DateTime(2020, 6, 16), EndingDate = new DateTime(2020, 6, 25) , StartTime = new TimeSpan(12, 0, 0), Sessions = 4, SessionsPerWeek = 2, DaysPerWeek = "24", Branch = "Q3", ClassId = muaDuongDai.Id, TrainerId = linh.Id },
                    new Schedule { Song = "Một đời đợi người", OpeningDate = new DateTime(2020, 6, 8), EndingDate = new DateTime(2020, 6, 26) , StartTime = new TimeSpan(18, 0, 0), Sessions = 6, SessionsPerWeek = 2, DaysPerWeek = "15", Branch = "Q3", ClassId = muaDuongDai.Id, TrainerId = thao.Id },
                    new Schedule { Song = "We don't talk anymore", OpeningDate = new DateTime(2020, 6, 6), EndingDate = new DateTime(2020, 6, 27) , StartTime = new TimeSpan(18, 0, 0), Sessions = 4, SessionsPerWeek = 1, DaysPerWeek = "6", Branch = "Q3", ClassId = danceCover.Id, TrainerId = linh.Id },
                    new Schedule { Song = "Lạnh lẽo", OpeningDate = new DateTime(2020, 6, 7), EndingDate = new DateTime(2020, 6, 21) , StartTime = new TimeSpan(19, 35, 0), Sessions = 5, SessionsPerWeek = 2, DaysPerWeek = "05", Branch = "PN", ClassId = muaDuongDai.Id, TrainerId = thao.Id },
                });

                context.SaveChanges();
            }

            if (!context.ScheduleDetails.Any())
            {
                
            }

            if (!context.Packages.Any())
            {
                context.Packages.AddRange(new List<Package>
                {
                    new Package { NumberOfSessions = 8, Price = 600000, Months = 2, IsDefault = true },
                    new Package { NumberOfSessions = 16, Price = 1000000, Months = 3, IsDefault = true },
                    new Package { NumberOfSessions = 24, Price = 1450000, Months = 5, IsDefault = true },
                    new Package { NumberOfSessions = 50, Price = 3000000, Months = 8, IsDefault = true },
                });

                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                context.Roles.Add(new Role { Name = "Admin" });
                context.Roles.Add(new Role { Name = "Member" });

                context.SaveChanges();
            }

            if (!context.Users.Any(x => x.UserName == "admin"))
            {
                var userStore = new ApplicationUserStore(context);
                var userManager = new ApplicationUserManager(userStore);
                var admin = new ApplicationUser { UserName = "admin", Birthdate = DateTime.Now };

                var result = userManager.CreateAsync(admin, "P@ssw0rd").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRole(admin.Id, "Admin");
                }

                context.SaveChanges();
            }
        }
    }
}
