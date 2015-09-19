using System.Collections.Generic;
using System.ComponentModel;

namespace SESM.DTO
{
    public class EntityUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        [DefaultValue(false)]
        public bool IsAdmin { get; set; }

        private ICollection<EntityHostRole> _hostRoles;
        public virtual ICollection<EntityHostRole> HostRoles
        {
            get
            {
                return _hostRoles ?? (_hostRoles = new HashSet<EntityHostRole>());
            }
            set
            {
                _hostRoles = value;
            }
        }

        private ICollection<EntityInstanceServerRole> _instanceServerRoles;
        public virtual ICollection<EntityInstanceServerRole> InstanceServerRoles
        {
            get
            {
                return _instanceServerRoles ?? (_instanceServerRoles = new HashSet<EntityInstanceServerRole>());
            }
            set
            {
                _instanceServerRoles = value;
            }
        }
    }
}