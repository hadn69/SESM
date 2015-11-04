using System.Windows.Controls;
using MahApps.Metro.Controls;
using SESMInstallerV2.Pages;

namespace SESMInstallerV2
{
    public partial class MainWindow : MetroWindow
    {
        public MasterUserControl CurrentPage;

        public MainWindow()
        {
            InitializeComponent();

            CurrentPage = new P0_Intro();

            CurrentPage.MoveForwardTo += CurrentPage_MoveForwardTo;

            pageTransitionControl.ShowPage(CurrentPage);
        }

        private void CurrentPage_MoveForwardTo(MasterUserControl sender, MasterUserControl page)
        {
            CurrentPage = page;

            CurrentPage.MoveForwardTo += CurrentPage_MoveForwardTo;

            pageTransitionControl.ShowPage(page);

        }
    }
}
