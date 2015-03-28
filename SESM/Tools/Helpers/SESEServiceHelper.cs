using System;
using System.ServiceModel;

namespace SESM.Tools.Helpers
{
    public class SESEServiceHelper : IDisposable
    {
        /*
        private IServerService _SESEService = null;
        public SESEServiceHelper(int port)
        {
            var myBinding = new BasicHttpBinding();
            var myEndpoint = new EndpointAddress("http://localhost:" + port + "/ServerService");
            var myChannelFactory = new ChannelFactory<IServerService>(myBinding, myEndpoint);

            try
            {
                _SESEService = myChannelFactory.CreateChannel();
            }
            catch
            {
                if (_SESEService != null)
                {
                    ((ICommunicationObject)_SESEService).Abort();
                }
            }
        }

        public void GetPlayerList()
        {
            var test = _SESEService.GetPlayersOnline();
            int i = 111;
        }
        */
        public void Dispose()
        {
            //((ICommunicationObject)_SESEService).Close();
        }
        
    }
}