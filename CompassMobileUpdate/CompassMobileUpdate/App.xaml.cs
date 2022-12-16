using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CompassMobileUpdate
{
    public partial class App : Application
    {
        public App ()
        {
            InitializeComponent();

            var navigationPage = new NavigationPage(Resolver.Resolve<MainPage>());
            navigationPage.BarBackgroundColor = Color.FromHex("#CC0033");
            navigationPage.BarTextColor = Color.White;
            MainPage = navigationPage;
            
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}

