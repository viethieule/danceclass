namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMemberPackageRelation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "PackageId", "dbo.Packages");
            DropIndex("dbo.AspNetUsers", new[] { "PackageId" });
            CreateTable(
                "dbo.MemberPackages",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        PackageId = c.Int(nullable: false),
                        RemainingSessions = c.Int(nullable: false),
                        ExpiryDate = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.UserId, t.PackageId })
                .ForeignKey("dbo.Packages", t => t.PackageId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.PackageId);
            
            AddColumn("dbo.Packages", "Months", c => c.Int(nullable: false));
            AddColumn("dbo.Packages", "IsDefault", c => c.Boolean(nullable: false));
            AlterColumn("dbo.AspNetUsers", "Birthdate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Packages", "Month");
            DropColumn("dbo.ScheduleMembers", "RemainingSessions");
            DropColumn("dbo.AspNetUsers", "PackageId");
            DropColumn("dbo.AspNetUsers", "IdentityNo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "IdentityNo", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "PackageId", c => c.Int());
            AddColumn("dbo.ScheduleMembers", "RemainingSessions", c => c.Int(nullable: false));
            AddColumn("dbo.Packages", "Month", c => c.Int(nullable: false));
            DropForeignKey("dbo.MemberPackages", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MemberPackages", "PackageId", "dbo.Packages");
            DropIndex("dbo.MemberPackages", new[] { "PackageId" });
            DropIndex("dbo.MemberPackages", new[] { "UserId" });
            AlterColumn("dbo.AspNetUsers", "Birthdate", c => c.DateTime());
            DropColumn("dbo.Packages", "IsDefault");
            DropColumn("dbo.Packages", "Months");
            DropTable("dbo.MemberPackages");
            CreateIndex("dbo.AspNetUsers", "PackageId");
            AddForeignKey("dbo.AspNetUsers", "PackageId", "dbo.Packages", "Id");
        }
    }
}
