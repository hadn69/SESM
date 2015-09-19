using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SESM.DTO;

namespace SESM.Tools
{
    public static class AuthHelper
    {
        public static PermSummaryContainer GetPermSummaries(EntityUser user)
        {
            // Todo : time current VS LINQ
            PermSummaryContainer permContainer = new PermSummaryContainer();

            // Host-Wide Perms
            foreach (EntityHostRole hostRole in user.HostRoles)
                foreach (EnumHostPerm perm in hostRole.Permissions)
                    if (!permContainer.HostPerm.Contains(perm))
                        permContainer.HostPerm.Add(perm);

            // Server-Wide Perms
            foreach (EntityInstanceServerRole instanceServerRole in user.InstanceServerRoles)
            {
                if (!permContainer.ServerPerms.ContainsKey(instanceServerRole.Server.Id))
                    permContainer.ServerPerms.Add(instanceServerRole.Server.Id, new HashSet<EnumServerPerm>());

                foreach (EnumServerPerm perm in instanceServerRole.ServerRole.Permissions)
                {
                    if (!permContainer.ServerPerms[instanceServerRole.Server.Id].Contains(perm))
                        permContainer.ServerPerms[instanceServerRole.Server.Id].Add(perm);
                }
            }

            permContainer.User = user;

            return permContainer;
        }

        public static bool HasAnyServerAccess(EntityServer server)
        {
            string[] permList = Enum.GetNames(typeof (EnumServerPerm)).Where(item => item != "SERVER_INFO").ToArray();
            return HasAccess(server, permList);
        }

        public static bool HasAccess(params string[] permStrings)
        {
            return HasAccess(HttpContext.Current.Session["PermSummary"] as PermSummaryContainer, null, permStrings);
        }

        public static bool HasAccess(EntityServer server, params string[] permStrings)
        {
            return HasAccess(HttpContext.Current.Session["PermSummary"] as PermSummaryContainer, server, permStrings);
        }

        public static bool HasAccess(PermSummaryContainer permSummary, EntityServer server, params string[] permStrings)
        {
            if (permStrings == null || permStrings.Length == 0)
                return true;

            if (permSummary == null)
                return false;

            // If the user is superadmin, then he has always access
            if (permSummary.User.IsAdmin)
                return true;

            // Try 1 : Host-wide
            foreach (string permString in permStrings)
            {
                EnumHostPerm hostPerm;
                if (Enum.TryParse(permString, out hostPerm))
                {
                    if (permSummary.HostPerm.Contains(hostPerm))
                        return true;
                }
            }

            // Try 2 : Server-wide
            if (server != null && permSummary.ServerPerms.ContainsKey(server.Id))
            {
                foreach (string permString in permStrings)
                {
                    EnumServerPerm enumServerPerm;
                    if (Enum.TryParse(permString, out enumServerPerm))
                    {
                        if (permSummary.ServerPerms[server.Id].Contains(enumServerPerm))
                            return true;
                    }
                }
            }

            // No host or server perm found in the summary, the user don't have access
            return false;
        }

    }
}