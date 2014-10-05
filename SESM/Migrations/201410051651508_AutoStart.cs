namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
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
