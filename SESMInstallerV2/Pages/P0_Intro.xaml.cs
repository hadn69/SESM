using System.Windows;

namespace SESMInstallerV2.Pages
{
    public partial class P0_Intro : MasterUserControl
    {
        public P0_Intro()
        {
            InitializeComponent();
        }

        private void BTN_Start_Click(object sender, RoutedEventArgs e)
        {
            OnMoveForwardTo(new P1_Mode());
        }
    }
}
