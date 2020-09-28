namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefactorPackageStructure : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "Package_Id", "dbo.Packages");
            DropForeignKey("dbo.MemberPackages", "PackageId", "dbo.Packages");
            DropForeignKey("dbo.MemberPackages", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetUsers", new[] { "Package_Id" });
            DropIndex("dbo.MemberPackages", new[] { "UserId" });
            DropIndex("dbo.MemberPackages", new[] { "PackageId" });
            CreateTable(
                "dbo.Memberships",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RemainingSessions = c.Int(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.DefaultPackages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NumberOfSessions = c.Int(nullable: false),
                        Price = c.Double(nullable: false),
                        Months = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Packages", "UserId", c => c.Int(nullable: false));
            AddColumn("dbo.Packages", "RemainingSessions", c => c.Int(nullable: false));
            AddColumn("dbo.Packages", "ExpiryDate", c => c.DateTime());
            AddColumn("dbo.Packages", "DefaultPackageId", c => c.Int());
            AddColumn("dbo.Packages", "IsActive", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Packages", "UserId");
            CreateIndex("dbo.Packages", "DefaultPackageId");
            AddForeignKey("dbo.Packages", "DefaultPackageId", "dbo.DefaultPackages", "Id");
            AddForeignKey("dbo.Packages", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            DropColumn("dbo.AspNetUsers", "Package_Id");
            DropColumn("dbo.Packages", "IsDefault");
            DropTable("dbo.MemberPackages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MemberPackages",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        PackageId = c.Int(nullable: false),
                        RemainingSessions = c.Int(nullable: false),
                        ExpiryDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.PackageId });
            
            AddColumn("dbo.Packages", "IsDefault", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "Package_Id", c => c.Int());
            DropForeignKey("dbo.Packages", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Packages", "DefaultPackageId", "dbo.DefaultPackages");
            DropForeignKey("dbo.Memberships", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Packages", new[] { "DefaultPackageId" });
            DropIndex("dbo.Packages", new[] { "UserId" });
            DropIndex("dbo.Memberships", new[] { "UserId" });
            DropColumn("dbo.Packages", "IsActive");
            DropColumn("dbo.Packages", "DefaultPackageId");
            DropColumn("dbo.Packages", "ExpiryDate");
            DropColumn("dbo.Packages", "RemainingSessions");
            DropColumn("dbo.Packages", "UserId");
            DropTable("dbo.DefaultPackages");
            DropTable("dbo.Memberships");
            CreateIndex("dbo.MemberPackages", "PackageId");
            CreateIndex("dbo.MemberPackages", "UserId");
            CreateIndex("dbo.AspNetUsers", "Package_Id");
            AddForeignKey("dbo.MemberPackages", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MemberPackages", "PackageId", "dbo.Packages", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AspNetUsers", "Package_Id", "dbo.Packages", "Id");
        }
    }
}
