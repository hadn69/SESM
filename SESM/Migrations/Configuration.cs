using System.Linq;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<SESM.DAL.DataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "SESM.DAL.DataContext";
        }

        protected override void Seed(DataContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //


            if (context.Users.ToList().Count == 0)
            {
                EntityUser firstAdmin = new EntityUser();
                firstAdmin.Login = "Admin";
                firstAdmin.Password = HashHelper.MD5Hash("password");
                firstAdmin.Email = "admin@test.lan";
                firstAdmin.IsAdmin = true;
                context.Users.Add(firstAdmin);
            }

            if (context.Hosts.ToList().Count == 0)
            {
                EntityHost localHost = new EntityHost();
                localHost.Name = "localServer";
                localHost.ManagementIP = "127.0.0.1";
                localHost.PublicIP = "127.0.0.1";
                localHost.MaxServer = 10;
                localHost.Account = "";
                localHost.Domain = "";
                localHost.Password = "";
                localHost.AddDateToLog = false;
                localHost.SendLogToKeen = false;
                localHost.StartingPort = 27016;
                localHost.StartingPort = 27020;
                
            }
        }
    }
}
