using System;
using System.Drawing;
using System.Text;

namespace SESM.Tools.Helpers
{
    public class SignParams
    {
        public int TemplateID; // 1 slot
        public Size SignSize; // 2 slots
        public Color BgColor; // 4 slots
        public Color PrimColor; // 4 slots
        public Color SecColor; // 4 slots

        private int NbParam = 15;
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
                TemplateID = int.Parse(ramParams[0]);

                SignSize = new Size(int.Parse(ramParams[1]), int.Parse(ramParams[2]));

                BgColor = Color.FromArgb(int.Parse(ramParams[6]), int.Parse(ramParams[3]), int.Parse(ramParams[4]), int.Parse(ramParams[5]));

                PrimColor = Color.FromArgb(int.Parse(ramParams[10]), int.Parse(ramParams[7]), int.Parse(ramParams[8]), int.Parse(ramParams[9]));

                SecColor = Color.FromArgb(int.Parse(ramParams[14]), int.Parse(ramParams[11]), int.Parse(ramParams[12]), int.Parse(ramParams[13]));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
            
        }
    }
}