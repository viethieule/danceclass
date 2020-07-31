namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRegistrationStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrations", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrations", "Status");
        }
    }
}
