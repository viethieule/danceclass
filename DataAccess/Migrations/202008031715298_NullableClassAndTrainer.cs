namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableClassAndTrainer : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Schedules", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.Schedules", "TrainerId", "dbo.Trainers");
            DropIndex("dbo.Schedules", new[] { "ClassId" });
            DropIndex("dbo.Schedules", new[] { "TrainerId" });
            AlterColumn("dbo.Schedules", "ClassId", c => c.Int());
            AlterColumn("dbo.Schedules", "TrainerId", c => c.Int());
            CreateIndex("dbo.Schedules", "ClassId");
            CreateIndex("dbo.Schedules", "TrainerId");
            AddForeignKey("dbo.Schedules", "ClassId", "dbo.Classes", "Id");
            AddForeignKey("dbo.Schedules", "TrainerId", "dbo.Trainers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Schedules", "TrainerId", "dbo.Trainers");
            DropForeignKey("dbo.Schedules", "ClassId", "dbo.Classes");
            DropIndex("dbo.Schedules", new[] { "TrainerId" });
            DropIndex("dbo.Schedules", new[] { "ClassId" });
            AlterColumn("dbo.Schedules", "TrainerId", c => c.Int(nullable: false));
            AlterColumn("dbo.Schedules", "ClassId", c => c.Int(nullable: false));
            CreateIndex("dbo.Schedules", "TrainerId");
            CreateIndex("dbo.Schedules", "ClassId");
            AddForeignKey("dbo.Schedules", "TrainerId", "dbo.Trainers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Schedules", "ClassId", "dbo.Classes", "Id", cascadeDelete: true);
        }
    }
}
