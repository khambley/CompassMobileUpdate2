using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{
    public partial class BasePage : ContentPage
    {
        public BasePage()
        {
            InitializeComponent();
        }

        void SettingsButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(Resolver.Resolve<SettingsPage>());
        }
    }
}

