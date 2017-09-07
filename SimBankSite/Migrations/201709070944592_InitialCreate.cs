namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            /*CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Customer = c.String(nullable: false),
                        TelNumber = c.String(),
                        Status = c.String(),
                        Message = c.String(),
                        Service_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.Service_Id)
                .Index(t => t.Service_Id);
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);*/
            
        }
        
        public override void Down()
        {
            /*DropForeignKey("dbo.Orders", "Service_Id", "dbo.Services");
            DropIndex("dbo.Orders", new[] { "Service_Id" });
            DropTable("dbo.Services");
            DropTable("dbo.Orders");*/
        }
    }
}
