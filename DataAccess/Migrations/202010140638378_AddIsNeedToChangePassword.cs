namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsNeedToChangePassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsNeedToChangePassword", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsNeedToChangePassword");
        }
    }
}
