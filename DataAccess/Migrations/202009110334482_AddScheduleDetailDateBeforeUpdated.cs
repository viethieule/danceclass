namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddScheduleDetailDateBeforeUpdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleDetails", "DateBeforeUpdated", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScheduleDetails", "DateBeforeUpdated");
        }
    }
}
