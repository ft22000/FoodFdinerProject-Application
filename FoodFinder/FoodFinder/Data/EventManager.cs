using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using FoodFinder.Models;
using System.Threading.Tasks;

namespace FoodFinder.Data
{
    /// <summary>
    /// Created this as a singleton class as a central location events can be stored,accessed and refreshed.
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// TE: The Observable Collection of events.
        /// </summary>
        public ObservableCollection<Event> Events { get; private set; }

        /// <summary>
        /// TE: Constructor function
        /// </summary>
        public EventManager() {
            LoadEventAsync();
        }

        /// <summary>
        /// Private method to allow the constructer to run Load Events Asynchronously
        /// </summary>
        private async void LoadEventAsync()
        {
            await LoadEvents();
        }

        /// <summary>
        /// This is used to refresh the events in the singleton.
        /// </summary>
        /// <returns></returns>
        public async Task LoadEvents() {
            Console.WriteLine("Event Manager is Loading Events...");
            Events = await App.RestManager.RefreshEventsAsync();
            await OffsetEventLocations();
            Console.WriteLine("Finished Loading Events!");
        }

        /// <summary>
        /// This adds a tiny offset to the end of each events lattitude. This makes the lattitude unique for each event allowing the selection of stacked pins to work cross platform.
        /// </summary>
        public async Task OffsetEventLocations() {

            /*TE: The count is added to 10000 so it becomes 10001, 10002 etc. This is then parsed to a string and concatenated to the end of the latitude making each latitude unique.
             * Just adding a number to the end wouldn't work as adding 1 and 10 would essentially be the same value. This keeps doubles the same length*/
            int eventCounter = 1;

            /*TE: Each event add the offset*/
            foreach (Event e in Events) {
                /*TE: Need all latitudes to have the same amounnt of decimals to keep it all even for comparisons.*/
                string latitude = Math.Round(e.Latitude,6).ToString();
                /*TE: adding leading 0s won't work, just add this number to the end and send it to a string. */
                int offset = 10000 + eventCounter;
                latitude += offset.ToString();
                eventCounter+=1;
                e.Latitude = Double.Parse(latitude);
                /*TE: It's a little dodgy but it works. Also*/
                Console.WriteLine("Event Number: "+ eventCounter+" New Latitude:"+ latitude);                
            }            
        }

        /// <summary>
        /// Returns the event collection. Loads the events first so you get the updated version.
        /// </summary>
        /// <returns>The event collection</returns>
        public async Task<ObservableCollection<Event>> GetEvents() {
            await LoadEvents();            
            return Events;
        }
    }
}