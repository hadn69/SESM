namespace SESM.DTO
{
    public enum EnumInstancePerm : int
    {
        // 100+ : Power-Cycling
        START_SERVER = 101,
        STOP_SERVER = 102,
        KILL_SERVER = 103,

        // 200+ : Server Settings
        WEB_NAME = 201,
        WEB_PUBLIC = 202,
        DISPLAY_NAME = 203,
        LISTENING_IP = 204,
        SERVER_PORT = 205,
        STEAM_PORT = 206,

        // 300+ : Game Settings
        GAME_MODE = 301,
        ENVIRONMENT_HOSTILITY = 302,
        ONLINE_MODE = 303,
        MAX_PLAYER = 304,
        MAX_FLOATING_OBJECTS = 305,

        // 400+ : Multiplier Settings


        // 500+ : 
    }
}