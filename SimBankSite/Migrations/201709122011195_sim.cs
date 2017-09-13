namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sim : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Sims", "State", c => c.Int(nullable: false));
            DropColumn("dbo.Sims", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Sims", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Sims", "State", c => c.Int());
        }
    }
}
