
using System.Collections.Generic;
using System.Web.Mvc;

namespace SESM.Tools.Helpers.HTMLHelper
{
    public static class NavHelperExtensions
    {
        public static MvcHtmlString NavServer(this HtmlHelper helper,int serverId, int selected)
        {
            List<string[]> Entries = new List<string[]>();
            Entries.Add(new[] { "/Server/Dashboard/" + serverId,"fa-tachometer", "Dashboard" });
            Entries.Add(new[] { "/Faction/Index/" + serverId, "fa-users", "Factions" });
            Entries.Add(new[] { "/Mods/Index/" + serverId,"fa-puzzle-piece fa-flip-horizontal", "Mods" });
            Entries.Add(new[] { "/Maps/Index/" + serverId, "fa-star", "Maps" });
            Entries.Add(new[] { "/Backups/Index/" + serverId, "fa-hdd-o", "Backups" });
            Entries.Add(new[] { "/Server/Configuration/" + serverId, "fa-cogs", "Configuration" });

            string builder = string.Empty;

            for(int i = 0; i < Entries.Count; i++)
            {
                if(i == selected)
                    builder += "<div class=\"col-md-1 navbar-selected\">\r\n";
                else
                    builder += "<div class=\"col-md-1\">\r\n";
                builder += "<a href=\"" + Entries[i][0] + "\">";
                builder += "<i class=\"fa fa-2x " + Entries[i][1] + "\"></i>\r\n";
                builder += Entries[i][2] + "\r\n";
                builder += "</a>";
                builder += "</div>\r\n";
            }
            return MvcHtmlString.Create(builder);
        }
    }
}