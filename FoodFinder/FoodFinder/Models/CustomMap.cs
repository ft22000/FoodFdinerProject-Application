using System.Collections.Generic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using FoodFinder.Views;

namespace FoodFinder.Models
{
    /// <summary>
    /// Custom Map Class for custom renderer.
    /// </summary>
    public class CustomMap : Map
    {
        /// <summary>
        /// List of custom pins that will be used by the custom map renderer to place them on a map.
        /// </summary>
        public List<CustomPin> CustomPins { get; set; }

        /// <summary>
        /// Delegate for PinCreated
        /// </summary>
        public delegate void PinCreated();

        /// <summary>
        /// Event for creating the Pin
        /// </summary>
        /// <returns>N/A</returns>
        public event PinCreated LocationSelected;

        /// <summary>
        /// The Selected Longtitude when the user taps on the screen
        /// </summary>
        public double selectedLongitude;

        /// <summary>
        /// The Selected Latitude when the user taps on the screen
        /// </summary>
        public double selectedLatitude;

        /// <summary>
        /// Allows the custom render to know if we are just selecting a location for an event and only want to view 1 pin.
        /// Otherwise location selection is disabled and all events are shown.
        /// </summary>
        public Boolean SelectingLocation { get; set; }

        /// <summary>
        /// Constructor for custom map. Instantiates the custom pin list. Without this adding to the list crashes the application.
        /// </summary>
        public CustomMap() {
            CustomPins = new List<CustomPin>();
        }

        /// <summary>
        /// Loads the details of an event for custom pin.
        /// Triggered in the CustomMapRendere.cs when a custom pins popup is tapped. 
        /// </summary>
        /// <param name="pinsEvent">The event we are loading the details for.</param>
        async public void PinsEvent(Event pinsEvent) {
            await Navigation.PushAsync(new EventDetailsPage(pinsEvent));
        }

        /// <summary>
        /// This function sends a message to the event creation page after the user has clicked on the info window on the map screen when selecting a custom location for an event.
        /// Pop async shifts the map page off the stack so we are back on the event creation page, but doesn't allow parameters. 
        /// I researched the issue and used this method after reading a suggestion on stackoverflow.
        /// </summary>
        /// <param name="sender">Object Data to be sent</param>
        /// <param name="e">Event Handler</param>
        async public void SetEventLocation(Object sender, EventArgs e) {
            Console.WriteLine("Pin Info clicked");
            MessagingCenter.Send<CustomMap, double[]>(this, "EventLocation",new double[]{ selectedLatitude,selectedLongitude });
            await Navigation.PopAsync();
        }

        /// <summary>
        /// Creates the Pin Button
        /// </summary>
        public void CreatePinButton() {
            LocationSelected();
        }
    }
}