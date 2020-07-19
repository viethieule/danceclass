namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddScheduleDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Registrations", "ScheduleId", "dbo.Schedules");
            DropIndex("dbo.Registrations", new[] { "ScheduleId" });
            CreateTable(
                "dbo.ScheduleDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        ScheduleId = c.Int(nullable: false),
                        SessionNo = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Schedules", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => t.ScheduleId);
            
            AddColumn("dbo.Registrations", "ScheduleDetailId", c => c.Int(nullable: false));
            CreateIndex("dbo.Registrations", "ScheduleDetailId");
            AddForeignKey("dbo.Registrations", "ScheduleDetailId", "dbo.ScheduleDetails", "Id", cascadeDelete: true);
            DropColumn("dbo.Registrations", "ScheduleId");
            DropColumn("dbo.Registrations", "DateAttending");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Registrations", "DateAttending", c => c.DateTime(nullable: false));
            AddColumn("dbo.Registrations", "ScheduleId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ScheduleDetails", "ScheduleId", "dbo.Schedules");
            DropForeignKey("dbo.Registrations", "ScheduleDetailId", "dbo.ScheduleDetails");
            DropIndex("dbo.ScheduleDetails", new[] { "ScheduleId" });
            DropIndex("dbo.Registrations", new[] { "ScheduleDetailId" });
            DropColumn("dbo.Registrations", "ScheduleDetailId");
            DropTable("dbo.ScheduleDetails");
            CreateIndex("dbo.Registrations", "ScheduleId");
            AddForeignKey("dbo.Registrations", "ScheduleId", "dbo.Schedules", "Id", cascadeDelete: true);
        }
    }
}
