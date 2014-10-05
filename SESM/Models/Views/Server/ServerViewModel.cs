using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SESM.Models.Views.Server
{
    public class ServerViewModel
    {
        [Required]
        [DisplayName("Web Name")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "The Name must be only composed of letters, numbers, dots, dashs and underscores")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Web Public")]
        public bool IsPublic { get; set; }

        [Required]
        [DisplayName("Backup LvL 1")]
        public bool IsLvl1BackupEnabled { get; set; }

        [Required]
        [DisplayName("Backup LvL 2")]
        public bool IsLvl2BackupEnabled { get; set; }

        [Required]
        [DisplayName("Backup LvL 3")]
        public bool IsLvl3BackupEnabled { get; set; }

        [Required]
        [DisplayName("Auto Restart")]
        public bool AutoRestart { get; set; }

        [DisplayName("Auto Restart Cron")]
        public string AutoRestartCron { get; set; }

        [Required]
        [DisplayName("Server Extender")]
        public bool UseServerExtender { get; set; }

        [Required]
        [DisplayName("SESE Port")]
        [RegularExpression(@"^([1-9]|[1-9]\d|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$", ErrorMessage = "The server extender port must be a vaild number")]
        public int ServerExtenderPort { get; set; }

        [Required]
        [DisplayName("Game Mode")]
        public GameMode GameMode { get; set; }

        [Required]
        [DisplayName("Inventory Size")]
        public int InventorySizeMultiplier { get; set; }

        [Required]
        [DisplayName("Assembler Speed")]
        public int AssemblerSpeedMultiplier { get; set; }

        [Required]
        [DisplayName("Assembler Efficiency")]
        public int AssemblerEfficiencyMultiplier { get; set; }

        [Required]
        [DisplayName("Refinery Speed")]
        public int RefinerySpeedMultiplier { get; set; }

        [Required]
        [DisplayName("Spawn Ship Time")]
        public int SpawnShipTimeMultiplier { get; set; }

        [Required]
        [DisplayName("Online Mode")]
        public OnlineMode OnlineMode { get; set; }

        [Required]
        [DisplayName("Max Players")]
        public int MaxPlayers { get; set; }

        [Required]
        [DisplayName("Max Floating Objects")]
        public int MaxFloatingObjects { get; set; }

        [Required]
        [DisplayName("Environment Hostility")]
        public EnvironmentHostility EnvironmentHostility { get; set; }

        [Required]
        [DisplayName("Auto Healing")]
        public bool AutoHealing { get; set; }

        [Required]
        [DisplayName("Copy Paste")]
        public bool EnableCopyPaste { get; set; }

        [Required]
        [DisplayName("Auto Save")]
        public bool AutoSave { get; set; }

        [Required]
        [DisplayName("Auto Save Interval")]
        public int AutoSaveInMinutes { get; set; }

        [Required]
        [DisplayName("Weapons")]
        public bool WeaponsEnabled { get; set; }

        [Required]
        [DisplayName("Show Player Names")]
        public bool ShowPlayerNamesOnHud { get; set; }

        [Required]
        [DisplayName("Thruster Damage")]
        public bool ThrusterDamage { get; set; }

        [Required]
        [DisplayName("Cargo Ships")]
        public bool CargoShipsEnabled { get; set; }

        [Required]
        [DisplayName("Spectator Mode")]
        public bool EnableSpectator { get; set; }

        [Required]
        [DisplayName("Auto Trash Remover")]
        public bool RemoveTrash { get; set; }

        [Required]
        [DisplayName("World Border Size")]
        public int WorldSizeKm { get; set; }

        [Required]
        [DisplayName("Respawn Ship Auto Delete")]
        public bool RespawnShipDelete { get; set; }

        [Required]
        [DisplayName("Reset Block Ownership")]
        public bool ResetOwnership { get; set; }

        [Required]
        [DisplayName("Welder Speed")]
        public double WelderSpeedMultiplier { get; set; }

        [Required]
        [DisplayName("Grinder Speed")]
        public double GrinderSpeedMultiplier { get; set; }

        [Required]
        [DisplayName("Realistic Sound")]
        public bool RealisticSound { get; set; }

        [Required]
        [DisplayName("Client-Side Saving")]
        public bool ClientCanSave { get; set; }

        [Required]
        [DisplayName("Hack Speed")]
        public double HackSpeedMultiplier { get; set; }

        [Required]
        [DisplayName("Permanent Death")]
        public bool PermanentDeath { get; set; }

        [DisplayName("Current Scenario")]
        public SubTypeId ScenarioType { get; set; }

        [DisplayName("Current Save")]
        public string SaveName { get; set; }

        [Required]
        [DisplayName("Listening IP")]
        [RegularExpression(@"^((\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])$", ErrorMessage = "The IP must be a vaild one")]
        public string IP { get; set; }

        [Required]
        [DisplayName("Steam Port")]
        [RegularExpression(@"^([1-9]|[1-9]\d|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$", ErrorMessage = "The server port must be a vaild number")]
        public int SteamPort { get; set; }

        [Required]
        [DisplayName("Server Port")]
        [RegularExpression(@"^([1-9]|[1-9]\d|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$", ErrorMessage = "The steam port must be a vaild number")]
        public int ServerPort { get; set; }

        [DisplayName("Asteroid Amount")]
        public int AsteroidAmount { get; set; }

        [DisplayName("In-Game Administrators")]
        public string Administrators { get; set; }

        [DisplayName("In-Game Banned")]
        public string Banned { get; set; }

        [DisplayName("In-Game Mod")]
        public string Mods { get; set; }

        [DisplayName("Steam Group ID")]
        public ulong GroupID { get; set; }

        [Required]
        [DisplayName("Server Display Name")]
        public string ServerName { get; set; }

        [DisplayName("Map Display Name")]
        public string WorldName { get; set; }

        [Required]
        [DisplayName("Pause on Empty")]
        public bool PauseGameWhenEmpty { get; set; }

        [Required]
        [DisplayName("Ignore Last Session")]
        public bool IgnoreLastSession { get; set; }

        [DisplayName("Web Administrators")]
        public string WebAdministrators { get; set; }

        [DisplayName("Web Managers")]
        public string WebManagers { get; set; }

        [DisplayName("Web Users")]
        public string WebUsers { get; set; }


        public ServerViewModel()
        {
            GameMode = GameMode.Survival;
            InventorySizeMultiplier = 1;
            AssemblerSpeedMultiplier = 1;
            AssemblerEfficiencyMultiplier = 1;
            RefinerySpeedMultiplier = 1;
            OnlineMode = OnlineMode.PUBLIC;
            MaxPlayers = 4;
            MaxFloatingObjects = 256;
            EnvironmentHostility = EnvironmentHostility.SAFE;
            AutoHealing = true;
            EnableCopyPaste = true;
            AutoSave = true;
            WeaponsEnabled = true;
            ShowPlayerNamesOnHud = true;
            ThrusterDamage = true;
            CargoShipsEnabled = false;
            EnableSpectator = false;
            RemoveTrash = false;
            WorldSizeKm = 0;
            RespawnShipDelete = true;
            ResetOwnership = false;
            WelderSpeedMultiplier = 1;
            GrinderSpeedMultiplier = 1;
            RealisticSound = false;
            ClientCanSave = false;
            HackSpeedMultiplier = 0.33;
            PermanentDeath = false;

            ScenarioType = SubTypeId.EasyStart1;
            SaveName = string.Empty;
            IP = "0.0.0.0";
            SteamPort = 8766;
            ServerPort = 27016;
            AsteroidAmount = 4;
            Administrators = string.Empty;
            Banned = string.Empty;
            GroupID = 0;
            ServerName = "SESM";
            WebAdministrators = string.Empty;
            WebManagers = string.Empty;
            WebUsers = string.Empty;

            IsPublic = false;
            IsLvl1BackupEnabled = false;
            IsLvl2BackupEnabled = false;
            IsLvl3BackupEnabled = false;
            AutoRestart = false;
            AutoRestartCron = "0 0 0 * * ?";
            UseServerExtender = false;
            ServerExtenderPort = 26016;
            IgnoreLastSession = false;
            //WorldName = "SESM - MyMap";
            AutoSaveInMinutes = 5;
        }
    }
}