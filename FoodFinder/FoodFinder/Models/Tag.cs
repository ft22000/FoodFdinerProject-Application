using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FoodFinder.Models
{
    /// <summary>
    /// Model for the list of Tags
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Name of the Tag
        /// </summary>
        [JsonProperty(PropertyName = "Description")]
        public string Tag_Name { get; set; }

        /// <summary>
        /// Unique ID of the Tag
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        public int Tag_ID { get; set; }

        /// <summary>
        /// Default Constructor Function
        /// </summary>
        public Tag()
        {

        }

        /// <summary>
        /// Forcibly casts the Tag Name to a string
        /// </summary>
        /// <returns>Name of the Tag</returns>
        public override string ToString()
        {
            return Tag_Name;
        }
    }
}