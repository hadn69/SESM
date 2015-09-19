using SESM.DAL;
using SESM.DTO;

namespace SESM.Controllers.API
{
    interface IAPIController
    {
        DataContext CurrentContext { get; set; }

        EntityServer RequestServer { get; set; }
    }
}
