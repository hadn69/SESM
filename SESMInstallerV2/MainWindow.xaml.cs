using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MahApps.Metro.Controls;
using SESMInstallerV2.Pages;

namespace SESMInstallerV2
{
    public partial class MainWindow : MetroWindow
    {
        public Page CurrentPage;
        public MainWindow()
        {
            InitializeComponent();

            Intro intro = new Intro();
            CurrentPage = intro;
            frame.Content = CurrentPage;

            intro.NextPageClicked += Intro_NextPageClicked;
        }

        private void Intro_NextPageClicked()
        {
            DoubleAnimation da = new DoubleAnimation
            {
                From = 10,
                To = -500,
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };
            TranslateTransform tt = new TranslateTransform();
            CurrentPage.BeginAnimation(TranslateTransform.XProperty, da);
        }
    }
}
