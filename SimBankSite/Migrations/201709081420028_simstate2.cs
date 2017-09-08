namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class simstate2 : DbMigration
    {
        public override void Up()
        {

            CreateTable(
                "dbo.Sims",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        TelNumber = c.String(),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.Sims");
        }
    }
}
