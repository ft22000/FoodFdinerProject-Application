using System;

namespace FoodFinder
{
    /// <summary>
    /// View Model for the Master Page Item
    /// </summary>
    class MasterPageItem
    {
        
        /// <summary>
        /// Getter and Setter for title of each item in the Menu
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// getter and setter for the icon source URI (as String)
        /// </summary>
        public string IconSource { get; set; }

        /// <summary>
        /// getter and setter for the target page type
        /// </summary>
        public Type TargetType { get; set; }  
        
        /// <summary>
        /// Getter and Setter for the Name of the Cell, unused at this stage 
        /// TODO: Remove if not needed.
        /// </summary>
        public string Name { get; set; }

    }
}