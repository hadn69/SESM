using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SESM.Models.View.Server
{
    public class NewServerViewModel
    {
        [Required]
        [DisplayName("Web Name")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "The Name must be only composed of letters, numbers, dot, dash and underscore")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Web Public")]
        public bool IsPublic { get; set; }

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
        [DisplayName("Enable Auto Healing")]
        public bool AutoHealing { get; set; }

        [Required]
        [DisplayName("Enable Copy Paste")]
        public bool EnableCopyPaste { get; set; }

        [Required]
        [DisplayName("Enable Auto Save")]
        public bool AutoSave { get; set; }

        [Required]
        [DisplayName("Enable Weapons")]
        public bool WeaponsEnabled { get; set; }

        [Required]
        [DisplayName("Show Player Names On Hud")]
        public bool ShowPlayerNamesOnHud { get; set; }

        [Required]
        [DisplayName("Enable Thruster Damage")]
        public bool ThrusterDamage { get; set; }

        [Required]
        [DisplayName("Enable Cargo Ships")]
        public bool CargoShipsEnabled { get; set; }

        [Required]
        [DisplayName("Enable Spectator Mode")]
        public bool EnableSpectator { get; set; }

        [Required]
        [DisplayName("Enable Auto Trash Remover")]
        public bool RemoveTrash { get; set; }

        [Required]
        [DisplayName("World Border Size (in Km)")]
        public int WorldSizeKm { get; set; }

        [Required]
        [DisplayName("Enable Respawn Ship Auto Delete")]
        public bool RespawnShipDelete { get; set; }

        [Required]
        [DisplayName("Listening IP")]
        [RegularExpression(@"^((\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])$", ErrorMessage = "The IP must be a vaild one")]
        public string IP { get; set; }

        [Required]
        [DisplayName("Steam Connexion Port")]
        [RegularExpression(@"^([1-9]|[1-9]\d|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$", ErrorMessage = "The server port must be a vaild number")]
        public int SteamPort { get; set; }

        [Required]
        [DisplayName("Server Connexion Port")]
        [RegularExpression(@"^([1-9]|[1-9]\d|[1-9]\d{0,3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$", ErrorMessage = "The steam port must be a vaild number")]
        public int ServerPort { get; set; }

        [DisplayName("In-Game Administrators List")]
        public string Administrators { get; set; }

        [DisplayName("In-Game Banned List")]
        public string Banned { get; set; }

        [DisplayName("Steam Group ID")]
        public ulong GroupID { get; set; }

        [Required]
        [DisplayName("Server Display Name")]
        public string ServerName { get; set; }

        [DisplayName("Web Administrators List")]
        public string WebAdministrators { get; set; }

        [DisplayName("Web Managers List")]
        public string WebManagers { get; set; }

        [DisplayName("Web Users List")]
        public string WebUsers { get; set; }


        public NewServerViewModel()
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
            IP = "0.0.0.0";
            SteamPort = 8766;
            ServerPort = 27016;
            Administrators = string.Empty;
            Banned = string.Empty;
            GroupID = 0;
            ServerName = "SESM";
            WebAdministrators = string.Empty;
            WebManagers = string.Empty;
            WebUsers = string.Empty;
        }
    }
}