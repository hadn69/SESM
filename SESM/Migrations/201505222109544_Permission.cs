namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Permission : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EntityUserEntityServers", "EntityUser_Id", "dbo.EntityUsers");
            DropForeignKey("dbo.EntityUserEntityServers", "EntityServer_Id", "dbo.EntityServers");
            DropForeignKey("dbo.EntityUserEntityServer1", "EntityUser_Id", "dbo.EntityUsers");
            DropForeignKey("dbo.EntityUserEntityServer1", "EntityServer_Id", "dbo.EntityServers");
            DropForeignKey("dbo.EntityUserEntityServer2", "EntityUser_Id", "dbo.EntityUsers");
            DropForeignKey("dbo.EntityUserEntityServer2", "EntityServer_Id", "dbo.EntityServers");
            DropIndex("dbo.EntityUserEntityServers", new[] { "EntityUser_Id" });
            DropIndex("dbo.EntityUserEntityServers", new[] { "EntityServer_Id" });
            DropIndex("dbo.EntityUserEntityServer1", new[] { "EntityUser_Id" });
            DropIndex("dbo.EntityUserEntityServer1", new[] { "EntityServer_Id" });
            DropIndex("dbo.EntityUserEntityServer2", new[] { "EntityUser_Id" });
            DropIndex("dbo.EntityUserEntityServer2", new[] { "EntityServer_Id" });
            CreateTable(
                "dbo.EntityInstanceServerRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ServerRole_Id = c.Int(nullable: false),
                        Server_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityServerRoles", t => t.ServerRole_Id, cascadeDelete: true)
                .ForeignKey("dbo.EntityServers", t => t.Server_Id, cascadeDelete: true)
                .Index(t => t.ServerRole_Id)
                .Index(t => t.Server_Id);
            
            CreateTable(
                "dbo.EntityHostRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PermissionsSerialized = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EntityServerRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PermissionsSerialized = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EntityUserEntityHostRoles",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityHostRole_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityHostRole_Id })
                .ForeignKey("dbo.EntityUsers", t => t.EntityUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.EntityHostRoles", t => t.EntityHostRole_Id, cascadeDelete: true)
                .Index(t => t.EntityUser_Id)
                .Index(t => t.EntityHostRole_Id);
            
            CreateTable(
                "dbo.EntityUserEntityInstanceServerRoles",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityInstanceServerRole_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityInstanceServerRole_Id })
                .ForeignKey("dbo.EntityUsers", t => t.EntityUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.EntityInstanceServerRoles", t => t.EntityInstanceServerRole_Id, cascadeDelete: true)
                .Index(t => t.EntityUser_Id)
                .Index(t => t.EntityInstanceServerRole_Id);
            
            DropTable("dbo.EntityUserEntityServers");
            DropTable("dbo.EntityUserEntityServer1");
            DropTable("dbo.EntityUserEntityServer2");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.EntityUserEntityServer2",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityServer_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityServer_Id });
            
            CreateTable(
                "dbo.EntityUserEntityServer1",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityServer_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityServer_Id });
            
            CreateTable(
                "dbo.EntityUserEntityServers",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityServer_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityServer_Id });
            
            DropForeignKey("dbo.EntityInstanceServerRoles", "Server_Id", "dbo.EntityServers");
            DropForeignKey("dbo.EntityInstanceServerRoles", "ServerRole_Id", "dbo.EntityServerRoles");
            DropForeignKey("dbo.EntityUserEntityInstanceServerRoles", "EntityInstanceServerRole_Id", "dbo.EntityInstanceServerRoles");
            DropForeignKey("dbo.EntityUserEntityInstanceServerRoles", "EntityUser_Id", "dbo.EntityUsers");
            DropForeignKey("dbo.EntityUserEntityHostRoles", "EntityHostRole_Id", "dbo.EntityHostRoles");
            DropForeignKey("dbo.EntityUserEntityHostRoles", "EntityUser_Id", "dbo.EntityUsers");
            DropIndex("dbo.EntityUserEntityInstanceServerRoles", new[] { "EntityInstanceServerRole_Id" });
            DropIndex("dbo.EntityUserEntityInstanceServerRoles", new[] { "EntityUser_Id" });
            DropIndex("dbo.EntityUserEntityHostRoles", new[] { "EntityHostRole_Id" });
            DropIndex("dbo.EntityUserEntityHostRoles", new[] { "EntityUser_Id" });
            DropIndex("dbo.EntityInstanceServerRoles", new[] { "Server_Id" });
            DropIndex("dbo.EntityInstanceServerRoles", new[] { "ServerRole_Id" });
            DropTable("dbo.EntityUserEntityInstanceServerRoles");
            DropTable("dbo.EntityUserEntityHostRoles");
            DropTable("dbo.EntityServerRoles");
            DropTable("dbo.EntityHostRoles");
            DropTable("dbo.EntityInstanceServerRoles");
            CreateIndex("dbo.EntityUserEntityServer2", "EntityServer_Id");
            CreateIndex("dbo.EntityUserEntityServer2", "EntityUser_Id");
            CreateIndex("dbo.EntityUserEntityServer1", "EntityServer_Id");
            CreateIndex("dbo.EntityUserEntityServer1", "EntityUser_Id");
            CreateIndex("dbo.EntityUserEntityServers", "EntityServer_Id");
            CreateIndex("dbo.EntityUserEntityServers", "EntityUser_Id");
            AddForeignKey("dbo.EntityUserEntityServer2", "EntityServer_Id", "dbo.EntityServers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EntityUserEntityServer2", "EntityUser_Id", "dbo.EntityUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EntityUserEntityServer1", "EntityServer_Id", "dbo.EntityServers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EntityUserEntityServer1", "EntityUser_Id", "dbo.EntityUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EntityUserEntityServers", "EntityServer_Id", "dbo.EntityServers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EntityUserEntityServers", "EntityUser_Id", "dbo.EntityUsers", "Id", cascadeDelete: true);
        }
    }
}
