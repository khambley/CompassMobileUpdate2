﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages;
using CompassMobileUpdate.Services;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
	public class MeterSearchViewModel : ViewModelBase
	{
        private readonly MeterService _meterService;

        public bool IsVisibleCustomerResults  { get; set; }

        public bool IsVisibleCustomerSearch { get; set; }

        public bool IsRunningCustomerSearch { get; set; }

        public bool IsVisibleRecentMetersLabel { get; set; }

        public List<LocalMeter> RecentMeters { get; set; }

        public List<Meter> Meters { get; set; }
        
        bool UseFirstAndLastName { get; set; }

        public MeterSearchViewModel(MeterService meterService)
		{
            _meterService = meterService;

            IsVisibleCustomerResults = true;
            IsVisibleRecentMetersLabel = false;

            RecentMeters = new List<LocalMeter>();
			//Task.Run(async () => await LoadData());
		}

        public ICommand PerformSearchCommand => new Command<string>(async (string searchText) =>
        {
            await PerformCustomerSearch(searchText);
        });

        public ICommand GetAPITokenCommand => new Command(async () =>
        {
            //await GetAPIToken();
        });

        public ICommand GetMeterByDeviceUtilityIDCommand => new Command(async () =>
        {
            await _meterService.GetMeterByDeviceUtilityIDAsync("G270280650");
        });

        public Meter SelectedMeterItem
        {
            get { return null; }
            set
            {
                Device.BeginInvokeOnMainThread(async () => await NavigateToMeterDetail(value));
                OnPropertyChanged(nameof(SelectedMeterItem));
            }
        }

        private async Task NavigateToMeterDetail(Meter meter)
        {
            if (meter == null)
            {
                return;
            }

            var meterDetailpage = Resolver.Resolve<MeterDetailPage>();

            var vm = meterDetailpage.BindingContext as MeterDetailViewModel;

            vm.MeterItem = meter;

            // set MeterTypeNumber on MeterDetail
            if (string.IsNullOrWhiteSpace(meter.DeviceUtilityIDWithLocation))
            {
                vm.MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityID;
            }
            else
            {
                vm.MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityIDWithLocation;
            }

            // set Meter Attributes on MeterDetail - which includes Meter State (MeterAttributes.Status)
            //vm.MeterAttributes = await _meterService.GetMeterAttributesAsync(meter);

            //set Last Comm on MeterDetail
            //vm.StatusDate = vm.MeterAttributes.StatusDate.ToLocalTime().ToString(AppVariables.MilitaryFormatStringShort);

            await Navigation.PushAsync(meterDetailpage);        
        }


        private async Task PerformCustomerSearch(string searchText)
        {
            searchText = searchText.Trim();
            SerialNumberFormatException ex;

            if(searchText.Any(c => char.IsDigit(c)))
            {
                if (_meterService.IsValidSerialNumber(searchText, out ex))
                {
                    // get meter by deviceutilityId
                    var meter = await _meterService.GetMeterByDeviceUtilityIDAsync(searchText);
                    await NavigateToMeterDetail(meter);
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage.DisplayAlert("Input Error", $"{ex.Message}", "OK");
                    });
                }
            }
            
            if (!searchText.Any(c => char.IsDigit(c)))
            {
                
                if (searchText.Replace(" ", "").Length > 5)
                {
                    string name, lastName, firstName;
                    firstName = lastName = null;
                    name = searchText.Trim();
                    UseFirstAndLastName = false;

                    if (searchText.Contains(" "))
                    {
                        int lastIndex = name.LastIndexOf(" ");
                        int lastNameIndex = lastIndex + 1;

                        if (lastNameIndex > name.Length)
                        {
                            lastNameIndex--;
                        }
                        int lastNameLength = searchText.Length - lastNameIndex;

                        lastName = name.Substring(lastNameIndex, lastNameLength);
                        firstName = name.Substring(0, lastIndex);
                        UseFirstAndLastName = true;
                    }
                    try
                    {
                        IsVisibleCustomerSearch = true;
                        IsRunningCustomerSearch = true;
                        if (UseFirstAndLastName)
                        {
                            Meters = await _meterService.GetMetersByCustomerName(name, firstName, lastName);
                        }
                        else
                        {
                            Meters = await _meterService.GetMetersByCustomerName(name);
                        }
                    }
                    catch (Exception e)
                    {
                        //TODO: Add error logging
                        //AppLogger.LogError(e)

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            App.Current.MainPage.DisplayAlert("Service Error", "Get Meters By Customer Name Failed." + Environment.NewLine + e.Message, "Close");
                        });
                    }
                    finally
                    {
                        IsVisibleCustomerSearch = false;
                        IsRunningCustomerSearch = false;
                    }
                }
            }
            //Meters = _meterService.GetMetersByCustomerName()
            //popup for testing purposes
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    App.Current.MainPage.DisplayAlert("PerformCustomerSearch method test", "The PerformCustomerSearch method was called successfully", "OK");
            //});
            //await Navigation.PushAsync(Resolver.Resolve<MeterDetailPage>());
        }

        private async Task LoadData()
		{
            RecentMeters.Add(new LocalMeter
            {
                DeviceUtilityID = "G230296247",
                DeviceUtilityIDWithLocation = "G230296247 ON",
                CustomerName = "Baxter Credit Union",
                CustomerAddress = "300 N Milwaukee Ave Vernon Hills IL 60061"
            });
            RecentMeters.Add(new LocalMeter
            {
                DeviceUtilityID = "G270280650",
                DeviceUtilityIDWithLocation = "G270280650 ON",
                CustomerName = "ENRIQUE LOPEZ",
                CustomerAddress = "5931 S MOODY AVE CHICAGO IL 60638"
            });
        }


	}
}

