using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using SESM.DTO;
using SESM.Models.Views.Server;

namespace SESM.Tools.Helpers
{
    public class ServerConfigHelper
    {
        public GameMode GameMode = GameMode.Survival;
        public int InventorySizeMultiplier = 1;
        public int AssemblerSpeedMultiplier = 1;
        public int AssemblerEfficiencyMultiplier = 1;
        public int RefinerySpeedMultiplier = 1;
        public OnlineMode OnlineMode = OnlineMode.PUBLIC;
        public int MaxPlayers = 4;
        public int MaxFloatingObjects = 256;
        public EnvironmentHostility EnvironmentHostility = EnvironmentHostility.SAFE;
        public bool AutoHealing = true;
        public bool EnableCopyPaste = true;
        public bool AutoSave = true;
        public bool WeaponsEnabled = true;
        public bool ShowPlayerNamesOnHud = true;
        public bool ThrusterDamage = true;
        public bool CargoShipsEnabled = false;
        public bool EnableSpectator = false;
        public bool RemoveTrash = false;
        public int WorldSizeKm = 0;
        public bool RespawnShipDelete = true;
        public bool ResetOwnership = false;
        public double WelderSpeedMultiplier = 1;
        public double GrinderSpeedMultiplier = 1;
        public bool RealisticSound = false;
        public bool ClientCanSave = false;
        public double HackSpeedMultiplier = 0.33;
        public bool PermanentDeath = false;

        public SubTypeId ScenarioType = SubTypeId.EasyStart1;
        public string SaveName = string.Empty;
        public string IP = "0.0.0.0";
        public int SteamPort = 8766;
        public int ServerPort = 27016;
        public int AsteroidAmount = 4;
        public List<ulong> Administrators = new List<ulong>();
        public List<ulong> Banned = new List<ulong>();
        public ulong GroupID = 0;
        public string ServerName = "SESM";
        public bool PauseGameWhenEmpty = false;

        /// <summary>
        /// Parse the View Model into the object
        /// </summary>
        public void ParseIn(ServerViewModel model)
        {
            GameMode = model.GameMode;
            InventorySizeMultiplier = model.InventorySizeMultiplier;
            AssemblerSpeedMultiplier = model.AssemblerSpeedMultiplier;
            AssemblerEfficiencyMultiplier = model.AssemblerEfficiencyMultiplier;
            RefinerySpeedMultiplier = model.RefinerySpeedMultiplier;
            OnlineMode = model.OnlineMode;
            MaxPlayers = model.MaxPlayers;
            MaxFloatingObjects = model.MaxFloatingObjects;
            EnvironmentHostility = model.EnvironmentHostility;
            AutoHealing = model.AutoHealing;
            EnableCopyPaste = model.EnableCopyPaste;
            AutoSave = model.AutoSave;
            WeaponsEnabled = model.WeaponsEnabled;
            ShowPlayerNamesOnHud = model.ShowPlayerNamesOnHud;
            ThrusterDamage = model.ThrusterDamage;
            CargoShipsEnabled = model.CargoShipsEnabled;
            EnableSpectator = model.EnableSpectator;
            RemoveTrash = model.RemoveTrash;
            WorldSizeKm = model.WorldSizeKm;
            RespawnShipDelete = model.RespawnShipDelete;
            ResetOwnership = model.ResetOwnership;
            WelderSpeedMultiplier = model.WelderSpeedMultiplier;
            GrinderSpeedMultiplier = model.GrinderSpeedMultiplier;
            RealisticSound = model.RealisticSound;
            ClientCanSave = model.ClientCanSave;
            HackSpeedMultiplier = model.HackSpeedMultiplier;
            PermanentDeath = model.PermanentDeath;

            ScenarioType = model.ScenarioType;
            SaveName = model.SaveName;
            IP = model.IP;
            SteamPort = model.SteamPort;
            ServerPort = model.ServerPort;
            AsteroidAmount = model.AsteroidAmount;

            Administrators = new List<ulong>();
            if (model.Administrators != null)
            {
                string[] splittedAdmins = model.Administrators.Split(';');

                foreach (string item in splittedAdmins)
                {
                    Administrators.Add(ulong.Parse(item));
                }
            }

            Banned = new List<ulong>();
            if (model.Banned != null)
            {
                string[] splittedBanned = model.Banned.Split(';');

                foreach (string item in splittedBanned)
                {
                    Banned.Add(ulong.Parse(item));
                }
            }
            GroupID = model.GroupID;
            ServerName = model.ServerName;
            PauseGameWhenEmpty = model.PauseGameWhenEmpty;
        }

        public void ParseIn(NewServerViewModel model)
        {
            GameMode = model.GameMode;
            InventorySizeMultiplier = model.InventorySizeMultiplier;
            AssemblerSpeedMultiplier = model.AssemblerSpeedMultiplier;
            AssemblerEfficiencyMultiplier = model.AssemblerEfficiencyMultiplier;
            RefinerySpeedMultiplier = model.RefinerySpeedMultiplier;
            OnlineMode = model.OnlineMode;
            MaxPlayers = model.MaxPlayers;
            MaxFloatingObjects = model.MaxFloatingObjects;
            EnvironmentHostility = model.EnvironmentHostility;
            AutoHealing = model.AutoHealing;
            EnableCopyPaste = model.EnableCopyPaste;
            AutoSave = model.AutoSave;
            WeaponsEnabled = model.WeaponsEnabled;
            ShowPlayerNamesOnHud = model.ShowPlayerNamesOnHud;
            ThrusterDamage = model.ThrusterDamage;
            CargoShipsEnabled = model.CargoShipsEnabled;
            EnableSpectator = model.EnableSpectator;
            RemoveTrash = model.RemoveTrash;
            WorldSizeKm = model.WorldSizeKm;
            RespawnShipDelete = model.RespawnShipDelete;
            ResetOwnership = model.ResetOwnership;
            WelderSpeedMultiplier = model.WelderSpeedMultiplier;
            GrinderSpeedMultiplier = model.GrinderSpeedMultiplier;
            RealisticSound = model.RealisticSound;
            ClientCanSave = model.ClientCanSave;
            HackSpeedMultiplier = model.HackSpeedMultiplier;
            PermanentDeath = model.PermanentDeath;

            IP = model.IP;
            SteamPort = model.SteamPort;
            ServerPort = model.ServerPort;

            Administrators = new List<ulong>();
            if (model.Administrators != null)
            {
                string[] splittedAdmins = model.Administrators.Split(';');

                foreach (string item in splittedAdmins)
                {
                    Administrators.Add(ulong.Parse(item));
                }
            }

            Banned = new List<ulong>();
            if (model.Banned != null)
            {
                string[] splittedBanned = model.Banned.Split(';');

                foreach (string item in splittedBanned)
                {
                    Banned.Add(ulong.Parse(item));
                }
            }
            GroupID = model.GroupID;
            ServerName = model.ServerName;
            PauseGameWhenEmpty = model.PauseGameWhenEmpty;
        }

        /// <summary>
        /// Parse the object into the View Model
        /// </summary>
        public ServerViewModel ParseOut(ServerViewModel model)
        {
            model.GameMode = GameMode;
            model.InventorySizeMultiplier = InventorySizeMultiplier;
            model.AssemblerSpeedMultiplier = AssemblerSpeedMultiplier;
            model.AssemblerEfficiencyMultiplier = AssemblerEfficiencyMultiplier;
            model.RefinerySpeedMultiplier = RefinerySpeedMultiplier;
            model.OnlineMode = OnlineMode;
            model.MaxPlayers = MaxPlayers;
            model.MaxFloatingObjects = MaxFloatingObjects;
            model.EnvironmentHostility = EnvironmentHostility;
            model.AutoHealing = AutoHealing;
            model.EnableCopyPaste = EnableCopyPaste;
            model.AutoSave = AutoSave;
            model.WeaponsEnabled = WeaponsEnabled;
            model.ShowPlayerNamesOnHud = ShowPlayerNamesOnHud;
            model.ThrusterDamage = ThrusterDamage;
            model.CargoShipsEnabled = CargoShipsEnabled;
            model.EnableSpectator = EnableSpectator;
            model.RemoveTrash = RemoveTrash;
            model.WorldSizeKm = WorldSizeKm;
            model.RespawnShipDelete = RespawnShipDelete;
            model.ResetOwnership = ResetOwnership;
            model.WelderSpeedMultiplier = WelderSpeedMultiplier;
            model.GrinderSpeedMultiplier = GrinderSpeedMultiplier;
            model.RealisticSound = RealisticSound;
            model.ClientCanSave = ClientCanSave;
            model.HackSpeedMultiplier = HackSpeedMultiplier;
            model.PermanentDeath = PermanentDeath;

            model.ScenarioType = ScenarioType;
            model.SaveName = SaveName;
            model.IP = IP;
            model.SteamPort = SteamPort;
            model.ServerPort = ServerPort;
            model.AsteroidAmount = AsteroidAmount;
            model.Administrators = String.Join(";", Administrators);
            model.Banned = String.Join(";", Banned);
            model.GroupID = GroupID;
            model.ServerName = ServerName;
            model.PauseGameWhenEmpty = PauseGameWhenEmpty;
            return model;
        }

        public void Save(EntityServer serv)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine(
                "<MyConfigDedicated xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine("  <SessionSettings>");
            sb.AppendLine("    <GameMode>" + GameMode.ToString() + "</GameMode>");
            sb.AppendLine("    <InventorySizeMultiplier>" + InventorySizeMultiplier + "</InventorySizeMultiplier>");
            sb.AppendLine("    <AssemblerSpeedMultiplier>" + AssemblerSpeedMultiplier + "</AssemblerSpeedMultiplier>");
            sb.AppendLine("    <AssemblerEfficiencyMultiplier>" + AssemblerEfficiencyMultiplier + "</AssemblerEfficiencyMultiplier>");
            sb.AppendLine("    <RefinerySpeedMultiplier>" + RefinerySpeedMultiplier + "</RefinerySpeedMultiplier>");
            sb.AppendLine("    <OnlineMode>" + OnlineMode.ToString() + "</OnlineMode>");
            sb.AppendLine("    <MaxPlayers>" + MaxPlayers + "</MaxPlayers>");
            sb.AppendLine("    <MaxFloatingObjects>" + MaxFloatingObjects + "</MaxFloatingObjects>");
            sb.AppendLine("    <EnvironmentHostility>" + EnvironmentHostility.ToString() + "</EnvironmentHostility>");
            sb.AppendLine("    <AutoHealing>" + AutoHealing.ToString().ToLower() + "</AutoHealing>");
            sb.AppendLine("    <EnableCopyPaste>" + EnableCopyPaste.ToString().ToLower() + "</EnableCopyPaste>");
            sb.AppendLine("    <AutoSave>" + AutoSave.ToString().ToLower() + "</AutoSave>");
            sb.AppendLine("    <WeaponsEnabled>" + WeaponsEnabled.ToString().ToLower() + "</WeaponsEnabled>");
            sb.AppendLine("    <ShowPlayerNamesOnHud>" + ShowPlayerNamesOnHud.ToString().ToLower() + "</ShowPlayerNamesOnHud>");
            sb.AppendLine("    <ThrusterDamage>" + ThrusterDamage.ToString().ToLower() + "</ThrusterDamage>");
            sb.AppendLine("    <CargoShipsEnabled>" + CargoShipsEnabled.ToString().ToLower() + "</CargoShipsEnabled>");
            sb.AppendLine("    <EnableSpectator>" + EnableSpectator.ToString().ToLower() + "</EnableSpectator>");
            sb.AppendLine("    <RemoveTrash>" + RemoveTrash.ToString().ToLower() + "</RemoveTrash>");
            sb.AppendLine("    <WorldSizeKm>" + WorldSizeKm + "</WorldSizeKm>");
            sb.AppendLine("    <RespawnShipDelete>" + RespawnShipDelete.ToString().ToLower() + "</RespawnShipDelete>");
            sb.AppendLine("    <ResetOwnership>" + ResetOwnership.ToString().ToLower() + "</ResetOwnership>");
            sb.AppendLine("    <WelderSpeedMultiplier>" + WelderSpeedMultiplier + "</WelderSpeedMultiplier>");
            sb.AppendLine("    <GrinderSpeedMultiplier>" + GrinderSpeedMultiplier + "</GrinderSpeedMultiplier>");
            sb.AppendLine("    <RealisticSound>" + RealisticSound.ToString().ToLower() + "</RealisticSound>");
            sb.AppendLine("    <ClientCanSave>" + ClientCanSave.ToString().ToLower() + "</ClientCanSave>");
            sb.AppendLine("    <HackSpeedMultiplier>" + HackSpeedMultiplier + "</HackSpeedMultiplier>");
            sb.AppendLine("    <PermanentDeath>" + PermanentDeath.ToString().ToLower() + "</PermanentDeath>");
            //sb.AppendLine("    <Mods />");
            sb.AppendLine("  </SessionSettings>");
            sb.AppendLine("  <Scenario>");
            sb.AppendLine("    <TypeId>MyObjectBuilder_ScenarioDefinition</TypeId>");
            sb.AppendLine("    <SubtypeId>" + ScenarioType.ToString() + "</SubtypeId>");
            sb.AppendLine("  </Scenario>");
            if (string.IsNullOrEmpty(SaveName))
                sb.AppendLine("  <LoadWorld />");
            else
                sb.AppendLine("  <LoadWorld>" + PathHelper.GetSavePath(serv, SaveName) + "</LoadWorld>");
            sb.AppendLine("  <IP>" + IP + "</IP>");
            sb.AppendLine("  <SteamPort>" + SteamPort + "</SteamPort>");
            sb.AppendLine("  <ServerPort>" + ServerPort + "</ServerPort>");
            sb.AppendLine("  <AsteroidAmount>" + AsteroidAmount + "</AsteroidAmount>");
            if (Administrators.Count == 0)
                sb.AppendLine("  <Administrators />");
            else
            {
                sb.AppendLine("  <Administrators>");
                foreach (ulong item in Administrators)
                {
                    sb.AppendLine("    <unsignedLong>" + item + "</unsignedLong>");
                }
                sb.AppendLine("  </Administrators>");
            }
            if (Banned.Count == 0)
                sb.AppendLine("  <Banned />");
            else
            {
                sb.AppendLine("  <Banned>");
                foreach (ulong item in Banned)
                {
                    sb.AppendLine("    <unsignedLong>" + item + "</unsignedLong>");
                }
                sb.AppendLine("  </Banned>");
            }
            sb.AppendLine("  <GroupID>" + GroupID + "</GroupID>");
            if (ServerName == string.Empty)
                sb.AppendLine("  <ServerName />");
            else
                sb.AppendLine("  <ServerName>" + ServerName + "</ServerName>");
            sb.AppendLine("  <PauseGameWhenEmpty>" + PauseGameWhenEmpty.ToString().ToLower() + "</PauseGameWhenEmpty>");
            sb.AppendLine("</MyConfigDedicated>");
            File.WriteAllText(PathHelper.GetConfigurationFilePath(serv), sb.ToString());
        }

        /// <summary>
        /// Load the config file located at "path"
        /// </summary>
        /// <param name="path">the full path to the config file</param>
        /// <returns>true if the load succeded, fals if not</returns>
        public bool Load(string path)
        {
            if (!File.Exists(path))
                return false;
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Element("MyConfigDedicated");
            if (root == null)
                return false;
            XElement sessionSettings = root.Element("SessionSettings");
            if (sessionSettings == null)
                return false;
            if (sessionSettings.Element("GameMode") != null)
                Enum.TryParse(sessionSettings.Element("GameMode").Value, out GameMode);
            if (sessionSettings.Element("InventorySizeMultiplier") != null)
                int.TryParse(sessionSettings.Element("InventorySizeMultiplier").Value, out InventorySizeMultiplier);
            if (sessionSettings.Element("AssemblerSpeedMultiplier") != null)
                int.TryParse(sessionSettings.Element("AssemblerSpeedMultiplier").Value, out AssemblerSpeedMultiplier);
            if (sessionSettings.Element("AssemblerEfficiencyMultiplier") != null)
                int.TryParse(sessionSettings.Element("AssemblerEfficiencyMultiplier").Value, out AssemblerEfficiencyMultiplier);
            if (sessionSettings.Element("RefinerySpeedMultiplier") != null)
                int.TryParse(sessionSettings.Element("RefinerySpeedMultiplier").Value, out RefinerySpeedMultiplier);
            if (sessionSettings.Element("OnlineMode") != null)
                Enum.TryParse(sessionSettings.Element("OnlineMode").Value, out OnlineMode);
            if (sessionSettings.Element("MaxPlayers") != null)
                int.TryParse(sessionSettings.Element("MaxPlayers").Value, out MaxPlayers);
            if (sessionSettings.Element("MaxFloatingObjects") != null)
                int.TryParse(sessionSettings.Element("MaxFloatingObjects").Value, out MaxFloatingObjects);
            if (sessionSettings.Element("EnvironmentHostility") != null)
                Enum.TryParse(sessionSettings.Element("EnvironmentHostility").Value, out EnvironmentHostility);
            if (sessionSettings.Element("AutoHealing") != null)
                bool.TryParse(sessionSettings.Element("AutoHealing").Value, out AutoHealing);
            if (sessionSettings.Element("EnableCopyPaste") != null)
                bool.TryParse(sessionSettings.Element("EnableCopyPaste").Value, out EnableCopyPaste);
            if (sessionSettings.Element("AutoSave") != null)
                bool.TryParse(sessionSettings.Element("AutoSave").Value, out AutoSave);
            if (sessionSettings.Element("WeaponsEnabled") != null)
                bool.TryParse(sessionSettings.Element("WeaponsEnabled").Value, out WeaponsEnabled);
            if (sessionSettings.Element("ShowPlayerNamesOnHud") != null)
                bool.TryParse(sessionSettings.Element("ShowPlayerNamesOnHud").Value, out ShowPlayerNamesOnHud);
            if (sessionSettings.Element("ThrusterDamage") != null)
                bool.TryParse(sessionSettings.Element("ThrusterDamage").Value, out ThrusterDamage);
            if (sessionSettings.Element("CargoShipsEnabled") != null)
                bool.TryParse(sessionSettings.Element("CargoShipsEnabled").Value, out CargoShipsEnabled);
            if (sessionSettings.Element("EnableSpectator") != null)
                bool.TryParse(sessionSettings.Element("EnableSpectator").Value, out EnableSpectator);
            if (sessionSettings.Element("RemoveTrash") != null)
                bool.TryParse(sessionSettings.Element("RemoveTrash").Value, out RemoveTrash);
            if (sessionSettings.Element("WorldSizeKm") != null)
                int.TryParse(sessionSettings.Element("WorldSizeKm").Value, out WorldSizeKm);
            if (sessionSettings.Element("RespawnShipDelete") != null)
                bool.TryParse(sessionSettings.Element("RespawnShipDelete").Value, out RespawnShipDelete);
            if (sessionSettings.Element("ResetOwnership") != null)
                bool.TryParse(sessionSettings.Element("ResetOwnership").Value, out ResetOwnership);
            if (sessionSettings.Element("WelderSpeedMultiplier") != null)
                double.TryParse(sessionSettings.Element("WelderSpeedMultiplier").Value, out WelderSpeedMultiplier);
            if (sessionSettings.Element("GrinderSpeedMultiplier") != null)
                double.TryParse(sessionSettings.Element("GrinderSpeedMultiplier").Value, out GrinderSpeedMultiplier);
            if (sessionSettings.Element("RealisticSound") != null)
                bool.TryParse(sessionSettings.Element("RealisticSound").Value, out RealisticSound);
            if (sessionSettings.Element("ClientCanSave") != null)
                bool.TryParse(sessionSettings.Element("ClientCanSave").Value, out ClientCanSave);
            if (sessionSettings.Element("HackSpeedMultiplier") != null)
                double.TryParse(sessionSettings.Element("HackSpeedMultiplier").Value, out HackSpeedMultiplier);
            if (sessionSettings.Element("PermanentDeath") != null)
                bool.TryParse(sessionSettings.Element("PermanentDeath").Value, out PermanentDeath);
            if (root.Element("Scenario") != null && root.Element("Scenario").Element("SubtypeId") != null)
                Enum.TryParse(root.Element("Scenario").Element("SubtypeId").Value, out ScenarioType);
            if (root.Element("LoadWorld") != null)
                SaveName = PathHelper.GetLastDirName(root.Element("LoadWorld").Value);
            if (root.Element("IP") != null)
                IP = root.Element("IP").Value;
            if (root.Element("SteamPort") != null)
                int.TryParse(root.Element("SteamPort").Value, out SteamPort);
            if (root.Element("ServerPort") != null)
                int.TryParse(root.Element("ServerPort").Value, out ServerPort);
            if (root.Element("AsteroidAmount") != null)
                int.TryParse(root.Element("AsteroidAmount").Value, out AsteroidAmount);

            if (root.Element("Administrators") != null)
                foreach (XElement item in root.Element("Administrators").Elements("unsignedLong"))
                {
                    ulong val = 0;
                    ulong.TryParse(item.Value, out val);
                    Administrators.Add(val);
                }
            if (root.Element("Banned") != null)
                foreach (XElement item in root.Element("Banned").Elements("unsignedLong"))
                {
                    ulong val = 0;
                    ulong.TryParse(item.Value, out val);
                    Banned.Add(val);
                }

            if (root.Element("GroupID") != null)
                ulong.TryParse(root.Element("GroupID").Value, out GroupID);
            if (root.Element("ServerName") != null)
                ServerName = root.Element("ServerName").Value;
            if (root.Element("PauseGameWhenEmpty") != null)
                bool.TryParse(root.Element("PauseGameWhenEmpty").Value, out PauseGameWhenEmpty);
            return true;
        }
    }
}