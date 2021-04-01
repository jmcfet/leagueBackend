namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeusertostring1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Matches", "captain_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Matches", new[] { "captain_Id" });
            AddColumn("dbo.Matches", "captain", c => c.String());
            AddColumn("dbo.Matches", "players", c => c.String());
            DropColumn("dbo.Matches", "captain_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Matches", "captain_Id", c => c.String(maxLength: 128));
            DropColumn("dbo.Matches", "players");
            DropColumn("dbo.Matches", "captain");
            CreateIndex("dbo.Matches", "captain_Id");
            AddForeignKey("dbo.Matches", "captain_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
