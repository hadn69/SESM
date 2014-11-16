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
        public int ServerExtenderPort { get; set; }
        public bool IsAutoStartEnabled { get; set; }
        public int? AutoSaveInMinutes { get; set; }

        private ICollection<EntityUser> _administrators;
        public virtual ICollection<EntityUser> Administrators
        {
            get { return _administrators ?? (_administrators = new HashSet<EntityUser>()); }
            set { _administrators = value; }
        }

        private ICollection<EntityUser> _managers;
        public virtual ICollection<EntityUser> Managers
        {
            get { return _managers ?? (_managers = new HashSet<EntityUser>()); }
            set { _managers = value; }
        }

        private ICollection<EntityUser> _users;
        public virtual ICollection<EntityUser> Users
        {
            get { return _users ?? (_users = new HashSet<EntityUser>()); }
            set { _users = value; }
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
            ServerExtenderPort = 26016;
            IsAutoStartEnabled = false;

        }
    }
}