namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SenderNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "SenderNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "SenderNumber");
        }
    }
}
