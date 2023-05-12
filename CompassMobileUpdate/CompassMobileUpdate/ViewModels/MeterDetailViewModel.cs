using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages;
using CompassMobileUpdate.Services;
using Xamarin.Forms;
using static CompassMobileUpdate.Models.ActivityMessage;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.ViewModels
{
	public class MeterDetailViewModel : ViewModelBase
	{
        private readonly MeterService _meterService;

        public Dictionary<int, bool> _isWebServiceRunningDictionary;

        const int _getMeterAttributes = 1,
                        _getMeterAvailabilityEventOutage = 2,
                        _getMeterAvailabilityEventRestoration = 3,
                        _getMeterStatus = 4,
                        _getMeterReads = 5,
                        _getMeterForDevice = 6;

        private CancellationTokenSource _ctsMeterAttributes, _ctsMeterStatus, _ctsMeterReads, _ctsMeterOutages, _ctsMeterRestores, _ctsGetMeterForDevice;

        private bool _isPingActivityRequestCompleted = false;

        public bool _AppMaintenanceInitiated { get; set; }
        public bool _isPageBeingPushed { get; set; }
        public bool _isTimeOutCountDownRunning { get; set; }
        public bool _isFirstPageLoad { get; set; }

        private object _lockMeter = new Object();
        
        public string _userState;

        public int? ActivityID { get; set; }

        public Color CustomerNameTextColor { get; set; }

        public string ErrorMessageText { get; set; }

        public bool? HasOverlappingVoltage
        {
            get
            {
                return AppHelper.HasOverlappingVoltage(MeterItem);
            }
        }

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

        public bool IsFinalMeterStatusGood { get; set; }     

        public bool IsVisibleErrorMessage { get; set; }

        public bool IsVisiblePingStatusValueImg { get; set; }

        public bool IsVisibleMeterStatusImage { get; set; }

        public bool IsVisibleVoltageStatusValueImg { get; set; }

        public bool IsVisibileVoltageAValueLabel { get; set; }

        public bool IsVisibileVoltageBValueLabel { get; set; }

        public bool IsVisibileVoltageCValueLabel { get; set; }

        public bool? IsVoltagePhaseAInRange { get; set; }
             
        public bool? IsVoltagePhaseBInRange
        {
            get
            {
                if (!MeterReads.AreAllVoltagesValid)
                {
                    var isInRange = AppHelper.IsVoltageInRangeAsync(MeterItem, MeterReads.VoltagePhaseB);
                    return isInRange;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool? IsVoltagePhaseCInRange
        {
            get
            {
                if (!MeterReads.AreAllVoltagesValid)
                {
                    var isInRange = AppHelper.IsVoltageInRangeAsync(MeterItem, MeterReads.VoltagePhaseC);
                    return isInRange;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsVisibleVoltageStatusMsg { get; set; }

        public bool AllowsPQRs { get; set; }

        public Manufacturer Manufacturer { get; set; }

        public Meter MeterItem { get; set; }

        public string MeterTypeNumber { get; set; }

        public MeterAttributesResponse MeterAttributes { get; set; }

        public MeterReadsResponse MeterReads { get; set; }

        public MeterStatusResponse MeterStatus { get; set; }

        public string MeterStatusImage { get; set; }

        public MeterAvailabilityEventsResponse Outages { get; set; }

        public MeterAvailabilityEventsResponse Restores { get; set; }

        public MeterAvailabilityEventsEventType MeterEventType { get; set; }

        public Color MeterStateTextColor { get; set; }

        public string OutagesValueText { get; set; }

        public Color OutagesValueTextColor { get; set; }

        public string PingStatusImage { get; set; }

        public string RestoresValueText { get; set; }

        public Color RestoresValueTextColor { get; set; }

        public string StatusDate { get; set; }

        public string VoltageAValueText { get; set; }

        public string VoltageBValueText { get; set; }

        public string VoltageCValueText { get; set; }

        public Color VoltageAValueTextColor { get; set; }

        public Color VoltageBValueTextColor { get; set; }

        public Color VoltageCValueTextColor { get; set; }

        public bool? VoltageStatus { get; set; }
        
        public string VoltageStatusImage { get; set; }

        public string VoltageStatusMessage { get; set; }

        public string CustomerContactNumber { get; set; }

        public ICommand TapOutageRestoreCommand => new Command(async () =>
        {
            await TapOutageRestore();
        });

        private async Task TapOutageRestore()
        {
            //var availabilityEventsPage = Resolver.Resolve<AvailabilityEventsPage>();
            if(MeterEventType == MeterAvailabilityEventsEventType.Outages)
            {
                await Navigation.PushAsync(new AvailabilityEventsPage(Outages.Events));
            }
            else
            {
                await Navigation.PushAsync(new AvailabilityEventsPage(Restores.Events));
            }           
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

        // ctor
        public MeterDetailViewModel(MeterService meterService)
		{
            _meterService = meterService;

            _isPageBeingPushed = false;
            _isTimeOutCountDownRunning = false;
            _isFirstPageLoad = true;
            _isLoginPageBeingPushed = false;
            _isAppMaintenanceBeingPushed = false;
            _AppMaintenanceInitiated = false;

            IsEnabledCheckStatusButton = true;
            IsVisibleErrorMessage = false;

            _ctsGetMeterForDevice = new CancellationTokenSource();
            _ctsMeterAttributes = new CancellationTokenSource();
            _ctsMeterStatus = new CancellationTokenSource();
            _ctsMeterReads = new CancellationTokenSource();
            _ctsMeterOutages = new CancellationTokenSource();
            _ctsMeterRestores = new CancellationTokenSource();

            InitializeIsWebServiceRunningDictionary();

            AddOrUpdateMeterLastAccessedTimeAsync();

            IsVisibleMeterStatusImage = true;
            IsVisiblePingStatusValueImg = true;
            IsVisibleVoltageStatusValueImg = true;
        }

        private async Task AddOrUpdateMeterLastAccessedTimeAsync()
        {
            var localSql = new LocalSql();
            await localSql.AddOrUpdateMeterLastAccessedTime(MeterItem);
        }

        private void InitializeIsWebServiceRunningDictionary()
        {
            _isWebServiceRunningDictionary = new Dictionary<int, bool>();

            _isWebServiceRunningDictionary[_getMeterAttributes] = false;
            _isWebServiceRunningDictionary[_getMeterAvailabilityEventOutage] = false;
            _isWebServiceRunningDictionary[_getMeterAvailabilityEventRestoration] = false;
            _isWebServiceRunningDictionary[_getMeterStatus] = false;
            _isWebServiceRunningDictionary[_getMeterReads] = false;
            _isWebServiceRunningDictionary[_getMeterForDevice] = false;
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



                    //var meter = await _meterService.GetMeterByDeviceUtilityIDAsync(MeterItem.DeviceUtilityID);

                    //if (meter != null)
                    //{
                    //MeterItem = meter;
                    _isWebServiceRunningDictionary[_getMeterForDevice] = true;
                    await GetMeterInfoAndCustomerInfo();

                    // set MeterAttributes.Status and StatusDate on MeterDetail in handler
                    _isWebServiceRunningDictionary[_getMeterAttributes] = true;
                        await _meterService.GetMeterAttributesAsync(MeterItem, HandleGetMeterAttributesCompleted, _ctsMeterAttributes.Token);

                        _isWebServiceRunningDictionary[_getMeterAvailabilityEventOutage] = true;
                        await _meterService.GetMeterAvailabilityEventsAsync(MeterItem, Enums.MeterAvailabilityEventsEventType.Outages, HandleGetMeterAvailabilityEventsCompleted, _ctsMeterOutages.Token);

                        _isWebServiceRunningDictionary[_getMeterAvailabilityEventRestoration] = true;
                        await _meterService.GetMeterAvailabilityEventsAsync(MeterItem, Enums.MeterAvailabilityEventsEventType.Restorations, HandleGetMeterAvailabilityEventsCompleted, _ctsMeterRestores.Token);

                        //startGetInfoCounter(); //for fade out animation on label
                        StartTimeOutCountDown();
                    //}
                }
                catch (AuthenticationRequiredException)
                {
                    HandleAuthorizationRequired();
                }
                catch (Exception e)
                {
                    await CancelAllServiceCallsAsync(true);
                    //stopAllAnimations();

                    //TODO: Add app logging
                    //this.LogApplicationError("MeterDetailPage.getAllMeterInfo", e);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage.DisplayAlert("Error", e.Message, "Close");
                    });
                }
            }
            if (AppVariables.IsLogging)
            {
                //TODO: Add app logging
                //AppLogger.Debug("GET ALL GetAllMeterInfo End");
            }
        }

        private async Task GetMeterInfoAndCustomerInfo()
        {
            Exception ex = null;
            Meter meter = null;

            _ctsGetMeterForDevice = new CancellationTokenSource();

            var localSql = new LocalSql();
            var lastUpdatedTime = await localSql.GetMeterLastUpdatedTime(MeterItem.DeviceUtilityID);
            if (MeterContainsAllCustomerInfo() &&  lastUpdatedTime > DateTime.Now.AddDays(-7))
            {
                meter = await _meterService.GetMeterByDeviceUtilityIDAsync(MeterItem.DeviceUtilityID, _ctsGetMeterForDevice.Token); //MeterItem;

                MeterItem = meter;
            }
            else
            {
                try
                {
                    meter = await _meterService.GetMeterByDeviceUtilityIDAsync(MeterItem.DeviceUtilityID, _ctsGetMeterForDevice.Token);

                    MeterItem = meter;

                    if (meter != null && !string.IsNullOrWhiteSpace(meter.DeviceUtilityID))
                        await localSql.UpdateMeterLastUpdatedTimeAsync(meter.DeviceUtilityID);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ApplicationMaintenanceException)
                {
                    HandleApplicationMaintenance();
                    return;
                }
                catch (Exception e)
                {
                    if (AppHelper.ContainsAuthenticationRequiredException(e))
                    {
                        HandleAuthorizationRequired();
                        return;
                    }
                    else
                    {
                        if (!AppHelper.ContainsNullResponseException(e))
                        {
                            //TODO: Add app logging
                            //AppLogger.LogError(e);
                        }

                        ex = e;
                    }
                }
            }
            HandleGetMeterForDeviceCompleted(meter, ex);
        }

        private async void HandleGetMeterForDeviceCompleted(Meter meter, Exception ex)
        {
            if (ex != null)
            {
                if (ex.GetType() == typeof(AuthenticationRequiredException))
                {
                    HandleAuthorizationRequired();
                    return;
                }
                else if (ex is ApplicationMaintenanceException)
                {
                    HandleApplicationMaintenance();
                    return;
                }
            }

            //stopMeterForDeviceAnimations();

            _isWebServiceRunningDictionary[_getMeterForDevice] = false;

            //TODO: Add app logging
            //AppLogger.Debug("  GetMeterForDeviceCompleted: Method Start");

            if (ex == null)
            {
                if(meter != null)
                {
                    // check that meter info matches
                    MeterItem.CustomerName = meter.CustomerName;
                    MeterItem.DeviceUtilityIDWithLocation = meter.DeviceUtilityIDWithLocation;
                    MeterItem.CustomerAddress = meter.CustomerAddress;
                    MeterItem.CustomerClass = meter.CustomerClass;
                    MeterItem.CustomerContactNumber = meter.CustomerContactNumber;

                    var localSql = new LocalSql();
                    await localSql.UpdateLocalMeterCustomerInformation(meter);

                    // set MeterTypeNumber on MeterDetail
                    if (string.IsNullOrWhiteSpace(MeterItem.DeviceUtilityIDWithLocation))
                    {
                        MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityID;
                    }
                    else
                    {
                        MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityIDWithLocation;
                    }

                    if (string.IsNullOrWhiteSpace(MeterItem.CustomerName) && string.IsNullOrWhiteSpace(MeterItem.CustomerAddress))
                    {
                       MeterItem.CustomerName = "Customer Not Found";
                    }
                    else
                    {
                        MeterItem.CustomerName = meter.CustomerName;
                        MeterItem.CustomerAddress = meter.CustomerAddress;
                    }

                    if (!string.IsNullOrWhiteSpace(MeterItem.CustomerContactNumber))
                    {
                        MeterItem.CustomerContactNumber = GetCustomerContactNumberFormatted();
                    }
                }
                else
                {
                    if (!_ctsGetMeterForDevice.IsCancellationRequested)
                       MeterItem.CustomerName = "Customer Not Found";
                }
            }
            else
            {
                if (ex is MeterNotFoundException || AppHelper.ContainsNullResponseException(ex))
                {
                    MeterItem.CustomerName = "Customer Not Found";
                }
                else
                {
                    MeterItem.CustomerName = "Customer Service Error";
                    CustomerNameTextColor = Color.Red;

                    //TODO: Add app logging
                    //AppLogger.LogError(ex);

                }
            }
            TryEnableCheckStatusButton();
        }

        private bool MeterContainsAllCustomerInfo()
        {
            string customerAddress = MeterItem.CustomerAddress;
            string contactNumber = GetCustomerContactNumberFormatted();
            string customerName = MeterItem.CustomerName;

            if (string.IsNullOrWhiteSpace(MeterItem.CustomerAddress)
                || string.IsNullOrWhiteSpace(GetCustomerContactNumberFormatted())
                || string.IsNullOrWhiteSpace(MeterItem.CustomerName)
            )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetCustomerContactNumberFormatted()
        {
            string temp;
            if (AppHelper.TryFormatPhoneNumberFromDigits(MeterItem.CustomerContactNumber, out temp))
            {
                return temp;
            }
            else
            {
                return MeterItem.CustomerContactNumber;
            }
        }

        protected async void StartTimeOutCountDown()        {
            if (_isTimeOutCountDownRunning == false)
            {
                _isTimeOutCountDownRunning = true;
                DateTime startTime = DateTime.Now;

                while (_isTimeOutCountDownRunning)
                {
                    await Task.Delay(500);

                    //If the webservices have stopped running prior to the timeout
                    if (!_isWebServiceRunningDictionary.ContainsValue(true))
                    {
                        _isTimeOutCountDownRunning = false;
                        //ErrorMessageText = "All Service Calls Completed";
                        //IsVisibleErrorMessage = true;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            App.Current.MainPage.DisplayAlert("Service Information", "All Service Calls Completed", "Close");
                        });
                        break;
                    }

                    if (DateTime.Now.AddSeconds(0 - AppVariables.MeterDetailTimeOutInSeconds) > startTime)
                    {

                        //Cancel any calls that are still running
                        if (_isWebServiceRunningDictionary.ContainsValue(true))
                        {
                            await CancelAllServiceCallsAsync(true);

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                App.Current.MainPage.DisplayAlert("Service Timeout", "Timeout Exceeded: remaining calls have been cancelled", "Close");
                            });
                            
                            //AppLogger.Debug("Timeout Exceeded: remaining calls have been cancelled");
                        }
                        _isTimeOutCountDownRunning = false;
                    }
                }
            }
        }

        public async Task CancelAllServiceCallsAsync(bool isCancelledBecauseOfError)
        {
            if (_isWebServiceRunningDictionary[_getMeterForDevice])
            {
                _ctsGetMeterForDevice.Cancel(false);
                _isWebServiceRunningDictionary[_getMeterForDevice] = false;

                //stopMeterForDeviceAnimations();

                //AppLogger.Debug("Stopping: GetMeterForDevice");
            }

            if (_isWebServiceRunningDictionary[_getMeterAvailabilityEventRestoration])
            {
                _ctsMeterRestores.Cancel(false);
                _isWebServiceRunningDictionary[_getMeterAvailabilityEventRestoration] = false;

                //stopRestorationAnimation();

                //AppLogger.Debug("Stopping: Get Restores");
            }

            if (_isWebServiceRunningDictionary[_getMeterAvailabilityEventOutage])
            {
                _ctsMeterOutages.Cancel(false);
                _isWebServiceRunningDictionary[_getMeterAvailabilityEventOutage] = false;

                //stopOutageAnimation();

                //AppLogger.Debug("Stopping: Get Outages");
            }

            if (_isWebServiceRunningDictionary[_getMeterAttributes])
            {
                _ctsMeterAttributes.Cancel(false);
                _isWebServiceRunningDictionary[_getMeterAttributes] = false;

                //stopMeterAttributesAnimations(true);

                //AppLogger.Debug("Stopping: GetMeterAttributes");

            }
            else
            {
                if (_isWebServiceRunningDictionary[_getMeterStatus])
                {
                    _ctsMeterStatus.Cancel(false);
                    _isWebServiceRunningDictionary[_getMeterStatus] = false;

                    if (ActivityID.HasValue)
                    {
                        var pingRequest = GetPostMeterPingActivityCompleteRequest();
                        pingRequest.Result = ResultEnum.NULL;
                        if (isCancelledBecauseOfError)
                        {
                            pingRequest.Status = StatusEnum.E;
                        }
                        else
                        {
                            pingRequest.Status = StatusEnum.I;
                            pingRequest.Result = ResultEnum.CANCELLED;
                        }
                        try
                        {
                            await _meterService.PerformActivityRequest(pingRequest);

                            await _meterService.PerformActivityRequest(GetPostActivityCompleteRequest());
                        }
                        catch (AuthenticationRequiredException)
                        {
                            HandleAuthorizationRequired();
                            return;
                        }
                    }

                    //stopMeterStatusAnimations(true);

                    //AppLogger.Debug("Stopping: GetMeterStatus");
                }

                if (_isWebServiceRunningDictionary[_getMeterReads])
                {
                    _ctsMeterReads.Cancel(false);
                    //AppVariables.AppService.CancelGetMeterReads(_userState);
                    _isWebServiceRunningDictionary[_getMeterReads] = false;

                    //stopMeterReadsAnimations();

                    if (ActivityID.HasValue)
                    {
                        var pqrRequest = GetPostMeterPQRActivityCompleteRequest();
                        if (isCancelledBecauseOfError)
                        {
                            pqrRequest.Status = StatusEnum.E;
                        }
                        else
                        {
                            pqrRequest.Status = StatusEnum.I;
                            pqrRequest.Result = ResultEnum.CANCELLED;
                        }
                        try
                        {
                            await _meterService.PerformActivityRequest(pqrRequest);

                            await _meterService.PerformActivityRequest(GetPostActivityCompleteRequest());
                        }
                        catch (AuthenticationRequiredException)
                        {
                            HandleAuthorizationRequired();
                            return;
                        }
                    }

                    //AppLogger.Debug("Stopping: GetMeterReads");
                }
            }
            //stopAllAnimations();
            TryEnableCheckStatusButton();
        }
//        if (!MeterReads.AreAllVoltagesValid)
//                {
//                    //var isInRange = 
//                    return AppHelper.IsVoltageInRangeAsync(MeterItem, MeterReads.VoltagePhaseA);
//                }
//                else
//                {
//                    return true;
//                }
//}
public ActivityMessage.PostPQRActivityCompleteRequest GetPostMeterPQRActivityCompleteRequest()
        {
            ActivityMessage.PostPQRActivityCompleteRequest request = new ActivityMessage.PostPQRActivityCompleteRequest();
            if (this.ActivityID.HasValue)
            {
                request.ActivityID = this.ActivityID.Value;
            }

            request.MeterDeviceUtilityID = MeterItem.DeviceUtilityID;

            if (!MeterReads.AreAllVoltagesValid)
            {
                IsVoltagePhaseAInRange = AppHelper.IsVoltageInRangeAsync(MeterItem, MeterReads.VoltagePhaseA);
            }

            if (this.MeterReads != null)
            {
                request.VoltagePhaseA = this.MeterReads.VoltagePhaseA;
                request.VoltagePhaseB = this.MeterReads.VoltagePhaseB;
                request.VoltagePhaseC = this.MeterReads.VoltagePhaseC;

                request.IsVoltagePhaseAInRange = this.IsVoltagePhaseAInRange;
                request.IsVoltagePhaseBInRange = this.IsVoltagePhaseBInRange;
                request.IsVoltagePhaseCInRange = this.IsVoltagePhaseCInRange;
            }

            return request;
        }

        public ActivityMessage.PostActivityComplete GetPostActivityCompleteRequest()
        {
            ActivityMessage.PostActivityComplete request = new ActivityMessage.PostActivityComplete();

            if (this.ActivityID.HasValue)
            {
                request.ActivityID = this.ActivityID.Value;
            }

            return request;
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
            request.MeterState = MeterAttributes.Status;

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
                _isPingActivityRequestCompleted = false;

                if(ex == null)
                {
                    // set MeterAttributes from response
                    MeterAttributes = meterAttributesResponse;
                    MeterItem.MacID = MeterAttributes.MacID;
                    MeterItem.Status = MeterAttributes.Status;
                    MeterItem.StatusDate = AppHelper.GetConfiguredTimeZone(MeterAttributes.StatusDate);
                    MeterItem.Form = MeterAttributes.Form;
                    MeterItem.Version = MeterAttributes.Version;
                    MeterItem.Model = MeterAttributes.Model;
                    MeterItem.ManufacturerName = MeterAttributes.ManufacturerName;
                    MeterItem.Latitude = MeterAttributes.Latitude;
                    MeterItem.Longitude = MeterAttributes.Longitude;
                    MeterItem.Type = MeterAttributes.Type;
                    MeterItem.TypeName = MeterAttributes.TypeName;
                    AllowsPQRs = MeterAttributes.IsPQRCapable;

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
                                var logActivityAndMeterPingActivityRequest = GetLogActivityAndMeterPingActivityRequest();

                                var response = await _meterService.PerformActivityRequest(logActivityAndMeterPingActivityRequest);

                                ActivityID = response.Value;

                                _ctsMeterStatus = new CancellationTokenSource();

                                await _meterService.GetMeterStatusAsync(MeterItem, ActivityID, HandleGetMeterStatusCompleted, _ctsMeterStatus.Token);

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
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    App.Current.MainPage.DisplayAlert("Error", $"{ex1.Message}", "OK");
                                });
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
            
            try
            {
                //TODO: Add app logging
                //AppLogger.Debug("    GetMeterStatusCompleted: Method Begin");
                if (ex != null)
                {
                    if (ex.GetType() == typeof(AuthenticationRequiredException))
                    {
                        HandleAuthorizationRequired();
                        return;
                    }
                    else if (ex is ApplicationMaintenanceException)
                    {
                        HandleApplicationMaintenance();
                        return;
                    }
                }
                _isWebServiceRunningDictionary[_getMeterStatus] = false;

                var pingActionRequest = GetPostMeterPingActivityCompleteRequest();

                IsVisiblePingStatusValueImg = true;

                if(ex == null)
                {
                    pingActionRequest.Status = StatusEnum.C;
                    MeterStatus = response;

                    //stopMeterStatusAnimations(false);

                    if (MeterStatus.Ping)
                    {
                        pingActionRequest.Result = ResultEnum.OK;

                        if(_userState != null)
                        {
                            PingStatusImage = "status_good.png";

                            if (AllowsPQRs)
                            {
                                try
                                {
                                    await _meterService.GetMeterReadsAsync(MeterItem, ActivityID, HandleGetMeterReadsCompleted, _ctsMeterReads.Token);

                                    _isWebServiceRunningDictionary[_getMeterReads] = true;

                                    if (ActivityID.HasValue)
                                    {
                                        await _meterService.PerformActivityRequest(GetLogActivityAndMeterPingActivityRequest());
                                    }
                                }
                                catch (AuthenticationRequiredException)
                                {
                                    HandleAuthorizationRequired();
                                    return;
                                }
                                catch (Exception ex1)
                                {
                                    //TODO: Add app logging
                                    //AppLogger.LogError(ex1);
                                }
                            }
                            else //(!AllowPQRs)
                            {
                                try
                                {
                                    if (ActivityID.HasValue)
                                    {
                                        await _meterService.PerformActivityRequest(GetPostActivityCompleteRequest());
                                        await _meterService.PerformActivityRequest(GetLogMeterPQRActivityRequest());
                                    }
                                }
                                catch (AuthenticationRequiredException)
                                {
                                    HandleAuthorizationRequired();
                                }
                                catch (Exception ex1)
                                {
                                    //TODO: Add app logging
                                    //AppLogger.LogError(ex1);
                                }

                                //stopMeterReadsAnimations();
                                VoltageStatusMessage= "N/A";
                                IsVisibleVoltageStatusMsg = true;
                                SetFinalMeterStatusImage();
                            }
                        }
                    }
                    else
                    {
                        pingActionRequest.Result = ResultEnum.FAILED;
                        PingStatusImage = "status_error.png";
                        MeterStatusImage = "status_error.png";
                        IsVisibleMeterStatusImage = true;
                        //stopMeterReadsAnimations();
                        VoltageStatusMessage = "N/A";
                        IsVisibleVoltageStatusMsg = true;
                    }
                }
                else
                {
                    pingActionRequest.Status = StatusEnum.E;
                    pingActionRequest.Result = ResultEnum.NULL;

                    //TODO: Add app logging
                    //AppLogger.LogError(ex);

                    PingStatusImage = "status_error.png";

                    //stopMeterStatusAnimations(true);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage.DisplayAlert("Service Error", "Get Meter Status service call failed.", "Close");
                    });
                    ErrorMessageText = ex.Message;
                    IsVisibleErrorMessage = true;
                }

                //Log completion of the Ping
                if (pingActionRequest.ActivityID != 0)
                {
                    try
                    {
                        await _meterService.PerformActivityRequest(pingActionRequest);
                    }
                    catch (AuthenticationRequiredException)
                    {
                        HandleAuthorizationRequired();
                    }
                    catch (Exception ex1)
                    {
                        //TODO: Add app logging
                        //AppLogger.LogError(ex1);
                    }
                }
                //An Event has completed see if we can enableTheCheckStatusButton
                TryEnableCheckStatusButton();
            }
            catch (Exception e)
            {
                //TODO: Add app logging
                //AppLogger.LogError(e);

                Device.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Service Error", "Get Meter Status service call failed.", "Close");
                });
            }
        }

        public ActivityMessage.LogMeterPQRActivityRequest GetLogMeterPQRActivityRequest()
        {
            ActivityMessage.LogMeterPQRActivityRequest request = new ActivityMessage.LogMeterPQRActivityRequest();
            if (ActivityID.HasValue)
            {
                request.ActivityID = ActivityID.Value;

            }
            request.MeterDeviceUtilityID = MeterItem.DeviceUtilityID;
            return request;
        }

        public async void HandleGetMeterReadsCompleted(MeterReadsResponse response, Exception ex)
        {
            //TODO: Add app logging
            //AppLogger.Debug("      GetMeterReadsCompleted: Method Begin");

            if (ex != null)
            {
                if (ex.GetType() == typeof(AuthenticationRequiredException))
                {
                    HandleAuthorizationRequired();
                    return;
                }
                else if (ex is ApplicationMaintenanceException)
                {
                    HandleApplicationMaintenance();
                    return;
                }
            }

            //stopMeterReadsAnimations();
            _isWebServiceRunningDictionary[_getMeterReads] = false;

            MeterReads = response;

            IsVisibleVoltageStatusValueImg = true;

            var meterPQRCompleteRequest = GetPostMeterPQRActivityCompleteRequest();

            if (_ctsMeterReads.IsCancellationRequested)
            {
                meterPQRCompleteRequest.Result = ResultEnum.CANCELLED;
            }
            else
            {
                meterPQRCompleteRequest.Result = meterPQRCompleteRequest.GetActivityPQRResult();
            }

            if(ex == null)
            {
                if (_ctsMeterReads.IsCancellationRequested)
                {
                    meterPQRCompleteRequest.Status = StatusEnum.I;
                }
                else
                {
                    meterPQRCompleteRequest.Status = StatusEnum.C;
                }

                //PhaseA
                if (MeterReads.VoltagePhaseA.HasValue)
                {
                    VoltageAValueText = Convert.ToInt32(MeterReads.VoltagePhaseA.Value).ToString();

                    if (IsVoltagePhaseAInRange.HasValue)
                    {
                        if (IsVoltagePhaseAInRange.Value)
                        {
                            if (HasOverlappingVoltage.Value)
                            {
                                VoltageAValueTextColor = Color.Orange;
                            }
                            else
                            {
                                VoltageAValueTextColor = Color.Green;
                            }
                        }
                        else
                        {
                            VoltageAValueTextColor = Color.Red;
                        }
                    }
                    else
                    {
                        VoltageAValueTextColor = Color.Black;
                    }
                }

                //Phase B
                if (MeterReads.VoltagePhaseB.HasValue)
                {
                    VoltageBValueText = Convert.ToInt32(MeterReads.VoltagePhaseB.Value).ToString();

                    if (IsVoltagePhaseBInRange.HasValue)
                    {
                        if (IsVoltagePhaseBInRange.Value)
                        {
                            if (HasOverlappingVoltage.Value)
                            {
                                VoltageBValueTextColor = Color.Orange;
                            }
                            else
                            {
                                VoltageBValueTextColor = Color.Green;
                            }
                        }
                        else
                        {
                            VoltageBValueTextColor = Color.Red;
                        }
                    }
                    else
                    {
                        VoltageBValueTextColor = Color.Black;
                    }
                }

                //Phase C
                if (MeterReads.VoltagePhaseC.HasValue)
                {
                    VoltageCValueText = Convert.ToInt32(MeterReads.VoltagePhaseC.Value).ToString();

                    if (IsVoltagePhaseCInRange.HasValue)
                    {
                        if (IsVoltagePhaseCInRange.Value)
                        {
                            if (HasOverlappingVoltage.Value)
                            {
                                VoltageCValueTextColor = Color.Orange;
                            }
                            else
                            {
                                VoltageCValueTextColor = Color.Green;
                            }
                        }
                        else
                        {
                            VoltageCValueTextColor = Color.Red;
                        }
                    }
                    else
                    {
                        VoltageCValueTextColor = Color.Black;
                    }
                }

                SetVoltageStatus();

                if (VoltageStatus.HasValue)
                {
                    if (VoltageStatus.Value)
                    {
                        if (HasOverlappingVoltage.Value)
                        {
                            //StatusUncertainImage
                            VoltageStatusImage = "question.png";
                        }
                        else
                        {
                            //StatusGoodImage
                            VoltageStatusImage = "status_good.png";
                        }
                    }
                    else
                    {
                        //StatusErrorImage
                        VoltageStatusImage = "status_error.png";
                    }
                }               
            }
            else
            {
                meterPQRCompleteRequest.Status = StatusEnum.E;
                VoltageStatusImage = "status_error.png";

                string logMessage = (ex.Message + ": " + ex.StackTrace);
                string errorMessage = ex.Message;

                if (ex.InnerException != null)
                {
                    errorMessage += Environment.NewLine + ex.InnerException.Message;
                    logMessage += ". " + ex.InnerException.Message + ": " + ex.InnerException.StackTrace;
                }
                //TODO: Add app logging
                //AppLogger.Debug(logMessage);
                //LogApplicationError("MeterDetailPage.handleGetMeterReadsCompleted", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Service Error", "Get Meter Reads service call failed", "close");
                });
                ErrorMessageText = ex.Message;
                IsVisibleErrorMessage = true;
            }
            ShowVoltageReads();

            SetFinalMeterStatusImage();

            if (ActivityID.HasValue)
            {
                try
                {
                    await _meterService.PerformActivityRequest(meterPQRCompleteRequest);
                    await _meterService.PerformActivityRequest(GetPostActivityCompleteRequest());
                }
                catch (AuthenticationRequiredException)
                {
                    HandleAuthorizationRequired();
                    return;
                }
                catch (Exception ex1)
                {
                    //TODO: Add app logging
                    //this.LogApplicationError("MeterDetailPage.HandleGetMeterStatusCompleted", ex1);
                }
            }
            TryEnableCheckStatusButton();
        }

        private void SetVoltageStatus()
        {
            bool nullA, nullB, nullC, noValueA, noValueB, noValueC;
            nullA = nullB = nullC = noValueA = noValueB = noValueC = false;

            if (MeterReads != null)
            {
                if (MeterReads.VoltagePhaseA.HasValue)
                {
                    if (IsVoltagePhaseAInRange.HasValue && !IsVoltagePhaseAInRange.Value)
                    {
                        VoltageStatus = false;
                    }
                    else if (IsVoltagePhaseAInRange == null)
                    {
                        nullA = true;
                    }
                }
                else
                {
                    noValueA = true;
                }

                if (MeterReads.VoltagePhaseB.HasValue)
                {
                    if (IsVoltagePhaseBInRange.HasValue && !IsVoltagePhaseBInRange.Value)
                        VoltageStatus =  false;

                    else if (IsVoltagePhaseBInRange == null)
                    {
                        nullB = true;
                    }
                }
                else
                {
                    noValueB = true;
                }

                if (MeterReads.VoltagePhaseC.HasValue)
                {
                    if (IsVoltagePhaseCInRange.HasValue && !IsVoltagePhaseCInRange.Value)
                        VoltageStatus =  false;
                    else if (IsVoltagePhaseCInRange == null)
                    {
                        nullC = true;
                    }
                }
                else
                {
                    noValueC = true;
                }
            }

            if (nullA || nullB || nullC)
            {
                VoltageStatus =  null;
            }
            else if (noValueA && noValueB && noValueC)
            {
                VoltageStatus =  false;
            }
            VoltageStatus =  true;
        }

        private void SetFinalMeterStatusImage()
        {
            IsVisibleMeterStatusImage = true;

            SetIsFinalMeterStatusGood();

            if (IsFinalMeterStatusGood)
            {
                MeterStatusImage = "status_good.png";
            }
            else
            {
                if (!VoltageStatus.HasValue)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage.DisplayAlert("Info", "Voltage Rules were missing for this meter. Voltage Read Quality is unknown.", "Close");
                    });
                }
                MeterStatusImage = "status_error.png";
            }
        }

        private void SetIsFinalMeterStatusGood()
        {
            bool goodPing = MeterStatus != null && MeterStatus.Ping;
            bool goodPQR = !AllowsPQRs || (AllowsPQRs && VoltageStatus.HasValue && VoltageStatus.Value);
            bool goodPQRViaService = MeterReads != null && MeterReads.AreAllVoltagesValid;

            IsFinalMeterStatusGood =  goodPing && (goodPQR || goodPQRViaService);
        }

        protected void ShowVoltageReads()
        {
            if(MeterReads != null)
            {
                if (MeterReads.VoltagePhaseA.HasValue)
                {
                    IsVisibileVoltageAValueLabel = true;
                }
                if (MeterReads.VoltagePhaseB.HasValue)
                {
                    IsVisibileVoltageBValueLabel = true;
                }
                if (MeterReads.VoltagePhaseC.HasValue)
                {
                    IsVisibileVoltageCValueLabel = true;
                }
            }
        }
    }
}

