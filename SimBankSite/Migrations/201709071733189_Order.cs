namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Order : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "DateCreated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "CustomerId", c => c.String(nullable: false));
            DropColumn("dbo.Orders", "Customer");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "Customer", c => c.String(nullable: false));
            DropColumn("dbo.Orders", "CustomerId");
            DropColumn("dbo.Orders", "DateCreated");
        }
    }
}
