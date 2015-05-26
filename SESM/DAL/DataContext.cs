﻿using System.Data.Entity;
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

        public DataContext() : base(SESMConfigHelper.DBConnString)
        {
            //Database.SetInitializer<DataContext>(new DataContextInitializer());
            Database.SetInitializer<DataContext>(new MigrateDatabaseToLatestVersion<DataContext, Configuration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            /*
            modelBuilder.Entity<EntityUser>()
                .HasMany<EntityServer>(x => x.AdministratorOf)
                .WithMany(x => x.Administrators);
            modelBuilder.Entity<EntityUser>()
                .HasMany<EntityServer>(x => x.ManagerOf)
                .WithMany(x => x.Managers);
            modelBuilder.Entity<EntityUser>()
                .HasMany<EntityServer>(x => x.UserOf)
                .WithMany(x => x.Users);
    */

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