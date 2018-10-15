using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FoodFinder.Views
{
    /// <summary>
    /// This page allows a user to send feedback regarding the application to the server so it can be viewed in the web portal by the sustainability team.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FeedbackPage : ContentPage
	{
        /// <summary>
        /// Default Constructor Function, initialises all components on the page.
        /// </summary>
		public FeedbackPage ()
		{
			InitializeComponent ();
		}

        /// <summary>
        /// This function sends the feedback message to the server and saves it against the users user id.
        /// </summary>
        /// <param name="sender">The object triggering this function</param>
        /// <param name="args">The arguements passed by the event</param>
        async void SubmitFeedback(object sender, EventArgs args) {
            
            var feedbackText = feedbackMessageField.Text;
            int ID = Int32.Parse(Preferences.Get("UserID", ""));

            /*TE: Do not do tostring() on the field. If it is empty you will crash the app with a null exception.*/
            if (feedbackText != null) {

                string feedbackString = feedbackText.ToString();

                /*TE: We don't want them being sneaky and entering whitespace or an empty string. Probably do not need isNullOrEmpty since we check if the field is null*/
                if (!String.IsNullOrEmpty(feedbackString) && !String.IsNullOrWhiteSpace(feedbackString))
                {
                    loadingSpinner.IsRunning = true;
                    if (await App.RestManager.SaveFeedback(feedbackText.ToString(), ID))
                    {
                        /*TE: Disable spinners and inform the user it all worked.*/
                        loadingSpinner.IsRunning = false;
                        await DisplayAlert("Thank You", "Your feedback is appreciated.", "OK");
                        feedbackMessageField.Text = "";
                        App.Current.MainPage = new MainPage();
                    }
                    else
                    {
                        loadingSpinner.IsRunning = false;
                        await DisplayAlert("Alert", "Error saving feedback", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Alert", "Please enter feedback", "OK");
                }
            }
            else
            {
                await DisplayAlert("Alert", "Please enter feedback", "OK");
            }
        }
    }
}
