using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.ViewModels
{
	public class SettingsViewModel : ViewModelBase
	{
		public List<string> Environments => new List<string>() { "Integration", "Stage", "Production" };

		public bool IsEnabledPickerEnvironment { get; set; }

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
			}
		}
        

        public ICommand LogoutCommand => new Command(async () =>
		{
			await Logout();
		});

        private async Task Logout()
        {
			await this.LoginRequired();
        }

        public SettingsViewModel()
		{
			CheckEnabledUser();
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

