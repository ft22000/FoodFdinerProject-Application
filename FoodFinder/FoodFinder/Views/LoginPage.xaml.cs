using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using FoodFinder.Models;
using Plugin.FirebasePushNotification;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace FoodFinder.Views
{
    /// <summary>
    /// The LoginPage Class displays the login form, facilitates the login process and then navigates the user to the default home page. In this case the
    /// default page is the Map Screen. If the user is unregistered, then the user can navigate to the Sign Up page from here.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {

        /// <summary>
        /// This stops the login function from running multiple times at once stopping the application from crashing.
        /// The button does get disabled but this is a backup precaution.
        /// </summary>
        bool attemptingLogin = false;

        /// <summary>
        /// Sets the password to be hidden by default
        /// </summary>
        bool passwordHide = true;

        /// <summary>
        /// LoginPage() initialises the LoginPage Components.
        /// </summary>
        public LoginPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Check the email and password fields to make sure they are not empty and attempts to log the user in.
        /// </summary>
        /// <param name="sender">Object sender passes relevant information to the method.</param>
        /// <param name="e">Event Handler.</param>
        void ClickedLogin(object sender, EventArgs e)
        {
            /* Avoids crash espeically if they manage to hit the button before the login attempt completes which shouldn't happen.*/
            if (!attemptingLogin)
            {
                /*TE: Grab entries from the form input fields.*/
                var emailInput = email.Text;
                var passwordInput = password.Text;

                /*Check to make sure the fields are not empty and notify the user if they are. If not then attempt to log the user into the application*/
                if (String.IsNullOrWhiteSpace(emailInput) || String.IsNullOrWhiteSpace(passwordInput))
                {
                    /*TE: Inform the user that they need to enter an email or password to login.*/
                    DisplayAlert("Error", "You must enter an email and password.", "Close");
                }
                else
                {
                    /*TE: We attempt to log the user in*/
                    CheckLogin(emailInput, passwordInput);
                }
            }
        }

        /// <summary>
        /// This function sends a request to the rest api to log the user in.
        /// </summary>
        /// <param name="email"> The users email. Unique.</param>
        /// <param name="password"> The users password.</param>
        async void CheckLogin(string email, string password)
        {
            //JW: enables the loading spinner
            loadingSpinner.IsRunning = true;

            /*TE: BUGFIX 21/07/2018. Disables login button so user cannot spam login crashing the application.*/
            loginButton.IsEnabled = false;
            attemptingLogin = true;

            /*TE: attempts to login. If unsuccessful the error message will be displayed.*/
            string errorMessage = await App.RestManager.Login(email, password);

            /*TE: This is a temporary hack to stop the app displaying an empty alert when sign in is successful. Considering returning a dictionary or result class from rest functions.*/
            if (errorMessage != "")
            {
                //JW: dodgy way to check whether the user is deleted or deactivated
                if (errorMessage == "This user has been deleted")
                {
                    loadingSpinner.IsRunning = false;
                    await DisplayAlert("Error", errorMessage, "OK");
                }
                else
                {
                    if (errorMessage == "user not activated")
                    {
                        loadingSpinner.IsRunning = false;

                        InitialPage.IsVisible = false;
                        verificationCode.IsVisible = true;
                    }
                    else
                    {
                        loadingSpinner.IsRunning = false;
                        await DisplayAlert("Error", errorMessage, "OK");
                    }
                }
            }
            else
            {
                loadingSpinner.IsRunning = false;

                Console.WriteLine("The current user's user ID is: " + Preferences.Get("UserID", ""));

                RetrieveLoginData(Preferences.Get("UserID", ""));
            }

            loginButton.IsEnabled = true;
            attemptingLogin = false;
        }

        /// <summary>
        /// This handles the event when the user presses on the signup button. The user is redirected to the signup page.
        /// </summary>
        /// <param name="sender">Object sender contains the information from the application.</param>
        /// <param name="e">Event Handler.</param>
        async void ClickedSignup(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignupPage());
        }

        /// <summary>
        /// Takes the user to the forgot password page
        /// </summary>
        /// <param name="sender">Object sender contains the information from the application.</param>
        /// <param name="e">Event Handler.</param>
        async void ForgotPasswordRecogniser(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ForgotPasswordPage());
        }

        /// <summary>
        /// Removes the navigation bar and exits the session.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            //JW: set current session email to temporary variable
            var emailTempHolder = Preferences.Get("Email", "");

            //JW: clear all session data
            Preferences.Clear();

            //JW: set session email to email stored in temporary email
            Preferences.Set("Email", emailTempHolder);

            //JW: obtain session email and set it to email field
            email.Text = Preferences.Get("Email", "");

            //TA: Sets the Navigation Bar (Hamburger Menu) to false, i.e is hidden.
            NavigationPage.SetHasNavigationBar(this, false);

            //JW: unsubscribe the device from all topics
            CrossFirebasePushNotification.Current.UnsubscribeAll();
            Console.WriteLine("Unsubscribed from all topics");
        }

        /// <summary>
        /// This function is four part
        /// 1) Obtains a list of all campuses and stores it to local device preferences -- setting them all to false (unselected)
        /// 2) Obtains a list of all subscribed campuses and updates them to true in preferences
        /// 3) Obtains notification preferences from db and sets default switch states based on that info
        /// 4) Subscribed the device to the subscribed firebase channels for push notifications (if enabled)
        /// </summary>
        /// <param name="userId">The user's unique id</param>
        async void RetrieveLoginData(string userId)
        {
            bool pushNotifications = false;

            ObservableCollection<Campus> campusList;
            ObservableCollection<Campus> subscribedCampuses;
            ObservableCollection<NotificationType> notificationSettings;

            campusList = await App.RestManager.RetrieveCampusList();
            subscribedCampuses = await App.RestManager.RetrieveSubscribedCampuses(userId);
            notificationSettings = await App.RestManager.NotificationTypeSettings(userId);

            foreach (var campus in campusList)
            {
                Preferences.Set(campus.Campus_Name, "False");
            }

            //JW: loop through observable collection and set switches to appropriate states
            //    These values should be default in the db
            //    TODO: remind someone (Joeby? Tom?) to make sure NotificationType values will be set in db on fresh setup
            foreach (var id in notificationSettings)
            {
                int notificationSettingsSwitch = id.Notification_ID;

                switch (notificationSettingsSwitch)
                {
                    case 1:
                        Preferences.Set("EmailNotificationSwitch", "True");
                        break;
                    case 2:
                        Preferences.Set("EmailNotificationSwitch", "False");
                        break;
                    case 3:
                        Preferences.Set("PushNotificationSwitch", "True");
                        pushNotifications = true;
                        break;
                    case 4:
                        Preferences.Set("PushNotificationSwitch", "False");
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }

            //JW: loop through observable collection, subscribing device to each of the campuses they have selected
            foreach (var campus in subscribedCampuses)
            {
                var subCampusNoSpace = campus.Campus_Name.Replace(" ", String.Empty);

                if (pushNotifications)
                {
                    Console.WriteLine("Subscribing device to campus: " + campus.Campus_Name);
                    CrossFirebasePushNotification.Current.Subscribe(subCampusNoSpace);
                }

                Preferences.Set(campus.Campus_Name, "True");
            }
        }

        /// <summary>
        /// Shows/hides the password button control
        /// </summary>
        /// <param name="sender">Object sender contains the information from the application.</param>
        /// <param name="e">Event Handler.</param>
        private void ShowHidePassword(object sender, EventArgs e)
        {
            
            if (passwordHide == true)
            {
                passwordHide = false;
                password.IsPassword = false;
                ShowHidePasswordLabel.Source = "show.png";
                
            }
            else
            {
                passwordHide = true;
                password.IsPassword = true;
                ShowHidePasswordLabel.Source = "hide.png";
                
            }
        }

        /// <summary>
        /// The function will verify the code sent to the user email and activate the account
        /// </summary>
        async void CheckVerification()
        {
            string VerificationCode = Code.Text;

            // JH: inputs from the signup page.
            var emailInput = email.Text;
            var passwordInput = password.Text;

            //JW: enables the loading spinner
            loadingSpinnerVerification.IsRunning = true;
            verifyButton.IsEnabled = false;

            if (String.IsNullOrWhiteSpace(VerificationCode))
            {
                await DisplayAlert("Alert", "Please enter your verification code", "close");
            }
            else
            {
                //Kim: using rest service to send verification code for comparison
                bool result = await App.RestManager.SendVerificationCode(emailInput, VerificationCode);

                if (result)
                {
                    //JW: Set push notifications to true by default
                    Preferences.Set("PushNotificationSwitch", "True");

                    //Kim: login the user
                    CheckLogin(emailInput, passwordInput);

                    //JW: disables the loading spinner
                    loadingSpinnerVerification.IsRunning = false;
                    verifyButton.IsEnabled = true;
                }
                else
                {
                    //JW: disables the loading spinner
                    loadingSpinnerVerification.IsRunning = false;
                    verifyButton.IsEnabled = true;

                    await DisplayAlert("Alert", "Your verification code is incorrect", "close");
                }
            }
        }

        /// <summary>
        /// Return to login page from popup window
        /// </summary>
        private void PopupReturn()
        {
            InitialPage.IsVisible = true;
            verificationCode.IsVisible = false;
        }

        /// <summary>
        /// Re-sends the verifcation code to the user
        /// </summary>
        async void SendVerificationCode()
        {
            var emailInput = email.Text;

            //JW: disable resend and verify buttons and enable loading spinner
            resendButton.IsEnabled = false;
            verifyButton.IsEnabled = false;
            loadingSpinnerVerification.IsRunning = true;

            await App.RestManager.SendSignupEmailAsync(emailInput);

            //JW: enable resend and verify buttons and disable loading spinner
            resendButton.IsEnabled = true;
            verifyButton.IsEnabled = true;
            loadingSpinnerVerification.IsRunning = false;
        }
    }
}
