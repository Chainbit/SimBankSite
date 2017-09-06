namespace SimBankSite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserMoney : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "UserCredentials_Id", "dbo.UserCredentials");
            DropIndex("dbo.AspNetUsers", new[] { "UserCredentials_Id" });
            AddColumn("dbo.AspNetUsers", "Money", c => c.Double(nullable: false));
            DropColumn("dbo.AspNetUsers", "UserCredentials_Id");
            DropTable("dbo.UserCredentials");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserCredentials",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Money = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "UserCredentials_Id", c => c.String(maxLength: 128));
            DropColumn("dbo.AspNetUsers", "Money");
            CreateIndex("dbo.AspNetUsers", "UserCredentials_Id");
            AddForeignKey("dbo.AspNetUsers", "UserCredentials_Id", "dbo.UserCredentials", "Id");
        }
    }
}
