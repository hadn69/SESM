namespace SESM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class MultiHosts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EntityHosts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PublicIP = c.String(),
                        ManagementIP = c.String(),
                        MaxServer = c.Int(nullable: false),
                        Domain = c.String(),
                        Account = c.String(),
                        Password = c.String(),
                        Prefix = c.String(),
                        SESavePath = c.String(),
                        SEDataPath = c.String(),
                        Arch = c.Int(nullable: false),
                        AddDateToLog = c.Boolean(nullable: false),
                        SendLogToKeen = c.Boolean(nullable: false),
                        StartingPort = c.Int(nullable: false),
                        EndingPort = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.EntityServers", "Host_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.EntityServers", "Host_Id");
            AddForeignKey("dbo.EntityServers", "Host_Id", "dbo.EntityHosts", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.EntityServers", "Host_Id", "dbo.EntityHosts");
            DropIndex("dbo.EntityServers", new[] { "Host_Id" });
            DropColumn("dbo.EntityServers", "Host_Id");
            DropTable("dbo.EntityHosts");
        }
    }
}
