namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatePackageTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FullName", c => c.String());
            AddColumn("dbo.Packages", "Month", c => c.Int(nullable: false));
            AlterColumn("dbo.AspNetUsers", "Birthdate", c => c.DateTime());
            DropColumn("dbo.AspNetUsers", "FirstName");
            DropColumn("dbo.AspNetUsers", "LastName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "LastName", c => c.String());
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String());
            AlterColumn("dbo.AspNetUsers", "Birthdate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Packages", "Month");
            DropColumn("dbo.AspNetUsers", "FullName");
        }
    }
}
