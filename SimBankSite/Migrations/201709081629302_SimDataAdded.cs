namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SimDataAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActiveSims",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SimBankId = c.String(),
                        State = c.Int(nullable: false),
                        TelNumber = c.String(),
                        UsedServices = c.String(),
                    })
                .PrimaryKey(t => t.Id);            
        }
        
        public override void Down()
        {          
            DropTable("dbo.ActiveSims");
        }
    }
}
