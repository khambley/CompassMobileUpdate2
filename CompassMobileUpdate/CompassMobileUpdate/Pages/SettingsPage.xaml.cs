﻿using System;
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
        }
    }
}

