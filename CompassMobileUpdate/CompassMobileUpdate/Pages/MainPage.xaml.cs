using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;

namespace CompassMobileUpdate
{
    public partial class MainPage : Pages.BasePage
    {
        private double width;
        private double height;
        
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if(width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;
                if(width > height)
                {
                    outerStack.Orientation = StackOrientation.Horizontal;
                    outerStack.HorizontalOptions = LayoutOptions.CenterAndExpand;
                }
                else
                {
                    outerStack.Orientation = StackOrientation.Vertical;
                }
            }
        }
    }
}

