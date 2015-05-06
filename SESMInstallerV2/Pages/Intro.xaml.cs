using System.Windows.Controls;

namespace SESMInstallerV2.Pages
{
    public partial class Intro : Page
    {
        public delegate void NextPageHandler();
        public event NextPageHandler NextPageClicked;



        public Intro()
        {
            InitializeComponent();
        }

        private void BTN_Start_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NextPage();
        }
    }
}
