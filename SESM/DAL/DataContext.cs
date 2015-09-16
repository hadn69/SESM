using System.Data.Entity;
using SESM.DTO;
using SESM.Migrations;
using SESM.Tools.Helpers;

namespace SESM.DAL
{
    public class DataContext : DbContext
    {
        public DbSet<EntityUser> Users { get; set; }
        public DbSet<EntityServer> Servers { get; set; }
        public DbSet<EntityPerfEntry> PerfEntries { get; set; }
        public DbSet<EntityHostRole> HostRoles { get; set; }
        public DbSet<EntityServerRole> ServerRoles { get; set; }
        public DbSet<EntityInstanceServerRole> InstanceServerRoles { get; set; }

        public DataContext() : base(SESMConfigHelper.DBConnString)
        {
            //Database.SetInitializer<DataContext>(new DataContextInitializer());
            Database.SetInitializer<DataContext>(new MigrateDatabaseToLatestVersion<DataContext, Configuration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityUser>()
                .HasMany<EntityHostRole>(x => x.HostRoles)
                .WithMany(x => x.Members);

            modelBuilder.Entity<EntityUser>()
                .HasMany<EntityInstanceServerRole>(x => x.InstanceServerRoles)
                .WithMany(x => x.Members);

            modelBuilder.Entity<EntityServer>()
                .HasMany<EntityInstanceServerRole>(x => x.InstanceServerRoles)
                .WithRequired(x => x.Server);

            modelBuilder.Entity<EntityServerRole>()
                .HasMany<EntityInstanceServerRole>(x => x.InstanceServerRoles)
                .WithRequired(x => x.ServerRole);

            modelBuilder.Entity<EntityServer>()
                .HasMany<EntityPerfEntry>(x => x.PerfEntries);
        }
    }
}