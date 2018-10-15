using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FoodFinder.Data;

namespace FoodFinder.Views
{
    /// <summary>
    /// The ForgotPasswordPage is for user who have forgoten the password of their account and need to have it 
    /// reset to gain access to their account
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgotPasswordPage : ContentPage
    {
        /// <summary>
        /// ForgotPasswordPage() initialises the ForgotPasswordPage Components.
        /// </summary>
		public ForgotPasswordPage()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Uses the supplied email and resets the password for that email account.
        /// </summary>
        /// <param name="sender">Object sender passes relevant information to the method.</param>
        /// <param name="e">Event Handler</param>
        async void ChangePassword(object sender, EventArgs e)
        {
            //JW: enables the loading spinner
            loadingSpinner.IsRunning = true;

            changePasswordButton.Clicked -= ChangePassword;
            changePasswordButton.IsEnabled = false;
            string emailText = email.Text;

            /*Kim: Current email not entered*/
            if (String.IsNullOrWhiteSpace(emailText))
            {
                //JW: disables the loading spinner
                loadingSpinner.IsRunning = false;

                await DisplayAlert("Alert", "Please enter your email.", "close");
                changePasswordButton.IsEnabled = true;
                changePasswordButton.Clicked += ChangePassword;

            }
            else
            {
                //Kim: send the email to api
                await App.RestManager.ForgotPassword(emailText);

                //JW: disables the loading spinner
                loadingSpinner.IsRunning = false;

                await DisplayAlert("Success", "An email has been sent to this address with a new password. Remember to change it on the preferences screen once you log in.", "close");
                //Kim: bring the user back to login screen
                await Navigation.PushAsync(new LoginPage());
                changePasswordButton.IsEnabled = true;
                changePasswordButton.Clicked += ChangePassword;
            }
        }
    }
}