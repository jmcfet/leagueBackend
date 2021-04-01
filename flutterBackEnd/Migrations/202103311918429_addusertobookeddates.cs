namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addusertobookeddates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookedDates", "user_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.BookedDates", "user_Id");
            AddForeignKey("dbo.BookedDates", "user_Id", "dbo.AspNetUsers", "Id");
            DropColumn("dbo.BookedDates", "Name");
            DropColumn("dbo.BookedDates", "level");
            DropColumn("dbo.BookedDates", "isCaptain");
            DropColumn("dbo.BookedDates", "numTimesCaptain");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BookedDates", "numTimesCaptain", c => c.Int(nullable: false));
            AddColumn("dbo.BookedDates", "isCaptain", c => c.Boolean(nullable: false));
            AddColumn("dbo.BookedDates", "level", c => c.Int(nullable: false));
            AddColumn("dbo.BookedDates", "Name", c => c.String());
            DropForeignKey("dbo.BookedDates", "user_Id", "dbo.AspNetUsers");
            DropIndex("dbo.BookedDates", new[] { "user_Id" });
            DropColumn("dbo.BookedDates", "user_Id");
        }
    }
}
