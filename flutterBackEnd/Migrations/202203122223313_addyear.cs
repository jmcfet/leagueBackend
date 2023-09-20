namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addyear : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Matches", "year", c => c.Int(nullable: false));
            AddColumn("dbo.BookedDates", "year", c => c.Int(nullable: false));
            DropColumn("dbo.Matches", "spare");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Matches", "spare", c => c.Int(nullable: false));
            DropColumn("dbo.BookedDates", "year");
            DropColumn("dbo.Matches", "year");
        }
    }
}
