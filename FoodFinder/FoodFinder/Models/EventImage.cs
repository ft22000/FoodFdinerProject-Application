using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FoodFinder.Models
{
    /// <summary>
    /// Uses Json properties to bind the data from an event photo, from the database.
    /// </summary>
    public class EventImage
    {
        /// <summary>
        /// Data Field of the Image
        /// </summary>
        [JsonProperty(PropertyName = "Data")]
        public string Data { get; set; }

        /// <summary>
        /// Default constructor function
        /// </summary>
        public EventImage()
        {

        }

        /// <summary>
        /// Overrides the Data to field to a string format
        /// </summary>
        /// <returns>Formatted Data</returns>
        public override string ToString()
        {
            return Data;
        }
    }
}
