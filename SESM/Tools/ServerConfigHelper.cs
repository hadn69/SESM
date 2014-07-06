using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using SESM.DTO;
using SESM.Models.View.Server;

namespace SESM.Tools
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
            return;
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
            return;
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
            model.ScenarioType = ScenarioType;
            model.SaveName = SaveName;
            model.IP = IP;
            model.SteamPort = SteamPort;
            model.ServerPort = ServerPort;
            model.AsteroidAmount = AsteroidAmount;
            model.Administrators = String.Join("\n",Administrators);
            model.Banned = String.Join("\n", Banned);
            model.GroupID = GroupID;
            model.ServerName = ServerName;
            return model;
        }

        public void Save(EntityServer serv)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<MyConfigDedicated xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
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
            sb.AppendLine("  </SessionSettings>");
            sb.AppendLine("  <Scenario>");
            sb.AppendLine("    <TypeId>ScenarioDefinition</TypeId>");
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
            sb.AppendLine("</MyConfigDedicated>");
            File.WriteAllText(PathHelper.GetConfigurationFilePath(serv), sb.ToString());
        }

        public void Load(string path)
        {
            try
            {

            
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Element("MyConfigDedicated");
            XElement sessionSettings = root.Element("SessionSettings");

            GameMode = (GameMode)Enum.Parse(typeof(GameMode), sessionSettings.Element("GameMode").Value);
            InventorySizeMultiplier = int.Parse(sessionSettings.Element("InventorySizeMultiplier").Value);
            AssemblerSpeedMultiplier = int.Parse(sessionSettings.Element("AssemblerSpeedMultiplier").Value);
            AssemblerEfficiencyMultiplier = int.Parse(sessionSettings.Element("AssemblerEfficiencyMultiplier").Value);
            RefinerySpeedMultiplier = int.Parse(sessionSettings.Element("RefinerySpeedMultiplier").Value);
            OnlineMode = (OnlineMode)Enum.Parse(typeof(OnlineMode), sessionSettings.Element("OnlineMode").Value);
            MaxPlayers = int.Parse(sessionSettings.Element("MaxPlayers").Value);
            MaxFloatingObjects = int.Parse(sessionSettings.Element("MaxFloatingObjects").Value);
            EnvironmentHostility = (EnvironmentHostility)Enum.Parse(typeof(EnvironmentHostility), sessionSettings.Element("EnvironmentHostility").Value);
            AutoHealing = bool.Parse(sessionSettings.Element("AutoHealing").Value);
            EnableCopyPaste = bool.Parse(sessionSettings.Element("EnableCopyPaste").Value);
            AutoSave = bool.Parse(sessionSettings.Element("AutoSave").Value);
            WeaponsEnabled = bool.Parse(sessionSettings.Element("WeaponsEnabled").Value); ;
            ShowPlayerNamesOnHud = bool.Parse(sessionSettings.Element("ShowPlayerNamesOnHud").Value);
            ThrusterDamage = bool.Parse(sessionSettings.Element("ThrusterDamage").Value);
            CargoShipsEnabled = bool.Parse(sessionSettings.Element("CargoShipsEnabled").Value);
            EnableSpectator = bool.Parse(sessionSettings.Element("EnableSpectator").Value);
            RemoveTrash = bool.Parse(sessionSettings.Element("RemoveTrash").Value);
            WorldSizeKm = int.Parse(sessionSettings.Element("WorldSizeKm").Value);
            RespawnShipDelete = bool.Parse(sessionSettings.Element("RespawnShipDelete").Value);
            ScenarioType = (SubTypeId)Enum.Parse(typeof(SubTypeId), root.Element("Scenario").Element("SubtypeId").Value);
            SaveName = PathHelper.GetLastDirName(root.Element("LoadWorld").Value);
            IP = root.Element("IP").Value;
            SteamPort = int.Parse(root.Element("SteamPort").Value);
            ServerPort = int.Parse(root.Element("ServerPort").Value);
            AsteroidAmount = int.Parse(root.Element("AsteroidAmount").Value);

            foreach (XElement item in root.Element("Administrators").Elements("unsignedLong"))
            {
                Administrators.Add(ulong.Parse(item.Value));
            }

            foreach (XElement item in root.Element("Banned").Elements("unsignedLong"))
            {
                Banned.Add(ulong.Parse(item.Value));
            }

            GroupID = ulong.Parse(root.Element("GroupID").Value);
            ServerName = root.Element("ServerName").Value;
            }
            catch (Exception)
            {

                return;
            }
        }
    }

    
}