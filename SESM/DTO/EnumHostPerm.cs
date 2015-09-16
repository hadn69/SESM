namespace SESM.DTO
{
    public enum EnumHostPerm : int
    {
        // 000+ : Misc
        MISC_CRON = 1,

        // 100+ : Server
        SERVER_CREATE = 101,

        // 200+ : Host Access
        ACCESS_HOST_READ = 200,
        ACCESS_HOST_CREATE = 201,
        ACCESS_HOST_DELETE = 202,
        ACCESS_HOST_EDIT_NAME = 203,
        ACCESS_HOST_EDIT_PERMISSION = 204,
        ACCESS_HOST_EDIT_USERS = 205,

        // 300+ - 1002 : Host Access
        ACCESS_SERVER_READ = 300,
        ACCESS_SERVER_CREATE = 301,
        ACCESS_SERVER_DELETE = 302,
        ACCESS_SERVER_EDIT_NAME = 303,
        ACCESS_SERVER_EDIT_PERMISSION = 304,
        ACCESS_SERVER_EDIT_USERS = 1002
    }
}