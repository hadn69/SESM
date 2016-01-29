namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServerStartupType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "ServerStartup", c => c.Int(nullable: false, defaultValue: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "ServerStartup");
        }
    }
}
