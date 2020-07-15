namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRegistration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ScheduleMembers", "ScheduleId", "dbo.Schedules");
            DropForeignKey("dbo.ScheduleMembers", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.ScheduleMembers", new[] { "ScheduleId" });
            DropIndex("dbo.ScheduleMembers", new[] { "UserId" });
            CreateTable(
                "dbo.Registrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        DateRegistered = c.DateTime(nullable: false),
                        DateAttending = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Schedules", t => t.ScheduleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ScheduleId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.MemberPackages", "IsActive", c => c.Boolean(nullable: false));
            DropTable("dbo.ScheduleMembers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ScheduleMembers",
                c => new
                    {
                        ScheduleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        DateRegistered = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.ScheduleId, t.UserId });
            
            DropForeignKey("dbo.Registrations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Registrations", "ScheduleId", "dbo.Schedules");
            DropIndex("dbo.Registrations", new[] { "UserId" });
            DropIndex("dbo.Registrations", new[] { "ScheduleId" });
            DropColumn("dbo.MemberPackages", "IsActive");
            DropTable("dbo.Registrations");
            CreateIndex("dbo.ScheduleMembers", "UserId");
            CreateIndex("dbo.ScheduleMembers", "ScheduleId");
            AddForeignKey("dbo.ScheduleMembers", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ScheduleMembers", "ScheduleId", "dbo.Schedules", "Id", cascadeDelete: true);
        }
    }
}
