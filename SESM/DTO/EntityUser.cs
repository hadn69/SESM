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

        private ICollection<EntityServer> _administratorOf;

        public virtual ICollection<EntityServer> AdministratorOf
        {
            get
            {
                return _administratorOf ?? (_administratorOf = new HashSet<EntityServer>());
            }
            set
            {
                _administratorOf = value;
            }
        }

        private ICollection<EntityServer> _managerOf;

        public virtual ICollection<EntityServer> ManagerOf
        {
            get
            {
                return _managerOf ?? (_managerOf = new HashSet<EntityServer>());
            }
            set
            {
                _managerOf = value;
            }
        }

        private ICollection<EntityServer> _userOf;

        public virtual ICollection<EntityServer> UserOf
        {
            get
            {
                return _userOf ?? (_userOf = new HashSet<EntityServer>());
            }
            set
            {
                _userOf = value;
            }
        }
    }
}