namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMembershipEdition : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packages", "MembershipEdition", c => c.Int(nullable: false));
            AddColumn("dbo.Packages", "MembershipEditedOn", c => c.DateTime());
            AddColumn("dbo.Packages", "MembershipEditedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Packages", "MembershipEditedBy");
            DropColumn("dbo.Packages", "MembershipEditedOn");
            DropColumn("dbo.Packages", "MembershipEdition");
        }
    }
}
