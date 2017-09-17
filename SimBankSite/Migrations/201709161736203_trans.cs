namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class trans : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Transactions", new[] { "AppUser_Id" });
            RenameColumn(table: "dbo.Transactions", name: "AppUser_Id", newName: "UserID");
            AlterColumn("dbo.Transactions", "UserID", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Transactions", "UserID");
            AddForeignKey("dbo.Transactions", "UserID", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "UserID", "dbo.AspNetUsers");
            DropIndex("dbo.Transactions", new[] { "UserID" });
            AlterColumn("dbo.Transactions", "UserID", c => c.String(maxLength: 128));
            RenameColumn(table: "dbo.Transactions", name: "UserID", newName: "AppUser_Id");
            CreateIndex("dbo.Transactions", "AppUser_Id");
            AddForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
