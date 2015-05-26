
namespace SESM.DTO
{
    public enum EnumServerPerm : int
    {
        // 1000+ : Global Server
        SERVER_INFO = 1000,

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
        SERVER_SETTINGS_JOBS_AUTOSTART_WR = 1123
    }
}