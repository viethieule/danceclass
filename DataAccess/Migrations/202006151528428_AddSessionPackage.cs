namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSessionPackage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Packages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NumberOfSessions = c.Int(nullable: false),
                        Price = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "Birthdate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "PackageId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "PackageId");
            AddForeignKey("dbo.AspNetUsers", "PackageId", "dbo.Packages", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "PackageId", "dbo.Packages");
            DropIndex("dbo.AspNetUsers", new[] { "PackageId" });
            DropColumn("dbo.AspNetUsers", "PackageId");
            DropColumn("dbo.AspNetUsers", "Birthdate");
            DropTable("dbo.Packages");
        }
    }
}
