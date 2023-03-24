using System;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;
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
            InitializeComponent();
            try
            {
                AppVariables.StartTime = DateTimeOffset.Now;
                AppVariables.Application = this;
                

                var RootPage = new NavigationPage(Resolver.Resolve<MainPage>());
                RootPage.BarBackgroundColor = Color.FromHex("#CC0033");
                RootPage.BarTextColor = Color.White;

                //var loginPage = Resolver.Resolve<LoginPage>();
                //MainPage = loginPage;
                var localSql = new LocalSql();
                var appUser = Task.Run(async () => await localSql.GetAppUserAsync());
                //var appUser = AppVariables.LocalAppSql.GetAppUser();
                if (appUser.Result != null)
                {
                    AppVariables.User = appUser.Result;
                }
                else
                {
                    AppVariables.User = null;
                }
                //TODO: take out in prod, for testing purposes only
                //AppVariables.User = null;

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

