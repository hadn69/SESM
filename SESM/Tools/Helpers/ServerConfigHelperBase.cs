
using SESM.DTO;

namespace SESM.Tools.Helpers
{
    public abstract class ServerConfigHelperBase
    {
        public string SaveName;
        public uint AutoSaveInMinutes;
        public string IP;
        public int ServerPort;
        public abstract void Save(EntityServer server);
        public abstract bool Load(EntityServer server);
        public abstract bool LoadFromServConf(string path);
        public abstract bool LoadFromSave(string path);
        public abstract bool LoadFromSaveManager(string path);
    }
}
