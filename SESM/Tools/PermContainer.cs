using System.Collections.Generic;
using SESM.DTO;

namespace SESM.Tools
{
    public class PermSummaryContainer
    {
        public Dictionary<EntityServer, HashSet<EnumServerPerm>> ServerPerms = new Dictionary<EntityServer, HashSet<EnumServerPerm>>();

        public HashSet<EnumHostPerm> HostPerm = new HashSet<EnumHostPerm>();

        public EntityUser User;
    }
}