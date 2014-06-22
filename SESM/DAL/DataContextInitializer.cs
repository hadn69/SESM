using System.Data.Entity;
using SESM.DTO;
using SESM.Tools;

namespace SESM.DAL
{
    public class DataContextInitializer : DropCreateDatabaseIfModelChanges<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            EntityUser sadminTest = new EntityUser();
            sadminTest.Login = "SAdmin";
            sadminTest.Password = HashHelper.MD5Hash("SAdmin");
            sadminTest.Email = "sadmin@test.lan";
            sadminTest.IsAdmin = true;
            context.Users.Add(sadminTest);

            EntityUser adminTest = new EntityUser();
            adminTest.Login = "Admin";
            adminTest.Password = HashHelper.MD5Hash("Admin");
            adminTest.Email = "admin@test.lan";
            context.Users.Add(adminTest);


            EntityUser managerTest = new EntityUser();
            managerTest.Login = "Manager";
            managerTest.Password = HashHelper.MD5Hash("Manager");
            managerTest.Email = "manager@test.lan";
            context.Users.Add(managerTest);


            EntityUser userTest = new EntityUser();
            userTest.Login = "User";
            userTest.Password = HashHelper.MD5Hash("User");
            userTest.Email = "user@test.lan";
            context.Users.Add(adminTest);



            EntityServer server1 = new EntityServer();
            server1.Ip = "0.0.0.0";
            server1.Port = 4242;
            server1.Name = "server1";
            server1.Administrators.Add(adminTest);
            server1.Managers.Add(managerTest);
            server1.Users.Add(userTest);
            context.Servers.Add(server1);


        }
    }
}