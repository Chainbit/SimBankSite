namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserId_ : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Transactions", new[] { "AppUser_Id" });
            RenameColumn(table: "dbo.Transactions", name: "AppUser_Id", newName: "UserId");
            AlterColumn("dbo.Transactions", "UserId", c => c.String(nullable: true, maxLength: 128));
            CreateIndex("dbo.Transactions", "UserId");
            AddForeignKey("dbo.Transactions", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Transactions", new[] { "UserId" });
            AlterColumn("dbo.Transactions", "UserId", c => c.String(maxLength: 128));
            RenameColumn(table: "dbo.Transactions", name: "UserId", newName: "AppUser_Id");
            CreateIndex("dbo.Transactions", "AppUser_Id");
            AddForeignKey("dbo.Transactions", "AppUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
