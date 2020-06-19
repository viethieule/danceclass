namespace DataAccess.Migrations
{
    using DataAccess.Entities;
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
            }

            if (!context.Trainers.Any())
            {
                context.Trainers.AddRange(new List<Trainer>
                {
                    new Trainer { Name = "Linh" },
                    new Trainer { Name = "Thảo" }
                });
            }

            context.SaveChanges();

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
                    new Schedule { Song = "Tay trái chỉ trăng", OpeningDate = new DateTime(2020, 6, 16), EndingDate = new DateTime(2020, 6, 25) , StartTime = new TimeSpan(13, 0, 0), Sessions = 4, SessionsPerWeek = 2, DaysPerWeek = "35", Branch = "Q3", ClassId = muaDuongDai.Id, TrainerId = linh.Id },
                    new Schedule { Song = "Một đời đợi người", OpeningDate = new DateTime(2020, 6, 8), EndingDate = new DateTime(2020, 6, 26) , StartTime = new TimeSpan(18, 0, 0), Sessions = 6, SessionsPerWeek = 2, DaysPerWeek = "26", Branch = "Q3", ClassId = muaDuongDai.Id, TrainerId = thao.Id },
                    new Schedule { Song = "We don't talk anymore", OpeningDate = new DateTime(2020, 6, 6), EndingDate = new DateTime(2020, 6, 27) , StartTime = new TimeSpan(18, 0, 0), Sessions = 4, SessionsPerWeek = 1, DaysPerWeek = "7", Branch = "Q3", ClassId = danceCover.Id, TrainerId = linh.Id },
                    new Schedule { Song = "Lạnh lẽo", OpeningDate = new DateTime(2020, 6, 7), EndingDate = new DateTime(2020, 6, 21) , StartTime = new TimeSpan(19, 35, 0), Sessions = 5, SessionsPerWeek = 2, DaysPerWeek = "68", Branch = "PN", ClassId = muaDuongDai.Id, TrainerId = thao.Id },
                });
            }

            context.SaveChanges();
        }
    }
}
