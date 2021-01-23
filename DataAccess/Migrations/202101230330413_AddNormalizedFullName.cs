namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNormalizedFullName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "NormalizedFullName", c => c.String(maxLength: 100));
            AlterColumn("dbo.AspNetUsers", "FullName", c => c.String(maxLength: 100));
            CreateIndex("dbo.AspNetUsers", "FullName");
            CreateIndex("dbo.AspNetUsers", "NormalizedFullName");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AspNetUsers", new[] { "NormalizedFullName" });
            DropIndex("dbo.AspNetUsers", new[] { "FullName" });
            AlterColumn("dbo.AspNetUsers", "FullName", c => c.String());
            DropColumn("dbo.AspNetUsers", "NormalizedFullName");
        }
    }
}
