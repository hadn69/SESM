using System.Collections.Generic;

namespace SESM.DTO
{
    public class EntityServerWideRole
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private IDictionary<EnumServerWidePerm, EnumPermAccess> _permissions;
        public virtual IDictionary<EnumServerWidePerm, EnumPermAccess> Permissions
        {
            get { return _permissions ?? (_permissions = new Dictionary<EnumServerWidePerm, EnumPermAccess>()); }
            set { _permissions = value; }
        }

        private ICollection<EntityUser> _members;
        public virtual ICollection<EntityUser> Members
        {
            get { return _members ?? (_members = new HashSet<EntityUser>()); }
            set { _members = value; }
        }

        private ICollection<EntityUser> _managers;
        public virtual ICollection<EntityUser> Managers
        {
            get { return _managers ?? (_managers = new HashSet<EntityUser>()); }
            set { _managers = value; }
        }

        private ICollection<EntityUser> _owners;
        public virtual ICollection<EntityUser> Owners
        {
            get { return _owners ?? (_owners = new HashSet<EntityUser>()); }
            set { _owners = value; }
        }
    }
}