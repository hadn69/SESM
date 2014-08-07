namespace SESM.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class backups : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "IsLvl1BackupEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.EntityServers", "IsLvl2BackupEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.EntityServers", "IsLvl3BackupEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "IsLvl3BackupEnabled");
            DropColumn("dbo.EntityServers", "IsLvl2BackupEnabled");
            DropColumn("dbo.EntityServers", "IsLvl1BackupEnabled");
        }
    }
}
