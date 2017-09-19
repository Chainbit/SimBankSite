namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notNullNumbers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "SenderNumber", c => c.String(nullable: false, defaultValue:""));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "SenderNumber");
        }
    }
}
