using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SESM.DTO
{
    public class EntityHostRole
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [NotMapped]
        private ICollection<EnumHostPerm> _permissions;

        [NotMapped]
        public ICollection<EnumHostPerm> Permissions
        {
            get
            {
                return _permissions ?? (_permissions = new HashSet<EnumHostPerm>());
            }
            set
            {
                _permissions = value;
            }
        }

        public string PermissionsSerialized
        {
            get
            {
                return String.Join(";", Permissions.Select(item => (int)item));
            }
            set
            {
                Permissions.Clear();
                foreach (string item in value.Split(';'))
                {
                    EnumHostPerm perm;
                    if (Enum.TryParse(item, out perm))
                    {
                        Permissions.Add(perm);
                    }
                }
            }
        }

        private ICollection<EntityUser> _members;
        public virtual ICollection<EntityUser> Members
        {
            get
            {
                return _members ?? (_members = new HashSet<EntityUser>());
            }
            set
            {
                _members = value;
            }
        }
    }
}