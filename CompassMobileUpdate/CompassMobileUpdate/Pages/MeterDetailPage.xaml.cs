using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace CompassMobileUpdate.Pages
{
    public partial class MeterDetailPage : BasePage
    {
        MeterDetailViewModel vm => BindingContext as MeterDetailViewModel;

        public MeterDetailPage(MeterDetailViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            vm.GetAllMeterInfo();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            vm._userState = null;
            vm.CancelAllServiceCallsAsync(false);
        }

        void tapPhone_Tapped(System.Object sender, System.EventArgs e)
        {
            if(sender.GetType() == typeof(Label))
            {
                string phoneNumber = ((Label)sender).Text;
                //may need to strip non-digit chars
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    Launcher.TryOpenAsync(new Uri("tel:" + phoneNumber));
                }
            }
        }

        

        
    }
}

