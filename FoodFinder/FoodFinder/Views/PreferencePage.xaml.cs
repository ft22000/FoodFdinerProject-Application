using Plugin.FirebasePushNotification;
using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using FoodFinder.Models;
using System.Collections;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace FoodFinder.Views
{
    /// <summary>
    /// Class that Defines the Preferences Page
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PreferencePage : TabbedPage
    {
        /*EDH & TE: This is the id of the user from the database. Used in requests to the database to update passwords etc on this page.*/
        string UserId = Preferences.Get("UserID", "");

        /// <summary>
        /// Sets the password to be hidden by default
        /// </summary>
        bool passwordHide = true;

        /// <summary>
        /// Sets the password to be hidden by default
        /// </summary>
        bool newpasswordHide = true;

        /// <summary>
        /// Sets the tooltips to be hidden by default
        /// </summary>
        bool showTips = false;

        //JW: used to avoid triggering OnToggledItem function when the campus list is refreshed, which would cause a null expeption crash
        bool campusListRefresh = false;

        //JW: used to set the notification switch states on page load without triggering api calls
        bool initialNotificationSet = false;

        /// <summary>
        /// Stores campuses the user has selected from the preferences screen
        /// </summary>
        ArrayList secondaryCampusArray = new ArrayList();

        /// <summary>
        /// Stores all campuses, used to set all switches to false before setting subscribed campuses to true
        /// </summary>
        ObservableCollection<Campus> campusList = new ObservableCollection<Campus>();

        /// <summary>
        /// stores the campus name and the switch state of all campuses in the preferences screen (excluding the primary campus)
        /// </summary>
        ObservableCollection<CampusState> listViewCampus;

        /// <summary>
        /// Default Constructor Function
        /// </summary>
        public PreferencePage()
        {
            InitializeComponent();

            //JW: Obtains observable collection used to populate the picker
            RetrieveCampusList();

            // JA: Checks if the current user already has event organiser permissions or higher
            if (Int32.Parse(Preferences.Get("PermissionLevel", "")) > Preferences.Get("GENERAL_USER", 1))
            {
                //JA: Hides the request button
                OrganiserRequestButton.IsVisible = false;
            }

            //JW: setting all static switches -- if statement prevents crash when preferences are empty
            if (Preferences.Get("PushNotificationSwitch", "") == "True" || Preferences.Get("PushNotificationSwitch", "") == "False")
            {
                initialNotificationSet = true;
                pushToggle.IsToggled = Boolean.Parse(Preferences.Get("PushNotificationSwitch", ""));

            }

            if (Preferences.Get("EmailNotificationSwitch", "") == "True" || Preferences.Get("EmailNotificationSwitch", "") == "False")
            {
                emailToggle.IsToggled = Boolean.Parse(Preferences.Get("EmailNotificationSwitch", ""));
                initialNotificationSet = false;
            }

            // JN: Prevent highlighting of selections in the list that are not made with the toggle.
            secondaryCampuses.ItemSelected += (object sender, SelectedItemChangedEventArgs e) =>
            {
                // Deselecting an item response
                if (e.SelectedItem == null)
                {
                    return;
                }

                // Deselect any selected event
                if (sender is ListView lv)
                {
                    lv.SelectedItem = null;
                }
                else
                {
                    return;
                }
            };

            //Kim: display Email in the account tab
            email.Text = Preferences.Get("Email", "");

            //JW: updates campus list view whenever primary campus picker changes
            primaryCampus.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                //JW: current primary campus
                var currentPrimaryCampus = Preferences.Get("PrimaryCampus", "");
                var currentPrimaryCampusNoSpace = currentPrimaryCampus.Replace(" ", String.Empty);

                //JW: currently selected primary campus from picker
                var primaryCampusSelection = primaryCampus.SelectedItem.ToString();
                var primaryCampusSelectionNoSpace = primaryCampusSelection.Replace(" ", String.Empty);

                //JW: if current and selected do not match
                if (currentPrimaryCampus != primaryCampusSelection)
                {
                    //JW: set switch state of old primary campus to false if not already
                    Preferences.Set(currentPrimaryCampus, "False");

                    Console.WriteLine("Updating primary campus subscription...");

                    //JW: update primary campus in session
                    Preferences.Set("PrimaryCampus", primaryCampusSelection);
                    Preferences.Set(primaryCampusSelection, "True");

                    //JW: Update the user's campus list in the db
                    UpdateCampuses();
                }

                //JW: update list view to remove primary campus
                UpdateCampusList();
            };
        }

        /// <summary>
        /// The function updates the password.
        /// </summary>
        /// <param name="sender">Object-Sender that passes required information to the method</param>
        /// <param name="e">Event handler</param>
        private void UpdatePasswordButton(object sender, EventArgs e)
        {
            string passwordText = password.Text;
            string newPasswordText = newPassword.Text;
            string confirmNewPasswordText = confirmNewPassword.Text;

            //TE: This returns specific errors for each field including mismatched passwords.

            /*TE: Current password not entered*/
            if (String.IsNullOrWhiteSpace(passwordText))
            {
                DisplayAlert("Alert", "Please enter your current password", "close");
            }
            else {
                /*TE: New password not entered.*/
                if (String.IsNullOrWhiteSpace(newPasswordText))
                {
                    DisplayAlert("Alert", "Please Enter New Password", "close");
                }
                else {
                    /*TE: New password not entered again to confirm.*/
                    if (String.IsNullOrWhiteSpace(confirmNewPasswordText))
                    {
                        DisplayAlert("Alert", "Please Confirm New Password", "close");
                    }
                    else
                    {
                        /*TE: New passwords do not match. Tests for case sensitive matches*/
                        if (String.Compare(newPasswordText, confirmNewPasswordText, ignoreCase: false) == 0)
                        {
                            bool passwordAdequate = true;
                            string errorMessage = "";

                            /*TE: Gotta work with regex.*/
                            var passwordUperCaseRegex = "[A-Z]";
                            var passwordLowerCaseRegex = "[a-z]";
                            var passwordNumberRegex = "[0-9]";

                            /*TE: Check for a capital*/
                            if (!(Regex.IsMatch(newPasswordText, passwordUperCaseRegex)))
                            {
                                passwordAdequate = false;
                                errorMessage += "Password must contain a capital\n";
                            }

                            /*TE: Check for a lowercase*/
                            if (!(Regex.IsMatch(newPasswordText, passwordLowerCaseRegex)))
                            {
                                passwordAdequate = false;
                                errorMessage += "Password must contain lowercase characters.\n";
                            }

                            /*TE: Check for a number*/
                            if (!(Regex.IsMatch(newPasswordText, passwordNumberRegex)))
                            {
                                passwordAdequate = false;
                                errorMessage += "Password must contain a number.\n";
                            }

                            /*TE: make sure password is longer than 8 characters*/
                            if (!(newPasswordText.Trim().Length > 7))
                            {
                                passwordAdequate = false;
                                errorMessage += "Password must be 8 digits or longer.\n";
                            }

                            /*TE: If there are no errors and we are happy with the password complexity, we update the password.*/
                            if (passwordAdequate) {
                                ChangePassword(passwordText.Trim(), newPasswordText.Trim(), UserId);
                            }
                            else {
                                DisplayAlert("Error", errorMessage, "OK");
                            }
                        }
                        else
                        {
                            DisplayAlert("Alert", "New passwords do not match", "close");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The function updates the users primary and secondary campuses
        /// </summary>
        public void UpdateCampuses()
        {
            //JW: current primary campus
            var primaryCampus = Preferences.Get("PrimaryCampus", "");
            var primaryCampusNoSpace = primaryCampus.Replace(" ", String.Empty);

            Console.WriteLine("Secondary campus return button was pressed!");

            //JW: unsubscribe the device from all topics
            CrossFirebasePushNotification.Current.UnsubscribeAll();
            Console.WriteLine("Unsubscribed from all topics");

            //JW: subscribe to main campus
            CrossFirebasePushNotification.Current.Subscribe(primaryCampusNoSpace);
            Console.WriteLine("Subscribed primary campus: " + primaryCampus);

            //JW: add all currently true selected campuses to secondary campus array
            foreach (var data in campusList)
            {
                if (Preferences.Get(data.ToString(), "") == "True")
                {
                    secondaryCampusArray.Add(data);
                }
            }

            Console.WriteLine(secondaryCampusArray.Count + "secondary campuses selected");

            // JW: subscribe the device to all secondary campuses stored in arraylist
            foreach (var data in secondaryCampusArray)
            {
                var secondaryCampusSelectionNoSpace = data.ToString().Replace(" ", String.Empty);

                CrossFirebasePushNotification.Current.Subscribe(secondaryCampusSelectionNoSpace);
                Console.WriteLine("Subscribed to secondary campus: " + data);
            }

            //JW: send the collected data (primary campus, all selected secondary campuses, the user's id from settings session and total count of secondary campuses selected)
            SaveCampus(Preferences.Get("PrimaryCampus", ""), secondaryCampusArray, UserId, secondaryCampusArray.Count);

            //JW: clear all data held in arraylist
            secondaryCampusArray.Clear();
        }

        /// <summary>
        /// The function navigates to Become An Event Organiser screen
        /// </summary>
        /// <param name="sender">Object-Sender that passes required information to the method</param>
        /// <param name="e">Event handler</param>
        private async void BecomeEventOrganiser(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BecomeAnEventOrganiserPage());
        }

        /// <summary>
        /// Checks if password is correct snd changes it to new password if correct
        /// </summary>
        /// <param name="oldPasswordText">Old password</param>
        /// <param name="newPasswordText">New password</param>
        /// <param name="id">User ID</param>
        async void ChangePassword(string oldPasswordText, string newPasswordText, string id)
        {
            //JW: enables the loading spinner
            loadingSpinnerAccount.IsRunning = true;

            string[] resultMessage = await App.RestManager.ChangePassword(oldPasswordText, newPasswordText, id);

            //JW: disables the loading spinner
            loadingSpinnerAccount.IsRunning = false;

            await DisplayAlert("Alert", resultMessage[1], "OK");

            /*TE: We use the boolean to determine if we should clear the fields. Only do this when the password is update.*/
            if (Boolean.Parse(resultMessage[0]))
            {
                password.Text = "";
                newPassword.Text = "";
                confirmNewPassword.Text = "";
            }
        }

        /// <summary>
        /// Sends campus subscriptions and user id through to the rest manager, which will then pass it on to the web api
        /// </summary>
        /// <param name="primaryCampus">Primary Campus that the user selected</param>
        /// <param name="secondaryCampuses">Subscribed campues that the user selected</param>
        /// <param name="id">User ID</param>
        /// <param name="numOfSecondaryCampuses">How many subscribed campuses selected</param>
        ///
        async void SaveCampus(string primaryCampus, ArrayList secondaryCampuses, string id, int numOfSecondaryCampuses)
        {
            //JW: enables the loading spinner
            loadingSpinnerLocation.IsRunning = true;

            await App.RestManager.SaveCampus(primaryCampus, secondaryCampuses, id, numOfSecondaryCampuses);

            //JW: disables the loading spinner
            loadingSpinnerLocation.IsRunning = false;
        }

        /// <summary>
        /// Async function called as soon as the page loads, populating the picker campuses and setting primary campus data
        /// </summary>
        async void RetrieveCampusList()
        {
            int campusListCounter = 0;

            //JW: Updates primary campus data
            await App.RestManager.RetrievePrimaryCampus(UserId);

            campusList = await App.RestManager.RetrieveCampusList();
            primaryCampus.ItemsSource = campusList;

            //JW: sets the default picker campus and adds the current campus (minus the primary campus) to the list view
            foreach (var campus in campusList)
            {
                if (Preferences.Get("PrimaryCampus", "") == campus.ToString())
                {
                    primaryCampus.SelectedIndex = campusListCounter;
                }

                campusListCounter++;
            }
        }

        /// <summary>
        /// Updates campus list to remove the primary campus whenever the primary campus is updated in the preferences
        /// </summary>
        void UpdateCampusList()
        {
            campusListRefresh = true;

            listViewCampus = new ObservableCollection<CampusState>();

            //JW: sets the default picker campus and adds the current campus (minus the primary campus) to the list view
            foreach (var campus in campusList)
            {
                if (Preferences.Get("PrimaryCampus", "") != campus.ToString())
                {
                    listViewCampus.Add(new CampusState { ListCampusName = campus.ToString(), ListSwitchState = Preferences.Get(campus.ToString(), "") });
                }
            }

            secondaryCampuses.ItemsSource = listViewCampus;

            campusListRefresh = false;
        }

        /// <summary>
        /// Obtains the name of campus from whichever switch the user selects via the listview
        /// and sets it to true/false in preferences
        /// </summary>
        /// <param name="sender">Object-Sender that passes required information to the method</param>
        /// <param name="args">Toggle handler</param>
        void OnToggledCampus(object sender, ToggledEventArgs args)
        {
            var switchItem = (Switch)sender;
            var SelectedParent = switchItem.BindingContext;

            //JW: Ignores api call after updating the primary campus to prevent the application from crashing now that the primary campus no longer exists in the campus list
            if (campusListRefresh == false)
            {

                //JW: Set campus state to true/enabled in the session data
                if (switchItem.IsToggled)
                {
                    Console.WriteLine("Selected item: " + SelectedParent.ToString());
                    Preferences.Set(SelectedParent.ToString(), "True");
                    Console.WriteLine(Preferences.Get(SelectedParent.ToString(), ""));
                }
                else
                {
                    Preferences.Set(SelectedParent.ToString(), "False");
                    Console.WriteLine(Preferences.Get(SelectedParent.ToString(), ""));
                }
            }
        }

        /// <summary>
        /// Updates the notification preferences in the database as soon as the toggles are toggled
        /// </summary>
        /// <param name="sender">Object-Sender that passes required information to the method</param>
        /// <param name="args">toggle handler</param>
        void OnToggledNotification(object sender, ToggledEventArgs args)
        {
            var switchItem = (Switch)sender;

            //EDH: checks toggle values and sends them to the database
            bool pushNotificationSelection = pushToggle.IsToggled;
            bool emailNotificationSelection = emailToggle.IsToggled;

            if (initialNotificationSet == false)
            {
                //JW: if the push notification toggle is enabled, obtain campuses from db and subscribe the device to their GCM channel. if disabled, unsub from all
                if (pushNotificationSelection)
                {
                    Console.WriteLine("Push notifications enabled!");
                    RetrieveSubscribedCampuses(Preferences.Get("UserID", ""));
                    Preferences.Set("PushNotificationSwitch", pushNotificationSelection.ToString());
                }
                else
                {
                    //Kim: When push notification toggle is off, unsubscribe all campus on firebase.
                    CrossFirebasePushNotification.Current.UnsubscribeAll();
                    Console.WriteLine("Disabled push notifications -- unsubscribing from all firebase topics");
                    Preferences.Set("PushNotificationSwitch", pushNotificationSelection.ToString());
                }

                if (emailNotificationSelection)
                {
                    Console.WriteLine("Email notifications enabled!");
                    Preferences.Set("EmailNotificationSwitch", emailNotificationSelection.ToString());
                }
                else
                {
                    Console.WriteLine("Disabled email notifications");
                    Preferences.Set("EmailNotificationSwitch", emailNotificationSelection.ToString());
                }

                NotificationToggle(Preferences.Get("EmailNotificationSwitch", "").ToString(), Preferences.Get("PushNotificationSwitch", "").ToString());
            }
        }

        /// <summary>
        /// Opens the campus selection custom popup
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void CampusSelection(object sender, EventArgs e)
        {
            InitialView.IsVisible = false;
            SecondaryCampusPicker.IsVisible = true;
        }

        /// <summary>
        /// Closes the campus selection custom popup
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void CloseCampusSelection(object sender, EventArgs e)
        {
            InitialView.IsVisible = true;
            SecondaryCampusPicker.IsVisible = false;

            //JW: update secondary campuses in db on save
            UpdateCampuses();
        }

        /// <summary>
        /// Obtains the campuses from the db that the user has subscribed to.
        /// </summary>
        /// <param name="userId">The user's unique id</param>
        async void RetrieveSubscribedCampuses(string userId)
        {
            ObservableCollection<Campus> subscribedCampuses;

            subscribedCampuses = await App.RestManager.RetrieveSubscribedCampuses(userId);

            //JW: loop through observable collection, subscribing device to each of the campuses they have selected
            foreach (var campus in subscribedCampuses)
            {
                var subCampusNoSpace = campus.Campus_Name.Replace(" ", String.Empty);

                Console.WriteLine("Subscribing device to campus: " + campus.Campus_Name);

                CrossFirebasePushNotification.Current.Subscribe(subCampusNoSpace);
            }
        }

        /// <summary>
        /// Send state of email notification and push notification toggle to database
        /// </summary>
        /// <param name="email">State of email notification</param>
        /// <param name="push">State of push notification</param>
        async void NotificationToggle(string email, string push)
        {
            await App.RestManager.NotificationToggle(email, push);
        }

        /// <summary>
        /// Shows/hides the password button control
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler.</param>
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
        /// Shows/hides the new password button control
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler.</param>
        private void ShowHideNewPassword(object sender, EventArgs e)
        {
            newpasswordHide = !newpasswordHide;
            newPassword.IsPassword = newpasswordHide;
            if (newpasswordHide == true)
            {
                ShowHideNewPasswordLabel.Source = "hide.png";
            }
            else
            {
                ShowHideNewPasswordLabel.Source = "show.png";
            }
        }

        /// <summary>
        /// Shows/hides the tooltips control for location tab
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler.</param>
        private void ShowHideTips(object sender, EventArgs e)
        {
            showTips = !showTips;

            subscribedCampusToolTip.IsVisible = showTips;
            primaryCampusToolTip.IsVisible = showTips;
            passwordToolTip.IsVisible = showTips;
        }
    }
}