namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServerRoleInstanceNameRemoval : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.EntityInstanceServerRoles", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EntityInstanceServerRoles", "Name", c => c.String());
        }
    }
}
