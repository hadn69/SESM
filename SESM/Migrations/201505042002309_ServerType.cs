namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServerType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "ServerType", c => c.Int(nullable: false, defaultValue:10));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "ServerType");
        }
    }
}
