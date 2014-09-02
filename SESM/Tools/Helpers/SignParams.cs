using System;
using System.Drawing;
using System.Text;

namespace SESM.Tools.Helpers
{
    public class SignParams
    {
        public int Version; // 1 slot
        public int ServerID; // 1 slot
        public int TemplateID; // 1 slot
        public Size SignSize; // 2 slots
        public Color BgColor; // 4 slots
        public Color PrimColor; // 4 slots
        public Color SecColor; // 4 slots
        public bool Logo; // 1 slot
        public bool StatusImg; // 1 slot

        private const int NbParam = 19;

        public bool Decode(string b64Encoded)
        {
            if(string.IsNullOrEmpty(b64Encoded))
                return false;
            byte[] data = Convert.FromBase64String(b64Encoded);
            string decodedString = Encoding.UTF8.GetString(data);
            string[] ramParams = decodedString.Split(';');

            if (ramParams.Length != NbParam)
                return false;
            try
            {
                int i = 0;
                Version = int.Parse(ramParams[i++]);

                ServerID = int.Parse(ramParams[i++]);

                TemplateID = int.Parse(ramParams[i++]);

                SignSize = new Size(int.Parse(ramParams[i++]), int.Parse(ramParams[i++]));

                if (SignSize.Width < 1)
                    SignSize.Width = 1;

                if (SignSize.Height < 1)
                    SignSize.Height = 1;

                if (SignSize.Width > 1000)
                    SignSize.Width = 1000;

                if (SignSize.Height > 1000)
                    SignSize.Height = 1000;

                BgColor = Color.FromArgb(int.Parse(ramParams[i++]), int.Parse(ramParams[i++]), int.Parse(ramParams[i++]), int.Parse(ramParams[i++]));

                PrimColor = Color.FromArgb(int.Parse(ramParams[i++]), int.Parse(ramParams[i++]), int.Parse(ramParams[i++]), int.Parse(ramParams[i++]));

                SecColor = Color.FromArgb(int.Parse(ramParams[i++]), int.Parse(ramParams[i++]), int.Parse(ramParams[i++]), int.Parse(ramParams[i++]));

                Logo = ramParams[i++] == "1";

                StatusImg = ramParams[i++] == "1";
            }
            catch (Exception)
            {
                return false;
            }
            return true;
            
        }
    }
}