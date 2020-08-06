namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateNullableForScheduleProps : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Schedules", "EndingDate", c => c.DateTime());
            AlterColumn("dbo.Schedules", "Sessions", c => c.Int());
            AlterColumn("dbo.Schedules", "SessionsPerWeek", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Schedules", "SessionsPerWeek", c => c.Int(nullable: false));
            AlterColumn("dbo.Schedules", "Sessions", c => c.Int(nullable: false));
            AlterColumn("dbo.Schedules", "EndingDate", c => c.DateTime(nullable: false));
        }
    }
}
