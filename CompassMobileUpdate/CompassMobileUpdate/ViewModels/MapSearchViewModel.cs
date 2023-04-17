using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace CompassMobileUpdate.ViewModels
{
	public class MapSearchViewModel : ViewModelBase
	{
        private readonly MeterService _meterService;

        public bool IsVisibleMessage { get; set; }

        public bool IsVisibleSearchResults { get; set; }

        public bool IsVisibleListViewMeters { get; set; }

        public bool IsVisibleMap { get; set; }

        public string MessageLabel { get; set; }

        public MapSearchViewModel(MeterService meterService)
        {
            _meterService = meterService;
            IsVisibleMessage = false;
            IsVisibleSearchResults = false;
            IsVisibleListViewMeters = false;
            IsVisibleMap = true;
            
        }

        public async Task<List<Meter>> GetMetersWithinXRadiusAsync(double latitude, double longitude, double radiusInMiles)
        {
            return await _meterService.GetMetersWithinXRadiusAsync(latitude, longitude, radiusInMiles);
        }

        public bool IsValidSerialNumber(string serialNumber, out SerialNumberFormatException ex)
        {
            return _meterService.IsValidSerialNumber(serialNumber, out ex);
        }
        public ICommand PerformSearch => new Command<string>(async (string searchText) =>
		{
            IsBusy = true;

            await PerformCustomerSearch(searchText);

            IsBusy = false;
            IsVisibleMessage = true;
        });

        private async Task PerformCustomerSearch(string searchText)
        {
            searchText = searchText.Trim();
            double radius = .05; //small for exact location or meter
            int temp;

            //If we're prety sure it's a zipcode
            if (searchText.Length == 5 && Int32.TryParse(searchText, out temp))
            {
                radius = 3;
                MessageLabel = "Searching zipcode: " + searchText;
            }
            else
            {
                MessageLabel = "Searching for: " + searchText;
            }
            //TODO: Decide where to place animation and fades, needs a label passed to it
            //await AppHelper.FadeOutLabelByEmptyString(_lblMessage, 3000);

            Position position = new Position();
        }
    }
}

