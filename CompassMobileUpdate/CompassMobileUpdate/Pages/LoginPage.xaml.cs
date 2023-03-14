using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{	
	public partial class LoginPage : ContentPage
	{	
		public LoginPage (LoginViewModel viewModel)
		{
			InitializeComponent ();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
	}
}

