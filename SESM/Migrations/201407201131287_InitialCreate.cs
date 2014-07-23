namespace SESM.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EntityServers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ip = c.String(),
                        Port = c.Int(nullable: false),
                        Name = c.String(),
                        IsPublic = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EntityUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        IsAdmin = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EntityUserEntityServers",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityServer_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityServer_Id })
                .ForeignKey("dbo.EntityUsers", t => t.EntityUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.EntityServers", t => t.EntityServer_Id, cascadeDelete: true)
                .Index(t => t.EntityUser_Id)
                .Index(t => t.EntityServer_Id);
            
            CreateTable(
                "dbo.EntityUserEntityServer1",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityServer_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityServer_Id })
                .ForeignKey("dbo.EntityUsers", t => t.EntityUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.EntityServers", t => t.EntityServer_Id, cascadeDelete: true)
                .Index(t => t.EntityUser_Id)
                .Index(t => t.EntityServer_Id);
            
            CreateTable(
                "dbo.EntityUserEntityServer2",
                c => new
                    {
                        EntityUser_Id = c.Int(nullable: false),
                        EntityServer_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityUser_Id, t.EntityServer_Id })
                .ForeignKey("dbo.EntityUsers", t => t.EntityUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.EntityServers", t => t.EntityServer_Id, cascadeDelete: true)
                .Index(t => t.EntityUser_Id)
                .Index(t => t.EntityServer_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EntityUserEntityServer2", "EntityServer_Id", "dbo.EntityServers");
            DropForeignKey("dbo.EntityUserEntityServer2", "EntityUser_Id", "dbo.EntityUsers");
            DropForeignKey("dbo.EntityUserEntityServer1", "EntityServer_Id", "dbo.EntityServers");
            DropForeignKey("dbo.EntityUserEntityServer1", "EntityUser_Id", "dbo.EntityUsers");
            DropForeignKey("dbo.EntityUserEntityServers", "EntityServer_Id", "dbo.EntityServers");
            DropForeignKey("dbo.EntityUserEntityServers", "EntityUser_Id", "dbo.EntityUsers");
            DropIndex("dbo.EntityUserEntityServer2", new[] { "EntityServer_Id" });
            DropIndex("dbo.EntityUserEntityServer2", new[] { "EntityUser_Id" });
            DropIndex("dbo.EntityUserEntityServer1", new[] { "EntityServer_Id" });
            DropIndex("dbo.EntityUserEntityServer1", new[] { "EntityUser_Id" });
            DropIndex("dbo.EntityUserEntityServers", new[] { "EntityServer_Id" });
            DropIndex("dbo.EntityUserEntityServers", new[] { "EntityUser_Id" });
            DropTable("dbo.EntityUserEntityServer2");
            DropTable("dbo.EntityUserEntityServer1");
            DropTable("dbo.EntityUserEntityServers");
            DropTable("dbo.EntityUsers");
            DropTable("dbo.EntityServers");
        }
    }
}
