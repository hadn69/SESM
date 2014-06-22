using System.Collections.Generic;

namespace SESM.DTO
{
    public class EntityServer
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }

        private ICollection<EntityUser> _administrators;
        public virtual ICollection<EntityUser> Administrators
        {
            get { return _administrators ?? (_administrators = new HashSet<EntityUser>()); }
            set { _administrators = value; }
        }

        private ICollection<EntityUser> _managers;
        public virtual ICollection<EntityUser> Managers
        {
            get { return _managers ?? (_managers = new HashSet<EntityUser>()); }
            set { _managers = value; }
        }

        private ICollection<EntityUser> _users;
        public virtual ICollection<EntityUser> Users
        {
            get { return _users ?? (_users = new HashSet<EntityUser>()); }
            set { _users = value; }
        }

        public EntityServer()
        {
            IsPublic = false;
        }
    }
}