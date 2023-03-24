using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Services;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{	
	public partial class LoginPage : ContentPage
	{
        private readonly AuthService _authService;

		public LoginPage (AuthService authService)
		{
            _authService = authService;
			InitializeComponent ();           
        }

        async void btnLogin_Clicked(System.Object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUserID.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a User ID", "Close");
                tbUserID.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                await DisplayAlert("Validation Error", "Please enter a Password", "Close");
                tbPassword.Focus();
                return;
            }

            try
            {
                aiIsAuthenticating.IsRunning = true;
                btnLogin.IsEnabled = false;
                string userID = tbUserID.Text.Trim().ToLower();
                string password = tbPassword.Text;
                AuthLoginResponse response = null;
                bool cancelled = false;
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(60000); // MJ changing from 20000 to a minute (60000) after the Layer7 debacle of 2017

                try
                {
                    response = await _authService.AuthenticateUser(userID, password, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    await DisplayAlert("Service Error", "The Service Timed out. Please check your connection and try again.", "Close");

                    cancelled = true;
                }

                if (response != null)
                {
                    var user = new AppUser();

                    if (response.IsAuthenticated)
                    {
                        var currentTime = DateTime.Now;
                        user.UserID = userID;
                        user.JWT = response.Token;
                        user.JWTExpirationUTC = currentTime.AddMinutes(30);
                        
                        AppVariables.User = user;
                        var localSql = new LocalSql();
                        await localSql.AddUser(AppVariables.User);
                        //await AppVariables.ResetVoltageRules(false);

                        if (App.Current.MainPage is LoginPage)
                        {
                            var mainPage = new NavigationPage(Resolver.Resolve<MainPage>());
                            mainPage.BarBackgroundColor = Color.FromHex("#CC0033");
                            mainPage.BarTextColor = Color.White;
                            App.Current.MainPage = mainPage;
                        }
                        else
                        {
                            await Navigation.PopModalAsync();
                        }
                    }
                    else
                    {
                        await DisplayAlert("Authentication Error", response.Message, "Close");
                    }
                }
                else if (response == null & !cancelled)
                {
                    //TODO: Add logging
                    //AppVariables.AppService.LogApplicationError("LoginPage.btnLoginClick", new Exception("We were unable to connect to the Authentication Service. Response was null"));
                    await DisplayAlert("Error", "We were unable to connect to the Authentication Service. Please try again later.", "Close");
                }
            }
            catch (Exception ex)
            {
                //TODO: Add logging
                //AppVariables.AppService.LogApplicationError("LoginPage.cs", ex);
                await DisplayAlert("Error", AppHelper.GetAmalgamatedExceptionMessage(ex), "Close");
            }
            finally
            {
                aiIsAuthenticating.IsRunning = false;
                btnLogin.IsEnabled = true;
            }
        }
    }
}

