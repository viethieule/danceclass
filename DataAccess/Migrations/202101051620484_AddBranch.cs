namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBranch : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Branches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Abbreviation = c.String(),
                        Address = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        UpdatedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "RegisteredBranchId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "RegisteredBranchId");
            AddForeignKey("dbo.AspNetUsers", "RegisteredBranchId", "dbo.Branches", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "RegisteredBranchId", "dbo.Branches");
            DropIndex("dbo.AspNetUsers", new[] { "RegisteredBranchId" });
            DropColumn("dbo.AspNetUsers", "RegisteredBranchId");
            DropTable("dbo.Branches");
        }
    }
}
