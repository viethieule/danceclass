namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateScheduleStructure : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Promotions", "Id", "dbo.Classes");
            DropForeignKey("dbo.Classes", "TrainerId", "dbo.Trainers");
            DropForeignKey("dbo.ClassMembers", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.ClassMembers", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Classes", new[] { "TrainerId" });
            DropIndex("dbo.Promotions", new[] { "Id" });
            DropIndex("dbo.ClassMembers", new[] { "ClassId" });
            DropIndex("dbo.ClassMembers", new[] { "UserId" });
            CreateTable(
                "dbo.ScheduleMembers",
                c => new
                    {
                        ScheduleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        DateRegistered = c.DateTime(nullable: false),
                        RemainingSessions = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ScheduleId, t.UserId })
                .ForeignKey("dbo.Schedules", t => t.ScheduleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ScheduleId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Schedules", "Song", c => c.String());
            AddColumn("dbo.Schedules", "OpeningDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Schedules", "StartTime", c => c.Time(nullable: false, precision: 7));
            AddColumn("dbo.Schedules", "Sessions", c => c.Int(nullable: false));
            AddColumn("dbo.Schedules", "SessionsPerWeek", c => c.Int(nullable: false));
            AddColumn("dbo.Schedules", "DaysPerWeek", c => c.String());
            AddColumn("dbo.Schedules", "Branch", c => c.String());
            AddColumn("dbo.Schedules", "TrainerId", c => c.Int(nullable: false));
            CreateIndex("dbo.Schedules", "TrainerId");
            AddForeignKey("dbo.Schedules", "TrainerId", "dbo.Trainers", "Id", cascadeDelete: true);
            DropColumn("dbo.Classes", "Price");
            DropColumn("dbo.Classes", "TrainerId");
            DropColumn("dbo.Classes", "PromotionId");
            DropColumn("dbo.Promotions", "ClassId");
            DropColumn("dbo.Schedules", "Start");
            DropColumn("dbo.Schedules", "End");
            DropTable("dbo.ClassMembers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ClassMembers",
                c => new
                    {
                        ClassId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        DateRegister = c.DateTime(nullable: false),
                        DaysRegistered = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ClassId, t.UserId });
            
            AddColumn("dbo.Schedules", "End", c => c.DateTime(nullable: false));
            AddColumn("dbo.Schedules", "Start", c => c.DateTime(nullable: false));
            AddColumn("dbo.Promotions", "ClassId", c => c.Int(nullable: false));
            AddColumn("dbo.Classes", "PromotionId", c => c.Int(nullable: false));
            AddColumn("dbo.Classes", "TrainerId", c => c.Int(nullable: false));
            AddColumn("dbo.Classes", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropForeignKey("dbo.ScheduleMembers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ScheduleMembers", "ScheduleId", "dbo.Schedules");
            DropForeignKey("dbo.Schedules", "TrainerId", "dbo.Trainers");
            DropIndex("dbo.Schedules", new[] { "TrainerId" });
            DropIndex("dbo.ScheduleMembers", new[] { "UserId" });
            DropIndex("dbo.ScheduleMembers", new[] { "ScheduleId" });
            DropColumn("dbo.Schedules", "TrainerId");
            DropColumn("dbo.Schedules", "Branch");
            DropColumn("dbo.Schedules", "DaysPerWeek");
            DropColumn("dbo.Schedules", "SessionsPerWeek");
            DropColumn("dbo.Schedules", "Sessions");
            DropColumn("dbo.Schedules", "StartTime");
            DropColumn("dbo.Schedules", "OpeningDate");
            DropColumn("dbo.Schedules", "Song");
            DropTable("dbo.ScheduleMembers");
            CreateIndex("dbo.ClassMembers", "UserId");
            CreateIndex("dbo.ClassMembers", "ClassId");
            CreateIndex("dbo.Promotions", "Id");
            CreateIndex("dbo.Classes", "TrainerId");
            AddForeignKey("dbo.ClassMembers", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClassMembers", "ClassId", "dbo.Classes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Classes", "TrainerId", "dbo.Trainers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Promotions", "Id", "dbo.Classes", "Id");
        }
    }
}
