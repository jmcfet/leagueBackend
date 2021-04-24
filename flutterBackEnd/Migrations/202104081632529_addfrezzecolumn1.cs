namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfrezzecolumn1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "bFreezeDB", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "bFreezeDB");
        }
    }
}
