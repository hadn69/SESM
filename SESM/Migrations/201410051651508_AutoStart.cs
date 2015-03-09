using System.Data.Entity.Migrations;

namespace SESM.Migrations
{
    public partial class AutoStart : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "IsAutoStartEnabled", c => c.Boolean(nullable: false,defaultValue:false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "IsAutoStartEnabled");
        }
    }
}
