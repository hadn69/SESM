using System;

namespace SESM.Tools
{
    public static class Constants
    {
        private static int _versionMajor = 0;
        private static int _versionMinor = 0;
        private static int _versionRevision = 0;

        public static string GetVersion()
        {
            if (_versionMinor == 0 && _versionRevision == 0)
            {
                return _versionMajor.ToString();
            }
            if (_versionRevision == 0)
            {
                return _versionMajor.ToString() + "." + _versionMinor.ToString();
            }
            return _versionMajor.ToString() + "." + _versionMinor.ToString() + "." + _versionRevision.ToString();
        }

        public static void SetVersion(int Major, int Minor, int Revision)
        {
            if (_versionMajor == 0 && _versionMinor == 0 && _versionRevision == 0)
            {
                _versionMajor = Major;
                _versionMinor = Minor;
                _versionRevision = Revision;
            }
            else
            {
                throw new Exception("Error 42.Ve");
            }
        }
    }
}