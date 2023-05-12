﻿using System;
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
                // AppVersion - saves the new version code every time the app starts
                Xamarin.Essentials.VersionTracking.Track();

                AppVariables.StartTime = DateTimeOffset.Now;
                AppVariables.Application = this;

                //Default to Integration (Test) environment for now...
                //TODO: change to default Production environment once implemented
                AppVariables.AppEnvironment = Enums.AppEnvironment.Integration;

                var RootPage = new NavigationPage(Resolver.Resolve<MainPage>());
                RootPage.BarBackgroundColor = Color.FromHex("#CC0033");
                RootPage.BarTextColor = Color.White;

               
                var localSql = new LocalSql();
                
                var appUser = localSql.GetAppUser();

                if (appUser != null)
                {
                    AppVariables.User = appUser;
                }
                else
                {
                    AppVariables.User = null;
                }

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
                        var localSql = new LocalSql();
                        localSql.DeleteUsers();

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            var loginPage = Resolver.Resolve<LoginPage>();
                            App.Current.MainPage = loginPage;
                        });
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

