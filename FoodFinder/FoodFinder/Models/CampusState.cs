using System;
using System.Collections.Generic;
using System.Text;

namespace FoodFinder.Models
{
    /// <summary>
    /// Model for the secondary campus switch state when in the preferences
    /// </summary>
    public class CampusState
    {
        /// <summary>
        /// Getter and Setter for ListCampusName
        /// </summary>
        public string ListCampusName { get; set; }

        /// <summary>
        /// Getter and Setter for the state of the Switches
        /// </summary>
        public string ListSwitchState { get; set; }

        /// <summary>
        /// Overriden ToString function to force the ListCampusName and cast it to a String
        /// </summary>
        /// <returns>The name of the campus</returns>
        public override string ToString()
        {
            return ListCampusName;
        }
    }
}
