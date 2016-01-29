using System.Collections.Generic;
using SESM.Models;

namespace SESM.Tools
{
    public static class SEDefault
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
        public static readonly SESubTypeId ScenarioType = SESubTypeId.EasyStart1;
        public static readonly float ProceduralDensity = 0;
        public static readonly int ProceduralSeed = 0;

        // Map Grade
        // -- Misc
        public static readonly bool EnableSpectator = false;
        public static readonly bool RealisticSound = false;
        public static readonly uint AutoSaveInMinutes = 5;

        // -- Production
        public static readonly float InventorySizeMultiplier = 1;
        public static readonly float AssemblerSpeedMultiplier = 1;
        public static readonly float AssemblerEfficiencyMultiplier = 1;
        public static readonly float RefinerySpeedMultiplier = 1;

        // -- Building
        public static readonly GameMode GameMode = GameMode.Survival;
        public static readonly bool EnableCopyPaste = true;
        public static readonly float WelderSpeedMultiplier = 1;
        public static readonly float GrinderSpeedMultiplier = 1;
        public static readonly float HackSpeedMultiplier = 0.33f;
        public static readonly bool DestructibleBlocks = true;

        // -- Caps
        public static readonly short MaxPlayers = 4;
        public static readonly short MaxFloatingObjects = 256;

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
        public static readonly bool EnableEncounters = true;

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

        public static readonly float SpawnShipTimeMultiplier = 1;
        public static readonly bool RespawnShipDelete = true;
        public static readonly bool EnableToolShake = false;
        public static readonly bool EnableIngameScripts = true;
        public static readonly int VoxelGeneratorVersion = 1;
        public static readonly bool EnableOxygen = true;
        public static readonly bool Enable3rdPersonView = true;

        // -- New 18/09
        public static readonly bool EnableFlora = false;
        public static readonly bool EnableStationVoxelSupport = false;
        public static readonly bool EnableSunRotation = true;
        public static readonly bool DisableRespawnShips = false;
        public static readonly bool ScenarioEditMode = false;
        public static readonly bool Battle = false;
        public static readonly bool Scenario = false;
        public static readonly bool CanJoinRunning = false;
        public static readonly int PhysicsIterations = 4;
        public static readonly float SunRotationIntervalMinutes = 240;
        public static readonly bool EnableJetpack = true;
        public static readonly bool SpawnWithTools = true;
        public static readonly bool StartInRespawnScreen = false;
        public static readonly bool EnableVoxelDestruction = true;
        public static readonly int MaxDrones = 5;
        public static readonly bool EnableDrones = true;

        // -- New 12/11
        public static readonly int FloraDensity = 20;

        // -- New 06/01/16
        public static readonly bool EnableCyberhounds = false;
        public static readonly bool EnableSpiders = true;

        // -- New 29/01/16
        public static readonly float FloraDensityMultiplier = 1;
    }
}