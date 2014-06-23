using System.Data.Entity;
using SESM.DTO;
using SESM.Tools;

namespace SESM.DAL
{
    public class DataContextInitializer : CreateDatabaseIfNotExists<DataContext>
    {
        protected override void Seed(DataContext context)
        {

            EntityUser firstAdmin = new EntityUser();
            firstAdmin.Login = "Admin";
            firstAdmin.Password = HashHelper.MD5Hash("password");
            firstAdmin.Email = "admin@test.lan";
            firstAdmin.IsAdmin = true;
            context.Users.Add(firstAdmin);

        }
    }
}