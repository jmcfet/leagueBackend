namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsCaptain : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookedDates", "isCaptain", c => c.Boolean(nullable: false));
            AddColumn("dbo.BookedDates", "numTimesCaptain", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BookedDates", "numTimesCaptain");
            DropColumn("dbo.BookedDates", "isCaptain");
        }
    }
}
