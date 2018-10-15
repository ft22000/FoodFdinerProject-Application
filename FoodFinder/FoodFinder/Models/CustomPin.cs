using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace FoodFinder.Models
{
    /// <summary>
    /// Extends the Xamarin.Forms.Maps Pin objects so we caan show extra details and attach an event. 
    /// </summary>
    public class CustomPin : Pin
    {
        /// <summary>
        /// Not used currently.
        /// TODO: Check to see if this can be removed. 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The name of the building the event is being held in. Displayed in the pop up.
        /// </summary>
        public string BuildingName { get; set; }

        /// <summary>
        /// The start time of the event. Displayed in the pop up.
        /// </summary>
        public string EventTime { get; set; }

        /// <summary>
        /// The event object containing all the details. 
        /// Could change in the future honestly, currently is sent to the Custom map when a pin is clicked so the event can be loaded.
        /// </summary>
        public Event foodFinderEvent { get; set; }
    }
}
