namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addlevel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookedDates", "level", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BookedDates", "level");
        }
    }
}
