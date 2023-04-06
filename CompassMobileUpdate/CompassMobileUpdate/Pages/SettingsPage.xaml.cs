using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.Pages
{
    public partial class SettingsPage : BasePage
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            pickerEnvironment.SelectedIndex = pickerEnvironment.Items.IndexOf(AppVariables.AppEnvironment.ToString());
            pickerEnvironment.SelectedIndexChanged += pickerEnvironment_SelectedIndexChanged;

            if (AppVariables.User.UserID.ToLower().Equals("kellpy") // Patrick Kelly
                || AppVariables.User.UserID.ToLower().Equals("jordmx") // Mark Jordan
                || AppVariables.User.UserID.ToLower().Equals("chenww") // Wei Chen
                || AppVariables.User.UserID.ToLower().Equals("rizvmr") // Rameez Rizvi
                || AppVariables.User.UserID.ToLower().Equals("ctsrr") // Bob Ruehrdanz
                || AppVariables.User.UserID.ToLower().Equals("ddcb2") // Mark Sayers
                || AppVariables.User.UserID.ToLower().Equals("mayerw") // Ryan Mayer
                || AppVariables.User.UserID.ToLower().Equals("c117324") // Adrian Arva
                || AppVariables.User.UserID.ToLower().Equals("c971939") // Katherine Hambley 6/12/22
            )
            {
                pickerEnvironment.IsEnabled = true;
            }
            else
            {
                pickerEnvironment.IsEnabled = false;
            }
        }

        private void pickerEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;

            AppEnvironment temp;
            if (Enum.TryParse<AppEnvironment>(picker.Items[picker.SelectedIndex], true, out temp))
            {
                AppVariables.AppEnvironment = temp;
            }
            else
            {
                DisplayAlert("Error", "Application Environment not found", "Close");
            }
        }
    }
}

