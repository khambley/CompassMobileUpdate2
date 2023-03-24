using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
	public class SettingsViewModel : ViewModelBase
	{
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
		}

	}
}

