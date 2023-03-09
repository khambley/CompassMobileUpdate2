using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages;
using CompassMobileUpdate.Services;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
	public class MeterDetailViewModel : ViewModelBase
	{
        private readonly MeterService _meterService;

        private Dictionary<int, bool> _isWebServiceRunningDictionary;

        const int _getMeterAttributes = 1,
                        _getMeterAvailabilityEventOutage = 2,
                        _getMeterAvailabilityEventRestoration = 3,
                        _getMeterStatus = 4,
                        _getMeterReads = 5,
                        _getMeterForDevice = 6;

        private CancellationTokenSource _ctsMeterAttributes, _ctsMeterStatus, _ctsMeterReads, _ctsMeterOutages, _ctsMeterRestores, _ctsGetMeterForDevice;

        private bool _isPingActivityRequestCompleted = false;

        private object _lockMeter = new Object();

        private string _userState;

        public int? ActivityID { get; set; }

        public bool IsEnabledCheckStatusButton { get; set; }

        public bool IsPingable { get; set; }

        public bool AllowsPQRs { get; set; }

        public Manufacturer Manufacturer { get; set; }

        public Meter MeterItem { get; set; }

        public string MeterTypeNumber { get; set; }

        public MeterAttributesResponse MeterAttributes { get; set; }

        public Color MeterStateTextColor { get; set; }

        public string StatusDate { get; set; }

        public string CustomerContactNumber { get; set; }

        public ICommand TapOutageRestoreCommand => new Command(async () =>
        {
            var availabilityEventsPage = Resolver.Resolve<AvailabilityEventsPage>();
            await Navigation.PushAsync(availabilityEventsPage);
        });

        public ICommand CheckStatusButtonCommand => new Command(async () =>
        {
            await LoadMeterData();
        });

        public MeterDetailViewModel(MeterService meterService)
		{
            _meterService = meterService;
            IsEnabledCheckStatusButton = true;
        }

        private async Task LoadMeterData()
        {
            IsEnabledCheckStatusButton = false;
            var meter = await _meterService.GetMeterByDeviceUtilityIDAsync(MeterItem.DeviceUtilityID);
            if (meter != null)
            {
                MeterItem = meter;
                // set MeterTypeNumber on MeterDetail
                if (string.IsNullOrWhiteSpace(meter.DeviceUtilityIDWithLocation))
                {
                    MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityID;
                }
                else
                {
                    MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityIDWithLocation;
                }

                // set Meter Attributes on MeterDetail - which includes Meter State (MeterAttributes.Status)
                MeterAttributes = await _meterService.GetMeterAttributesAsync(meter);

                //set Last Comm on MeterDetail
                StatusDate = MeterAttributes.StatusDate.ToLocalTime().ToString(AppVariables.MilitaryFormatStringShort);

                IsEnabledCheckStatusButton = true;
            }
        }
        public void CopyMeterAttributeValues(MeterAttributesResponse source)
        {
            if (source == null)
                return;

            lock (_lockMeter)
            {
                MeterItem.MacID = source.MacID;
                MeterItem.Status = source.Status;
                MeterItem.StatusDate = AppVariables.GetConfiguredTimeZone(source.StatusDate);
                MeterItem.Form = source.Form;
                MeterItem.Version = source.Version;
                MeterItem.Model = source.Model;
                MeterItem.ManufacturerName = Manufacturer.UIQ_ManufacturerName = source.ManufacturerName;
                MeterItem.Latitude = source.Latitude;
                MeterItem.Longitude = source.Longitude;
                MeterItem.TypeName = source.TypeName;
                MeterItem.Type = source.Type;
                IsPingable = source.IsPingable;
                AllowsPQRs = source.IsPQRCapable;
            }
        }

        public ActivityMessage.LogActivityAndMeterPingActivityRequest GetLogActivityAndMeterPingActivityRequest()
        {
            ActivityMessage.LogActivityAndMeterPingActivityRequest request = new ActivityMessage.LogActivityAndMeterPingActivityRequest();
            request.UserID = AppVariables.User.UserID;
            request.MeterDeviceUtilityID = MeterItem.DeviceUtilityID;
            request.MeterState = MeterItem.Status;

            return request;

        }

        public async void handleGetMeterAttributesCompleted(MeterAttributesResponse meterAttributesResponse, Exception ex)
        {
            try
            {
                //if (ex != null)
                //{
                //    if (ex.GetType() == typeof(AuthenticationRequiredException))
                //    {
                //        HandleAuthorizationRequired();
                //        return;
                //    }
                //    else if (ex is ApplicationMaintenanceException)
                //    {
                //        HandleApplicationMaintenance();
                //        return;
                //    }
                //}
                //AppLogger.Debug("  GetMeterAttributesCompleted: Method Start");

                _isWebServiceRunningDictionary[_getMeterAttributes] = false;

                _isPingActivityRequestCompleted = false;

                if (ex == null)
                {
                    CopyMeterAttributeValues(meterAttributesResponse);

                    if (MeterItem.StatusDate != DateTimeOffset.MinValue)
                    {
                        MeterItem.StatusDate.ToLocalTime().ToString(AppVariables.MilitaryFormatStringShort);
                    }

                    if (IsPingable)
                    {
                        MeterStateTextColor = Color.Green;
                        if (_userState != null)
                        {
                            try
                            {
                                var response = await _meterService.PerformActivityRequest(GetLogActivityAndMeterPingActivityRequest());
                                ActivityID = response.Value;

                                _meterService.GetMeterStatusAsync(MeterItem, this.ActivityID, handleGetMeterStatusCompleted, _ctsMeterStatus.Token);
                                
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        protected async void handleGetMeterStatusCompleted(MeterStatusResponse response, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}

