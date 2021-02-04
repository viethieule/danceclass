namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBranchInPackage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "RegisteredBranchId", c => c.Int());
            CreateIndex("dbo.Packages", "RegisteredBranchId");
            AddForeignKey("dbo.Packages", "RegisteredBranchId", "dbo.Branches", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Packages", "RegisteredBranchId", "dbo.Branches");
            DropIndex("dbo.Packages", new[] { "RegisteredBranchId" });
            DropColumn("dbo.Packages", "RegisteredBranchId");
        }
    }
}
