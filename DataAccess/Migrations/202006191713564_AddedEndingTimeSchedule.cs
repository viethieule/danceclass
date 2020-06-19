namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedEndingTimeSchedule : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Schedules", "EndingDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Schedules", "EndingDate");
        }
    }
}
