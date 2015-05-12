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
    public class MEServerConfigHelper : ServerConfigHelperBase
    {
        // Server Grade
        //public string SaveName = MEDefault.SaveName;
        //public string IP = MEDefault.IP;
        public int SteamPort = MEDefault.SteamPort;
        //public int ServerPort = MEDefault.ServerPort;
        public string ServerName = MEDefault.ServerName;
        public bool IgnoreLastSession = MEDefault.IgnoreLastSession;
        public bool PauseGameWhenEmpty = MEDefault.PauseGameWhenEmpty;

        // New Map Grade
        public int AsteroidAmount = MEDefault.AsteroidAmount;
        public MESubTypeId ScenarioType = MEDefault.ScenarioType;

        // Map Grade
        // -- Misc
        public bool EnableSpectator = MEDefault.EnableSpectator;
        //public uint AutoSaveInMinutes = MEDefault.AutoSaveInMinutes;

        // -- Building
        public GameMode GameMode = MEDefault.GameMode;
        public bool EnableCopyPaste = MEDefault.EnableCopyPaste;
        public bool EnableStructuralSimulation = MEDefault.EnableStructuralSimulation;
        public uint MaxActiveFracturePieces = MEDefault.MaxActiveFracturePieces;

        // -- Caps
        public int MaxPlayers = MEDefault.MaxPlayers;

        // -- Maps
        public string WorldName = MEDefault.WorldName;
        public bool ClientCanSave = MEDefault.ClientCanSave;
        public List<ulong> Mods = MEDefault.Mods;
        
        // -- Access
        public OnlineMode OnlineMode = MEDefault.OnlineMode;
        public ulong GroupID = MEDefault.GroupID;
        public List<ulong> Administrators = MEDefault.Administrators;
        public List<ulong> Banned = MEDefault.Banned;

        // -- Gameplay
        public bool EnableBarbarians = MEDefault.EnableBarbarians;
        public uint MaximumBots = MEDefault.MaximumBots;
        public uint GameDayInRealMinutes = MEDefault.GameDayInRealMinutes;
        public double DayNightRatio = MEDefault.DayNightRatio;
        public bool EnableAnimals = MEDefault.EnableAnimals;

        public override void Save(EntityServer serv)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<MyConfigDedicated xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine("  <SessionSettings>");
            sb.AppendLine("    <GameMode>" + GameMode.ToString() + "</GameMode>");
            sb.AppendLine("    <OnlineMode>" + OnlineMode.ToString() + "</OnlineMode>");
            sb.AppendLine("    <MaxPlayers>" + MaxPlayers + "</MaxPlayers>");
            sb.AppendLine("    <EnableCopyPaste>" + EnableCopyPaste.ToString().ToLower() + "</EnableCopyPaste>");
            sb.AppendLine("    <EnableStructuralSimulation>" + EnableStructuralSimulation.ToString().ToLower() + "</EnableStructuralSimulation>");
            sb.AppendLine("    <EnableSpectator>" + EnableSpectator.ToString().ToLower() + "</EnableSpectator>");
            sb.AppendLine("    <MaxActiveFracturePieces>" + MaxActiveFracturePieces + "</MaxActiveFracturePieces>");
            sb.AppendLine("    <ClientCanSave>" + ClientCanSave.ToString().ToLower() + "</ClientCanSave>");
            sb.AppendLine("    <AutoSaveInMinutes>" + AutoSaveInMinutes + "</AutoSaveInMinutes>");
            sb.AppendLine("    <EnableBarbarians>" + EnableBarbarians.ToString().ToLower() + "</EnableBarbarians>");
            sb.AppendLine("    <MaximumBots>" + MaximumBots + "</MaximumBots>");
            sb.AppendLine("    <GameDayInRealMinutes>" + GameDayInRealMinutes + "</GameDayInRealMinutes>");
            sb.AppendLine("    <DayNightRatio>" + DayNightRatio + "</DayNightRatio>");
            sb.AppendLine("    <EnableAnimals>" + EnableAnimals.ToString().ToLower() + "</EnableAnimals>");
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


                valueNode = settingsNode.SelectSingleNode("descendant::OnlineMode");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "OnlineMode", null));
                settingsNode.SelectSingleNode("descendant::OnlineMode").InnerText = OnlineMode.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::MaxPlayers");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "MaxPlayers", null));
                settingsNode.SelectSingleNode("descendant::MaxPlayers").InnerText = MaxPlayers.ToString();

                
                valueNode = settingsNode.SelectSingleNode("descendant::EnableCopyPaste");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableCopyPaste", null));
                settingsNode.SelectSingleNode("descendant::EnableCopyPaste").InnerText =
                    EnableCopyPaste.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableStructuralSimulation");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableStructuralSimulation", null));
                settingsNode.SelectSingleNode("descendant::EnableStructuralSimulation").InnerText =
                    EnableStructuralSimulation.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableSpectator");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableSpectator", null));
                settingsNode.SelectSingleNode("descendant::EnableSpectator").InnerText =
                    EnableSpectator.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::MaxActiveFracturePieces");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "MaxActiveFracturePieces", null));
                settingsNode.SelectSingleNode("descendant::MaxActiveFracturePieces").InnerText =
                    MaxActiveFracturePieces.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::ClientCanSave");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "ClientCanSave", null));
                settingsNode.SelectSingleNode("descendant::ClientCanSave").InnerText =
                    ClientCanSave.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::AutoSaveInMinutes");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "AutoSaveInMinutes", null));
                settingsNode.SelectSingleNode("descendant::AutoSaveInMinutes").InnerText =
                    AutoSaveInMinutes.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableBarbarians");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableBarbarians", null));
                settingsNode.SelectSingleNode("descendant::EnableBarbarians").InnerText =
                    EnableBarbarians.ToString().ToLower();


                valueNode = settingsNode.SelectSingleNode("descendant::MaximumBots");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "MaximumBots", null));
                settingsNode.SelectSingleNode("descendant::MaximumBots").InnerText =
                    MaximumBots.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::GameDayInRealMinutes");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "GameDayInRealMinutes", null));
                settingsNode.SelectSingleNode("descendant::GameDayInRealMinutes").InnerText =
                    GameDayInRealMinutes.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::DayNightRatio");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "DayNightRatio", null));
                settingsNode.SelectSingleNode("descendant::DayNightRatio").InnerText =
                    DayNightRatio.ToString();


                valueNode = settingsNode.SelectSingleNode("descendant::EnableAnimals");
                if (valueNode == null)
                    settingsNode.AppendChild(doc.CreateNode(XmlNodeType.Element, "EnableAnimals", null));
                settingsNode.SelectSingleNode("descendant::EnableAnimals").InnerText =
                    EnableAnimals.ToString().ToLower();


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
                if (sessionSettings.Element("OnlineMode") != null)
                    Enum.TryParse(sessionSettings.Element("OnlineMode").Value, out OnlineMode);
                if (sessionSettings.Element("MaxPlayers") != null)
                    int.TryParse(sessionSettings.Element("MaxPlayers").Value, out MaxPlayers);
                if (sessionSettings.Element("EnableCopyPaste") != null)
                    bool.TryParse(sessionSettings.Element("EnableCopyPaste").Value, out EnableCopyPaste);
                if (sessionSettings.Element("EnableStructuralSimulation") != null)
                    bool.TryParse(sessionSettings.Element("EnableStructuralSimulation").Value, out EnableStructuralSimulation);
                if (sessionSettings.Element("MaxActiveFracturePieces") != null)
                    uint.TryParse(sessionSettings.Element("MaxActiveFracturePieces").Value, out MaxActiveFracturePieces);
                if (sessionSettings.Element("EnableSpectator") != null)
                    bool.TryParse(sessionSettings.Element("EnableSpectator").Value, out EnableSpectator);
                if (sessionSettings.Element("ClientCanSave") != null)
                    bool.TryParse(sessionSettings.Element("ClientCanSave").Value, out ClientCanSave);
                if (sessionSettings.Element("AutoSaveInMinutes") != null)
                    uint.TryParse(sessionSettings.Element("AutoSaveInMinutes").Value, out AutoSaveInMinutes);
                if (sessionSettings.Element("EnableBarbarians") != null)
                    bool.TryParse(sessionSettings.Element("EnableBarbarians").Value, out EnableBarbarians);
                if (sessionSettings.Element("MaximumBots") != null)
                    uint.TryParse(sessionSettings.Element("MaximumBots").Value, out MaximumBots);
                if (sessionSettings.Element("GameDayInRealMinutes") != null)
                    uint.TryParse(sessionSettings.Element("GameDayInRealMinutes").Value, out GameDayInRealMinutes);
                if (sessionSettings.Element("DayNightRatio") != null)
                    double.TryParse(sessionSettings.Element("DayNightRatio").Value, out DayNightRatio);
                if (sessionSettings.Element("EnableAnimals") != null)
                    bool.TryParse(sessionSettings.Element("EnableAnimals").Value, out EnableAnimals);

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
            if (settings.Element("OnlineMode") != null)
                Enum.TryParse(settings.Element("OnlineMode").Value, out OnlineMode);
            if (settings.Element("MaxPlayers") != null)
                int.TryParse(settings.Element("MaxPlayers").Value, out MaxPlayers);
            if (settings.Element("EnableCopyPaste") != null)
                bool.TryParse(settings.Element("EnableCopyPaste").Value, out EnableCopyPaste);
            if (settings.Element("EnableStructuralSimulation") != null)
                bool.TryParse(settings.Element("EnableStructuralSimulation").Value, out EnableStructuralSimulation);
            if (settings.Element("MaxActiveFracturePieces") != null)
                uint.TryParse(settings.Element("MaxActiveFracturePieces").Value, out MaxActiveFracturePieces);
            if (settings.Element("EnableSpectator") != null)
                bool.TryParse(settings.Element("EnableSpectator").Value, out EnableSpectator);
            if (settings.Element("ClientCanSave") != null)
                bool.TryParse(settings.Element("ClientCanSave").Value, out ClientCanSave);
            if (settings.Element("AutoSaveInMinutes") != null)
                uint.TryParse(settings.Element("AutoSaveInMinutes").Value, out AutoSaveInMinutes);
            if (settings.Element("EnableBarbarians") != null)
                bool.TryParse(settings.Element("EnableBarbarians").Value, out EnableBarbarians);
            if (settings.Element("MaximumBots") != null)
                uint.TryParse(settings.Element("MaximumBots").Value, out MaximumBots);
            if (settings.Element("GameDayInRealMinutes") != null)
                uint.TryParse(settings.Element("GameDayInRealMinutes").Value, out GameDayInRealMinutes);
            if (settings.Element("DayNightRatio") != null)
                double.TryParse(settings.Element("DayNightRatio").Value, out DayNightRatio);
            if (settings.Element("EnableAnimals") != null)
                bool.TryParse(settings.Element("EnableAnimals").Value, out EnableAnimals);

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
            uint locMaxActiveFracturePieces = MaxActiveFracturePieces;
            uint locMaximumBots = MaximumBots;

            bool ret = LoadFromSave(path);

            MaxPlayers = locMaxplayer;
            MaxActiveFracturePieces = locMaxActiveFracturePieces;
            MaximumBots = locMaximumBots;

            return ret;
        }
    }
}