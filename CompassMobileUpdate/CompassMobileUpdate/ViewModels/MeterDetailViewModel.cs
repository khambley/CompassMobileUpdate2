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
using static CompassMobileUpdate.Models.Enums;

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

        public string ErrorMessageText { get; set; }

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

        public bool IsVisibleErrorMessage { get; set; }

        public bool AllowsPQRs { get; set; }

        public Manufacturer Manufacturer { get; set; }

        public Meter MeterItem { get; set; }

        public string MeterTypeNumber { get; set; }

        public MeterAttributesResponse MeterAttributes { get; set; }

        public MeterAvailabilityEventsResponse Outages { get; set; }

        public MeterAvailabilityEventsResponse Restores { get; set; }

        public MeterAvailabilityEventsEventType MeterEventType { get; set; }

        public Color MeterStateTextColor { get; set; }

        public string OutagesValueText { get; set; }

        public Color OutagesValueTextColor { get; set; }

        public string RestoresValueText { get; set; }

        public Color RestoresValueTextColor { get; set; }

        public string StatusDate { get; set; }

        public string CustomerContactNumber { get; set; }

        public ICommand TapOutageRestoreCommand => new Command(async () =>
        {
            await TapOutageRestore();
        });

        private async Task TapOutageRestore()
        {
            var availabilityEventsPage = Resolver.Resolve<AvailabilityEventsPage>();
            await Navigation.PushAsync(availabilityEventsPage);
        }

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
            IsVisibleErrorMessage = false;
        }

        public async Task GetAllMeterInfo()
        {
            if (!_isWebServiceRunningDictionary.ContainsValue(true))
            {
                _userState = "MeterDetailPage" + System.DateTime.Now.ToUniversalTime().ToLongTimeString();
                _ctsMeterAttributes = new CancellationTokenSource();
                _ctsMeterStatus = new CancellationTokenSource();
                _ctsMeterReads = new CancellationTokenSource();
                _ctsMeterRestores = new CancellationTokenSource();
                _ctsMeterOutages = new CancellationTokenSource();

                //TODO: Add app logging
                //AppLogger.Debug("GET ALL GetAllMeterInfo Begin: User State: " + _userState);

                IsEnabledCheckStatusButton = false;

                try
                {
                    // get meter customer info - Name, Address, and PhoneNumber
                    _isWebServiceRunningDictionary[_getMeterForDevice] = true;
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

                        // set MeterAttributes.Status and StatusDate on MeterDetail in handler
                        _isWebServiceRunningDictionary[_getMeterAttributes] = true;
                        await _meterService.GetMeterAttributesAsync(MeterItem, HandleGetMeterAttributesCompleted, _ctsMeterAttributes.Token);

                        _isWebServiceRunningDictionary[_getMeterAvailabilityEventOutage] = true;
                        await _meterService.GetMeterAvailabilityEventsAsync(MeterItem, Enums.MeterAvailabilityEventsEventType.Outages, HandleGetMeterAvailabilityEventsCompleted, _ctsMeterOutages.Token);

                        // call GetMeterAvailabilityEventsAsync - Restorations L1555

                        IsEnabledCheckStatusButton = true;
                    }
                }
                catch
                {

                }
            }
        }

        private async void HandleGetMeterAvailabilityEventsCompleted(MeterAvailabilityEventsResponse response, Enums.MeterAvailabilityEventsEventType eventType, Exception ex)
        {
            try
            {
                if (ex != null)
                {
                    if (ex.GetType() == typeof(AuthenticationRequiredException))
                    {
                        HandleAuthorizationRequired();
                        return;
                    }
                }

                //TODO: Add app logging
                //AppLogger.Debug("  GetMeterAvailabilityEventsCompleted: Method Begin " + eventType.ToString());

                CopyMeterAvailabilityEvents(response, eventType);
                MeterEventType = eventType;

                if(MeterEventType == MeterAvailabilityEventsEventType.Outages)
                {
                    //stopOutageAnimation();

                    _isWebServiceRunningDictionary[_getMeterAvailabilityEventOutage] = false;
                }
                else if(MeterEventType == MeterAvailabilityEventsEventType.Restorations)
                {
                    //stopRestorationAnimation();

                    _isWebServiceRunningDictionary[_getMeterAvailabilityEventRestoration] = false;
                }

                if(ex == null)
                {
                    if(MeterEventType == MeterAvailabilityEventsEventType.Outages)
                    {
                       if(Outages != null)
                        {
                            OutagesValueText = Outages.Events.Count.ToString();
                            OutagesValueTextColor = Color.Blue;

                            if(Outages.Events.Count > 0)
                            {
                                await TapOutageRestore();
                            }
                        }
                    }
                    else if (MeterEventType == MeterAvailabilityEventsEventType.Restorations)
                    {
                        if(Restores != null)
                        {
                            RestoresValueText = Restores.Events.Count.ToString();
                            RestoresValueTextColor = Color.Blue;

                           if(Restores.Events.Count > 0)
                            {
                                await TapOutageRestore();
                            }
                        }
                    }
                }
                else
                {
                    //AppLogger.Debug("handleGetMeterAvailabilityEventsCompleted: Error Occured:  " + ex.Message);
                    if(MeterEventType == MeterAvailabilityEventsEventType.Restorations)
                    {
                        RestoresValueText = "N/A";
                        RestoresValueTextColor = Color.Red;
                    }
                    else if (MeterEventType == MeterAvailabilityEventsEventType.Outages)
                    {
                        OutagesValueText = "N/A";
                        OutagesValueTextColor = Color.Red;
                    }
                    ErrorMessageText = ex.Message;
                    IsVisibleErrorMessage = true;
                }
                TryEnableCheckStatusButton();
            }
            catch (Exception e)
            {
                //TODO: Add error logging
                //AppLogger.LogError(e);

                Device.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Error", $"{e.Message}: {e.StackTrace}", "Close");
                });
            }
        }

        private void CopyMeterAvailabilityEvents(MeterAvailabilityEventsResponse response, MeterAvailabilityEventsEventType eventType)
        {
            if(response == null)
            {
                if(eventType == MeterAvailabilityEventsEventType.Outages)
                {
                    Outages = null;
                }
                else if (eventType == MeterAvailabilityEventsEventType.Restorations)
                {
                    Restores = null;
                }
            }
            else
            {
                if (response.EventType == MeterAvailabilityEventsEventType.Outages)
                {
                    Outages = response;
                }
                else if(response.EventType == MeterAvailabilityEventsEventType.Restorations)
                {
                    Restores = response;
                }
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
                if(ex != null)
                {
                    if(ex.GetType() == typeof(AuthenticationRequiredException))
                    {
                        HandleAuthorizationRequired();
                        return;
                    }
                }

                //TODO: Add app logging
                //AppLogger.Debug("  GetMeterAttributesCompleted: Method Start");

                //TODO: implement method L1320
                //stopMeterAttributesAnimations(false);

                _isWebServiceRunningDictionary[_getMeterAttributes] = false;

                if(ex == null)
                {
                    // set MeterAttributes from response
                    MeterAttributes = meterAttributesResponse;

                    if (MeterAttributes.StatusDate != DateTimeOffset.MinValue)
                    {
                        //sets Meter Last Comm date (Status Date)
                        StatusDate = MeterAttributes.StatusDate.ToLocalTime().ToString(AppVariables.MilitaryFormatStringShort);
                    }

                    if (MeterAttributes.IsPingable)
                    {
                        MeterStateTextColor = Color.Green;

                        if(_userState != null)
                        {
                            try
                            {
                                var response = await _meterService.PerformActivityRequest(GetLogActivityAndMeterPingActivityRequest());
                                ActivityID = response.Value;

                                _ctsMeterStatus = new CancellationTokenSource();

                                _meterService.GetMeterStatusAsync(MeterItem, this.ActivityID, HandleGetMeterStatusCompleted, _ctsMeterStatus.Token);

                                _isWebServiceRunningDictionary[_getMeterStatus] = true;
                            }
                            catch (AuthenticationRequiredException)
                            {
                                HandleAuthorizationRequired();
                                return;
                            }
                            catch (Exception ex1)
                            {
                                //TODO: Add app logging
                                //this.LogApplicationError("MeterDetailPage.HandleGetMeterAttributesCompleted", ex1);
                            }
                            finally
                            {
                                _isPingActivityRequestCompleted = true;
                            }
                        } // end if userState
                    } // end if MeterAttributes.IsPingable
                    else
                    {
                        MeterStateTextColor = Color.Red;
                        //stopMeterAttributesAnimations(true);

                        if (string.IsNullOrWhiteSpace(MeterAttributes.Status))
                        {
                            IsVisibleErrorMessage = true;
                            ErrorMessageText = "Not Found";
                        }
                    }
                }
                else
                {
                    //TODO: Add app logging
                    //AppLogger.Debug("handleGetMeterAttributesCompleted: Error: " + ex.Message);

                    //stopMeterStatusAnimations(true);

                    if(ex is MeterNotFoundException)
                    {
                        MeterAttributes.Status = "Not found";
                        MeterStateTextColor = Color.Red;
                    }
                    else
                    {
                        MeterAttributes.Status = "Error";
                        MeterStateTextColor = Color.Red;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            App.Current.MainPage.DisplayAlert("Service Error", "Get Meter Attributes service call failed.", "Close");
                        });
                    }
                    ErrorMessageText = ex.Message;
                    IsVisibleErrorMessage = true;
                }

                //see if we can enable the CheckStatus button
                TryEnableCheckStatusButton();
            }
            catch (Exception e)
            {
                //TODO: Add error logging
                //AppLogger.LogError(e);
                Device.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Error", $"{e.Message}: {e.StackTrace}", "Close");
                });
            }
        }

        protected void TryEnableCheckStatusButton()
        {
            if (!_isWebServiceRunningDictionary.ContainsValue(true))
            {
                IsEnabledCheckStatusButton = true;
            }
        }

        protected async void HandleGetMeterStatusCompleted(MeterStatusResponse response, Exception ex)
        {
            var pingActionRequest = GetPostMeterPingActivityCompleteRequest();
        }
    }
}

