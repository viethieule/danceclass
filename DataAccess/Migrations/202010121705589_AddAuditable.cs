namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuditable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Classes", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Classes", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Classes", "CreatedBy", c => c.String());
            AddColumn("dbo.Classes", "UpdatedBy", c => c.String());
            AddColumn("dbo.Schedules", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Schedules", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Schedules", "CreatedBy", c => c.String());
            AddColumn("dbo.Schedules", "UpdatedBy", c => c.String());
            AddColumn("dbo.ScheduleDetails", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.ScheduleDetails", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.ScheduleDetails", "CreatedBy", c => c.String());
            AddColumn("dbo.ScheduleDetails", "UpdatedBy", c => c.String());
            AddColumn("dbo.Registrations", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Registrations", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Registrations", "CreatedBy", c => c.String());
            AddColumn("dbo.Registrations", "UpdatedBy", c => c.String());
            AddColumn("dbo.AspNetUsers", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "CreatedBy", c => c.String());
            AddColumn("dbo.AspNetUsers", "UpdatedBy", c => c.String());
            AddColumn("dbo.Memberships", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Memberships", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Memberships", "CreatedBy", c => c.String());
            AddColumn("dbo.Memberships", "UpdatedBy", c => c.String());
            AddColumn("dbo.Packages", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Packages", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Packages", "CreatedBy", c => c.String());
            AddColumn("dbo.Packages", "UpdatedBy", c => c.String());
            AddColumn("dbo.DefaultPackages", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.DefaultPackages", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.DefaultPackages", "CreatedBy", c => c.String());
            AddColumn("dbo.DefaultPackages", "UpdatedBy", c => c.String());
            AddColumn("dbo.Trainers", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Trainers", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Trainers", "CreatedBy", c => c.String());
            AddColumn("dbo.Trainers", "UpdatedBy", c => c.String());
            AddColumn("dbo.Promotions", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Promotions", "UpdatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Promotions", "CreatedBy", c => c.String());
            AddColumn("dbo.Promotions", "UpdatedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promotions", "UpdatedBy");
            DropColumn("dbo.Promotions", "CreatedBy");
            DropColumn("dbo.Promotions", "UpdatedDate");
            DropColumn("dbo.Promotions", "CreatedDate");
            DropColumn("dbo.Trainers", "UpdatedBy");
            DropColumn("dbo.Trainers", "CreatedBy");
            DropColumn("dbo.Trainers", "UpdatedDate");
            DropColumn("dbo.Trainers", "CreatedDate");
            DropColumn("dbo.DefaultPackages", "UpdatedBy");
            DropColumn("dbo.DefaultPackages", "CreatedBy");
            DropColumn("dbo.DefaultPackages", "UpdatedDate");
            DropColumn("dbo.DefaultPackages", "CreatedDate");
            DropColumn("dbo.Packages", "UpdatedBy");
            DropColumn("dbo.Packages", "CreatedBy");
            DropColumn("dbo.Packages", "UpdatedDate");
            DropColumn("dbo.Packages", "CreatedDate");
            DropColumn("dbo.Memberships", "UpdatedBy");
            DropColumn("dbo.Memberships", "CreatedBy");
            DropColumn("dbo.Memberships", "UpdatedDate");
            DropColumn("dbo.Memberships", "CreatedDate");
            DropColumn("dbo.AspNetUsers", "UpdatedBy");
            DropColumn("dbo.AspNetUsers", "CreatedBy");
            DropColumn("dbo.AspNetUsers", "UpdatedDate");
            DropColumn("dbo.AspNetUsers", "CreatedDate");
            DropColumn("dbo.Registrations", "UpdatedBy");
            DropColumn("dbo.Registrations", "CreatedBy");
            DropColumn("dbo.Registrations", "UpdatedDate");
            DropColumn("dbo.Registrations", "CreatedDate");
            DropColumn("dbo.ScheduleDetails", "UpdatedBy");
            DropColumn("dbo.ScheduleDetails", "CreatedBy");
            DropColumn("dbo.ScheduleDetails", "UpdatedDate");
            DropColumn("dbo.ScheduleDetails", "CreatedDate");
            DropColumn("dbo.Schedules", "UpdatedBy");
            DropColumn("dbo.Schedules", "CreatedBy");
            DropColumn("dbo.Schedules", "UpdatedDate");
            DropColumn("dbo.Schedules", "CreatedDate");
            DropColumn("dbo.Classes", "UpdatedBy");
            DropColumn("dbo.Classes", "CreatedBy");
            DropColumn("dbo.Classes", "UpdatedDate");
            DropColumn("dbo.Classes", "CreatedDate");
        }
    }
}
