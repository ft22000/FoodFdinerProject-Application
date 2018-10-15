using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace FoodFinder.Models
{
    

    /// <summary>
    ///  Class that defines an event. 
    /// </summary>
    public class Event
    {
        
        /// <summary>
        /// Use json properties like this so Json can bind values received from the database. LoadALLEvents.php is responsible for creating the json arrays.
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }

        /// <summary>
        /// Event Name
        /// </summary>
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Event Location
        /// </summary>
        [JsonProperty(PropertyName = "Location")]
        public string Location { get; set; }

        /// <summary>
        /// Event Longitude
        /// </summary>
        [JsonProperty(PropertyName = "Longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Event Latitude
        /// </summary>
        [JsonProperty(PropertyName = "Latitude")]
        public double Latitude { get; set; }

        /// <summary>
        /// Campus the event is located on.
        /// </summary>
        [JsonProperty(PropertyName = "Campus")]
        public Models.Campus Campus { get; set; }

        /// <summary>
        /// Event Start Time
        /// </summary>
        [JsonProperty(PropertyName = "StartTime")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Event Food Serve Time
        /// </summary>
        [JsonProperty(PropertyName = "FoodServeTime")]
        public DateTime FoodServeTime { get; set; }

        /// <summary>
        /// Event Closing Time
        /// </summary>
        [JsonProperty(PropertyName = "ClosingTime")] 
        public DateTime closingTime { get; set; }

        /// <summary>
        /// Tags associated with the event.
        /// </summary>
        [JsonProperty(PropertyName = "Tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// The organiser of the event
        /// </summary>
        [JsonProperty(PropertyName = "Organiser")]
        public int Organiser { get; set; }

        /// <summary>
        /// The person who suggested the event be closed.
        /// </summary>
        [JsonProperty(PropertyName = "SuggestedToClose")]
        public int SuggestedToClose { get; set; }


        /// <summary>
        /// Default Constructor
        /// </summary>
        public Event()
        {
            
        }

        /// <summary>
        /// Formats the DateTime element of the Event
        /// </summary>
        /// <param name="Date">Date of the Event</param>
        /// <returns>Formattted DateTime</returns>
        public string DateTimeToSQLFormat(DateTime Date)
        {
            return Date.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }    
    }
}
