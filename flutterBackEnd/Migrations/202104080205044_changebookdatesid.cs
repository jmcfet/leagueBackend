namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changebookdatesid : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BookedDates", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.BookedDates", new[] { "ApplicationUser_Id" });
            RenameColumn(table: "dbo.BookedDates", name: "ApplicationUser_Id", newName: "UserId");
            AlterColumn("dbo.BookedDates", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.BookedDates", "UserId");
            AddForeignKey("dbo.BookedDates", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookedDates", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.BookedDates", new[] { "UserId" });
            AlterColumn("dbo.BookedDates", "UserId", c => c.String(maxLength: 128));
            RenameColumn(table: "dbo.BookedDates", name: "UserId", newName: "ApplicationUser_Id");
            CreateIndex("dbo.BookedDates", "ApplicationUser_Id");
            AddForeignKey("dbo.BookedDates", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
