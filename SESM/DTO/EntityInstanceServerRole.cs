using System.Collections.Generic;

namespace SESM.DTO
{
    public class EntityInstanceServerRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual EntityServer Server { get; set; }
        public virtual EntityServerRole ServerRole { get; set; }

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