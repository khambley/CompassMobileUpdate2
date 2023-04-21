using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{
    public partial class ScannerPage : BasePage
    {
        public ScannerPage()
        {
            InitializeComponent();

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

                // Navigate away
                await Navigation.PopAsync();
            });
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

