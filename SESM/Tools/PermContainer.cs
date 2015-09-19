using System.Collections.Generic;
using SESM.DTO;

namespace SESM.Tools
{
    public class PermSummaryContainer
    {
        public Dictionary<int, HashSet<EnumServerPerm>> ServerPerms = new Dictionary<int, HashSet<EnumServerPerm>>();

        public HashSet<EnumHostPerm> HostPerm = new HashSet<EnumHostPerm>();

        public EntityUser User;
    }
}