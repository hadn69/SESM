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
    public class SEServerConfigHelper : ServerConfigHelperBase
    {
        // Server Grade
        //public string SaveName = SEDefault.SaveName;
        //public string IP = SEDefault.IP;
        public int SteamPort = SEDefault.SteamPort;
        //public int ServerPort = SEDefault.ServerPort;
        public string ServerName = SEDefault.ServerName;
        public bool IgnoreLastSession = SEDefault.IgnoreLastSession;
        public bool PauseGameWhenEmpty = SEDefault.PauseGameWhenEmpty;

        // New Map Grade
        public int AsteroidAmount = SEDefault.AsteroidAmount;
        public SESubTypeId ScenarioType = SEDefault.ScenarioType;
        public float ProceduralDensity = SEDefault.ProceduralDensity;
        public int ProceduralSeed = SEDefault.ProceduralSeed;

        // Map Grade
        // -- Misc
        public bool EnableSpectator = SEDefault.EnableSpectator;
        public bool RealisticSound = SEDefault.RealisticSound;
        //public uint AutoSaveInMinutes = SEDefault.AutoSaveInMinutes;

        // -- Production
        public float InventorySizeMultiplier = SEDefault.InventorySizeMultiplier;
        public float AssemblerSpeedMultiplier = SEDefault.AssemblerSpeedMultiplier;
        public float AssemblerEfficiencyMultiplier = SEDefault.AssemblerEfficiencyMultiplier;
        public float RefinerySpeedMultiplier = SEDefault.RefinerySpeedMultiplier;

        // -- Building
        public GameMode GameMode = SEDefault.GameMode;
        public bool EnableCopyPaste = SEDefault.EnableCopyPaste;
        public float WelderSpeedMultiplier = SEDefault.WelderSpeedMultiplier;
        public float GrinderSpeedMultiplier = SEDefault.GrinderSpeedMultiplier;
        public float HackSpeedMultiplier = SEDefault.HackSpeedMultiplier;
        public bool DestructibleBlocks = SEDefault.DestructibleBlocks;

        // -- Caps
        public int MaxPlayers = SEDefault.MaxPlayers;
        public int MaxFloatingObjects = SEDefault.MaxFloatingObjects;

        // -- Maps
        public string WorldName = SEDefault.WorldName;
        public EnvironmentHostility EnvironmentHostility = SEDefault.EnvironmentHostility;
        public int WorldSizeKm = SEDefault.WorldSizeKm;
        public bool PermanentDeath = SEDefault.PermanentDeath;
        public bool CargoShipsEnabled = SEDefault.CargoShipsEnabled;
        public bool RemoveTrash = SEDefault.RemoveTrash;
        public bool ClientCanSave = SEDefault.ClientCanSave;
        public List<ulong> Mods = SEDefault.Mods;
        public int ViewDistance = SEDefault.ViewDistance;
        public bool EnableEncounters = SEDefault.EnableEncounters;

        // -- Access
        public OnlineMode OnlineMode = SEDefault.OnlineMode;
        public bool ResetOwnership = SEDefault.ResetOwnership;
        public ulong GroupID = SEDefault.GroupID;
        public List<ulong> Administrators = SEDefault.Administrators;
        public List<ulong> Banned = SEDefault.Banned;

        // -- Gameplay
        public bool AutoHealing = SEDefault.AutoHealing;
        public bool WeaponsEnabled = SEDefault.WeaponsEnabled;
        public bool ShowPlayerNamesOnHud = SEDefault.ShowPlayerNamesOnHud;
        public bool ThrusterDamage = SEDefault.ThrusterDamage;
        public float SpawnShipTimeMultiplier = SEDefault.SpawnShipTimeMultiplier;
        public bool RespawnShipDelete = SEDefault.RespawnShipDelete;
        public bool EnableToolShake = SEDefault.EnableToolShake;
        public bool EnableIngameScripts = SEDefault.EnableIngameScripts;
        public int VoxelGeneratorVersion = SEDefault.VoxelGeneratorVersion;
        public bool EnableOxygen = SEDefault.EnableOxygen;
        public bool Enable3rdPersonView = SEDefault.Enable3rdPersonView;

        // -- New 18/09
        public bool EnableFlora = SEDefault.EnableFlora;
        public bool EnableStationVoxelSupport = SEDefault.EnableStationVoxelSupport;
        public bool EnableSunRotation = SEDefault.EnableSunRotation;
        public bool DisableRespawnShips = SEDefault.DisableRespawnShips;
        public bool ScenarioEditMode = SEDefault.ScenarioEditMode;
        public bool Battle = SEDefault.Battle;
        public bool Scenario = SEDefault.Scenario;
        public bool CanJoinRunning = SEDefault.CanJoinRunning;
        public int PhysicsIterations = SEDefault.PhysicsIterations;
        public float SunRotationIntervalMinutes = SEDefault.SunRotationIntervalMinutes;
        public bool EnableJetpack = SEDefault.EnableJetpack;
        public bool SpawnWithTools = SEDefault.SpawnWithTools;
        public bool StartInRespawnScreen = SEDefault.StartInRespawnScreen;
        public bool EnableVoxelDestruction = SEDefault.EnableVoxelDestruction;
        public int MaxDrones = SEDefault.MaxDrones;
        public bool EnableDrones = SEDefault.EnableDrones;

        public override void Save(EntityServer serv)
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
            sb.AppendLine("    <ViewDistance>" + ViewDistance + "</ViewDistance>");
            sb.AppendLine("    <EnableToolShake>" + EnableToolShake.ToString().ToLower() + "</EnableToolShake>");
            sb.AppendLine("    <VoxelGeneratorVersion>" + VoxelGeneratorVersion + "</VoxelGeneratorVersion>");
            sb.AppendLine("    <EnableOxygen>" + EnableOxygen.ToString().ToLower() + "</EnableOxygen>");
            sb.AppendLine("    <Enable3rdPersonView>" + Enable3rdPersonView.ToString().ToLower() + "</Enable3rdPersonView>");
            sb.AppendLine("    <EnableEncounters>" + EnableEncounters.ToString().ToLower() + "</EnableEncounters>");
            sb.AppendLine("    <EnableFlora>" + EnableFlora.ToString().ToLower() + "</EnableFlora>");
            sb.AppendLine("    <EnableStationVoxelSupport>" + EnableStationVoxelSupport.ToString().ToLower() + "</EnableStationVoxelSupport>");
            sb.AppendLine("    <EnableSunRotation>" + EnableSunRotation.ToString().ToLower() + "</EnableSunRotation>");
            sb.AppendLine("    <DisableRespawnShips>" + DisableRespawnShips.ToString().ToLower() + "</DisableRespawnShips>");
            sb.AppendLine("    <ScenarioEditMode>" + ScenarioEditMode.ToString().ToLower() + "</ScenarioEditMode>");
            sb.AppendLine("    <Battle>" + Battle.ToString().ToLower() + "</Battle>");
            sb.AppendLine("    <Scenario>" + Scenario.ToString().ToLower() + "</Scenario>");
            sb.AppendLine("    <CanJoinRunning>" + CanJoinRunning.ToString().ToLower() + "</CanJoinRunning>");
            sb.AppendLine("    <PhysicsIterations>" + PhysicsIterations + "</PhysicsIterations>");
            sb.AppendLine("    <SunRotationIntervalMinutes>" + SunRotationIntervalMinutes + "</SunRotationIntervalMinutes>");
            sb.AppendLine("    <EnableJetpack>" + EnableJetpack.ToString().ToLower() + "</EnableJetpack>");
            sb.AppendLine("    <SpawnWithTools>" + SpawnWithTools.ToString().ToLower() + "</SpawnWithTools>");
            sb.AppendLine("    <StartInRespawnScreen>" + StartInRespawnScreen.ToString().ToLower() + "</StartInRespawnScreen>");
            sb.AppendLine("    <EnableVoxelDestruction>" + EnableVoxelDestruction.ToString().ToLower() + "</EnableVoxelDestruction>");
            sb.AppendLine("    <MaxDrones>" + MaxDrones + "</MaxDrones>");
            sb.AppendLine("    <EnableDrones>" + EnableDrones.ToString().ToLower() + "</EnableDrones>");

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


                valueNode = settingsNode.SelectSingleNode("descendant::VoxelGeneratorVersion");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "VoxelGeneratorVersion", null));
                settingsNode.SelectSingleNode("descendant::VoxelGeneratorVersion").InnerText =
                    VoxelGeneratorVersion.ToString();
  

                valueNode = settingsNode.SelectSingleNode("descendant::EnableOxygen");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableOxygen", null));
                settingsNode.SelectSingleNode("descendant::EnableOxygen").InnerText =
                    EnableOxygen.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::Enable3rdPersonView");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "Enable3rdPersonView", null));
                settingsNode.SelectSingleNode("descendant::Enable3rdPersonView").InnerText =
                    Enable3rdPersonView.ToString().ToLower();
                

                valueNode = settingsNode.SelectSingleNode("descendant::EnableEncounters");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableEncounters", null));
                settingsNode.SelectSingleNode("descendant::EnableEncounters").InnerText =
                    EnableEncounters.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableFlora");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableFlora", null));
                settingsNode.SelectSingleNode("descendant::EnableFlora").InnerText =
                    EnableFlora.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableStationVoxelSupport");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableStationVoxelSupport", null));
                settingsNode.SelectSingleNode("descendant::EnableStationVoxelSupport").InnerText =
                    EnableStationVoxelSupport.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableSunRotation");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableSunRotation", null));
                settingsNode.SelectSingleNode("descendant::EnableSunRotation").InnerText =
                    EnableSunRotation.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::DisableRespawnShips");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "DisableRespawnShips", null));
                settingsNode.SelectSingleNode("descendant::DisableRespawnShips").InnerText =
                    DisableRespawnShips.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::ScenarioEditMode");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "ScenarioEditMode", null));
                settingsNode.SelectSingleNode("descendant::ScenarioEditMode").InnerText =
                    ScenarioEditMode.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::Battle");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "Battle", null));
                settingsNode.SelectSingleNode("descendant::Battle").InnerText =
                    Battle.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::Scenario");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "Scenario", null));
                settingsNode.SelectSingleNode("descendant::Scenario").InnerText =
                    Scenario.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::CanJoinRunning");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "CanJoinRunning", null));
                settingsNode.SelectSingleNode("descendant::CanJoinRunning").InnerText =
                    CanJoinRunning.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::PhysicsIterations");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "PhysicsIterations", null));
                settingsNode.SelectSingleNode("descendant::PhysicsIterations").InnerText =
                    PhysicsIterations.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::SunRotationIntervalMinutes");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "SunRotationIntervalMinutes", null));
                settingsNode.SelectSingleNode("descendant::SunRotationIntervalMinutes").InnerText =
                    SunRotationIntervalMinutes.ToString().Replace(',', '.');


                valueNode = settingsNode.SelectSingleNode("descendant::EnableJetpack");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableJetpack", null));
                settingsNode.SelectSingleNode("descendant::EnableJetpack").InnerText =
                    EnableJetpack.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::SpawnWithTools");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "SpawnWithTools", null));
                settingsNode.SelectSingleNode("descendant::SpawnWithTools").InnerText =
                    SpawnWithTools.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::StartInRespawnScreen");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "StartInRespawnScreen", null));
                settingsNode.SelectSingleNode("descendant::StartInRespawnScreen").InnerText =
                    StartInRespawnScreen.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableVoxelDestruction");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableVoxelDestruction", null));
                settingsNode.SelectSingleNode("descendant::EnableVoxelDestruction").InnerText =
                    EnableVoxelDestruction.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::MaxDrones");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "MaxDrones", null));
                settingsNode.SelectSingleNode("descendant::MaxDrones").InnerText =
                    MaxDrones.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableDrones");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableDrones", null));
                settingsNode.SelectSingleNode("descendant::EnableDrones").InnerText =
                    EnableDrones.ToString().ToLower();




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

        public override bool Load(EntityServer server)
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
        public override bool LoadFromServConf(string path)
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
                    float.TryParse(sessionSettings.Element("InventorySizeMultiplier").Value, out InventorySizeMultiplier);
                if (sessionSettings.Element("AssemblerSpeedMultiplier") != null)
                    float.TryParse(sessionSettings.Element("AssemblerSpeedMultiplier").Value, out AssemblerSpeedMultiplier);
                if (sessionSettings.Element("AssemblerEfficiencyMultiplier") != null)
                    float.TryParse(sessionSettings.Element("AssemblerEfficiencyMultiplier").Value, out AssemblerEfficiencyMultiplier);
                if (sessionSettings.Element("RefinerySpeedMultiplier") != null)
                    float.TryParse(sessionSettings.Element("RefinerySpeedMultiplier").Value, out RefinerySpeedMultiplier);
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
                    float.TryParse(sessionSettings.Element("WelderSpeedMultiplier").Value, out WelderSpeedMultiplier);
                if (sessionSettings.Element("GrinderSpeedMultiplier") != null)
                    float.TryParse(sessionSettings.Element("GrinderSpeedMultiplier").Value, out GrinderSpeedMultiplier);
                if (sessionSettings.Element("RealisticSound") != null)
                    bool.TryParse(sessionSettings.Element("RealisticSound").Value, out RealisticSound);
                if (sessionSettings.Element("ClientCanSave") != null)
                    bool.TryParse(sessionSettings.Element("ClientCanSave").Value, out ClientCanSave);
                if (sessionSettings.Element("HackSpeedMultiplier") != null)
                    float.TryParse(sessionSettings.Element("HackSpeedMultiplier").Value, out HackSpeedMultiplier);
                if (sessionSettings.Element("PermanentDeath") != null)
                    bool.TryParse(sessionSettings.Element("PermanentDeath").Value, out PermanentDeath);
                if (sessionSettings.Element("AutoSaveInMinutes") != null)
                    uint.TryParse(sessionSettings.Element("AutoSaveInMinutes").Value, out AutoSaveInMinutes);
                if (sessionSettings.Element("SpawnShipTimeMultiplier") != null)
                    float.TryParse(sessionSettings.Element("SpawnShipTimeMultiplier").Value, out SpawnShipTimeMultiplier);
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
                if (sessionSettings.Element("Enable3rdPersonView") != null)
                    bool.TryParse(sessionSettings.Element("Enable3rdPersonView").Value, out Enable3rdPersonView);
                if (sessionSettings.Element("EnableFlora") != null)
                    bool.TryParse(sessionSettings.Element("EnableFlora").Value, out EnableFlora);
                if (sessionSettings.Element("EnableStationVoxelSupport") != null)
                    bool.TryParse(sessionSettings.Element("EnableStationVoxelSupport").Value, out EnableStationVoxelSupport);
                if (sessionSettings.Element("EnableSunRotation") != null)
                    bool.TryParse(sessionSettings.Element("EnableSunRotation").Value, out EnableSunRotation);
                if (sessionSettings.Element("DisableRespawnShips") != null)
                    bool.TryParse(sessionSettings.Element("DisableRespawnShips").Value, out DisableRespawnShips);
                if (sessionSettings.Element("ScenarioEditMode") != null)
                    bool.TryParse(sessionSettings.Element("ScenarioEditMode").Value, out ScenarioEditMode);
                if (sessionSettings.Element("Battle") != null)
                    bool.TryParse(sessionSettings.Element("Battle").Value, out Battle);
                if (sessionSettings.Element("Scenario") != null)
                    bool.TryParse(sessionSettings.Element("Scenario").Value, out Scenario);
                if (sessionSettings.Element("CanJoinRunning") != null)
                    bool.TryParse(sessionSettings.Element("CanJoinRunning").Value, out CanJoinRunning);
                if (sessionSettings.Element("PhysicsIterations") != null)
                    int.TryParse(sessionSettings.Element("PhysicsIterations").Value, out PhysicsIterations);
                if (sessionSettings.Element("SunRotationIntervalMinutes") != null)
                    float.TryParse(sessionSettings.Element("SunRotationIntervalMinutes").Value, out SunRotationIntervalMinutes);
                if (sessionSettings.Element("EnableJetpack") != null)
                    bool.TryParse(sessionSettings.Element("EnableJetpack").Value, out EnableJetpack);
                if (sessionSettings.Element("SpawnWithTools") != null)
                    bool.TryParse(sessionSettings.Element("SpawnWithTools").Value, out SpawnWithTools);
                if (sessionSettings.Element("StartInRespawnScreen") != null)
                    bool.TryParse(sessionSettings.Element("StartInRespawnScreen").Value, out StartInRespawnScreen);
                if (sessionSettings.Element("EnableVoxelDestruction") != null)
                    bool.TryParse(sessionSettings.Element("EnableVoxelDestruction").Value, out EnableVoxelDestruction);
                if (sessionSettings.Element("MaxDrones") != null)
                    int.TryParse(sessionSettings.Element("MaxDrones").Value, out MaxDrones);
                if (sessionSettings.Element("EnableDrones") != null)
                    bool.TryParse(sessionSettings.Element("EnableDrones").Value, out EnableDrones);

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

        public override bool LoadFromSave(string path)
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
                float.TryParse(settings.Element("InventorySizeMultiplier").Value, out InventorySizeMultiplier);
            if (settings.Element("AssemblerSpeedMultiplier") != null)
                float.TryParse(settings.Element("AssemblerSpeedMultiplier").Value, out AssemblerSpeedMultiplier);
            if (settings.Element("AssemblerEfficiencyMultiplier") != null)
                float.TryParse(settings.Element("AssemblerEfficiencyMultiplier").Value, out AssemblerEfficiencyMultiplier);
            if (settings.Element("RefinerySpeedMultiplier") != null)
                float.TryParse(settings.Element("RefinerySpeedMultiplier").Value, out RefinerySpeedMultiplier);
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
                float.TryParse(settings.Element("WelderSpeedMultiplier").Value, out WelderSpeedMultiplier);
            if (settings.Element("GrinderSpeedMultiplier") != null)
                float.TryParse(settings.Element("GrinderSpeedMultiplier").Value, out GrinderSpeedMultiplier);
            if (settings.Element("RealisticSound") != null)
                bool.TryParse(settings.Element("RealisticSound").Value, out RealisticSound);
            if (settings.Element("ClientCanSave") != null)
                bool.TryParse(settings.Element("ClientCanSave").Value, out ClientCanSave);
            if (settings.Element("HackSpeedMultiplier") != null)
                float.TryParse(settings.Element("HackSpeedMultiplier").Value, out HackSpeedMultiplier);
            if (settings.Element("PermanentDeath") != null)
                bool.TryParse(settings.Element("PermanentDeath").Value, out PermanentDeath);
            if (settings.Element("AutoSaveInMinutes") != null)
                uint.TryParse(settings.Element("AutoSaveInMinutes").Value, out AutoSaveInMinutes);
            if (settings.Element("SpawnShipTimeMultiplier") != null)
                float.TryParse(settings.Element("SpawnShipTimeMultiplier").Value, out SpawnShipTimeMultiplier);
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
            if (settings.Element("Enable3rdPersonView") != null)
                bool.TryParse(settings.Element("Enable3rdPersonView").Value, out Enable3rdPersonView);
            if (settings.Element("EnableEncounters") != null)
                bool.TryParse(settings.Element("EnableEncounters").Value, out EnableEncounters);
            if (settings.Element("EnableFlora") != null)
                bool.TryParse(settings.Element("EnableFlora").Value, out EnableFlora);
            if (settings.Element("EnableStationVoxelSupport") != null)
                bool.TryParse(settings.Element("EnableStationVoxelSupport").Value, out EnableStationVoxelSupport);
            if (settings.Element("EnableSunRotation") != null)
                bool.TryParse(settings.Element("EnableSunRotation").Value, out EnableSunRotation);
            if (settings.Element("DisableRespawnShips") != null)
                bool.TryParse(settings.Element("DisableRespawnShips").Value, out DisableRespawnShips);
            if (settings.Element("ScenarioEditMode") != null)
                bool.TryParse(settings.Element("ScenarioEditMode").Value, out ScenarioEditMode);
            if (settings.Element("Battle") != null)
                bool.TryParse(settings.Element("Battle").Value, out Battle);
            if (settings.Element("Scenario") != null)
                bool.TryParse(settings.Element("Scenario").Value, out Scenario);
            if (settings.Element("CanJoinRunning") != null)
                bool.TryParse(settings.Element("CanJoinRunning").Value, out CanJoinRunning);
            if (settings.Element("PhysicsIterations") != null)
                int.TryParse(settings.Element("PhysicsIterations").Value, out PhysicsIterations);
            if (settings.Element("SunRotationIntervalMinutes") != null)
                float.TryParse(settings.Element("SunRotationIntervalMinutes").Value, out SunRotationIntervalMinutes);
            if (settings.Element("EnableJetpack") != null)
                bool.TryParse(settings.Element("EnableJetpack").Value, out EnableJetpack);
            if (settings.Element("SpawnWithTools") != null)
                bool.TryParse(settings.Element("SpawnWithTools").Value, out SpawnWithTools);
            if (settings.Element("StartInRespawnScreen") != null)
                bool.TryParse(settings.Element("StartInRespawnScreen").Value, out StartInRespawnScreen);
            if (settings.Element("EnableVoxelDestruction") != null)
                bool.TryParse(settings.Element("EnableVoxelDestruction").Value, out EnableVoxelDestruction);
            if (settings.Element("MaxDrones") != null)
                int.TryParse(settings.Element("MaxDrones").Value, out MaxDrones);
            if (settings.Element("EnableDrones") != null)
                bool.TryParse(settings.Element("EnableDrones").Value, out EnableDrones);


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

        public override bool LoadFromSaveManager(string path)
        {
            int locMaxplayer = MaxPlayers;
            int locMaxFloatingObjects = MaxFloatingObjects;

            bool ret = LoadFromSave(path);

            MaxPlayers = locMaxplayer;
            MaxFloatingObjects = locMaxFloatingObjects;

            return ret;
        }
    }
}