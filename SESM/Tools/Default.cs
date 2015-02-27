using SESM.Models.Views.Server;

namespace SESM.Tools
{
    public static class Default
    {
        public static readonly GameMode GameMode = GameMode.Survival;
        public static readonly int InventorySizeMultiplier = 1;
        public static readonly int AssemblerSpeedMultiplier = 1;
        public static readonly int AssemblerEfficiencyMultiplier = 1;
        public static readonly int RefinerySpeedMultiplier = 1;
        public static readonly OnlineMode OnlineMode = OnlineMode.PUBLIC;
        public static readonly int MaxPlayers = 4;
        public static readonly int MaxFloatingObjects = 256;
        public static readonly EnvironmentHostility EnvironmentHostility = EnvironmentHostility.SAFE;
        public static readonly bool AutoHealing = true;
        public static readonly bool EnableCopyPaste = true;
        public static readonly bool WeaponsEnabled = true;
        public static readonly bool ShowPlayerNamesOnHud = true;
        public static readonly bool ThrusterDamage = true;
        public static readonly bool CargoShipsEnabled = false;
        public static readonly bool EnableSpectator = false;
        public static readonly bool RemoveTrash = false;
        public static readonly int WorldSizeKm = 20;
        public static readonly bool RespawnShipDelete = true;
        public static readonly bool ResetOwnership = false;
        public static readonly int WelderSpeedMultiplier = 1;
        public static readonly int GrinderSpeedMultiplier = 1;
        public static readonly bool RealisticSound = false;
        public static readonly bool ClientCanSave = false;
        public static readonly double HackSpeedMultiplier = 0.33;
        public static readonly bool PermanentDeath = false;
        public static readonly SubTypeId ScenarioType = SubTypeId.EasyStart1;
        public static readonly string SaveName = string.Empty;
        public static readonly string IP = "0.0.0.0";
        public static readonly int SteamPort = 8766;
        public static readonly int ServerPort = 27016;
        public static readonly int AsteroidAmount = 4;
        public static readonly ulong GroupID = 0;
        public static readonly string ServerName = "SESM-Default";
        public static readonly bool IsPublic = false;
        public static readonly bool UseServerExtender = false;
        public static readonly int ServerExtenderPort = 26016;
        public static readonly bool IgnoreLastSession = false;
        public static readonly string WorldName = "SESM - MyMap";
        public static readonly int AutoSaveInMinutes = 5;
        public static readonly bool PauseGameWhenEmpty = false;
        public static readonly int SpawnShipTimeMultiplier = 1;
    }
}