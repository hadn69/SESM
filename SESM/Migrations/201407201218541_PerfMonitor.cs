using System.Data.Entity.Migrations;

namespace SESM.Migrations
{
    public partial class PerfMonitor : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EntityPerfEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Timestamp = c.DateTime(nullable: false),
                        RamUsage = c.Int(nullable: false),
                        RamUsagePeak = c.Int(),
                        RamUsageTrough = c.Int(),
                        RamUsageQ1 = c.Int(),
                        RamUsageQ3 = c.Int(),
                        CPUUsage = c.Int(nullable: false),
                        CPUUsagePeak = c.Int(),
                        CPUUsageTrough = c.Int(),
                        CPUUsageQ1 = c.Int(),
                        CPUUsageQ3 = c.Int(),
                        EntityServer_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.EntityServers", t => t.EntityServer_Id)
                .Index(t => t.EntityServer_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EntityPerfEntries", "EntityServer_Id", "dbo.EntityServers");
            DropIndex("dbo.EntityPerfEntries", new[] { "EntityServer_Id" });
            DropTable("dbo.EntityPerfEntries");
        }
    }
}
