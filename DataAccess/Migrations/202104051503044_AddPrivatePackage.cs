namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPrivatePackage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "IsPrivate", c => c.Boolean(nullable: false));
            AddColumn("dbo.DefaultPackages", "IsPrivate", c => c.Boolean(nullable: false));
            AddColumn("dbo.Schedules", "IsPrivate", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Schedules", "IsPrivate");
            DropColumn("dbo.DefaultPackages", "IsPrivate");
            DropColumn("dbo.Packages", "IsPrivate");
        }
    }
}
