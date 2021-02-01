namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEntityIdChangeLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FieldChangeLogs", "EntityId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FieldChangeLogs", "EntityId");
        }
    }
}
