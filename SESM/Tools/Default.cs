using System.Collections.Generic;
using SESM.Models;

namespace SESM.Tools
{
    public static class Default
    {
        // SESM Grade
        public static readonly bool UseServerExtender = false;
        public static readonly int ServerExtenderPort = 26016;
        public static readonly bool IsPublic = false;

        // Server Grade
        public static readonly string SaveName = string.Empty;
        public static readonly string IP = "0.0.0.0";
        public static readonly int SteamPort = 8766;
        public static readonly int ServerPort = 27016;
        public static readonly string ServerName = "SESM-Default";
        public static readonly bool IgnoreLastSession = false;
        public static readonly bool PauseGameWhenEmpty = false;

        // New Map Grade
        public static readonly int AsteroidAmount = 4;
        public static readonly SubTypeId ScenarioType = SubTypeId.EasyStart1;
        public static readonly float ProceduralDensity = 0;
        public static readonly int ProceduralSeed = 0;

        // Map Grade
        // -- Misc
        public static readonly bool EnableSpectator = false;
        public static readonly bool RealisticSound = false;
        public static readonly int AutoSaveInMinutes = 5;

        // -- Production
        public static readonly int InventorySizeMultiplier = 1;
        public static readonly int AssemblerSpeedMultiplier = 1;
        public static readonly int AssemblerEfficiencyMultiplier = 1;
        public static readonly int RefinerySpeedMultiplier = 1;

        // -- Building
        public static readonly GameMode GameMode = GameMode.Survival;
        public static readonly bool EnableCopyPaste = true;
        public static readonly int WelderSpeedMultiplier = 1;
        public static readonly int GrinderSpeedMultiplier = 1;
        public static readonly double HackSpeedMultiplier = 0.33;
        public static readonly bool DestructibleBlocks = true;

        // -- Caps
        public static readonly int MaxPlayers = 4;
        public static readonly int MaxFloatingObjects = 256;

        // -- Maps
        public static readonly string WorldName = "SESM - MyMap";
        public static readonly EnvironmentHostility EnvironmentHostility = EnvironmentHostility.SAFE;
        public static readonly int WorldSizeKm = 20;
        public static readonly bool PermanentDeath = false;
        public static readonly bool CargoShipsEnabled = false;
        public static readonly bool RemoveTrash = false;
        public static readonly bool ClientCanSave = false;
        public static readonly List<ulong> Mods = new List<ulong>();
        public static readonly int ViewDistance = 20000;

        // -- Access
        public static readonly OnlineMode OnlineMode = OnlineMode.PUBLIC;
        public static readonly bool ResetOwnership = false;
        public static readonly ulong GroupID = 0;
        public static readonly List<ulong> Administrators = new List<ulong>();
        public static readonly List<ulong> Banned = new List<ulong>();

        // -- Gameplay
        public static readonly bool AutoHealing = true;
        public static readonly bool WeaponsEnabled = true;
        public static readonly bool ShowPlayerNamesOnHud = true;
        public static readonly bool ThrusterDamage = true;

        public static readonly int SpawnShipTimeMultiplier = 1;
        public static readonly bool RespawnShipDelete = true;
        public static readonly bool EnableToolShake = false;
        public static readonly bool EnableIngameScripts = true;
    }
}