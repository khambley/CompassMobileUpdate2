using System;
using Xamarin.Forms;

namespace CompassMobileUpdate.Helpers
{
	public class CustomCell : ViewCell
	{
		public CustomCell()
		{
			StackLayout cellWrapper = new StackLayout();
			cellWrapper.Orientation = StackOrientation.Horizontal;

			StackLayout verticalLayout = new StackLayout();
			verticalLayout.Orientation = StackOrientation.Vertical;
			verticalLayout.Padding = new Thickness(10, 5, 0, 10);

			Label lblCustomerName = new Label();
			lblCustomerName.FontSize = 20;
			lblCustomerName.FontAttributes = FontAttributes.Bold;

			Label lblCustomerAddress = new Label();
			lblCustomerAddress.TextColor = Color.Blue;
			lblCustomerAddress.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));

            //set bindings
            lblCustomerName.SetBinding(Label.TextProperty, "CustomerNameAndDeviceUtilityId");
			lblCustomerAddress.SetBinding(Label.TextProperty, "DistanceAndCustomerAddress");

			verticalLayout.Children.Add(lblCustomerName);
			verticalLayout.Children.Add(lblCustomerAddress);
			cellWrapper.Children.Add(verticalLayout);
			View = cellWrapper;
			


        }
	}
}

