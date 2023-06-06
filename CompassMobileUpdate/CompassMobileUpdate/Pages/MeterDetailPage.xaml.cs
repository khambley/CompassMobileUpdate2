using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Linq;

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
            vm._isPageBeingPushed = false;

            if(vm._isFirstPageLoad || vm._isLoginPageBeingPushed || vm._isAppMaintenanceBeingPushed)
            {
                vm.GetAllMeterInfo();
            }

            vm._isFirstPageLoad = false;
            vm._isAppMaintenanceBeingPushed = false;
            vm._AppMaintenanceInitiated = false;

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!vm._isPageBeingPushed)
            {
                //TODO: Add app logging
                //AppLogger.LogNewLine();

                vm._isTimeOutCountDownRunning = false;
                vm.CancelAllServiceCallsAsync(false);

                vm._userState = null;

                //TODO: Add app logging
                //AppLogger.Debug("Meter Detail Page: Closing");

                if (vm._isWebServiceRunningDictionary.ContainsValue(true))
                {
                    List<string> servicesRunning = (from service in vm._isWebServiceRunningDictionary
                                                    where service.Value == true
                                                    select service.Key.ToString()).ToList();

                    string servicesRunningText = string.Empty;

                    for (int i = 0; i < servicesRunning.Count; i++)
                    {
                        if (i == 0)
                            servicesRunningText += servicesRunning[i];
                        else
                            servicesRunningText += ", " + servicesRunning[i];
                    }

                    //TODO: Add app logging
                    //AppLogger.Debug("These services are still running: " + servicesRunningText);
                }
            }
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

