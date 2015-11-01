using System.Windows;

namespace SESMInstallerV2.Pages
{
    public partial class P1_Mode : MasterUserControl
    {
        public P1_Mode()
        {
            InitializeComponent();
        }

        private void BTN_Start_Click(object sender, RoutedEventArgs e)
        {
            OnMoveForwardTo(new P0_Intro());
        }
    }
}
