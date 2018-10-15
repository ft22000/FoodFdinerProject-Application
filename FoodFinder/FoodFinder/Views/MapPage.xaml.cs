using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using FoodFinder.Data;
using FoodFinder.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Media;
using Xamarin.Essentials;

namespace FoodFinder.Views
{
    /// <summary>
    ///  Implementation Date: 25/07/2018
    ///  Handles the implementation of the map so we can display the locations of events as pins that can be taapped for more information.
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        /// <summary>
        /// Boolean value for if the user is selecting a location on the map
        /// </summary>
        Boolean userIsSelectingLocation;
        
        /// <summary>
        /// Constructor function for the map page. Creates the map for the map page. 
        /// </summary>
        public MapPage ()
		{
            /*Defaults to false. Only time we set this dyncamically is when we call it from the event creation page.*/
            userIsSelectingLocation = false;
            InitializeComponent ();

            //JW: Updates primary campus data
            RetrievePrimaryCampus(Preferences.Get("UserID", ""));

            CreateMap();
        }

        
        /// <summary>
        /// Second overload constructor. This allows us to create a map in location selection mode. 
        /// Events will not be loaded and you can tap the map and create location pin.
        /// We can have this as the only constructor, but we would need to change all the code throughout the app to call new MapPage(true or false);
        /// </summary>
        /// <param name="creatingAnEvent">Boolean value that denotes if the appliation is loaded into CreateEvent Location Selection Screen.</param>
        public MapPage(bool creatingAnEvent)
        {
            userIsSelectingLocation = creatingAnEvent;
            InitializeComponent();
            CreateMap();
        }

        /// <summary>
        /// This function creates the map, checks to see if we need to load the pins if the user is not creating an event and centres the user on their default campus location.
        /// </summary>
        async void CreateMap() {

            /*TE BUGFIX: We must instantiate the map, but disable the user location. Craating the map in the try / catch will result in a race condition.
              The main thread will not wait for the user to allow access to their location and will carry on creating the map before. Removing the await to make the methods synchronous freezes the application.
              So we create the map and toggle the IsShowingUser later on when the permissions are handled.
              This is aall still a little unkown to me. Avoid making changes.
            */

            /*TE: The map is now created in XAML. This was for IOS compatibility reasons.*/

            map.SelectingLocation = userIsSelectingLocation;
            
            /*TE: Check if we are allowed access to the users location and toggle the IsShowingUser field to true if we do or the user approves the location. 
             *If not the app will not crash since we do not use the location.*/
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                /*TE: If location is not enabled, we request it.*/
                if (status != PermissionStatus.Granted)
                {
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    //TE: Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Location)) {
                        status = results[Permission.Location];
                    }                        
                }

                /*TE: If the app is permitted access to the users location, we allow it on the map. This will happen after the user allows or disallows the location permissions.*/
                if (status == PermissionStatus.Granted)
                {
                    map.IsShowingUser = true;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

           // MapStack.Children.Add(map);
           // Content = MapStack;

            /*TE: Had this as a function when it was a switch. Kept it like this since it makes sense to be a function*/
            SetMapDefaultCampus();

            /* TE: Subscribe once here other wise the custom maps LocationSelected function event will be subscribed to the "Use Pins Location" multiple times each time a pin is added.             * 
             * When the button was pressed, multiple of the same LocationSelected events would fire repeatedly popping async the page.*/
            LocationButton.Clicked += map.SetEventLocation;

            /*TE: Need to clear pins if we are creating 1 pin for selecting a new events location.*/
            if (userIsSelectingLocation)
            {
                map.LocationSelected += ShowUseLocationButton;
                map.CustomPins.Clear();
                map.Pins.Clear();
            }
            else
            {
                LoadPins();
            }
        }

        /// <summary>
        /// Sets the default position of the map based on the users primary campus.
        /// </summary>        
        void SetMapDefaultCampus() {
            try{
                Console.WriteLine("Setting Default Map Location");
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(Double.Parse(Preferences.Get("PrimaryCampusLatitude", "")), Double.Parse(Preferences.Get("PrimaryCampusLongitude", ""))), Distance.FromKilometers(.1)));
            } catch (Exception e) {
                Console.WriteLine("Cannot set default map position becuase: {0}",e.Message);
            }            
        }

        /// <summary>
        /// Shows the button on the map when the user creates a pin on the map after tapping it. Used for creating an event and choosing the location when tapped.
        /// </summary>
        void ShowUseLocationButton() {
            LocationButton.IsVisible = true;            
        }

        /// <summary>
        /// This function refreshers the events in the event manager. After that it iterates over each event and creates a pin on the map for that event. NOT COMPLETED.2
        /// </summary>
        async void LoadPins() {

            if (!userIsSelectingLocation) {

                Console.WriteLine("Loading Pins For Map...");

                /*TE: Refresh pins and wait for them to fresh. Otherwise you might be dealing with an empty collection and get errors.*/
                await App.EventManager.LoadEvents();

                /*TE: Empty the Maps pin collections so we don't double up on pins.*/
                map.CustomPins.Clear();
                map.Pins.Clear();
               
                //TE: Loops over the list of events and creates a pin for each event.
                
                foreach (Event current in App.EventManager.Events)
                {
                    Console.WriteLine("Creating Pin at Lat: {0} - Long: {1}", current.Latitude, current.Longitude);
                    /*TE: Create a new pin for each event in the event list.*/
                    var pin = new CustomPin
                    {
                        Id = "Xamarin",
                        Type = PinType.Place,
                        Position = new Position(current.Latitude, current.Longitude),
                        Label = current.Name,
                        Address = current.Campus.ToString(),
                        Url = "EventDetailsPage",
                        BuildingName = current.Location,
                        EventTime = current.StartTime.ToString(),
                        foodFinderEvent = current
                    };
                    /*TE: Need to add the custom pin to both the maps pins and the custom maps custom pins list or they will not display.*/
                    map.CustomPins.Add(pin);
                    map.Pins.Add(pin);
                }
            }                        
        }

        /// <summary>
        /// Updates the pins on the map everytime the user navigates to it. Updates the Nav Bar Color
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPins();
        }

        /// <summary>
        /// Obtains the users primary campus and campus coordinates, and updates their session values.
        /// </summary>
        /// <param name="userId">The unique identification number of the user.</param>
        async void RetrievePrimaryCampus(string userId)
        {
            await App.RestManager.RetrievePrimaryCampus(userId);
        }
    }   
}