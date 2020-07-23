namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeToICollection : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Package_Id", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "Package_Id");
            AddForeignKey("dbo.AspNetUsers", "Package_Id", "dbo.Packages", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "Package_Id", "dbo.Packages");
            DropIndex("dbo.AspNetUsers", new[] { "Package_Id" });
            DropColumn("dbo.AspNetUsers", "Package_Id");
        }
    }
}
