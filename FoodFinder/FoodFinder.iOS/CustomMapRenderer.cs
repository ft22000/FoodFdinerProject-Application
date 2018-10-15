using System;
using System.Collections.Generic;
using CoreGraphics;
using FoodFinder;
using FoodFinder.Data;
using FoodFinder.Models;
using FoodFinder.iOS;
using MapKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace FoodFinder.iOS
{
    /// <summary>
    /// Creator: Thomas Enniss
    /// Created: 30/07/2018
    ///  NOTE: We ran out of time to test and fully get the IOS version of the maps working. Android is working fine.
    ///  Most of this code has been source from an online example at the Microsoft Xamarin Samples
    ///  https://developer.xamarin.com/samples/xamarin-forms/CustomRenderers/Map/Pin/
    ///  Leaving comments blank for now until we work on this.
    /// </summary>
    class CustomMapRenderer : MapRenderer
    {
        UIView customPinView;
        List<CustomPin> customPins;
        CustomMap formsMap;

        /// <summary>
        /// When the map page is loaded, this custom renderer overrides elements from the base class so we can customize things the way we want.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                var nativeMap = Control as MKMapView;
                nativeMap.GetViewForAnnotation = null;
                nativeMap.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;
            }

            if (e.NewElement != null)
            {
                Console.WriteLine("Map Page Appeared");
                formsMap = (CustomMap)e.NewElement;
                var nativeMap = Control as MKMapView;
                nativeMap.ZoomEnabled = true;
                nativeMap.ScrollEnabled = true;
                nativeMap.AddGestureRecognizer(new UITapGestureRecognizer(MapTap));
                customPins = formsMap.CustomPins;

                nativeMap.GetViewForAnnotation = GetViewForAnnotation;
                nativeMap.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;

            }
        }

        /// <summary>
        /// This  function is used to create a pin at the users current location. Only used for event creation location selection.
        /// </summary>
        async void SetUsersStartingLocation()
        {

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
        /// Creates a pin on the map when the user taps on it so they can choose a location on the map for the event.
        /// </summary>
        /// <param name="recognizer">The object holding the argeuments for the gesture.</param>
        private void MapTap(UITapGestureRecognizer recognizer) {

            Console.WriteLine("It's been tapped again.");

            var map = Control as MKMapView;
            if (map == null) return;
            var pixelLocation = recognizer.LocationInView(map);
            var geoCoordinate = map.ConvertPoint(pixelLocation, map);

            Console.WriteLine("Lat:{0} - Long:{1}",geoCoordinate.Latitude, geoCoordinate.Longitude);

            /*TE: We do not want the user creating pins when they are just viewing events on the map*/
            if (formsMap.SelectingLocation)
            {
                CreateLocationPin(geoCoordinate.Latitude, geoCoordinate.Longitude);
            }
            else
            {
                Console.WriteLine("You are not selectiing a location!");
            }
        }

        /*TE: Created a separate function to to create a pin on the map.*/
        private void CreateLocationPin(double Latitude, double Longitude) {

            Console.WriteLine("Creating pin for location selection");

            /*TE: Clear both list otherwise multiple pins will be created.*/
            formsMap.CustomPins.Clear();
            formsMap.Pins.Clear();
            formsMap.selectedLatitude = Latitude;
            formsMap.selectedLongitude = Longitude;            

            var nativeMap = Control as MKMapView;

            CustomPin newEventPin = new CustomPin
            {
                Id = "Xamarin",
                Type = PinType.Place,
                Position = new Xamarin.Forms.Maps.Position(Latitude, Longitude),
                Label = "Location Pin",
                Address = "1<$EVENT_ID$Use This Location for Event",
                Url = "EventDetailsPage",
                BuildingName = "",
                EventTime = "",
                foodFinderEvent = null
            };
            /*TE: Need to add custom pin to both of these*/
            formsMap.CustomPins.Add(newEventPin);
            formsMap.Pins.Add(newEventPin);
            /*TE: Shows button on map to use this location*/
            formsMap.CreatePinButton();
        }

        /// <summary>
        /// Uses values from the custom pin to set up the values of the custom annotation (IOS Pin marker).
        /// </summary>
        /// <param name="mapView"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            Console.WriteLine("Number of Custom Pins: "+customPins.Count);


            MKAnnotationView annotationView = null;
            
            Console.WriteLine("Number of Actual Map Pins: " + formsMap.Pins.Count);

            if (annotation is MKUserLocation)
                return null;

            /*TE: The application can crashe becuase of this*/
            if (!(annotation is MKPointAnnotation)) {
                Console.WriteLine("Type wasn't a MKPointAnnotation, skipped");
                return null;
            }
            
            var customPin = GetCustomPin(annotation as MKPointAnnotation);
            
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }          
            
            annotationView = mapView.DequeueReusableAnnotation(customPin.Id.ToString());
            if (annotationView == null)
            {
                annotationView = new CustomMKAnnotationView(annotation, customPin.Id.ToString());                
                annotationView.Image = UIImage.FromFile("MapPin.png");
                annotationView.CalloutOffset = new CGPoint(0, 0);

                annotationView.RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure);
                

                ((CustomMKAnnotationView)annotationView).Id = customPin.Id.ToString();
                ((CustomMKAnnotationView)annotationView).Url = customPin.Url;
                ((CustomMKAnnotationView)annotationView).BuildingName = customPin.BuildingName;
                ((CustomMKAnnotationView)annotationView).EventTime = customPin.EventTime;
                ((CustomMKAnnotationView)annotationView).foodFinderEvent = customPin.foodFinderEvent;

                annotationView.RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure);
                ((CustomMKAnnotationView)annotationView).Id = customPin.Id.ToString();
                ((CustomMKAnnotationView)annotationView).Url = customPin.Url;
                ((CustomMKAnnotationView)annotationView).BuildingName = customPin.BuildingName;
            }

            /*TE: Disable callout if we adding an event. This code should not stop the IOS stuff from working.*/
            if (formsMap.SelectingLocation)
            {
                annotationView.CanShowCallout = false;
            }
            else {
                annotationView.CanShowCallout = true;
            }

            return annotationView;
        }

        /// <summary>
        /// This handles when a pin's callout or popup is clicked. Used to load that pin's event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e)
        {
            var customView = e.View as CustomMKAnnotationView;

            if (!string.IsNullOrWhiteSpace(customView.Url))
            {
                Console.WriteLine("Callout tapped");

                if (!formsMap.SelectingLocation) {

                    formsMap.PinsEvent(customView.foodFinderEvent);
                }
            }
        }

        /// <summary>
        /// Helps setup the annotation callout view. If deleted the app will crash when you click outside a pin.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            var customView = e.View as CustomMKAnnotationView;
            customPinView = new UIView();

            if (customView.Id == "Xamarin")
            {
                customPinView.Frame = new CGRect(0, 0, 200, 84);
                var image = new UIImageView(new CGRect(0, 0, 200, 84));
                customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));
                e.View.AddSubview(customPinView);
                UIView building = new UIView();

                customPinView.AddSubview(building);
            }
        }

        /// <summary>
        /// Removes the popup or callout when the user taps elsewhere on the map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            if (!e.View.Selected)
            {
                customPinView.RemoveFromSuperview();
                customPinView.Dispose();
                customPinView = null;
            }
        }

        /// <summary>
        /// Gets a custom pin from the maps custom pin list so the renderer can use it's custom properties.
        /// </summary>
        /// <param name="annotation"></param>
        /// <returns>Returns a custom pin which can be used to setup up the maps annotations.</returns>
        CustomPin GetCustomPin(MKPointAnnotation annotation)
        {
            if (annotation != null)
            {
                /*TE: Look through the events and return the pin whose coordinates match the coordinates of the pin clicked on.*/
                foreach (CustomPin pin in customPins)
                {
                    /*TE: If positions match we return the pin.*/
                    if (pin.Position.Latitude == annotation.Coordinate.Latitude && pin.Position.Longitude == annotation.Coordinate.Longitude)
                    {
                        return pin;
                    }
                }
                return null;
            }
            else {
                return null;
            }
        }
    }
}