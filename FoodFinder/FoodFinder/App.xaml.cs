using Plugin.FirebasePushNotification;
﻿using Xamarin.Forms;
using FoodFinder.Views;
using FoodFinder.Data;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace FoodFinder
{
    /// <summary>
    /// App.xaml.cs is used to init the application and the opening pages.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
	{
        
        /// <summary>
        /// Rest manager is started so it can be accessed anywhere for data calls.
        /// </summary>
        public static RestManager RestManager { get; private set; }

        
        /// <summary>
        /// Event manager singleton. Allows access to and the refreshing of events from anywhere.
        /// </summary>
        public static EventManager EventManager { get; private set; }

        /// <summary>
        /// Default Init function, also initialises all components within the application. 
        /// </summary>
        public App ()
		{

			InitializeComponent();
            RestManager = new RestManager(new RestService());
            EventManager = new EventManager();
            
            
            //JW: Provides feedback when token is updated.
            
            CrossFirebasePushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine($"TOKEN : {p.Token}");
				System.Console.WriteLine($"TOKEN : {p.Token}");
            };

            //JW: Provides feedback when notification is received.
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {

                System.Diagnostics.Debug.WriteLine("Received");

            };

            //JW: Provides feedback when notification is opened.
            CrossFirebasePushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Opened");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    System.Diagnostics.Debug.WriteLine($"ActionId: {p.Identifier}");
                }

            };

            //JW: Provides feedback when notification is deleted (android only).
            CrossFirebasePushNotification.Current.OnNotificationDeleted += (s, p) =>
            {

                System.Diagnostics.Debug.WriteLine("Deleted");

            };

            //JW: Check to see if there is a session
            if (Preferences.Get("UserID", "no session") != "no session")
            {
                // JA: Navigate to the main page of the application
                MainPage = new MainPage();
            }
            else
            {
                
                // JA: Navigate to the login page                
                MainPage = new NavigationPage(new LoginPage()) ;

                
            }
        }

        /// <summary>
        /// Anything that needs handling on App Start
        /// </summary>
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        /// <summary>
        /// Anything that needs handling on app sleep
        /// </summary>
        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        /// <summary>
        /// Anything that needs handling on App Resume
        /// </summary>
		protected override void OnResume ()
		{
			// Handle when your app resumes
		}

	}
}