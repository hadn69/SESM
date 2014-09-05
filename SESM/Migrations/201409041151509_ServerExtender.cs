namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServerExtender : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "UseServerExtender", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.EntityServers", "ServerExtenderPort", c => c.Int(nullable: false, defaultValue: 26016));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "ServerExtenderPort");
            DropColumn("dbo.EntityServers", "UseServerExtender");
        }
    }
}
