namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class simbase : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sims", "SimBankId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sims", "SimBankId");
        }
    }
}
