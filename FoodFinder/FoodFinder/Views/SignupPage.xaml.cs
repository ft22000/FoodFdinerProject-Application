using Plugin.FirebasePushNotification;
using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FoodFinder.Models;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace FoodFinder.Views
{
    /// <summary>
    /// Defines the Signup Page
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignupPage : ContentPage
    {
        /// <summary>
        /// Sets the password to be hidden by default
        /// </summary>
        bool passwordHide = true;

        /// <summary>
        /// Sets the tooltips to be hidden by default
        /// </summary>
        bool showTips = false;

        /// <summary>
        /// Default Sign Up Page Class
        /// </summary>
        public SignupPage()
        {

            InitializeComponent();
            //JW: Obtains observable collection used to populate the picker
            RetrieveCampusList();
            UpdatePasswordStrength();
        }


        /// <summary>
        /// This event is function is called everytime the new password is changed. Calls a function to update the passwords strength meter and provide some feedback.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void PasswordUpdated(object sender, EventArgs e)
        {
            UpdatePasswordStrength();
        }

        /// <summary>
        /// The function quickly analyses the strength of the password and updates the password meter. Created it as a separate function so it could be called when this page loads.
        /// </summary>
        void UpdatePasswordStrength()
        {
            /*TE: number of condions the password has to meet (Use to calculate how large the password meter becomes.*/
            const int passwordConditions = 5;
            /*TE: The full length of the meter bar. (Might need to find a way to make it responsive. Not sure atm)*/
            double MaximumIndicatorLength = StrengthMeterContainer.Width;
            /*TE: How far the meter extends each time a condition is met*/
            double IndicatorStepLength = MaximumIndicatorLength / passwordConditions;
            /*TE: The colours of the passwword meter. Set here in an array to make them easier to change rather than searching below.*/
            Color[] IndicatorColours = { Color.Red, Color.Orange, Color.Yellow, Color.LawnGreen, Color.Green };

            /*TE: How many conditions the currently entered passwword has met. Used to set the length of the meter and it's colour.*/
            int conditionsMet = 0;
            /*TE: The currently entered password we are checking.*/
            var passwordInput = password.Text;

            /*TE: Prevents crash when field is empty*/
            if (String.IsNullOrWhiteSpace(passwordInput))
            {
                passwordInput = "";
            }

            /*TE: Gotta work with regex...*/
            var passwordUperCaseRegex = "[A-Z]";
            var passwordLowerCaseRegex = "[a-z]";
            var passwordNumberRegex = "[0-9]";
            var passwordSpecialCharRegex = "[\\W]";

            /*TE: Check for a capital*/
            if (Regex.IsMatch(passwordInput, passwordUperCaseRegex))
            {
                conditionsMet++;
            }

            /*TE: Check for a lowercase*/
            if (Regex.IsMatch(passwordInput, passwordLowerCaseRegex))
            {
                conditionsMet++;
            }

            /*TE: Check for a number*/
            if (Regex.IsMatch(passwordInput, passwordNumberRegex))
            {
                conditionsMet++;
            }

            /*TE: Check for a special character*/
            if (Regex.IsMatch(passwordInput, passwordSpecialCharRegex))
            {
                conditionsMet++;
            }

            /*TE: make sure password is longer than 8 characters*/
            if (passwordInput.Trim().Length > 7)
            {
                conditionsMet++;
            }

            /*TE: Based on how many conditions the current password statisfies.*/
            double newIndicatorLength = IndicatorStepLength * conditionsMet;
            /*TE: Set the length of the password meter*/
            StrengthIndicator.WidthRequest = newIndicatorLength;

            /*TE: Based on how many conditions that are met, we change the colour of the strength meter and provide feedback about the passwords strength.*/
            switch (conditionsMet)
            {
                case 0:
                    StrengthIndicator.BackgroundColor = IndicatorColours[0];
                    ConditionsAdvice.Text = "";
                    break;
                case 1:
                    StrengthIndicator.BackgroundColor = IndicatorColours[0];
                    ConditionsAdvice.Text = "Very Weak";
                    break;
                case 2:
                    StrengthIndicator.BackgroundColor = IndicatorColours[1];
                    ConditionsAdvice.Text = "Weak";
                    break;
                case 3:
                    StrengthIndicator.BackgroundColor = IndicatorColours[2];
                    ConditionsAdvice.Text = "Good";
                    break;
                case 4:
                    StrengthIndicator.BackgroundColor = IndicatorColours[3];
                    ConditionsAdvice.Text = "Strong";
                    break;
                case 5:
                    StrengthIndicator.BackgroundColor = IndicatorColours[4];
                    ConditionsAdvice.Text = "Very Strong";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Will add the user to the database, log them in and
        /// send them to the MapPage if they have correctly supplied all the required information
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        public void ClickedSignup(object sender, EventArgs e)
        {
            Console.WriteLine("Signup button was pressed!");

            // JH: inputs from the signup page.
            var emailInput = email.Text;
            var passwordInput = password.Text;
            var passwordConfirmInput = passwordConfirm.Text;
            var campusPrimaryInput = primaryCampus.SelectedIndex;
            bool eulaSwitchInput = switchEULAcheck.IsToggled;

            /* JH: emailPattern to allow for the max. & min. amount of characters for a user with a UTas email.
             * passwordPattern handles for upper & lowercase letters and also number, at a minimum of 8 length */
            var emailPattern = "^[0-9a-zA-Z._-]{1,52}@utas.edu.au";

            /*TE: Indiviudal tests for the password strength*/
            var passwordUperCaseRegex = "[A-Z]";
            var passwordLowerCaseRegex = "[a-z]";
            var passwordNumberRegex = "[0-9]";

            /*TE: This boolean is set to true if an error is found on the form. Makes sure that the user can never sign up if an error is found.*/
            Boolean signUpFormError = false;

            /*TE: This will hold all the error messages that will be presented to the user if they made a mistake on the signup form.*/
            string errorMessage = "";

            // JH: Checks that the email and passwords have at least any inputs.
            /*TE: Check the email is not null(empty) first or it can cause a crash otherwise.*/
            if (String.IsNullOrWhiteSpace(emailInput))
            {
                errorMessage += "Please enter an email address.\n";
                signUpFormError = true;
            }
            else {

                // JH: Checks that the email is consistent with utas email patterns
                /*TE: Check the password matches the pattern here. Need to check if null first or the application will crash.*/
                if (!(Regex.IsMatch(emailInput, emailPattern)))
                {
                    errorMessage += "Please enter a valid Utas email.\n";
                    signUpFormError = true;
                }
            }

            /*TE: We must check to make sure the password field has a value. Any operations performed on it will crash the app if we do not check it and it is null.*/
            if (String.IsNullOrWhiteSpace(passwordInput))
            {
                errorMessage += "Please enter a password.\n";
                signUpFormError = true;
            }
            else {

                /*TE: UPPERCASE CHECK - The user must include at least a single upper case character in their password.*/
                if (!Regex.IsMatch(passwordInput, passwordUperCaseRegex))
                {
                    errorMessage += "Password needs at least 1 uppercase character (A-Z).\n";
                    signUpFormError = true;
                }

                /*TE: LOWERCASE CHECK - The user must include at least a single lower case character in their password.*/
                if (!Regex.IsMatch(passwordInput, passwordLowerCaseRegex))
                {
                    errorMessage += "Password needs at least 1 lowercase character (a-z).\n";
                    signUpFormError = true;
                }

                /*TE: NUMBER CHECK - The user must include at least a single number in their password.*/
                if (!Regex.IsMatch(passwordInput, passwordNumberRegex))
                {
                    errorMessage += "Password needs at least 1 number (0-9).\n";
                    signUpFormError = true;
                }

                /*TE: PASSWORD LENGTH CHECK - The user must enter a password that is longer than 8 characters.*/
                if (!(passwordInput.Trim().Length > 7))
                {
                    errorMessage += "Password must be at least 8 characters long.\n";
                    signUpFormError = true;
                }

                /* TE: No point checking this unless the password has been entered first.
                 * Need to check it is not null or comparing null to the entered password will crash the app.
                 */
                if (String.IsNullOrWhiteSpace(passwordConfirmInput))
                {
                    errorMessage += "Please confirm your password.\n";
                    signUpFormError = true;
                }
                else
                {
                    // JH: Checks the user hasn't typed 2 different passwords
                    if (!(String.Compare(passwordInput, passwordConfirmInput, false) == 0))
                    {
                        errorMessage += "The entered passwords do not match.\n";
                        signUpFormError = true;
                    }
                }
            }

            // JH: Checks the user has checked the EULA
            if (!(eulaSwitchInput))
            {
                errorMessage += "Please read and accept the EULA.\n";
                signUpFormError = true;
            }

            /*TE: The user must select a primary campus*/
            if (primaryCampus.SelectedIndex == -1)
            {
                errorMessage += "Please Enter primary campus.\n";
                signUpFormError = true;
            }

            /*TE Do not allow the user to sign up if there is an error.*/
            if (!signUpFormError) {
                // TE: We convert the campus to a string here since we already checked if it was empty earlier.
                string primaryCampusText = primaryCampus.SelectedItem.ToString();

                SignupNewUser(emailInput, passwordInput, primaryCampusText);
            }
            else
            {
                /*TE: Display all the errors to the user in a little list.*/
                DisplayAlert("Alert", errorMessage, "OK");
            }
        }

        /// <summary>
        /// Checks the provided user information againt the database for existing disabled users.
        /// If a user already exists then reenables that account, else a new user account is created.
        /// </summary>
        /// <param name="email">Users Email</param>
        /// <param name="password">Users Password</param>
        /// <param name="campus">Users Primary Campus</param>
        async void SignupNewUser(string email, string password, string campus)
        {
            //JW: enables the loading spinner
            loadingSpinner.IsRunning = true;

            buttonSignup.IsEnabled = false;

            string response = await App.RestManager.Signup(email, password, campus);

            if (response == "Signup Successful")
            {
                //JW: disables the loading spinner
                loadingSpinner.IsRunning = false;
                buttonSignup.IsEnabled = true;

                //Kim: after sing up, display verification popup
                signUpLayout.IsVisible = false;
                verificationCode.IsVisible = true;
            }
            else
            {
                //JW: disables the loading spinner
                loadingSpinner.IsRunning = false;
                buttonSignup.IsEnabled = true;

                await DisplayAlert("Alert", response, "OK");
            }
        }


        /// <summary>
        /// Displays the EULA popup
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void OnEULASelect(object sender, EventArgs e)
        {
            EULAView.IsVisible = true;
            signUpLayout.IsVisible = false;
        }

        /// <summary>
        /// Takes the user to the UTAS privacy website
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void EULALink(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://www.utas.edu.au/privacy/"));
        }

        /// <summary>
        /// Closes the EULA popup
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void CloseEULA(object sender, EventArgs e)
        {
            EULAView.IsVisible = false;
            signUpLayout.IsVisible = true;
        }

        /// <summary>
        /// Agrees to the EULA and enables the slider and sets it to true and closes the popup
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void AgreeToEULA(object sender, EventArgs e)
        {
            switchEULAcheck.IsEnabled = true;
            switchEULAcheck.IsToggled = true;
            EULAView.IsVisible = false;
            signUpLayout.IsVisible = true;
        }

        /// <summary>
        /// Takes User back to LoginPage
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        /// <summary>
        /// async function called as soon as the page loads, populating the picker campuses
        /// </summary>
        async void RetrieveCampusList()
        {
            primaryCampus.ItemsSource = await App.RestManager.RetrieveCampusList();
        }

        /// <summary>
        /// Shows/hides the password button control
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        private void ShowHidePassword(object sender, EventArgs e)
        {
            passwordHide = !passwordHide;
            password.IsPassword = passwordHide;
            if (passwordHide == true)
            {
                ShowHidePasswordLabel.Source = "hide.png";
            }
            else
            {
                ShowHidePasswordLabel.Source = "show.png";
            }
        }

        /// <summary>
        /// Shows/hides the tooltips control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHideTips(object sender, EventArgs e)
        {
            showTips = !showTips;

            emailToolTip.IsVisible = showTips;
            primaryCampusToolTip.IsVisible = showTips;
            passwordToolTip.IsVisible = showTips;
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

                    // TE: We convert the campus to a string here since we already checked if it was empty earlier.
                    string primaryCampusText = primaryCampus.SelectedItem.ToString();

                    //JW: remove any whitespace in campus names
                    var primaryCampusTextNoSpace = primaryCampusText.Replace(" ", String.Empty);
                    
                    //JW: Subscribe to primary campus
                    CrossFirebasePushNotification.Current.Subscribe(primaryCampusTextNoSpace);
                    Console.WriteLine("Subscribed to '" + primaryCampusText + "' campus");

                    //Kim: login the user
                    await App.RestManager.Login(emailInput, passwordInput);

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
        async void PopupReturn()
        {
            await Navigation.PopAsync();
        }
    }
}
