using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace CompassMobileUpdate.Pages
{
    public partial class MeterDetailPage : BasePage
    {
        public MeterDetailPage(MeterDetailViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
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

