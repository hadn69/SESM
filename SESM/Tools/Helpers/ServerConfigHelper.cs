using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SESM.DTO;
using SESM.Models;

namespace SESM.Tools.Helpers
{
    public class ServerConfigHelper
    {
        // Server Grade
        public string SaveName = Default.SaveName;
        public string IP = Default.IP;
        public int SteamPort = Default.SteamPort;
        public int ServerPort = Default.ServerPort;
        public string ServerName = Default.ServerName;
        public bool IgnoreLastSession = Default.IgnoreLastSession;
        public bool PauseGameWhenEmpty = Default.PauseGameWhenEmpty;

        // New Map Grade
        public int AsteroidAmount = Default.AsteroidAmount;
        public SubTypeId ScenarioType = Default.ScenarioType;
        public float ProceduralDensity = Default.ProceduralDensity;
        public int ProceduralSeed = Default.ProceduralSeed;

        // Map Grade
        // -- Misc
        public bool EnableSpectator = Default.EnableSpectator;
        public bool RealisticSound = Default.RealisticSound;
        public int AutoSaveInMinutes = Default.AutoSaveInMinutes;

        // -- Production
        public double InventorySizeMultiplier = Default.InventorySizeMultiplier;
        public double AssemblerSpeedMultiplier = Default.AssemblerSpeedMultiplier;
        public double AssemblerEfficiencyMultiplier = Default.AssemblerEfficiencyMultiplier;
        public double RefinerySpeedMultiplier = Default.RefinerySpeedMultiplier;

        // -- Building
        public GameMode GameMode = Default.GameMode;
        public bool EnableCopyPaste = Default.EnableCopyPaste;
        public double WelderSpeedMultiplier = Default.WelderSpeedMultiplier;
        public double GrinderSpeedMultiplier = Default.GrinderSpeedMultiplier;
        public double HackSpeedMultiplier = Default.HackSpeedMultiplier;
        public bool DestructibleBlocks = Default.DestructibleBlocks;

        // -- Caps
        public int MaxPlayers = Default.MaxPlayers;
        public int MaxFloatingObjects = Default.MaxFloatingObjects;

        // -- Maps
        public string WorldName = Default.WorldName;
        public EnvironmentHostility EnvironmentHostility = Default.EnvironmentHostility;
        public int WorldSizeKm = Default.WorldSizeKm;
        public bool PermanentDeath = Default.PermanentDeath;
        public bool CargoShipsEnabled = Default.CargoShipsEnabled;
        public bool RemoveTrash = Default.RemoveTrash;
        public bool ClientCanSave = Default.ClientCanSave;
        public List<ulong> Mods = Default.Mods;
        public int ViewDistance = Default.ViewDistance;

        // -- Access
        public OnlineMode OnlineMode = Default.OnlineMode;
        public bool ResetOwnership = Default.ResetOwnership;
        public ulong GroupID = Default.GroupID;
        public List<ulong> Administrators = Default.Administrators;
        public List<ulong> Banned = Default.Banned;

        // -- Gameplay
        public bool AutoHealing = Default.AutoHealing;
        public bool WeaponsEnabled = Default.WeaponsEnabled;
        public bool ShowPlayerNamesOnHud = Default.ShowPlayerNamesOnHud;
        public bool ThrusterDamage = Default.ThrusterDamage;
        public double SpawnShipTimeMultiplier = Default.SpawnShipTimeMultiplier;
        public bool RespawnShipDelete = Default.RespawnShipDelete;
        public bool EnableToolShake = Default.EnableToolShake;
        public bool EnableIngameScripts = Default.EnableIngameScripts;
        public int VoxelGeneratorVersion = Default.VoxelGeneratorVersion;
        public bool EnableOxygen = Default.EnableOxygen;

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
            sb.AppendLine("    <AutoSaveInMinutes>" + AutoSaveInMinutes + "</AutoSaveInMinutes>");
            sb.AppendLine("    <SpawnShipTimeMultiplier>" + SpawnShipTimeMultiplier + "</SpawnShipTimeMultiplier>");
            if (ProceduralDensity != 0)
            {
                sb.AppendLine("    <ProceduralDensity>" + ProceduralDensity + "</ProceduralDensity>");
                sb.AppendLine("    <ProceduralSeed>" + ProceduralSeed + "</ProceduralSeed>");
            }
            sb.AppendLine("    <DestructibleBlocks>" + DestructibleBlocks.ToString().ToLower() + "</DestructibleBlocks>");
            sb.AppendLine("    <EnableIngameScripts>" + EnableIngameScripts.ToString().ToLower() + "</EnableIngameScripts>");
            if (VoxelGeneratorVersion != 0)
                sb.AppendLine("    <VoxelGeneratorVersion>" + VoxelGeneratorVersion + "</VoxelGeneratorVersion>");
            sb.AppendLine("    <EnableOxygen>" + EnableOxygen.ToString().ToLower() + "</EnableOxygen>");
            sb.AppendLine("    <ViewDistance>" + ViewDistance + "</ViewDistance>");
            sb.AppendLine("    <EnableToolShake>" + EnableToolShake.ToString().ToLower() + "</EnableToolShake>");
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
            if (string.IsNullOrEmpty(ServerName))
                sb.AppendLine("  <ServerName />");
            else
                sb.AppendLine("  <ServerName>" + ServerName.Replace('&', ' ') + "</ServerName>");
            if (string.IsNullOrEmpty(WorldName))
                sb.AppendLine("  <WorldName />");
            else
                sb.AppendLine("  <WorldName>" + WorldName.Replace('&', ' ') + "</WorldName>");
            sb.AppendLine("  <PauseGameWhenEmpty>" + PauseGameWhenEmpty.ToString().ToLower() + "</PauseGameWhenEmpty>");
            sb.AppendLine("  <IgnoreLastSession>" + IgnoreLastSession.ToString().ToLower() + "</IgnoreLastSession>");
            sb.AppendLine("</MyConfigDedicated>");
            File.WriteAllText(PathHelper.GetConfigurationFilePath(serv), sb.ToString());

            // Saving the parameters also to the save file
            if (!String.IsNullOrEmpty(SaveName) && File.Exists(PathHelper.GetSavePath(serv, SaveName) + @"\Sandbox.sbc"))
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


                valueNode = settingsNode.SelectSingleNode("descendant::AutoSaveInMinutes");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "AutoSaveInMinutes", null));
                settingsNode.SelectSingleNode("descendant::AutoSaveInMinutes").InnerText =
                    AutoSaveInMinutes.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::SpawnShipTimeMultiplier");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "SpawnShipTimeMultiplier", null));
                settingsNode.SelectSingleNode("descendant::SpawnShipTimeMultiplier").InnerText =
                    SpawnShipTimeMultiplier.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::DestructibleBlocks");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "DestructibleBlocks", null));
                settingsNode.SelectSingleNode("descendant::DestructibleBlocks").InnerText =
                    DestructibleBlocks.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableIngameScripts");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableIngameScripts", null));
                settingsNode.SelectSingleNode("descendant::EnableIngameScripts").InnerText =
                    EnableIngameScripts.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::ViewDistance");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "ViewDistance", null));
                settingsNode.SelectSingleNode("descendant::ViewDistance").InnerText =
                    ViewDistance.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableToolShake");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableToolShake", null));
                settingsNode.SelectSingleNode("descendant::EnableToolShake").InnerText =
                    EnableToolShake.ToString().ToLower();

                if (VoxelGeneratorVersion != 0)
                {
                    valueNode = settingsNode.SelectSingleNode("descendant::VoxelGeneratorVersion");
                    if (valueNode == null)
                        settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "VoxelGeneratorVersion", null));
                    settingsNode.SelectSingleNode("descendant::VoxelGeneratorVersion").InnerText =
                        VoxelGeneratorVersion.ToString();
                }
                else
                {
                    valueNode = settingsNode.SelectSingleNode("descendant::VoxelGeneratorVersion");
                    if (valueNode != null)
                        settingsNode.RemoveChild(valueNode);
                }


                valueNode = settingsNode.SelectSingleNode("descendant::EnableOxygen");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableOxygen", null));
                settingsNode.SelectSingleNode("descendant::EnableOxygen").InnerText =
                    EnableOxygen.ToString().ToLower();


                valueNode = root.SelectSingleNode("descendant::Mods");
                if (valueNode == null)
                    root.AppendChild(doc.CreateNode(XmlNodeType.Element, "Mods", null));
                valueNode = root.SelectSingleNode("descendant::Mods");
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

                valueNode = root.SelectSingleNode("descendant::PreviousEnvironmentHostility");
                if (valueNode == null)
                {
                    root.AppendChild(doc.CreateNode(XmlNodeType.Element, "PreviousEnvironmentHostility", null));
                }

                doc.Save(PathHelper.GetSavePath(serv, SaveName) + @"\Sandbox.sbc");

                string text = File.ReadAllText(PathHelper.GetSavePath(serv, SaveName) + @"\Sandbox.sbc");
                text = text.Replace("<PreviousEnvironmentHostility />", "<PreviousEnvironmentHostility xsi:nil=\"true\" />");
                File.WriteAllText(PathHelper.GetSavePath(serv, SaveName) + @"\Sandbox.sbc", text);
            }
        }

        public bool Load(EntityServer server)
        {
            bool confLoad = LoadFromServConf(PathHelper.GetConfigurationFilePath(server));
            bool saveLoad = false;
            if (confLoad)
                saveLoad = LoadFromSave(PathHelper.GetSavePath(server, SaveName) + @"\Sandbox.sbc");

            return confLoad && saveLoad;
        }

        /// <summary>
        /// Load the config file located at "path"
        /// </summary>
        /// <param name="path">the full path to the config file</param>
        /// <returns>true if the load succeded, false if not</returns>
        public bool LoadFromServConf(string path)
        {
            try
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
                    double.TryParse(sessionSettings.Element("InventorySizeMultiplier").Value, out InventorySizeMultiplier);
                if (sessionSettings.Element("AssemblerSpeedMultiplier") != null)
                    double.TryParse(sessionSettings.Element("AssemblerSpeedMultiplier").Value, out AssemblerSpeedMultiplier);
                if (sessionSettings.Element("AssemblerEfficiencyMultiplier") != null)
                    double.TryParse(sessionSettings.Element("AssemblerEfficiencyMultiplier").Value, out AssemblerEfficiencyMultiplier);
                if (sessionSettings.Element("RefinerySpeedMultiplier") != null)
                    double.TryParse(sessionSettings.Element("RefinerySpeedMultiplier").Value, out RefinerySpeedMultiplier);
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
                if (sessionSettings.Element("AutoSaveInMinutes") != null)
                    int.TryParse(sessionSettings.Element("AutoSaveInMinutes").Value, out AutoSaveInMinutes);
                if (sessionSettings.Element("SpawnShipTimeMultiplier") != null)
                    double.TryParse(sessionSettings.Element("SpawnShipTimeMultiplier").Value, out SpawnShipTimeMultiplier);
                if (sessionSettings.Element("DestructibleBlocks") != null)
                    bool.TryParse(sessionSettings.Element("DestructibleBlocks").Value, out DestructibleBlocks);
                if (sessionSettings.Element("EnableIngameScripts") != null)
                    bool.TryParse(sessionSettings.Element("EnableIngameScripts").Value, out EnableIngameScripts);
                if (sessionSettings.Element("ViewDistance") != null)
                    int.TryParse(sessionSettings.Element("ViewDistance").Value, out ViewDistance);
                if (sessionSettings.Element("EnableToolShake") != null)
                    bool.TryParse(sessionSettings.Element("EnableToolShake").Value, out EnableToolShake);

                if (sessionSettings.Element("ProceduralDensity") != null)
                    float.TryParse(sessionSettings.Element("ProceduralDensity").Value, out ProceduralDensity);
                if (sessionSettings.Element("ProceduralSeed") != null)
                {
                    if (sessionSettings.Element("ProceduralSeed").Value.Contains("."))
                        int.TryParse(sessionSettings.Element("ProceduralSeed").Value.Split('.')[0], out ProceduralSeed);
                    else
                        int.TryParse(sessionSettings.Element("ProceduralSeed").Value, out ProceduralSeed);

                }

                if (sessionSettings.Element("VoxelGeneratorVersion") != null)
                    int.TryParse(sessionSettings.Element("VoxelGeneratorVersion").Value, out VoxelGeneratorVersion);
                else
                    VoxelGeneratorVersion = 0;
                if (sessionSettings.Element("EnableOxygen") != null)
                    bool.TryParse(sessionSettings.Element("EnableOxygen").Value, out EnableOxygen);

                if (root.Element("Scenario") != null && root.Element("Scenario").Element("SubtypeId") != null)
                    Enum.TryParse(root.Element("Scenario").Element("SubtypeId").Value, out ScenarioType);
                if (root.Element("LoadWorld") != null)
                    SaveName = PathHelper.GetLastLeaf(root.Element("LoadWorld").Value);
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
                if (root.Element("WorldName") != null)
                    WorldName = root.Element("WorldName").Value;
                if (root.Element("PauseGameWhenEmpty") != null)
                    bool.TryParse(root.Element("PauseGameWhenEmpty").Value, out PauseGameWhenEmpty);
                if (root.Element("IgnoreLastSession") != null)
                    bool.TryParse(root.Element("IgnoreLastSession").Value, out IgnoreLastSession);


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool LoadFromSave(string path)
        {
            if (!File.Exists(path))
                return false;
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Element("MyObjectBuilder_Checkpoint");
            XElement settings = root?.Element("Settings");
            if (settings == null)
                return false;
            if (settings.Element("GameMode") != null)
                Enum.TryParse(settings.Element("GameMode").Value, out GameMode);
            if (settings.Element("InventorySizeMultiplier") != null)
                double.TryParse(settings.Element("InventorySizeMultiplier").Value, out InventorySizeMultiplier);
            if (settings.Element("AssemblerSpeedMultiplier") != null)
                double.TryParse(settings.Element("AssemblerSpeedMultiplier").Value, out AssemblerSpeedMultiplier);
            if (settings.Element("AssemblerEfficiencyMultiplier") != null)
                double.TryParse(settings.Element("AssemblerEfficiencyMultiplier").Value, out AssemblerEfficiencyMultiplier);
            if (settings.Element("RefinerySpeedMultiplier") != null)
                double.TryParse(settings.Element("RefinerySpeedMultiplier").Value, out RefinerySpeedMultiplier);
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
            if (settings.Element("AutoSaveInMinutes") != null)
                int.TryParse(settings.Element("AutoSaveInMinutes").Value, out AutoSaveInMinutes);
            if (settings.Element("SpawnShipTimeMultiplier") != null)
                double.TryParse(settings.Element("SpawnShipTimeMultiplier").Value, out SpawnShipTimeMultiplier);
            if (settings.Element("DestructibleBlocks") != null)
                bool.TryParse(settings.Element("DestructibleBlocks").Value, out DestructibleBlocks);
            if (settings.Element("EnableIngameScripts") != null)
                bool.TryParse(settings.Element("EnableIngameScripts").Value, out EnableIngameScripts);
            if (settings.Element("ViewDistance") != null)
                int.TryParse(settings.Element("ViewDistance").Value, out ViewDistance);
            if (settings.Element("EnableToolShake") != null)
                bool.TryParse(settings.Element("EnableToolShake").Value, out EnableToolShake);
            if (settings.Element("VoxelGeneratorVersion") != null)
                int.TryParse(settings.Element("VoxelGeneratorVersion").Value, out VoxelGeneratorVersion);
            if (settings.Element("EnableOxygen") != null)
                bool.TryParse(settings.Element("EnableOxygen").Value, out EnableOxygen);

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

        public bool LoadFromSaveManager(string path)
        {
            int locMaxplayer = MaxPlayers;
            int locMaxFloatingObjects = MaxFloatingObjects;
            bool locRemoveTrash = RemoveTrash;

            bool ret = LoadFromSave(path);

            MaxPlayers = locMaxplayer;
            MaxFloatingObjects = locMaxFloatingObjects;
            RemoveTrash = locRemoveTrash;

            return ret;
        }
    }
}