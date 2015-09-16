﻿
namespace SESM.DTO
{
    public enum EnumServerPerm : int
    {
        // 1000+ : Global Server
        SERVER_INFO = 1000,
        SERVER_DELETE = 1001,
        ACCESS_SERVER_EDIT_USERS = 1002,

        // 1010+ : Server Power
        SERVER_START = 1011,
        SERVER_STOP = 1012,
        SERVER_RESTART = 1013,
        SERVER_KILL = 1014,

        // 1100+ : Server Settings
        // 1110+ : Server global Settings
        SERVER_SETTINGS_GLOBAL_RD = 1110,
        SERVER_SETTINGS_GLOBAL_NAME_WR = 1111,
        SERVER_SETTINGS_GLOBAL_USESESE_WR = 1112,
        SERVER_SETTINGS_GLOBAL_PROCESSPRIO_WR = 1113,
        SERVER_SETTINGS_GLOBAL_PUBLIC_WR = 1114,

        // 1120+ : Server jobs Settings
        SERVER_SETTINGS_JOBS_RD = 1120,
        SERVER_SETTINGS_JOBS_AUTORESTART_WR = 1121,
        SERVER_SETTINGS_JOBS_AUTORESTARTCRON_WR = 1122,
        SERVER_SETTINGS_JOBS_AUTOSTART_WR = 1123,

        // 1130+ : Server Backup Settings
        SERVER_SETTINGS_BACKUPS_RD = 1130,
        SERVER_SETTINGS_BACKUPS_LVL1_WR = 1131,
        SERVER_SETTINGS_BACKUPS_LVL2_WR = 1132,
        SERVER_SETTINGS_BACKUPS_LVL3_WR = 1133,

        // 1200+ : SE Server Configuration
        SERVER_CONFIG_SE_RD = 1200,
        SERVER_CONFIG_SE_IP_WR = 1201,
        SERVER_CONFIG_SE_STEAMPORT_WR = 1202,
        SERVER_CONFIG_SE_SERVERPORT_WR = 1203,
        SERVER_CONFIG_SE_SERVERNAME_WR = 1204,
        SERVER_CONFIG_SE_IGNORELASTSESSION_WR = 1205,
        SERVER_CONFIG_SE_PAUSEGAMEWHENEMPTY_WR = 1206,
        SERVER_CONFIG_SE_ENABLESPECTATOR_WR = 1207,
        SERVER_CONFIG_SE_REALISTICSOUND_WR = 1208,
        SERVER_CONFIG_SE_AUTOSAVEINMINUTES_WR = 1209,
        SERVER_CONFIG_SE_INVENTORYSIZEMULTIPLIER_WR = 1210,
        SERVER_CONFIG_SE_ASSEMBLERSPEEDMULTIPLIER_WR = 1211,
        SERVER_CONFIG_SE_ASSEMBLEREFFICIENCYMULTIPLIER_WR = 1212,
        SERVER_CONFIG_SE_REFINERYSPEEDMULTIPLIER_WR = 1213,
        SERVER_CONFIG_SE_GAMEMODE_WR = 1214,
        SERVER_CONFIG_SE_ENABLECOPYPASTE_WR = 1215,
        SERVER_CONFIG_SE_WELDERSPEEDMULTIPLIER_WR = 1216,
        SERVER_CONFIG_SE_GRINDERSPEEDMULTIPLIER_WR = 1217,
        SERVER_CONFIG_SE_HACKSPEEDMULTIPLIER_WR = 1218,
        SERVER_CONFIG_SE_DESTRUCTIBLEBLOCKS_WR = 1219,
        SERVER_CONFIG_SE_MAXPLAYERS_WR = 1220,
        SERVER_CONFIG_SE_MAXFLOATINGOBJECTS_WR = 1221,
        SERVER_CONFIG_SE_WORLDNAME_WR = 1222,
        SERVER_CONFIG_SE_ENVIRONMENTHOSTILITY_WR = 1223,
        SERVER_CONFIG_SE_WORLDSIZEKM_WR = 1224,
        SERVER_CONFIG_SE_PERMANENTDEATH_WR = 1225,
        SERVER_CONFIG_SE_CARGOSHIPSENABLED_WR = 1226,
        SERVER_CONFIG_SE_REMOVETRASH_WR = 1227,
        SERVER_CONFIG_SE_CLIENTCANSAVE_WR = 1228,
        SERVER_CONFIG_SE_MODS_WR = 1229,
        SERVER_CONFIG_SE_VIEWDISTANCE_WR = 1230,
        SERVER_CONFIG_SE_ONLINEMODE_WR = 1231,
        SERVER_CONFIG_SE_RESETOWNERSHIP_WR = 1232,
        SERVER_CONFIG_SE_GROUPID_WR = 1233,
        SERVER_CONFIG_SE_ADMINISTRATORS_WR = 1234,
        SERVER_CONFIG_SE_BANNED_WR = 1235,
        SERVER_CONFIG_SE_AUTOHEALING_WR = 1236,
        SERVER_CONFIG_SE_WEAPONSENABLED_WR = 1237,
        SERVER_CONFIG_SE_SHOWPLAYERNAMESONHUD_WR = 1238,
        SERVER_CONFIG_SE_THRUSTERDAMAGE_WR = 1239,
        SERVER_CONFIG_SE_SPAWNSHIPTIMEMULTIPLIER_WR = 1240,
        SERVER_CONFIG_SE_RESPAWNSHIPDELETE_WR = 1241,
        SERVER_CONFIG_SE_ENABLETOOLSHAKE_WR = 1242,
        SERVER_CONFIG_SE_ENABLEINGAMESCRIPTS_WR = 1243,
        SERVER_CONFIG_SE_VOXELGENERATORVERSION_WR = 1244,
        SERVER_CONFIG_SE_ENABLEOXYGEN_WR = 1245,
        SERVER_CONFIG_SE_ENABLE3RDPERSONVIEW_WR = 1246,
        SERVER_CONFIG_SE_ENABLEENCOUNTERS_WR = 1247,

        // 1300+ : ME Server Configuration
        SERVER_CONFIG_ME_RD = 1300,
        SERVER_CONFIG_ME_IP_WR = 1301,
        SERVER_CONFIG_ME_STEAMPORT_WR = 1302,
        SERVER_CONFIG_ME_SERVERPORT_WR = 1303,
        SERVER_CONFIG_ME_SERVERNAME_WR = 1304,
        SERVER_CONFIG_ME_IGNORELASTSESSION_WR = 1305,
        SERVER_CONFIG_ME_PAUSEGAMEWHENEMPTY_WR = 1306,
        SERVER_CONFIG_ME_ENABLESPECTATOR_WR = 1307,
        SERVER_CONFIG_ME_AUTOSAVEINMINUTES_WR = 1308,
        SERVER_CONFIG_ME_GAMEMODE_WR = 1309,
        SERVER_CONFIG_ME_ENABLECOPYPASTE_WR = 1310,
        SERVER_CONFIG_ME_MAXPLAYERS_WR = 1311,
        SERVER_CONFIG_ME_WORLDNAME_WR = 1312,
        SERVER_CONFIG_ME_MODS_WR = 1313,
        SERVER_CONFIG_ME_ONLINEMODE_WR = 1314,
        SERVER_CONFIG_ME_GROUPID_WR = 1315,
        SERVER_CONFIG_ME_ADMINISTRATORS_WR = 1316,
        SERVER_CONFIG_ME_BANNED_WR = 1317,
        SERVER_CONFIG_ME_CLIENTCANSAVE_WR = 1318,
        SERVER_CONFIG_ME_ENABLESTRUCTURALSIMULATION_WR = 1319,
        SERVER_CONFIG_ME_MAXACTIVEFRACTUREPIECES_WR = 1320,
        SERVER_CONFIG_ME_ENABLEBARBARIANS_WR = 1321,
        SERVER_CONFIG_ME_MAXIMUMBOTS_WR = 1322,
        SERVER_CONFIG_ME_GAMEDAYINREALMINUTES_WR = 1323,
        SERVER_CONFIG_ME_DAYNIGHTRATIO_WR = 1324,
        SERVER_CONFIG_ME_ENABLEANIMALS_WR = 1325,

        // 1400+ : Server Explorer
        SERVER_EXPLORER_LIST = 1400,
        SERVER_EXPLORER_DELETE = 1401,
        SERVER_EXPLORER_RENAME = 1402,
        SERVER_EXPLORER_RENAMETODLL = 1403,
        SERVER_EXPLORER_DOWNLOAD = 1404,
        SERVER_EXPLORER_CREATE_FOLDER = 1405,
        SERVER_EXPLORER_CREATE_FILE = 1406,
        SERVER_EXPLORER_UPLOAD = 1407,
        SERVER_EXPLORER_UPLOADDLL = 1408,
        SERVER_EXPLORER_READFILE = 1409,
        SERVER_EXPLORER_WRITEFILE = 1410,
    }
}