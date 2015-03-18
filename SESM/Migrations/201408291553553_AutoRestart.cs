using System.Data.Entity.Migrations;

namespace SESM.Migrations
{
    public partial class AutoRestart : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "IsAutoRestartEnabled", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.EntityServers", "AutoRestartCron", c => c.String(defaultValue: "0 0 0 * * ?"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "AutoRestartCron");
            DropColumn("dbo.EntityServers", "IsAutoRestartEnabled");
        }
    }
}
