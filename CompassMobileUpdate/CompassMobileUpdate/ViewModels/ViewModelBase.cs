using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(params string[] propertyNames)
        {
            foreach(var propertyName in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public bool popUpSessionTimeOut { get; set; }

        protected bool _isLoginPageBeingPushed { get; set; }

        public INavigation Navigation { get; set; }

        public bool IsBusy { get; set; }

        protected async Task LoginRequired()
        {
            _isLoginPageBeingPushed = true;
            var localSql = new LocalSql();
            await localSql.DeleteUsers();

            Device.BeginInvokeOnMainThread(() =>
            {
                var loginPage = Resolver.Resolve<LoginPage>();
                App.Current.MainPage = loginPage;                          
            });
        }
    }
}

