namespace SESM.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class SESEAutoSave : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EntityServers", "AutoSaveInMinutes", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EntityServers", "AutoSaveInMinutes");
        }
    }
}
