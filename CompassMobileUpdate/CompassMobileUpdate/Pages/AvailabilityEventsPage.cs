using System;
using System.Collections.Generic;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages.Views;
using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{
    class AvailabilityEventsPage : ContentPage
    {
        ListView _eventTimes = new ListView();
        public AvailabilityEventsPage(List<AvailabilityEvent> events)
        {
            AvailabilityEventsView view = new AvailabilityEventsView(events);

            if (view.EventTypeCount == 1)
            {
                this.Title = "Recent " + view.GetDistinctEventTypes()[0];
            }
            else
            {
                this.Title = "Recent Events";
            }

            _eventTimes.ItemsSource = view.AvailabilityEvents;

            _eventTimes.ItemTemplate = new DataTemplate(() =>
            {

                // Create views with bindings for displaying each property.
                Label itemLabel = new Label();
                itemLabel.HorizontalOptions = LayoutOptions.Center;

                Label eventTypeLetter = new Label();
                eventTypeLetter.HorizontalOptions = LayoutOptions.Center;
                eventTypeLetter.VerticalOptions = LayoutOptions.Center;

                eventTypeLetter.SetBinding(Label.TextProperty, AvailabilityEventView.EventTypeCapitalFirstLetterField);
                //eventTypeLetter.Text = eventTypeLetter.Text.Substring(0, 1).ToUpper();
                eventTypeLetter.TextColor = Color.White;
                eventTypeLetter.FontAttributes = FontAttributes.Bold;
                //eventTypeLetter.SetBinding(Label.BackgroundColorProperty, AvailabilityEvent.DataColorField);
                //The dot syntax tells Xamarin.Forms to use the BindingContext as the data source instead of a property on the BindingContext. 
                //This is handy when the BindingContext is a more simple type, such as a string or an integer.
                itemLabel.SetBinding(Label.TextProperty, new Binding(AvailabilityEventView.EventTimeField));

                BoxView boxView = new BoxView();
                boxView.SetBinding(BoxView.ColorProperty, AvailabilityEventView.DataColorField);

                Grid grid = new Grid();
                grid.Children.Add(boxView, 0, 0);
                grid.Children.Add(eventTypeLetter, 0, 0);

                return new ViewCell
                {
                    View = new StackLayout
                    {
                        Padding = new Thickness(10, 5),
                        Orientation = StackOrientation.Horizontal,
                        Children = {
                            grid,
                            new StackLayout {
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.End,
                                Spacing = 0,
                                Children = {
                                    itemLabel
                                }
                            }
                        }
                    }
                };
            });


            _eventTimes.HorizontalOptions = LayoutOptions.Center;
            //_eventTimes.IsEnabled = false;
            //_eventTimes.ChildAdded += _eventTimes_ChildAdded;
            //_eventTimes.Refreshing += _eventTimes_Refreshing;
            //_eventTimes.ItemAppearing += _eventTimes_ItemAppearing;
            ScrollView sv = new ScrollView();
            sv.HorizontalOptions = LayoutOptions.Center;
            sv.Content = _eventTimes;
            this.Content = sv;
        }

        void _eventTimes_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {

        }

        void _eventTimes_Refreshing(object sender, EventArgs e)
        {

            if (sender.GetType() == typeof(Label))
            {
                ;
            }
        }

        void _eventTimes_ChildAdded(object sender, ElementEventArgs e)
        {
            if (sender.GetType() == typeof(Label))
            {
                ;
            }

            if (e.Element is Label)
            {
                ;
            }
        }
    }
}


