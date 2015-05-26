using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SESM.DTO
{
    public class EntityServerRole
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [NotMapped]
        private ICollection<EnumServerPerm> _permissions;

        [NotMapped]
        public ICollection<EnumServerPerm> Permissions {
            get
            {
                return _permissions ?? (_permissions = new HashSet<EnumServerPerm>());
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
                    EnumServerPerm perm;
                    if (Enum.TryParse(item, out perm))
                    {
                        Permissions.Add(perm);
                    }
                }
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