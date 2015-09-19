namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SESEWCFPortRemoval : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.EntityServers", "ServerExtenderPort");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EntityServers", "ServerExtenderPort", c => c.Int(nullable: false));
        }
    }
}
