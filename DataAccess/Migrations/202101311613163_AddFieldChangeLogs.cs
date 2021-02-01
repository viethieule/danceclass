namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldChangeLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FieldChangeLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Entity = c.String(),
                        ChangeLog = c.String(),
                        Action = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        UpdatedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Memberships", "LatestAction", c => c.String());
            AddColumn("dbo.Packages", "LatestAction", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Packages", "LatestAction");
            DropColumn("dbo.Memberships", "LatestAction");
            DropTable("dbo.FieldChangeLogs");
        }
    }
}
