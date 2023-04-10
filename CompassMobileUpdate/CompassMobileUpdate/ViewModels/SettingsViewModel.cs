using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CompassMobileUpdate.Extensions;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Pages;
using Xamarin.Forms;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.ViewModels
{
	public class SettingsViewModel : ViewModelBase
	{
		public List<string> Environments => new List<string>() { "Integration", "Stage", "Production" };

		public bool IsEnabledPickerEnvironment { get; set; }

		public bool IsEnabledResyncVoltageRulesButton { get; set; }

		public bool IsRunningResyncVoltageRules { get; set; }

        public string DefaultEnvironment { get; set; }

		public string SelectedEnvironment
		{
			get { return null; }
			set
			{
				if(value == "Integration") {
					AppVariables.AppEnvironment = AppEnvironment.Integration;
				} else if (value == "Stage") {
					AppVariables.AppEnvironment = AppEnvironment.Stage;
				} else if (value == "Production") {
					AppVariables.AppEnvironment = AppEnvironment.Production;
				} else {
					Device.BeginInvokeOnMainThread(() =>
					{
						App.Current.MainPage.DisplayAlert("Error", "Application Environment not found", "Close");
					});
				}
				OnPropertyChanged(nameof(SelectedEnvironment));
			}
		}

		public string SessionExpirationTime { get; set; }

		public ICommand ForceCustInfoUpdateCommand => new Command(() =>
		{
			ForceCustInfoUpdate();
		});

        private void ForceCustInfoUpdate()
        {
			Device.BeginInvokeOnMainThread(async () =>
			{
				await App.Current.MainPage.DisplayAlert("Success", "On the next Meter Check Status Customer Info will be pulled from the database", "Close");
			});
        }

        public ICommand LogoutCommand => new Command(async () =>
		{
			await Logout();
		});

		public ICommand ViewLogsCommand => new Command(async () =>
		{
			await ViewLogs();
		});

        private async Task ViewLogs()
        {
			await Navigation.PushAsync(Resolver.Resolve<LogPage>());
        }

        private async Task Logout()
        {
			await this.LoginRequired();
        }

		public ICommand ResyncVoltageRulesCommand => new Command(async () =>
		{
			await ResyncVoltageRules();

		});



        private async Task ResyncVoltageRules()
        {
			bool success;
			try
			{
				IsEnabledResyncVoltageRulesButton = false;
				IsRunningResyncVoltageRules = true;
				await AppVariables.ResetVoltageRules(true);
				Device.BeginInvokeOnMainThread(async () =>
				{
					await App.Current.MainPage.DisplayAlert("Success", "On the next Meter Check Status Customer Info will be pulled from the database", "Close");
				});
			}
			catch (Exception ex)
			{
				if (AppHelper.ContainsAuthenticationRequiredException(ex))
				{
                    await this.LoginRequired();
                }
                else
                {
                    success = false;
                    //TODO: Add app logging
                    //AppLogger.LogError(ex);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "The Sync failed: " + ex.GetInnermostMessage(), "Close");
                    });                   
                }
            }
        }

        public SettingsViewModel()
		{
			CheckEnabledUser();
			SetSessionExpirationTime();
			DefaultEnvironment = AppVariables.AppEnvironment.ToString();
		}

        
        protected void SetSessionExpirationTime()
        {
			SessionExpirationTime = AppVariables.User.JWTExpirationUTC.ToLocalTime().ToString(AppVariables.MilitaryFormatStringNoSeconds);
        }

        private void CheckEnabledUser()
        {
            if (AppVariables.User.UserID.ToLower().Equals("kellpy") // Patrick Kelly
                 || AppVariables.User.UserID.ToLower().Equals("jordmx") // Mark Jordan
                 || AppVariables.User.UserID.ToLower().Equals("chenww") // Wei Chen
                 || AppVariables.User.UserID.ToLower().Equals("rizvmr") // Rameez Rizvi
                 || AppVariables.User.UserID.ToLower().Equals("ctsrr") // Bob Ruehrdanz
                 || AppVariables.User.UserID.ToLower().Equals("ddcb2") // Mark Sayers
                 || AppVariables.User.UserID.ToLower().Equals("mayerw") // Ryan Mayer
                 || AppVariables.User.UserID.ToLower().Equals("c117324") // Adrian Arva
				 || AppVariables.User.UserID.ToLower().Equals("c971939") // Katherine Hambley
             )
            {
                IsEnabledPickerEnvironment = true;
            }
            else
            {
                IsEnabledPickerEnvironment = false;
            }
        }

        public void GetPickerEnvironmentList()
		{
			Environments.Add("Integration");
            Environments.Add("Stage");
            Environments.Add("Production");
        }
	}
}

