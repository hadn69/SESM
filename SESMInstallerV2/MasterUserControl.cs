using System.Windows.Controls;

namespace SESMInstallerV2
{
    public class MasterUserControl : UserControl
    {
        public delegate void MoveForwardToHandler(MasterUserControl sender, MasterUserControl page);
        public event MoveForwardToHandler MoveForwardTo;

        public delegate void MoveBackwardToHandler(MasterUserControl sender, MasterUserControl page);
        public event MoveBackwardToHandler MoveBackwardTo;

        protected virtual void OnMoveForwardTo(MasterUserControl page)
        {
            MoveForwardTo?.Invoke(this, page);
        }

        protected virtual void OnMoveBackwardTo(MasterUserControl page)
        {
            MoveForwardTo?.Invoke(this, page);
        }

    }
}