namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addmanytomanymatchtouser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationUserMatches",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Match_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Match_id })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Matches", t => t.Match_id, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Match_id);
            
            DropColumn("dbo.Matches", "players");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Matches", "players", c => c.String());
            DropForeignKey("dbo.ApplicationUserMatches", "Match_id", "dbo.Matches");
            DropForeignKey("dbo.ApplicationUserMatches", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserMatches", new[] { "Match_id" });
            DropIndex("dbo.ApplicationUserMatches", new[] { "ApplicationUser_Id" });
            DropTable("dbo.ApplicationUserMatches");
        }
    }
}
