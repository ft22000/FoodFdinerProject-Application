using System;
using System.Collections.Generic;
using System.Text;

namespace FoodFinder.Models
{
    /// <summary>
    /// Model for the tag switch state when editing an event
    /// </summary>
    public class TagState
    {
        /// <summary>
        /// Getter and Setter for ListCampusName
        /// </summary>
        public string ListTagName { get; set; }

        /// <summary>
        /// Getter and Setter for the state of the Switches
        /// </summary>
        public string ListTagState { get; set; }

        /// <summary>
        /// Overriden ToString function to force the ListCampusName and cast it to a String
        /// </summary>
        /// <returns>The name of the campus</returns>
        public override string ToString()
        {
            return ListTagName;
        }
    }
}
