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
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    internal sealed class Configuration : DbMigrationsConfiguration<DataAccess.DanceClassDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        public string ConvertToUnsignedString(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        protected override void Seed(DataAccess.DanceClassDbContext context)
        {
            var unnormalizedUsers = context.Users.Where(u => !string.IsNullOrEmpty(u.FullName) && string.IsNullOrEmpty(u.NormalizedFullName));
            foreach (ApplicationUser user in unnormalizedUsers)
            {
                user.NormalizedFullName = ConvertToUnsignedString(user.FullName);
            }

            context.SaveChanges();

            return;

            if (!context.Branches.Any())
            {
                context.Branches.AddRange(new List<Branch>
                {
                    new Branch { Name = "Quận 3", Abbreviation = "Q3", Address = "Ngô Thời Nhiệm" },
                    new Branch { Name = "Phú Nhuận", Abbreviation = "PN", Address = "Phùng Văn Cung" },
                    new Branch { Name = "Lê Văn Sỹ", Abbreviation = "LVS", Address = "Lê Văn Sỹ" },
                });

                context.SaveChanges();
            }

            if (!context.Roles.Any(r => r.Name == "Collaborator"))
            {
                context.Roles.Add(new Role { Name = "Collaborator" });
            }

            if (!context.Users.Any(x => x.UserName == "collaborator"))
            {
                var userStore = new ApplicationUserStore(context);
                using (var userManager = new ApplicationUserManager(userStore))
                {
                    var collaborator = new ApplicationUser { UserName = "collaborator" };

                    var result = userManager.CreateAsync(collaborator, "Mistake1234").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(collaborator.Id, "Collaborator");
                    }
                }

                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                context.Roles.Add(new Role { Name = "Admin" });
                context.Roles.Add(new Role { Name = "Member" });
                context.Roles.Add(new Role { Name = "Receptionist" });

                context.SaveChanges();
            }

            if (!context.DefaultPackages.Any())
            {
                context.DefaultPackages.AddRange(new List<DefaultPackage>
                {
                    new DefaultPackage { NumberOfSessions = 8, Price = 600000, Months = 2 },
                    new DefaultPackage { NumberOfSessions = 16, Price = 1000000, Months = 3 },
                    new DefaultPackage { NumberOfSessions = 24, Price = 1450000, Months = 5 },
                    new DefaultPackage { NumberOfSessions = 50, Price = 3000000, Months = 8 },
                });

                context.SaveChanges();
            }

            if (!context.Users.Any(x => x.UserName == "admin"))
            {
                var userStore = new ApplicationUserStore(context);
                using (var userManager = new ApplicationUserManager(userStore))
                {
                    var admin = new ApplicationUser { UserName = "admin", Birthdate = DateTime.Now };

                    var result = userManager.CreateAsync(admin, "Mistake1234").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(admin.Id, "Admin");
                    }
                }

                context.SaveChanges();
            }

            if (!context.Users.Any(x => x.UserName == "member.test"))
            {
                var userStore = new ApplicationUserStore(context);
                using (var userManager = new ApplicationUserManager(userStore))
                {
                    var defaultPackage = context.DefaultPackages.FirstOrDefault();
                    var member = new ApplicationUser
                    {
                        UserName = "member.test",
                        FullName = "Test Member",
                        Birthdate = DateTime.Now,
                        PhoneNumber = "01234567890",
                        Membership = new Membership
                        {
                            RemainingSessions = defaultPackage.NumberOfSessions - 1, // will add registration later
                            ExpiryDate = DateTime.Now.AddMonths(defaultPackage.Months)
                        },
                        Packages = new List<Package>
                        {
                            new Package
                            {
                                DefaultPackageId = defaultPackage.Id,
                                ExpiryDate = DateTime.Now.AddMonths(defaultPackage.Months),
                                Price = defaultPackage.Price,
                                Months = defaultPackage.Months,
                                NumberOfSessions = defaultPackage.NumberOfSessions,
                                IsActive = true,
                                RemainingSessions = defaultPackage.NumberOfSessions - 1 // will add registration later
                            }
                        }
                    };

                    var result = userManager.CreateAsync(member, "123456").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(member.Id, "Member");
                    }

                    context.SaveChanges();
                }
            }

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
                var tayTraiChiTrang = context.Schedules.FirstOrDefault(s => s.Song == "Tay trái chỉ trăng");
                if (tayTraiChiTrang != null)
                {
                    context.ScheduleDetails.AddRange(new List<ScheduleDetail>
                    {
                        new ScheduleDetail { Date = new DateTime(2020, 6, 16), ScheduleId = tayTraiChiTrang.Id, SessionNo = 1 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 18), ScheduleId = tayTraiChiTrang.Id, SessionNo = 2 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 23), ScheduleId = tayTraiChiTrang.Id, SessionNo = 3 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 25), ScheduleId = tayTraiChiTrang.Id, SessionNo = 4 },
                    });
                }

                var motDoiDoiNguoi = context.Schedules.FirstOrDefault(s => s.Song == "Một đời đợi người");
                if (motDoiDoiNguoi != null)
                {
                    context.ScheduleDetails.AddRange(new List<ScheduleDetail>
                    {
                        new ScheduleDetail { Date = new DateTime(2020, 6, 8), ScheduleId = motDoiDoiNguoi.Id, SessionNo = 1 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 12), ScheduleId = motDoiDoiNguoi.Id, SessionNo = 2 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 15), ScheduleId = motDoiDoiNguoi.Id, SessionNo = 3 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 19), ScheduleId = motDoiDoiNguoi.Id, SessionNo = 4 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 22), ScheduleId = motDoiDoiNguoi.Id, SessionNo = 5 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 26), ScheduleId = motDoiDoiNguoi.Id, SessionNo = 6 },
                    });
                }

                var weDontTalkAnymore = context.Schedules.FirstOrDefault(s => s.Song == "We don't talk anymore");
                if (weDontTalkAnymore != null)
                {
                    context.ScheduleDetails.AddRange(new List<ScheduleDetail>
                    {
                        new ScheduleDetail { Date = new DateTime(2020, 6, 6), ScheduleId = weDontTalkAnymore.Id, SessionNo = 1 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 13), ScheduleId = weDontTalkAnymore.Id, SessionNo = 2 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 20), ScheduleId = weDontTalkAnymore.Id, SessionNo = 3 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 27), ScheduleId = weDontTalkAnymore.Id, SessionNo = 4 },
                    });
                }

                var lanhLeo = context.Schedules.FirstOrDefault(s => s.Song == "Lạnh lẽo");
                if (lanhLeo != null)
                {
                    context.ScheduleDetails.AddRange(new List<ScheduleDetail>
                    {
                        new ScheduleDetail { Date = new DateTime(2020, 6, 7), ScheduleId = lanhLeo.Id, SessionNo = 1 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 12), ScheduleId = lanhLeo.Id, SessionNo = 2 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 14), ScheduleId = lanhLeo.Id, SessionNo = 3 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 19), ScheduleId = lanhLeo.Id, SessionNo = 4 },
                        new ScheduleDetail { Date = new DateTime(2020, 6, 21), ScheduleId = lanhLeo.Id, SessionNo = 5 },
                    });
                }

                context.SaveChanges();
            }

            if (!context.Registrations.Any())
            {
                var tayTraiChiTrangBuoi2 = context.ScheduleDetails.FirstOrDefault(x => x.Schedule.Song == "Tay trái chỉ trăng" && x.SessionNo == 2);
                var memberTest = context.Users.FirstOrDefault(x => x.UserName == "member.test");
                if (tayTraiChiTrangBuoi2 != null)
                {
                    context.Registrations.Add(new Registration
                    {
                        DateRegistered = DateTime.Now,
                        ScheduleDetailId = tayTraiChiTrangBuoi2.Id,
                        UserId = memberTest.Id
                    });

                    context.SaveChanges();
                }
            }
        }
    }
}
