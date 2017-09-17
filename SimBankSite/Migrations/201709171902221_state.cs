namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class state : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Transactions", new[] { "AppUser_Id" });
            AddColumn("dbo.Transactions", "State", c => c.Int(nullable: false));
            AlterColumn("dbo.Transactions", "AppUser_Id", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Transactions", "AppUser_Id");
            AddForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Transactions", new[] { "AppUser_Id" });
            AlterColumn("dbo.Transactions", "AppUser_Id", c => c.String(maxLength: 128));
            DropColumn("dbo.Transactions", "State");
            CreateIndex("dbo.Transactions", "AppUser_Id");
            AddForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
