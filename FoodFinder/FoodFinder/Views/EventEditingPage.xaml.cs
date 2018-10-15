using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FoodFinder.Models;
using FoodFinder.Data;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.IO;

namespace FoodFinder.Views
{
    /// <summary>
    /// Class for Editing the Events
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventEditingPage : ContentPage
    {
        /// <summary>
        /// Used for handling selected events
        /// </summary>
        private Event SelectedEvent;

        /// <summary>
        /// Array list of Selected Tags assosciated with an event
        /// </summary>
        ArrayList SelectedTagList = new ArrayList();

        /// <summary>
        /// Stores all images as base64strings
        /// </summary>
        ObservableCollection<String> base64Images = new ObservableCollection<string>();

        /// <summary>
        /// Stores all campuses, used to set all switches to false before setting subscribed campuses to true
        /// </summary>
        ObservableCollection<Tag> tagList = new ObservableCollection<Tag>();

        /// <summary>
        /// stores the tag name and the switch state of all tags
        /// </summary>
        ObservableCollection<TagState> listViewTag;

        /// <summary>
        /// Sets the tooltips to be hidden by default
        /// </summary>
        bool showTips = false;

        /// <summary>
        /// Default Constructor Function
        /// </summary>
        /// <param name="e">Is passed an event (e) to edit</param>
        public EventEditingPage(Event e)
        {
            /*TE: Store event so we can potentially edit it later.*/
            SelectedEvent = e;

            InitializeComponent();
            //campusOptions = new ObservableCollection<Campus>();

            //Disable start time editing if the event has started
            DateTime currentTime = DateTime.Now;
            DateTime eventStartTime = e.StartTime;

            if (DateTime.Compare(currentTime, eventStartTime) > 0)
            {
                startTimeLabel.IsVisible = false;
                startTimePicker.IsVisible = false;
            }

            //JW: Obtains observable collection used to populate the picker
            RetrieveCampusList();

            //EDH: Obtains observable collection used to populate the tag picker
            RetrieveTagList(e);

            BindingContext = SelectedEvent;

            /*TE: Set default time.*/
            startTimePicker.Time = e.StartTime.TimeOfDay;

            /*TE: Receives a message from, the custom map. This is triggered when the user clicks the info window on an info window of a pin*/
            MessagingCenter.Subscribe<CustomMap, double[]>(this, "EventLocation", (sender, args) => {
                Console.WriteLine("EVENT EDITING: Latitude: {0} - Longitude: {1}", args[0], args[1]);

                /*TE: Sets the selected event since we are keeping that event.*/
                SelectedEvent.Latitude = args[0];
                SelectedEvent.Longitude = args[1];
            });

            // JN: Prevent highlighting of selections in the list that are not made with the toggle.
            selectTags.ItemSelected += (object sender, SelectedItemChangedEventArgs s) =>
            {
                // Deselecting an item response
                if (s.SelectedItem == null)
                {
                    return;
                }

                // Deselect any selected event
                if (sender is ListView lv)
                {
                    lv.SelectedItem = null;
                }
                else
                {
                    return;
                }
            };
        }

        /// <summary>
        /// This loads the map page so we can select a neww location for the event.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="args">Event Handler</param>
        async void SelectEventLocationOnMap(object sender, EventArgs args)
        {

            await Navigation.PushAsync(new MapPage(true));

        }

        /// <summary>
        /// Opens the tag selection custom popup
        /// </summary>
        /// <param name="sender">Data Object Sender</param>
        /// <param name="e">Event Handler</param>
        void TagSelection(object sender, EventArgs e)
        {
            InitialPage.IsVisible = false;
            TagSelectPage.IsVisible = true;
        }

        /// <summary>
        /// Close tag picker takes the selected values within the listview and stores them until the event is created.
        /// </summary>
        /// <param name="sender">Data Object Sender</param>
        /// <param name="e">Event Handler</param>
        void CloseTagPicker(object sender, EventArgs e)
        {
            InitialPage.IsVisible = true;
            TagSelectPage.IsVisible = false;
        }

        /// <summary>
        /// Triggered when the camera image/button is pressed.
        /// Can add photos to the event from the devices's camera.
        /// Accesses the device's camera to facilitate adding a picture to the event.
        /// </summary>
        /// <param name="sender">Object sender passes relevant information to the method</param>
        /// <param name="args">Event Handler</param>
        async void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            var action = await DisplayActionSheet("Add a photo?", "No thanks", null, "Camera");

            if (action == "Camera")
            {
                TakePhoto();
            }
        }

        /// <summary>
        /// Checks if a valid camera is avaliable, if one is then opens the device's camera to take a photo. Photo is added to the new event.
        /// </summary>
        async void TakePhoto()
        {
            //Kim: Save camera permission status
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            //Kim: Save storage permission status
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            //Kim: Check camera status, if camera permission is not granted, request user to be enabled
            if (cameraStatus != PermissionStatus.Granted)
            {
                var cameraResults = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);

                //TE: Best practice to always check that the key exists
                if (cameraResults.ContainsKey(Permission.Camera))
                {
                    cameraStatus = cameraResults[Permission.Camera];
                }

            }
            if (storageStatus != PermissionStatus.Granted)
            {
                var storageResults = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);

                if (storageResults.ContainsKey(Permission.Storage))
                {
                    storageStatus = storageResults[Permission.Storage];
                }
            }

            //Kim: If camera permission is granted, run camera function
            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", ":( No camera avaliable.", "OK");
                    return;
                }
                else
                {
                    Console.Out.WriteLine("Camera detected");

                    //Opens the camera and sets the storage options
                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        Directory = "FoodFinder",
                        Name = "eventImage.jpg",
                        PhotoSize = PhotoSize.Custom,
                        CustomPhotoSize = 10,
                        CompressionQuality = 10
                    });

                    //Checks that the file exists
                    if (file == null)
                        return;

                    //Reads the photo path and saves as a Base64String that can be saved to the database
                    FileStream fileStream = new FileStream(file.Path, FileMode.Open, FileAccess.Read);

                    //Stores the image as a base64String and increments the count of how many images have been stored
                    base64Images.Add(ConvertImageToBase64(fileStream));
                }

            }
        }

        /// <summary>
        /// Converts an image stream to a Base64String.
        /// </summary>
        /// <param name="fileStream">File stream of the image</param>
        /// <returns>A Base64String</returns>
        string ConvertImageToBase64(FileStream fileStream)
        {
            //Create a byte array of the file stream length
            byte[] ImageData = new byte[fileStream.Length];

            //Read the block of bytes from the stream into the byte array
            fileStream.Read(ImageData, 0, Convert.ToInt32(fileStream.Length));

            //Close the file stream
            fileStream.Close();

            string _base64String = Convert.ToBase64String(ImageData);

            return _base64String;
        }

        /// <summary>
        /// When the update button is clicked, the app will attempt to update the event after checking the fields.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void ClickedUpdateEvent(object sender, SelectedItemChangedEventArgs e)
        {

            string error = "";
            // JN: Validate each field
            if (name.Text == "")
            {
                error += "Please enter a valid name.\n";
            }
            if (campus.SelectedIndex == -1)
            {
                error += "Please select a campus.\n";
            }

            if (Room.Text == "")
            {
                error += "Please select a room.\n";
            }

            if (error == "")
            {
                /*TE: Update the event*/
                UpdateEvent();
            }
            else
            {
                // JN: Display an alert containing any errors we have collected
                DisplayAlert("Alert", error, "Close");
            }
        }

        /// <summary>
        /// This function is running asynchronously to update the selected event.
        /// </summary>
        async void UpdateEvent()
        {
            /*Instead of creating a new event, I am altering the existing one.*/
            SelectedEvent.Name = name.Text;

            SelectedEvent.Campus = (Campus)campus.SelectedItem;

            // JN: eventStartTime is Today + the time from the selector
            DateTime eventStartTime = DateTime.Today;
            eventStartTime += startTimePicker.Time;
            SelectedEvent.StartTime = eventStartTime;

            SelectedEvent.Location = Room.Text;

            Console.WriteLine("Updating the Event! Coordinates at Lat{0} - Long{1}", SelectedEvent.Latitude,SelectedEvent.Longitude);
            /*TE: We attempt to update the event. This returns a boolean which is used to navigate between pages.*/
            if (await App.RestManager.UpdateEventAsync(SelectedEvent, SelectedTagList, SelectedTagList.Count, base64Images))
            {
                await DisplayAlert("Alert", "Event Updated", "OK");
                App.Current.MainPage = new MainPage();
            }
            else
            {
                await DisplayAlert("Alert", "Error Updating Event", "OK");
            }
        }

        /// <summary>
        /// Async function called as soon as the page loads, populating the picker campuses
        /// </summary>
        async void RetrieveCampusList()
        {
            // JW: observable collection which is used to store all campuses in order to select default campus
            ObservableCollection<Campus> campusOptions;

            // JW: both picker itemsource and observable collection are set to the values returned by the RetrieveCampusList function
            campus.ItemsSource = campusOptions = await App.RestManager.RetrieveCampusList();

            //TE: Since the event already has campus, we use linq to search for the campus in the collection and retirve its index.
            //    Then we can set it as the default in the picker.
            //    TODO: Improve/change this later.
            //
            var selectedCampus = campusOptions.Where(X => X.Campus_ID == SelectedEvent.Campus.Campus_ID).FirstOrDefault();

            /*TE: If there is no campus, the app crashes for null exception. Leaves picker blank.*/
            if (selectedCampus != null)
            {
                int selectedCampusIndex = campusOptions.IndexOf(selectedCampus);
                campus.SelectedIndex = selectedCampusIndex;
            }
        }

        ///<summary>
        /// Async function called as soon as the page loads, populating the picker tags
        /// </summary>
        async void RetrieveTagList(Event e)
        {
            tagList = await App.RestManager.RetrieveTagList();

            //JW: Obtains original event's tags and enables them in the tag list
            SetEventTagState(e);
        }

        /// <summary>
        /// Obtains the tag name from the switch, adding it to an array list to send off to the db
        /// </summary>
        /// <param name="sender">Object-Sender that passes required information to the method</param>
        /// <param name="args">toggle handler</param>
        void OnToggledItem(object sender, ToggledEventArgs args)
        {
            var switchItem = (Switch)sender;
            var SelectedParent = switchItem.BindingContext;

            if (switchItem.IsToggled)
            {
                SelectedTagList.Add(SelectedParent);
            }
            else
            {
                SelectedTagList.Remove(SelectedParent);
            }
        }

        /// <summary>
        /// Sets the switch state of the event's tags
        /// </summary>
        /// <param name="e"></param>
        void SetEventTagState(Event e)
        {
            //JW: original tags for the event
            string[] tags = e.Tags;

            listViewTag = new ObservableCollection<TagState>();

            //JW: loops through all tags and adds each of the event's original tags to the new list, set to the on/true state
            foreach (var tag in tagList)
            {

                if (tags.Contains(tag.ToString()))
                {
                    listViewTag.Add(new TagState { ListTagName = tag.ToString(), ListTagState = "True" });
                }
                else
                {
                    listViewTag.Add(new TagState { ListTagName = tag.ToString(), ListTagState = "False" });
                }

                //JW: set the list view item source to the new observable collection with the updated switch states
                selectTags.ItemsSource = listViewTag;
            }
        }

        /// <summary>
        /// This function handles the event triggered by the close event button.
        /// The user is prompted for confirmation and then the request is sent to the server to close the event.
        /// </summary>
        /// <param name="sender">UI element triggered</param>
        /// <param name="args">Event arguements</param>
        async void CloseEvent(object sender, ToggledEventArgs args)
        {
            /*TE: Check user wants to close event*/
            var confirmation = await DisplayAlert("Warning", "Are you sure you want to close this event?", "Yes", "No");

            /*TE: Confirmation ends up being a boolean with Yes = true*/
            if (confirmation)
            {
                /*TE: Trigger loading spinner and disable the close event button so it cannot be spammed.*/
                loadingSpinner.IsRunning = true;
                CloseEventButton.IsEnabled = false;
                /*TE: Close the event and store the boolean result so we can inform the user if it fails or take them to the map page.*/
                string[] result = await App.RestManager.CloseEvent(SelectedEvent);

                /*TE: IF the event is closed or someone else beat this user to closing it since the event list was last refreshed, inform the user and return to mappage.
                 *Otherwise inform the user there was an error and re-enable the buttons etc.*/
                if (Boolean.Parse(result[0]))
                {
                    loadingSpinner.IsRunning = false;
                    await DisplayAlert("Success", result[1], "OK");
                    App.Current.MainPage = new MainPage();
                }
                else
                {
                    /*TE: notify the user it failed and re-enable the close event button etc*/
                    CloseEventButton.IsEnabled = true;
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHideTips(object sender, EventArgs e)
        {
            showTips = !showTips;

            eventNameToolTip.IsVisible = showTips;
            campusPickerToolTip.IsVisible = showTips;
            roomToolTip.IsVisible = showTips;
            locationToolTip.IsVisible = showTips;
            tagSelectToolTip.IsVisible = showTips;
            cameraToolTip.IsVisible = showTips;
        }
    }
}
