using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
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

        public SubTypeId ScenarioType = SubTypeId.LoneSurvivor;
        public string SaveName = string.Empty;
        public string IP = "0.0.0.0";
        public int SteamPort = 8766;
        public int ServerPort = 27016;
        public int AsteroidAmount = 4;
        public List<ulong> Administrators = new List<ulong>();
        public List<ulong> Banned = new List<ulong>();
        public List<ulong> Mods = new List<ulong>();
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

            Mods = new List<ulong>();
            if (model.Mods != null)
            {
                string[] splittedMod = model.Mods.Split(';');

                foreach (string item in splittedMod)
                {
                    Mods.Add(ulong.Parse(item));
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

            Mods = new List<ulong>();
            if (model.Mods != null)
            {
                string[] splittedMod = model.Mods.Split(';');

                foreach (string item in splittedMod)
                {
                    Mods.Add(ulong.Parse(item));
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
            model.Mods = String.Join(";", Mods);
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

            if (Mods.Count == 0)
                sb.AppendLine("  <Mods />");
            else
            {
                sb.AppendLine("  <Mods>");
                foreach (ulong item in Mods)
                {
                    sb.AppendLine("    <unsignedLong>" + item + "</unsignedLong>");
                }
                sb.AppendLine("  </Mods>");
            }
            sb.AppendLine("  <GroupID>" + GroupID + "</GroupID>");
            if (ServerName == string.Empty)
                sb.AppendLine("  <ServerName />");
            else
                sb.AppendLine("  <ServerName>" + ServerName + "</ServerName>");
            sb.AppendLine("  <PauseGameWhenEmpty>" + PauseGameWhenEmpty.ToString().ToLower() + "</PauseGameWhenEmpty>");
            sb.AppendLine("</MyConfigDedicated>");
            File.WriteAllText(PathHelper.GetConfigurationFilePath(serv), sb.ToString());

            // Saving the parameters also to the save file
            if (!String.IsNullOrEmpty(SaveName))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(PathHelper.GetSavePath(serv, SaveName) + @"\Sandbox.sbc");
                XmlNode root = doc.DocumentElement;
                XmlNode settingsNode = root.SelectSingleNode("descendant::Settings");
                XmlNode valueNode = null;

                valueNode = settingsNode.SelectSingleNode("descendant::GameMode");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "GameMode", null));
                settingsNode.SelectSingleNode("descendant::GameMode").InnerText = GameMode.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::InventorySizeMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "InventorySizeMultiplier", null));
                settingsNode.SelectSingleNode("descendant::InventorySizeMultiplier").InnerText =
                    InventorySizeMultiplier.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::AssemblerEfficiencyMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "AssemblerEfficiencyMultiplier", null));
                settingsNode.SelectSingleNode("descendant::AssemblerEfficiencyMultiplier").InnerText =
                    AssemblerEfficiencyMultiplier.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::AssemblerSpeedMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "AssemblerSpeedMultiplier", null));
                settingsNode.SelectSingleNode("descendant::AssemblerSpeedMultiplier").InnerText =
                    AssemblerSpeedMultiplier.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::RefinerySpeedMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "RefinerySpeedMultiplier", null));
                settingsNode.SelectSingleNode("descendant::RefinerySpeedMultiplier").InnerText =
                    RefinerySpeedMultiplier.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::OnlineMode");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "OnlineMode", null));
                settingsNode.SelectSingleNode("descendant::OnlineMode").InnerText = OnlineMode.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::MaxPlayers");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "MaxPlayers", null));
                settingsNode.SelectSingleNode("descendant::MaxPlayers").InnerText = MaxPlayers.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::MaxFloatingObjects");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "MaxFloatingObjects", null));
                settingsNode.SelectSingleNode("descendant::MaxFloatingObjects").InnerText =
                    MaxFloatingObjects.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::EnvironmentHostility");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnvironmentHostility", null));
                settingsNode.SelectSingleNode("descendant::EnvironmentHostility").InnerText =
                    EnvironmentHostility.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::AutoHealing");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "AutoHealing", null));
                settingsNode.SelectSingleNode("descendant::AutoHealing").InnerText = AutoHealing.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableCopyPaste");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableCopyPaste", null));
                settingsNode.SelectSingleNode("descendant::EnableCopyPaste").InnerText =
                    EnableCopyPaste.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::AutoSave");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "AutoSave", null));
                settingsNode.SelectSingleNode("descendant::AutoSave").InnerText = AutoSave.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::WeaponsEnabled");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "WeaponsEnabled", null));
                settingsNode.SelectSingleNode("descendant::WeaponsEnabled").InnerText =
                    WeaponsEnabled.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::ShowPlayerNamesOnHud");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "ShowPlayerNamesOnHud", null));
                settingsNode.SelectSingleNode("descendant::ShowPlayerNamesOnHud").InnerText =
                    ShowPlayerNamesOnHud.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::ThrusterDamage");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "ThrusterDamage", null));
                settingsNode.SelectSingleNode("descendant::ThrusterDamage").InnerText =
                    ThrusterDamage.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::CargoShipsEnabled");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "CargoShipsEnabled", null));
                settingsNode.SelectSingleNode("descendant::CargoShipsEnabled").InnerText =
                    CargoShipsEnabled.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableSpectator");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableSpectator", null));
                settingsNode.SelectSingleNode("descendant::EnableSpectator").InnerText =
                    EnableSpectator.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::RemoveTrash");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "RemoveTrash", null));
                settingsNode.SelectSingleNode("descendant::RemoveTrash").InnerText = RemoveTrash.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::WorldSizeKm");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "WorldSizeKm", null));
                settingsNode.SelectSingleNode("descendant::WorldSizeKm").InnerText = WorldSizeKm.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::RespawnShipDelete");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "RespawnShipDelete", null));
                settingsNode.SelectSingleNode("descendant::RespawnShipDelete").InnerText =
                    RespawnShipDelete.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::ResetOwnership");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "ResetOwnership", null));
                settingsNode.SelectSingleNode("descendant::ResetOwnership").InnerText =
                    ResetOwnership.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::WelderSpeedMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "WelderSpeedMultiplier", null));
                settingsNode.SelectSingleNode("descendant::WelderSpeedMultiplier").InnerText =
                    WelderSpeedMultiplier.ToString().Replace(',', '.');


                valueNode = settingsNode.SelectSingleNode("descendant::GrinderSpeedMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "GrinderSpeedMultiplier", null));
                settingsNode.SelectSingleNode("descendant::GrinderSpeedMultiplier").InnerText =
                    GrinderSpeedMultiplier.ToString().Replace(',', '.');


                valueNode = settingsNode.SelectSingleNode("descendant::RealisticSound");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "RealisticSound", null));
                settingsNode.SelectSingleNode("descendant::RealisticSound").InnerText =
                    RealisticSound.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::ClientCanSave");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "ClientCanSave", null));
                settingsNode.SelectSingleNode("descendant::ClientCanSave").InnerText =
                    ClientCanSave.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::HackSpeedMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "HackSpeedMultiplier", null));
                settingsNode.SelectSingleNode("descendant::HackSpeedMultiplier").InnerText =
                    HackSpeedMultiplier.ToString().Replace(',', '.');


                valueNode = settingsNode.SelectSingleNode("descendant::PermanentDeath");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "PermanentDeath", null));
                settingsNode.SelectSingleNode("descendant::PermanentDeath").InnerText =
                    PermanentDeath.ToString().ToLower();

                valueNode = root.SelectSingleNode("descendant::Mods");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "PermanentDeath", null));
                valueNode.RemoveAll();
                foreach (ulong item in Mods)
                {
                    XmlNode modItem = doc.CreateNode(XmlNodeType.Element, "ModItem", null);
                    XmlNode name = doc.CreateNode(XmlNodeType.Element, "Name", null);
                    XmlNode publishedFileId = doc.CreateNode(XmlNodeType.Element, "PublishedFileId", null);
                    name.InnerText = item.ToString() + ".sbm";
                    publishedFileId.InnerText = item.ToString();
                    valueNode.AppendChild(modItem);
                    modItem.AppendChild(name);
                    modItem.AppendChild(publishedFileId);
                }
                doc.Save(PathHelper.GetSavePath(serv, SaveName) + @"\Sandbox.sbc");
            }
        }

        /// <summary>
        /// Load the config file located at "path"
        /// </summary>
        /// <param name="path">the full path to the config file</param>
        /// <returns>true if the load succeded, false if not</returns>
        public bool LoadFromServConf(string path)
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

            Administrators = new List<ulong>();
            if (root.Element("Administrators") != null)
                foreach (XElement item in root.Element("Administrators").Elements("unsignedLong"))
                {
                    ulong val = 0;
                    ulong.TryParse(item.Value, out val);
                    Administrators.Add(val);
                }
            Banned = new List<ulong>();
            if (root.Element("Banned") != null)
                foreach (XElement item in root.Element("Banned").Elements("unsignedLong"))
                {
                    ulong val = 0;
                    ulong.TryParse(item.Value, out val);
                    Banned.Add(val);
                }
            Mods = new List<ulong>();
            if (root.Element("Mods") != null)
                foreach (XElement item in root.Element("Mods").Elements("unsignedLong"))
                {
                    ulong val = 0;
                    ulong.TryParse(item.Value, out val);
                    Mods.Add(val);
                }

            if (root.Element("GroupID") != null)
                ulong.TryParse(root.Element("GroupID").Value, out GroupID);
            if (root.Element("ServerName") != null)
                ServerName = root.Element("ServerName").Value;
            if (root.Element("PauseGameWhenEmpty") != null)
                bool.TryParse(root.Element("PauseGameWhenEmpty").Value, out PauseGameWhenEmpty);
            return true;
        }

        public bool LoadFromSave(string path)
        {
            if (!File.Exists(path))
                return false;
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Element("MyObjectBuilder_Checkpoint");
            if (root == null)
                return false;
            XElement settings = root.Element("Settings");
            if (settings == null)
                return false;
            if (settings.Element("GameMode") != null)
                Enum.TryParse(settings.Element("GameMode").Value, out GameMode);
            if (settings.Element("InventorySizeMultiplier") != null)
                int.TryParse(settings.Element("InventorySizeMultiplier").Value, out InventorySizeMultiplier);
            if (settings.Element("AssemblerSpeedMultiplier") != null)
                int.TryParse(settings.Element("AssemblerSpeedMultiplier").Value, out AssemblerSpeedMultiplier);
            if (settings.Element("AssemblerEfficiencyMultiplier") != null)
                int.TryParse(settings.Element("AssemblerEfficiencyMultiplier").Value, out AssemblerEfficiencyMultiplier);
            if (settings.Element("RefinerySpeedMultiplier") != null)
                int.TryParse(settings.Element("RefinerySpeedMultiplier").Value, out RefinerySpeedMultiplier);
            if (settings.Element("OnlineMode") != null)
                Enum.TryParse(settings.Element("OnlineMode").Value, out OnlineMode);
            if (settings.Element("MaxPlayers") != null)
                int.TryParse(settings.Element("MaxPlayers").Value, out MaxPlayers);
            if (settings.Element("MaxFloatingObjects") != null)
                int.TryParse(settings.Element("MaxFloatingObjects").Value, out MaxFloatingObjects);
            if (settings.Element("EnvironmentHostility") != null)
                Enum.TryParse(settings.Element("EnvironmentHostility").Value, out EnvironmentHostility);
            if (settings.Element("AutoHealing") != null)
                bool.TryParse(settings.Element("AutoHealing").Value, out AutoHealing);
            if (settings.Element("EnableCopyPaste") != null)
                bool.TryParse(settings.Element("EnableCopyPaste").Value, out EnableCopyPaste);
            if (settings.Element("AutoSave") != null)
                bool.TryParse(settings.Element("AutoSave").Value, out AutoSave);
            if (settings.Element("WeaponsEnabled") != null)
                bool.TryParse(settings.Element("WeaponsEnabled").Value, out WeaponsEnabled);
            if (settings.Element("ShowPlayerNamesOnHud") != null)
                bool.TryParse(settings.Element("ShowPlayerNamesOnHud").Value, out ShowPlayerNamesOnHud);
            if (settings.Element("ThrusterDamage") != null)
                bool.TryParse(settings.Element("ThrusterDamage").Value, out ThrusterDamage);
            if (settings.Element("CargoShipsEnabled") != null)
                bool.TryParse(settings.Element("CargoShipsEnabled").Value, out CargoShipsEnabled);
            if (settings.Element("EnableSpectator") != null)
                bool.TryParse(settings.Element("EnableSpectator").Value, out EnableSpectator);
            if (settings.Element("RemoveTrash") != null)
                bool.TryParse(settings.Element("RemoveTrash").Value, out RemoveTrash);
            if (settings.Element("WorldSizeKm") != null)
                int.TryParse(settings.Element("WorldSizeKm").Value, out WorldSizeKm);
            if (settings.Element("RespawnShipDelete") != null)
                bool.TryParse(settings.Element("RespawnShipDelete").Value, out RespawnShipDelete);
            if (settings.Element("ResetOwnership") != null)
                bool.TryParse(settings.Element("ResetOwnership").Value, out ResetOwnership);
            if (settings.Element("WelderSpeedMultiplier") != null)
                double.TryParse(settings.Element("WelderSpeedMultiplier").Value, out WelderSpeedMultiplier);
            if (settings.Element("GrinderSpeedMultiplier") != null)
                double.TryParse(settings.Element("GrinderSpeedMultiplier").Value, out GrinderSpeedMultiplier);
            if (settings.Element("RealisticSound") != null)
                bool.TryParse(settings.Element("RealisticSound").Value, out RealisticSound);
            if (settings.Element("ClientCanSave") != null)
                bool.TryParse(settings.Element("ClientCanSave").Value, out ClientCanSave);
            if (settings.Element("HackSpeedMultiplier") != null)
                double.TryParse(settings.Element("HackSpeedMultiplier").Value, out HackSpeedMultiplier);
            if (settings.Element("PermanentDeath") != null)
                bool.TryParse(settings.Element("PermanentDeath").Value, out PermanentDeath);
            
            Mods = new List<ulong>();
            if (root.Element("Mods") != null)
            {
                foreach (XElement item in root.Element("Mods").Elements("ModItem"))
                {
                    if (item.Element("Name") != null
                        && item.Element("PublishedFileId") != null
                        && item.Element("PublishedFileId").Value ==
                        item.Element("Name").Value.Substring(0, item.Element("Name").Value.Length - 4))
                    {
                        ulong val = 0;
                        ulong.TryParse(item.Element("PublishedFileId").Value, out val);
                        Mods.Add(val);
                    }
                }
            }
            return true;
        }
    }
}