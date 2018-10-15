using System;
using System.Collections.ObjectModel;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FoodFinder.Models;
using Xamarin.Essentials;

namespace FoodFinder.Views
{
    /// <summary>
    /// The Event Details Page displays the details of an event.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventDetailsPage : ContentPage
    {
        /// <summary>
        /// A collection of food tags that updates when changed
        /// </summary>
        private ObservableCollection<Tag> FoodTags { get; set; }

        /// <summary>
        /// A collection of images that updates when changed
        /// </summary>
        private ObservableCollection<Image> Images { get; set; }

        /// <summary>
        /// The event is stored here so we can reference the closing date in the update timer method.
        /// </summary>
        Event selectedEvent;

        /// <summary>
        /// Sets the tooltips view to false by default. 
        /// </summary>
        bool showTips = false;

        /// <summary>
        /// Default Constructor Fucntion
        /// </summary>
        /// <param name="e">Event Handler</param>
        public EventDetailsPage(Event e)
        {
            /*TE: We store the event to be able to reference it later.*/
            selectedEvent = e;

            FoodTags = new ObservableCollection<Tag>();
            Images = new ObservableCollection<Image>();

            InitializeComponent();

            BindingContext = e;

            /*TE: Bind the name stright from the Event.Campus.Campus_Name.*/
            campus.Text = e.Campus.Campus_Name;

            /*TE: I did change a couple of things. Only the event organiser and a sustainability team member can edit and close an event.
             *We enable the Edit event button only for these two types of people. No need to disable the Close Event button on the edit page since only authorised people get access.*/
            if (Int32.Parse(Preferences.Get("UserID", "")) == selectedEvent.Organiser || Int32.Parse(Preferences.Get("PermissionLevel", "")) == Preferences.Get("SUSTAINABILITY_TEAM", 3))
            {
                editThisEventButton.IsVisible = true;
            }
            else {
                /*TE: All general users and other event organisers who do not own this event, cannot edit it or close it. The can only suggest it be closed.*/
                SuggestCloseButton.IsVisible = true;

                Console.WriteLine("You are a general user or a different organiser. You cannot edit or close this event");

                /*TE: If someone has suggested this event be closed already, the suggestedtoclose field will not be null (-1) and we disable the Suggest to close button and chnge it's text.
                 * The user should then know someone else has requested it be closed.
                 * I return SuggestedToClose as -1 if it is null since it is an integer and no user can have -1 as an ID.*/
                if (selectedEvent.SuggestedToClose != -1)
                {
                    Console.WriteLine("Someone suggested this event should be closed!");
                    SuggestCloseButton.Text = "Closure suggested";
                    SuggestCloseButton.InputTransparent = true;
                    SuggestCloseButton.Clicked -= SuggestEventbeClosed;
                }
            }

            /*TE: TODO Bind the name stright from the Event.Campus.Name. Did this to get it working.*/
            campus.Text = e.Campus.Campus_Name;
            /*TE: We set the initial value of the count down timer.*/
            UpdateEventTimer();
            /*JA: Sets the default position for the food tags carousel*/
            //foodTagsCarousel.Position = 0;

            /*JA: Sets the source for the food tags carousel*/
            foodTagsCarousel.ItemsSource = FoodTags;

            /*JA: Food tags added to the carousel*/
            /*TE: updates from the database now. The number of food tags allows us to create that many Tags for the carousel.*/
            int numberOfFoodTags = e.Tags.Length;

            /*TE: We loop through the tags and create a Tag object for each tag string.*/
            for (int i = 0; i < numberOfFoodTags; i++)
            {
                FoodTags.Add(new Tag() { Tag_Name = e.Tags[i] });
            }

            /*JA: Sets the default position for the images carousel*/
            imagesCarousel.Position = 0;

            /*JA: Sets the source for the images carousel*/
            imagesCarousel.ItemsSource = Images;

            /*JA: Images added to the carousel*/
            LoadImagesAsync(e.Id);

            /*TE: We initialize a timer method that will run every second and update the event countdown time.*/
            Device.StartTimer(TimeSpan.FromSeconds(1), UpdateEventTimer);

            /*TE: A request is sent to the database to record that the user was interested in this event. Duplicates are not tolerated.*/
            App.RestManager.RegisterEventInterest(e.Id, Preferences.Get("UserID", ""));
        }

        /// <summary>
        /// This method updates the Remaining Event Time by finding the difference between the current time and the time the event closes.
        /// Returns a bool for the simple sake that the Device.StartTimer requires it.
        /// </summary>
        /// <returns>Returns true for (insert reason here).</returns>
        bool UpdateEventTimer()
        {
            /*TE: Find differnce between event close time and current time. We add 10 hours to account for our time zone which is UTC + 10.
             *TODO: Fix timezones in database so we don't have to hard code it.*/
            TimeSpan timeLeft = selectedEvent.closingTime - DateTime.Now;

            /*TE: We format the time into a nice little string which is set as the remaining time's Text value.*/
            remainingTime.Text = timeLeft.ToString("%h'h '%m'm '%s's'");
            return true;
        }

        /// <summary>
        /// On pressing the suggest event closure button
        /// </summary>
        /// <param name="sender">sender contains information required by method.</param>
        /// <param name="e">Event Handler</param>
        void OnSuggestClosure(object sender, EventArgs e)
        {
            /*JA: Displays an alert asking to confirm the suggestion*/
            /*JA: TODO React to the 'Yes' response*/
            DisplayAlert("Close this event?", "Are you sure you want to suggest the event be closed?", "Yes", "No");
        }


        /// <summary>
        /// Handles the user pressing the edit button.
        /// </summary>
        /// <param name="sender">Sender contains all the information required for e</param>
        /// <param name="e">Event Handler</param>
        async void OnEditEvent(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EventEditingPage(selectedEvent));
        }

        /// <summary>
        /// Converts a Base64String to an Image.
        /// </summary>
        /// <param name="imageString">A base64String that contains an image</param>
        /// <returns>An Image</returns>
        Image ConvertBase64ToImage(string imageString)
        {
            //JA: Creates a new image and sets the source to the base64String
            Image img = new Image
            {
                Source = ImageSource.FromStream(() =>
                new MemoryStream(Convert.FromBase64String(imageString)))
            };

            return img;
        }

        /// <summary>
        /// Loads an event's images
        /// </summary>
        /// <param name="id">ID of the event</param>
        async void LoadImagesAsync(int id)
        {
            // JA: Gets a list of the images
            ObservableCollection<EventImage> imageList = await App.RestManager.LoadEventImageList(id);
            int x = 0;

            // JA: For each image for the list
            try
            {
                for (int i = 0; i < imageList.Count; i++)
                {
                    // JA: Adds an image to the Observational Collection
                    Images.Add(ConvertBase64ToImage(imageList[i].ToString()));
                    x++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// This function handles the event triggered by the Suggest to close button.
        /// The user is prompted for confirmation and then the request is sent to the server for suggesting the event be closed.
        /// </summary>
        /// <param name="sender">UI element triggered</param>
        /// <param name="args">Event arguements</param>
        async void SuggestEventbeClosed(object sender, EventArgs args)
        {
            /*TE: Confirm the user wants to suggest the event be closed*/
            var confirmation = await DisplayAlert("Warning", "Are you sure this event should be closed?", "Yes", "No");

            /*TE: Confirmation ends up being a boolean with Yes = true*/
            if (confirmation)
            {
                /*TE: Trigger loadding spinner until this is all done, disable the suggest to close button so it cannot spam requests.*/
                loadingSpinner.IsRunning = true;
                SuggestCloseButton.InputTransparent = true;
                string[] result = await App.RestManager.SuggestEventClosure(selectedEvent);

                /*TE: If event was updated with the suggestion successfully, a suggestion was made recently or the event has recently been closed since leaving the mappage,
                 * we inform the user and return user to map page. Otherwise Inform the user it failed and re-enable the buttons etc*/
                if (Boolean.Parse(result[0]))
                {
                    /*TE: Closure suggested is added here in case we do not want to go straight back to main page. Can't see why not. They want it closed.*/
                    SuggestCloseButton.Text = "Suggestion Sent!";
                    loadingSpinner.IsRunning = false;
                    await DisplayAlert("Success", result[1], "OK");
                    App.Current.MainPage = new MainPage();
                }
                else {
                    /*TE:Display error saying it cannot be done. etc*/
                    SuggestCloseButton.InputTransparent = false;
                    loadingSpinner.IsRunning = false;
                    await DisplayAlert("Error", result[1], "OK");
                }
            }
            else
            {
                Console.WriteLine("Close event cancelled by user.");
            }
        }

        /// <summary>
        /// Shows/hides the tooltips control
        /// </summary>
        /// <param name="sender">Object to send the data.</param>
        /// <param name="e">Event Arguments.</param>
        private void ShowHideTips(object sender, EventArgs e)
        {
            showTips = !showTips;

            detailsCutleryTooltip.IsVisible = showTips;
            
        }
    }
}
