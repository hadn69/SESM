using System.Collections.Generic;
using SESM.Models;

namespace SESM.Tools
{
    public static class MEDefault
    {
        // SESM Grade
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
        public static readonly MESubTypeId ScenarioType = MESubTypeId.Quickstart;

        // Map Grade
        // -- Misc
        public static readonly bool EnableSpectator = false;
        public static readonly uint AutoSaveInMinutes = 5;

        // -- Building
        public static readonly GameMode GameMode = GameMode.Survival;
        public static readonly bool EnableCopyPaste = true;
        public static readonly bool EnableStructuralSimulation = true;
        public static readonly uint MaxActiveFracturePieces = 400;

        // -- Caps
        public static readonly short MaxPlayers = 4;

        // -- Maps
        public static readonly string WorldName = "SESM - MyMap";
        public static readonly bool ClientCanSave = false;
        public static readonly List<ulong> Mods = new List<ulong>();

        // -- Access
        public static readonly OnlineMode OnlineMode = OnlineMode.PUBLIC;
        public static readonly ulong GroupID = 0;
        public static readonly List<ulong> Administrators = new List<ulong>();
        public static readonly List<ulong> Banned = new List<ulong>();

        // -- Gameplay
        public static readonly bool EnableBarbarians = false;
        public static readonly uint MaximumBots = 10;
        public static readonly uint GameDayInRealMinutes = 20;
        public static readonly double DayNightRatio = 0.67;
        public static readonly bool EnableAnimals = false;
    }
}