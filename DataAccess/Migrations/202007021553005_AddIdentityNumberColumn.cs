namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdentityNumberColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IdentityNo", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IdentityNo");
        }
    }
}
