namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class numTimesCaptainisint : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BookedDates", "numTimesCaptain", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.BookedDates", "numTimesCaptain", c => c.Boolean(nullable: false));
        }
    }
}
