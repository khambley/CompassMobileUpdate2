using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{	
	public partial class AvailabilityEventsPage : BasePage
	{	
		public AvailabilityEventsPage (AvailabilityEventsViewModel viewModel)
		{
			InitializeComponent ();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
	}
}

