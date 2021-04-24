namespace flutterBackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adduserid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "memberName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "memberName");
        }
    }
}
