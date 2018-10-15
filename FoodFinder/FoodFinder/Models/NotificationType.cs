using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoodFinder.Models
{
    /// <summary>
    /// The model for the notification states
    /// </summary>
    public class NotificationType
    {
        /// <summary>
        /// Unique Notification Identifier. 
        /// </summary>
        [JsonProperty(PropertyName = "Notification_ID")]
        public int Notification_ID { get; set; }

        /// <summary>
        /// Default Constructor Function
        /// </summary>
        public NotificationType()
        {

        }
    }
}