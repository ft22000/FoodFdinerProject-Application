using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FoodFinder.Models;
using FoodFinder.Data;

namespace FoodFinder.Views
{
    ///<summary>
    /// Class that defines and provides functionality to the Event List.
    ///</summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EventListPage : ContentPage
	{
        const String EMPTY_MESSAGE = "There are no events to display";        

        /// <summary>
        /// Default constructor function, initisalises all components, and populates the event list.
        /// </summary>
        public EventListPage ()
        {
            InitializeComponent();

            emptyTextLabel.IsVisible = false;
            emptyTextLabel.Text = EMPTY_MESSAGE;

            /*TE: Request a list of events from the database.*/
            eventList.ItemsSource = App.EventManager.Events;

            CheckIfEmptyAndDisplayMessage();

            /*JA: Reaction to selecting an event from the event list*/
            eventList.ItemSelected += (sender, e) => OnSelection(sender, e);
        }

        ///<summary>
        ///Checks if the empty message should be displayed.
        /// </summary>
        void CheckIfEmptyAndDisplayMessage()
        {
            if(App.EventManager.Events.Count == 0)
            {
                emptyTextLabel.IsVisible = true;
            }
            else
            {
                emptyTextLabel.IsVisible = false;
            }
        }

        ///<summary>
        ///This function is used to send an asynchronous request to the database to events.
        ///</summary>
        async void LoadEvents() {
            /*TE: For some reason this would be called twice. Potentially a conflict between the event list auto refreshing and on page appearing. */
            Console.WriteLine("Loading Events for the event list.");

            /*TE: Shows native loading spinner*/
            eventList.ItemsSource = await App.EventManager.GetEvents();
            /*TE: Hides native loading spinner after events have been loaded*/
            eventList.IsRefreshing = false;
            CheckIfEmptyAndDisplayMessage();
        }

        /// <summary>
        /// JA: Reacts to an event being selected from the event list
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        async void OnSelection(object sender, SelectedItemChangedEventArgs e)
		{
			((ListView)sender).SelectedItem = null;

			/*JA: Returns if an event is deselected, rather than selected*/
			if (e.SelectedItem == null)
			{
				return;
			}
			else
			{
				await Navigation.PushAsync(new EventDetailsPage((Event)e.SelectedItem));
			}
		}

        /// <summary>
        /// TE: This event is triggered when the user pulls downwards on the list to refresh it.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void RefreshEvents(object sender, EventArgs e) {
            LoadEvents();
        }

        /// <summary>
        /// This function is triggered when this page appears when navigating to it again. When clicking a back arrow to this page this will load the events again.
        /// </summary>
        protected override void OnAppearing() {
            base.OnAppearing();

            LoadEvents();
        }
    }
}
