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
        public bool IsLvl1BackupEnabled { get; set; }
        public bool IsLvl2BackupEnabled { get; set; }
        public bool IsLvl3BackupEnabled { get; set; }
        public bool IsAutoRestartEnabled { get; set; }
        public string AutoRestartCron { get; set; }
        public bool UseServerExtender { get; set; }
        public bool IsAutoStartEnabled { get; set; }
        public int? AutoSaveInMinutes { get; set; }
        public EnumProcessPriority ProcessPriority { get; set; }
        public EnumServerType ServerType { get; set; }

        private ICollection<EntityInstanceServerRole> _instanceServerRoles;
        public virtual ICollection<EntityInstanceServerRole> InstanceServerRoles
        {
            get { return _instanceServerRoles ?? (_instanceServerRoles = new HashSet<EntityInstanceServerRole>()); }
            set { _instanceServerRoles = value; }
        }
        
        private ICollection<EntityPerfEntry> _perfEntries;
        public virtual ICollection<EntityPerfEntry> PerfEntries
        {
            get { return _perfEntries ?? (_perfEntries = new HashSet<EntityPerfEntry>()); }
            set { _perfEntries = value; }
        }
        
        public EntityServer()
        {
            IsPublic = false;
            IsLvl1BackupEnabled = false;
            IsLvl2BackupEnabled = false;
            IsLvl3BackupEnabled = false;
            IsAutoRestartEnabled = false;
            AutoRestartCron = "0 0 0 * * ?";
            UseServerExtender = false;
            IsAutoStartEnabled = false;
            ProcessPriority = EnumProcessPriority.Normal;
        }
    }
}