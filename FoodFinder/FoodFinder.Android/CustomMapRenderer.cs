using System;
using System.Collections.Generic;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using FoodFinder.Models;
using FoodFinder.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace FoodFinder.Droid
{    

    /// <summary>
    /// Creator: Timothy Anderson
    /// Created: 30/07/2018
    /// Last updated by Tom Enniss 22/08/2018.
    /// This class extends the custom renderer in Xamarin.Forms.Maps allowing us to customise the appearance and behaviour of the map.
    /// Our primary reason for doing this was to customise the look of the pins on the map by adding more information to the call outs (pin pop-ups),
    /// and adding event handlers to those callouts allowing us to open the event details for the event the pin represents.
    /// </summary>
    public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        /// <summary>
        /// List of custom pins retrieved from the custom map. Used to create pins on the map.
        /// </summary>
        List<CustomPin> customPins;

        /// <summary>
        /// When the pins are being created, Google markers are created but we cannot add all the details we want to that marker.
        /// This variable holds the current custom pin we are making a marker for so we can access the extra properties when creating the callouts.
        /// </summary>
        CustomPin tempPin;

        /// <summary>
        /// Base Constructor
        /// </summary>
        /// <param name="context"></param>
        public CustomMapRenderer(Context context) : base(context)
        {

        }

        /// <summary>
        /// Removes some event handlers from the Regular map class and applies them to the custom map.
        /// Also retreives the custom pins list so we can display the pins.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if(e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }
            
            if(e.NewElement != null)
            {
                Console.WriteLine("Android Renderer Has been loaded");
                var formsMaps = (CustomMap)e.NewElement;                
                customPins = formsMaps.CustomPins;
                Control.GetMapAsync(this);
            }
        }

        /// <summary>
        /// When the map screen is loaded, we set up some event handlers for tapping on the pins.
        /// </summary>
        /// <param name="map"></param>
        protected override void OnMapReady(GoogleMap map)
        {            
            base.OnMapReady(map);
            NativeMap.InfoWindowClick += OnInfoWindowClick;
            NativeMap.MapClick += GoogleMap_MapClick;
            NativeMap.MarkerClick += OnPinCliked;
            NativeMap.SetInfoWindowAdapter(this);

            /*TE: Call this when the map loads. It wwill not create a pin if we are not selecting a location for a new event.*/
            SetUsersStartingLocation();
        }
        /// <summary>
        /// This  function is used to create a pin at the users current location. Only used for event creation location selection.
        /// </summary>
        async void SetUsersStartingLocation() {

            /*TE: Check if we are allowed access to the users location and toggle the IsShowingUser field to true if we do or the user approves the location. 
             *If not the app will not crash since we do not use the location.*/
            try
            {
                /*TE: I did remove extra code that requested the users location. This method still requests the location. 
                 * We should leave it as it is since it works???. (EDIT: This is becuase the API Target of 27 automaticaally requests permissions)
                 * If a user somehow bypasses the maps screen, we still want to request access. If it works and ain't broke DO NOT TOUCH IT!!! :P*/
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);

                if (status == PermissionStatus.Granted)
                {

                    /*TE: VERY IMPORTANT. We use the Plugin.Geolocator plugin for this. Gets the users location for the default pin to be placed on the map.*/
                    var locator = CrossGeolocator.Current;
                    var position = await locator.GetPositionAsync(TimeSpan.FromTicks(10000));

                    var formsMap = Element as CustomMap;

                    /*TE: We do not want the user creating pins when they are just viewing events on the map*/
                    if (formsMap.SelectingLocation)
                    {
                        CreateLocationPin(position.Latitude, position.Longitude);
                    }
                    else
                    {
                        Console.WriteLine("You are not selecting a location!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// This attaches an event handler to the google map allowing the user to place a map on the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoogleMap_MapClick(object sender, GoogleMap.MapClickEventArgs e) {

            var formsMap = Element as CustomMap;

            /*TE: We do not want the user creating pins when they are just viewing events on the map*/
            if (formsMap.SelectingLocation) {
                
                CreateLocationPin(e.Point.Latitude,e.Point.Longitude);
            }
            else
            {
                Console.WriteLine("You are not selecting a location!");                
            }            
        }

        /// <summary>
        /// This function creates 1 pin at a location on the map.
        /// </summary>
        /// <param name="latitude">The laatitude we will place it at</param>
        /// <param name="longitude">The longitude we will place it at</param>
        void CreateLocationPin(double latitude, double longitude) {

            var formsMap = Element as CustomMap;

            /*TE: Clear all existing pins. We do not want event pins to be shown on the map and we do not want two location selection pins either. Its a hack but it works.*/
            customPins.Clear();
            Map.Pins.Clear();
            formsMap.selectedLatitude = latitude;
            formsMap.selectedLongitude = longitude;
            /*TE: Show button to use this location*/
            formsMap.CreatePinButton();

            CustomPin newEventPin = new CustomPin
            {
                Id = "Xamarin",
                Type = PinType.Place,
                Position = new Xamarin.Forms.Maps.Position(latitude, longitude),
                Label = "Location Pin",
                Address = "Use This Location for Event",
                Url = "EventDetailsPage",
                BuildingName = "",
                EventTime = "",
                foodFinderEvent = null
            };

            /*TE: Need to add to both pin lists or they will not show up. Map.Pins is where there are taken from to place on the map, custom pins is used for the extra info in the info popups.*/
            customPins.Add(newEventPin);
            Map.Pins.Add(newEventPin);

            Console.WriteLine("Location Pin Added!");
        }

        /// <summary>
        /// This function creates a Google Maps marker for the Xamarin Map.
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var marker = new MarkerOptions();
            
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);
            
            /*Since we need extra details when creating the callout, we store the current pin temporarily for those details.*/
            tempPin = (CustomPin)pin;
            
            marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.MapPin));
            return marker;
        }

        /// <summary>
        /// This is the event that is triggered when an event is clicked. Instructs the custom map class to load the event details for the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var formsMap = Element as CustomMap;
            var customPin = GetCustomPin(e.Marker);
            if (customPin == null)
            {
                throw new Exception("Custom Pin Not Found");
            }
            /*TE: We check if the user is creating an event.
             * Then we either tell the custom map to go to the event details screen or back to the event creation.*/
            if (!formsMap.SelectingLocation)
            {
                /*TE: Tell the custom map to go to the event details screen and load the event.*/
                Event selectedEvent = customPin.foodFinderEvent;                    
                formsMap.PinsEvent(selectedEvent);
            }           
        }

        /// <summary>
        /// This creates the customised callouts for the custom pin markers on the map.
        /// </summary>
        /// <param name="marker"></param>
        /// <returns></returns>
        public Android.Views.View GetInfoContents(Marker marker)
        {
            var formsMap = Element as CustomMap;
            /*TE: I do not want the info window popping up when they are using the pin to select a location. We want the button to show up instead.
             This actually used the smaller pin infor window which is great!*/
            if (!formsMap.SelectingLocation)
            {
                var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
                if (inflater != null)
                {
                    Android.Views.View view;
                    var customPin = GetCustomPin(marker);
                    if (customPin == null)
                    {
                        throw new Exception("Custom Pin Not Found");
                    }
                    if (customPin.Id.ToString() == "Xamarin")
                    {
                        view = inflater.Inflate(Resource.Layout.XamarinMapInfoWindow, null);
                    }
                    else
                    {
                        view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
                    }
                    //fix variable names --TODO-- TIM
                    /*This is why we have the temp custom pin.*/
                    var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                    var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);
                    var infoSubtitleTwo = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitleTwo);

                    if (infoTitle != null)
                    {
                        infoTitle.Text = marker.Title;
                    }
                    if (infoSubtitle != null)
                    {
                        infoSubtitle.Text = marker.Snippet;
                    }
                	if (infoSubtitleTwo != null)
                    {
                    
                        infoSubtitleTwo.Text = customPin.EventTime;
                    }
                    return view;
                }
            }
            return null;
        }

        /// <summary>
        /// Unsure of this function.
        /// </summary>
        /// <param name="marker"></param>
        /// <returns></returns>
        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }

        /// <summary>
        /// This stops the info window from popping up from the pin that is being used to select a location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPinCliked(object sender, GoogleMap.MarkerClickEventArgs e) {
            var formsMap = Element as CustomMap;

            /*TE: We do not want the infow window to display if the pin is the location selection pin*/
            if (formsMap.SelectingLocation)
            {
                /*TE: Info window will not be displayed since the pin is the new event location selection pin*/
                e.Handled = true;
            }
            else {
                /*TE: Info window will be displayed since the pin is an event pin*/
                e.Handled = false;
            }           
        }

        /// <summary>
        /// Gets and returns the custom pin that is linked to the Google Map marker that was tapped on.
        /// TE: We have to get custom pins by their Ids, since sometimes events with have exactly the same coordinates espcially if the campuses coordinates are used.
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns></returns>
        CustomPin GetCustomPin(Marker annotation)
        {   

            foreach (CustomPin pin in customPins)
            {            
                /*TE: Since I added an offset function in the Event manager, you can use the position once again.*/
                if (pin.Position.Latitude == annotation.Position.Latitude && pin.Position.Longitude == annotation.Position.Longitude)
                {
                    return pin;
                }
            }
            return null;
        }
    }
}