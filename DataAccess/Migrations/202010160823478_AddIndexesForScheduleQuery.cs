namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexesForScheduleQuery : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Schedules", new[] { "ClassId" });
            DropIndex("dbo.Schedules", new[] { "TrainerId" });
            DropIndex("dbo.ScheduleDetails", new[] { "ScheduleId" });
            DropIndex("dbo.Registrations", new[] { "UserId" });
            CreateIndex("dbo.Schedules", "ClassId");
            CreateIndex("dbo.Schedules", "TrainerId");
            CreateIndex("dbo.ScheduleDetails", "Date");
            CreateIndex("dbo.ScheduleDetails", "ScheduleId");
            CreateIndex("dbo.Registrations", "UserId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Registrations", new[] { "UserId" });
            DropIndex("dbo.ScheduleDetails", new[] { "ScheduleId" });
            DropIndex("dbo.ScheduleDetails", new[] { "Date" });
            DropIndex("dbo.Schedules", new[] { "TrainerId" });
            DropIndex("dbo.Schedules", new[] { "ClassId" });
            CreateIndex("dbo.Registrations", "UserId");
            CreateIndex("dbo.ScheduleDetails", "ScheduleId");
            CreateIndex("dbo.Schedules", "TrainerId");
            CreateIndex("dbo.Schedules", "ClassId");
        }
    }
}
