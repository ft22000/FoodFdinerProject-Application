using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace FoodFinder.Views
{
    ///<summary>
    ///Send request reason to database
    ///</summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BecomeAnEventOrganiserPage : ContentPage
	{
        /// <summary>
        /// Default Constructor Function, initialises all components on the page.
        /// </summary>
		public BecomeAnEventOrganiserPage ()
		{
			InitializeComponent ();
		}

        /// <summary>
        /// Sends a permission request to the database and email all Sustainability Team users
        /// </summary>
        async void SendRequest()
        {
            //JW: Enables the loading spinner
            loadingSpinner.IsRunning = true;

            //Kim: Disable the request button to prevent spamming
            BecomeEventOrganiserButton.Clicked -= RequestPermission;
            BecomeEventOrganiserButton.IsEnabled = false;

            //JA: Get the ID of the current user from the session
            int userID = Int32.Parse(Preferences.Get("UserID", ""));
            //JA: Get the current user's email from the session
            string email = Preferences.Get("Email", "");
            //JA: Get the request reason
            string reason = requestReasonField.Text;

            //JA: Send the request
            await App.RestManager.CreateRequest(userID, email, reason);

            loadingSpinner.IsRunning = false;
            BecomeEventOrganiserButton.IsEnabled = true;
            BecomeEventOrganiserButton.Clicked += RequestPermission;
            await DisplayAlert("Success", "Your request has been submitted.", "Close");

            //JA: Navigate away from the request page
            await Navigation.PopAsync();
        }

        /// <summary>
        /// Respond to pressing the request permission button
        /// </summary>
        /// <param name="sender">Objects Data Sender</param>
        /// <param name="e">Event Handler</param>
        void RequestPermission(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(requestReasonField.Text))
            {
                DisplayAlert("Alert", "Please enter your reason.", "close");
            }
            else
            {
                SendRequest();
            }
        }
    }
}