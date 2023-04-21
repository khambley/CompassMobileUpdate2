using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{
    public partial class ScannerPage : BasePage
    {
        ScannerViewModel vm => BindingContext as ScannerViewModel;

        public string DeviceUtilityID { get; set; }

        public Meter Meter { get; set; }

        public ScannerPage(ScannerViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            zxingScanner.Options.CameraResolutionPreset = ZXing.Mobile.CameraResolutionPreset.Preset1280x720;

            zxingScanner.AutoFocus();
        }

        // Scanning happens on a background thread, so if you want to post something back with these results
        // you have to make sure you're on the main thread
        void ZXingScannerView_OnScanResult(ZXing.Result result)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                // Stop analysis until we navigate away so we don't keep reading barcodes
                zxingScanner.IsAnalyzing = false;

                // Show an alert
                await DisplayAlert("Scanned Barcode", result.Text, "OK");

                var meterId = await GetMeterIDFromScanAsync(result.Text);

                if(meterId != null)
                {
                    await SetMeterByDeviceUtilityIDAsync(meterId);
                    NavigateToMeterDetailAsync();
                }

            });
        }

        private async Task SetMeterByDeviceUtilityIDAsync(string meterId)
        {
            var meter = new Meter();
            meterId = "G270146674"; // Integration testing purposes
            this.Meter = await vm.GetMeterByDeviceUtilityIDAsync(meterId);          
        }

        private async void NavigateToMeterDetailAsync()
        {
            if(this.Meter == null)
            {
                return;
            }
            var meterDetailpage = Resolver.Resolve<MeterDetailPage>();

            var detailViewModel = meterDetailpage.BindingContext as MeterDetailViewModel;

            detailViewModel.MeterItem = this.Meter;

            await detailViewModel.GetAllMeterInfo();

            var localSql = new LocalSql();

            await localSql.AddOrUpdateMeterLastAccessedTime(this.Meter);

            await Navigation.PushAsync(meterDetailpage);
        }

        private async Task<string> GetMeterIDFromScanAsync(string result)
        {
            try
            {
                if (result == null)
                {
                    return null;
                }
                else
                {
                    aiScannerPage.IsRunning = false;

                    var barcode = result.Trim();

                    int length = barcode.Length;

                    int idLength = 10;

                    var tempBarCode = "";

                    bool tryAgain = false;

                    SerialNumberFormatException ex;

                    for (int i = 0; i < length - 9; i++)
                    {
                        if (i + idLength > length)
                        {
                            break;
                        }
                        else
                        {
                            //tempBarCode = barcode.Substring(i, idLength);

                            tempBarCode = barcode.Substring(barcode.Length - 10, 10);

                            return tempBarCode;
                            //if (vm.IsValidSerialNumber(tempBarCode, out ex))
                            //{
                            //    return tempBarCode;
                            //}
                        }
                    }

                    tryAgain = await DisplayAlert("Format Exception", "Invalid MeterID or Partial Scan: " + result, "Scan Again", "Cancel");

                    aiScannerPage.IsRunning = true;

                    if (tryAgain)
                    {
                        return await GetMeterIDFromScanAsync(result);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {   
                await DisplayAlert("Error", "An error occured " + ex.Message, "Close");
                throw;
            }
        }

        void overlay_FlashButtonClicked(Xamarin.Forms.Button sender, System.EventArgs e)
        {
            if (zxingScanner.IsTorchOn)
            {
                Flashlight.TurnOffAsync();
            }
            else
            {
                Flashlight.TurnOnAsync();
            }
            zxingScanner.IsTorchOn = !zxingScanner.IsTorchOn;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            zxingScanner.IsScanning = true;
        }

        protected override void OnDisappearing()
        {
            zxingScanner.IsScanning = false;

            base.OnDisappearing();
        }
    }
}

