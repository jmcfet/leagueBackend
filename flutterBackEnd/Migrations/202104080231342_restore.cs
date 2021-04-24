namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class restore : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BookedDates", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.BookedDates", new[] { "UserId" });
            RenameColumn(table: "dbo.BookedDates", name: "UserId", newName: "user_Id");
            AlterColumn("dbo.BookedDates", "user_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.BookedDates", "user_Id");
      //      AddForeignKey("dbo.BookedDates", "user_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookedDates", "user_Id", "dbo.AspNetUsers");
            DropIndex("dbo.BookedDates", new[] { "user_Id" });
            AlterColumn("dbo.BookedDates", "user_Id", c => c.String(nullable: false, maxLength: 128));
            RenameColumn(table: "dbo.BookedDates", name: "user_Id", newName: "UserId");
            CreateIndex("dbo.BookedDates", "UserId");
            AddForeignKey("dbo.BookedDates", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
