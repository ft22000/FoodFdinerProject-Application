using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FoodFinder.Models
{
    /// <summary>
    /// Model for List of Campuses
    /// </summary>
    public class Campus
    {
        /// <summary>
        /// Getter and Setter for the Campus Name
        /// </summary>
        [JsonProperty(PropertyName = "Name")]
        public string Campus_Name { get; set; }

        /// <summary>
        /// Getter and Setter for Campus ID
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        public int Campus_ID { get; set; }

        /// <summary>
        /// Campus List Wrapper
        /// </summary>
        public Campus()
        {

        }

        /// <summary>
        /// Override for Campus Name, Forces the Campus_Name to a string 
        /// </summary>
        /// <returns>Returns a String of the Campus_Name</returns>
        public override string ToString()
        {
            return Campus_Name;
        }
    }
}
