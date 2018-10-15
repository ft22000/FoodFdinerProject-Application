using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoodFinder.Models;
using Foundation;
using UIKit;
using MapKit;

/// <summary>
/// Creator: Thomas Enniss
/// Created: 30/07/2018
/// </summary>
namespace FoodFinder.iOS
{
    /// <summary>
    /// We followed a Xamarin example from the Microsoft Xamarin samples website.
    /// This class is for the custom popups on the Apples Google map. It works differently to the Android version.
    /// </summary>
    class CustomMKAnnotationView : MKAnnotationView
    {
        /*TE: We do not use this class currently. Leaving the variable comments blank for now until this is all in use and better understood.*/

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BuildingName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EventTime { get; set; }

        /// <summary>
        /// The event object containing all the details. 
        /// Could change in the future honestly, currently is sent to the Custom map when a pin is clicked so the event can be loaded.
        /// </summary>
        public Event foodFinderEvent { get; set; }

        public CustomMKAnnotationView(IMKAnnotation annotation, string id)
            : base(annotation, id)
        {
        }
    }
}