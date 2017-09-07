namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Decim1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "Money");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "Money", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
