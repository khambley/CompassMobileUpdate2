using System;
using System.Threading.Tasks;
using CompassMobileUpdate.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CompassMobileUpdate
{
    public partial class App : Application
    {
        bool JWTIsExpired = false;
        public App ()
        {
            try
            {
                AppVariables.StartTime = DateTimeOffset.Now;
                AppVariables.Application = this;
                InitializeComponent();

                var RootPage = new NavigationPage(Resolver.Resolve<MainPage>());
                RootPage.BarBackgroundColor = Color.FromHex("#CC0033");
                RootPage.BarTextColor = Color.White;

                var appUser = AppVariables.LocalAppSql.GetAppUser();
                AppVariables.User = appUser;
                CheckJWTExpiration();

                if (AppVariables.User == null || JWTIsExpired)
                {
                    var loginPage = Resolver.Resolve<LoginPage>();
                    MainPage = loginPage;
                }
                else
                {
                    Task.Run(async () => await AppVariables.ResetVoltageRules(false));
                    MainPage = RootPage;
                }
            }
            catch (Exception e)
            {
                string message = e.Message;
                string stackTrace = e.StackTrace;
                throw;
            }        
        }

        private void CheckJWTExpiration()
        {
            if (AppVariables.User != null)
            {
                if (!string.IsNullOrWhiteSpace(AppVariables.User.JWT))
                {
                    if (AppVariables.User.JWTExpirationUTC < DateTime.UtcNow)
                    {
                        JWTIsExpired = true;
                    }
                }
            }
        }
        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
            CheckJWTExpiration();
        }
    }
}

