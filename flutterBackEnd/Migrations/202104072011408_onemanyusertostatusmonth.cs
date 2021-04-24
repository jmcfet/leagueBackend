namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class onemanyusertostatusmonth : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BookedDates", name: "user_Id", newName: "ApplicationUser_Id");
            RenameIndex(table: "dbo.BookedDates", name: "IX_user_Id", newName: "IX_ApplicationUser_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.BookedDates", name: "IX_ApplicationUser_Id", newName: "IX_user_Id");
            RenameColumn(table: "dbo.BookedDates", name: "ApplicationUser_Id", newName: "user_Id");
        }
    }
}
