namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class match : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Matches",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        month = c.Int(nullable: false),
                        day = c.Int(nullable: false),
                        skillLevel = c.Int(nullable: false),
                        spare = c.Int(nullable: false),
                        captain_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.AspNetUsers", t => t.captain_Id)
                .Index(t => t.captain_Id);
            
            AddColumn("dbo.AspNetUsers", "Match_id", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "Match_id");
            AddForeignKey("dbo.AspNetUsers", "Match_id", "dbo.Matches", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "Match_id", "dbo.Matches");
            DropForeignKey("dbo.Matches", "captain_Id", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetUsers", new[] { "Match_id" });
            DropIndex("dbo.Matches", new[] { "captain_Id" });
            DropColumn("dbo.AspNetUsers", "Match_id");
            DropTable("dbo.Matches");
        }
    }
}
