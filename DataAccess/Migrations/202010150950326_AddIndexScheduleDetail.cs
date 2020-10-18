namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexScheduleDetail : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Registrations", new[] { "ScheduleDetailId" });
            CreateIndex("dbo.Registrations", "ScheduleDetailId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Registrations", new[] { "ScheduleDetailId" });
            CreateIndex("dbo.Registrations", "ScheduleDetailId");
        }
    }
}
