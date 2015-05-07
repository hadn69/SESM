namespace SESM.Models
{

    public enum GameMode
    {
        Survival,
        Creative
    }

    public enum OnlineMode
    {
        OFFLINE,
        PUBLIC,
        FRIENDS,
        PRIVATE
    }

    public enum EnvironmentHostility
    {
        SAFE,
        NORMAL,
        CATACLYSM,
        CATACLYSM_UNREAL
    }

    public enum SESubTypeId
    {
        LoneSurvivor,
        EasyStart1,
        EasyStart2,
        CrashedRedShip,
        TwoPlatforms,
        Asteroids,
        EmptyWorld
    }

    public enum MESubTypeId
    {
        Quickstart,
        PreviewDestructionMap,
        TestLab1_Neverland,
        TestLab2_Castle,
        TestLab3_Integrity,
        LargeTerrain,
        NormalTerrain,
        SmallTerrain,
        VerySmallTerrain,
        GenerateTerrain,
        GenerateFromMaps,
        TestTrees,
        BattleBaseMap2
    }
}