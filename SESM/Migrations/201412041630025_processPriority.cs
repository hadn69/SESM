using System.Data.Entity.Migrations;

namespace SESM.Migrations
{
    public partial class processPriority : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "ProcessPriority", c => c.Int(nullable: false, defaultValue:10));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "ProcessPriority");
        }
    }
}
