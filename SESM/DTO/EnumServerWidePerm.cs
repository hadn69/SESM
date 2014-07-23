namespace SESM.DTO
{
    public enum EnumServerWidePerm : int
    {
        // 10+ : Settings
        EDIT_SETTINGS = 11,
        UPLOAD_GAME_FILE = 12,
        ACCESS_DIAGNOSIS = 13,

        // 20+ : Users
        EDIT_USER = 21,
        DELETE_USER = 22,

        // 30+ : Servers (Instances),
        CREATE_INSTANCE = 31,
        EDIT_INSTANCE = 32,
        DELETE_INSTANCE = 33,

        // 40+ : Role
        CREATE_ROLE = 41,
        EDIT_ROLE = 42,
        DELETE_ROLE = 43,
        MERGE_ROLE = 44,
        PROMOTE_USER = 45,
        DEMOTE_USER = 46

    }
}