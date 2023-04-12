using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CompassMobileUpdate.Exceptions;
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

        public bool IsFavorite { get; set; }

        public string IsFavoriteImage
        {
            get
            {
                if (IsFavorite)
                {
                    return "star_yellow.png";
                }
                else
                {
                    return "star_gray.png";
                }               
            }
        }

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
            await GetAllMeterInfo();
        });

        public ICommand SetIsFavoriteCommand => new Command(async () =>
        {
            await SetIsFavoriteAsync();
        });

        private async Task SetIsFavoriteAsync()
        {
            IsFavorite = !IsFavorite;
            var localSql = new LocalSql();
            var localMeter = await localSql.GetLocalMeter(MeterItem.DeviceUtilityID);
            if (localMeter != null)
            {
                localMeter.IsFavorite = IsFavorite;
            }
            else
            {
                await localSql.AddMeter(MeterItem);
            }
            await localSql.UpdateMeterIsFavorite(MeterItem.DeviceUtilityID, IsFavorite);
        }

        public MeterDetailViewModel(MeterService meterService)
		{
            _meterService = meterService;
            IsEnabledCheckStatusButton = true;
        }

        public async Task GetAllMeterInfo()
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

                _ctsMeterAttributes = new CancellationTokenSource();

                // set MeterAttributes on MeterDetail in handler
                await _meterService.GetMeterAttributesAsync(meter, HandleGetMeterAttributesCompleted, _ctsMeterAttributes.Token);
                           
                IsEnabledCheckStatusButton = true;
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

        public ActivityMessage.PostMeterPingActivityCompleteRequest GetPostMeterPingActivityCompleteRequest()
        {
            ActivityMessage.PostMeterPingActivityCompleteRequest request = new ActivityMessage.PostMeterPingActivityCompleteRequest();
            if (ActivityID.HasValue)
            {
                request.ActivityID = this.ActivityID.Value;
            }

            request.MeterDeviceUtilityID = MeterItem.DeviceUtilityID;

            return request;
        }

        protected void HandleApplicationMaintenance()
        {
            //AppLogger.Debug("HandleApplicationMaintenance: Begin");
            //cancelAllServiceCalls(false);
            //if (!_AppMaintenanceInitiated)
            //{
            //    AppLogger.Debug("_AppMaintenanceInitiated is false, showing app maintenance");
            //    _AppMaintenanceInitiated = true;
            //    this.ShowApplicationMaintenance();
            //}
            //else
            //{
            //    AppLogger.Debug("_AppMaintenanceInitiated already set to true");
            //}
            //AppLogger.Debug("HandleApplicationMaintenance: End");
        }

        protected void HandleAuthorizationRequired()
        {
            //cancelAllServiceCalls(false);
            //this.LoginRequired(true);
        }

        public async void HandleGetMeterAttributesCompleted(MeterAttributesResponse meterAttributesResponse, Exception ex)
        {
            try
            {

                MeterAttributes = meterAttributesResponse;

                if (MeterAttributes.StatusDate != DateTimeOffset.MinValue)
                {
                    //sets Meter Last Comm date (Status Date)
                    StatusDate = MeterAttributes.StatusDate.ToLocalTime().ToString(AppVariables.MilitaryFormatStringShort);
                }

                //TODO: Logging
                //AppLogger.Debug("  GetMeterAttributesCompleted: Method Start");

                //TODO: implement method L1320
                //stopMeterAttributesAnimations(false);

                if (MeterAttributes.IsPingable)
                {
                    MeterStateTextColor = Color.Green;
                    try
                    {
                        var response = await _meterService.PerformActivityRequest(GetLogActivityAndMeterPingActivityRequest());
                        ActivityID = response.Value;

                        _ctsMeterStatus = new CancellationTokenSource();

                        _meterService.GetMeterStatusAsync(MeterItem, this.ActivityID, HandleGetMeterStatusCompleted, _ctsMeterStatus.Token);

                    }
                    catch
                    {

                    }
                }
                else
                {
                    MeterStateTextColor = Color.Red;
                }
            }
            catch
            {

            }
        }

        protected async void HandleGetMeterStatusCompleted(MeterStatusResponse response, Exception ex)
        {
            var pingActionRequest = GetPostMeterPingActivityCompleteRequest();
        }
    }
}

