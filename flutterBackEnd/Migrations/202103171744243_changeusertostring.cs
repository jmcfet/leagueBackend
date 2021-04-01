namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeusertostring : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "Match_id", "dbo.Matches");
            DropIndex("dbo.AspNetUsers", new[] { "Match_id" });
            DropColumn("dbo.AspNetUsers", "Match_id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Match_id", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "Match_id");
            AddForeignKey("dbo.AspNetUsers", "Match_id", "dbo.Matches", "id");
        }
    }
}
