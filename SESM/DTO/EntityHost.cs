using System.Collections.Generic;
using SESM.Models.Views.Settings;

namespace SESM.DTO
{
    public class EntityHost
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PublicIP { get; set; }
        public string ManagementIP { get; set; }
        public int MaxServer { get; set; }
        public string Domain { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Prefix { get; set; }
        public string SESavePath { get; set; }
        public string SEDataPath { get; set; }
        public EnumArchType Arch { get; set; }
        public bool AddDateToLog { get; set; }
        public bool SendLogToKeen { get; set; }
        public int StartingPort { get; set; }
        public int EndingPort { get; set; }

        private ICollection<EntityServer> _servers;
        public virtual ICollection<EntityServer> Servers
        {
            get { return _servers ?? (_servers = new HashSet<EntityServer>()); }
            set { _servers = value; }
        }
    }
}