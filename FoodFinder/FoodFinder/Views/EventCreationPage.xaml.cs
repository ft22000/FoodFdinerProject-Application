using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using System.IO;
using FoodFinder.Models;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Collections;
using Xamarin.Essentials;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace FoodFinder.Views
{
    /// <summary>
    /// The EventCreationPage Class displays the event creation form, facilitates the creation of a new event and then navigates the user back to the default home page. In this case the
    /// default page is the Map Screen. This screen can only be accessed by user's with the permission level of Event Organiser or higher.
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventCreationPage : ContentPage
    {
        ArrayList SelectedTagList = new ArrayList();
        ObservableCollection<String> base64Images = new ObservableCollection<string>();

        /// <summary>
        /// Stores all campuses, used to set all switches to false before setting subscribed campuses to true
        /// </summary>
        ObservableCollection<Campus> campusList = new ObservableCollection<Campus>();

        /// <summary>
        /// Sets the tooltips to be hidden by default
        /// </summary>
        bool showTips = false;

        double latitude;
        double longitude;

        /// <summary>
        /// EventCreationPage() initialises the EventCreationPage components.
        /// </summary>
        public EventCreationPage ()
        {
            InitializeComponent();

            //This was causing the crash. The updated webportal login.php that returns the default location has not been merged so the primary campus was not being set.
            try
            {
                latitude = Double.Parse(Preferences.Get("PrimaryCampusLatitude", ""));
                longitude = Double.Parse(Preferences.Get("PrimaryCampusLongitude", ""));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error when setting events default coordinates: {0}",e.Message);
                latitude = 0;
                longitude = 0;
            }

            //Obtains observable collection used to populate the picker
            RetrieveCampusList();

            //Obtains observable collection used to populate the tag picker
            RetrieveTagList();

            //Defaults the event start time to the current time
            startTime.Time = DateTime.Now.TimeOfDay;

            //Receives a message from, the custom map. This is triggered when the user clicks the info window on an info window of a pin
            MessagingCenter.Subscribe<CustomMap, double[]>(this, "EventLocation", (sender,args) => {
                Console.WriteLine("Latitude: {0} - Longitude: {1}", args[0], args[1]);
                latitude = args[0];
                longitude = args[1];
            });

            //adds values to duration picker
            DurationPicker.Items.Add("Select Duration (h:mm)");
            for (int h = 0; h <= 4; h++)
            {
                if (h > 0)
                {
                    DurationPicker.Items.Add(h.ToString() + ":00");
                }
                if (h < 4)
                {
                    for (int m = 15; m < 59; m += 15)
                    {
                        string toAdd = h.ToString() + ":" + m.ToString();

                        DurationPicker.Items.Add(toAdd);
                    }
                }
            }

            //Sets the initially selected value
            DurationPicker.SelectedIndex = 0;

            //Prevent highlighting of selections in the list that are not made with the toggle.
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
        /// Opens the tag selection custom popup
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void TagSelection(object sender, EventArgs e)
        {
            InitialPage.IsVisible = false;
            TagSelectPage.IsVisible = true;
        }

        /// <summary>
        /// Close tag picker takes the selected values within the listview and stores them until the event is created.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void CloseTagPicker(object sender, EventArgs e)
        {
            TagSelectButton.Text = "Reselect Tags";
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
            if (cameraStatus != PermissionStatus.Granted )
            {
                var cameraResults = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);

                //TE: Best practice to always check that the key exists
                if (cameraResults.ContainsKey(Permission.Camera))
                {
                    cameraStatus = cameraResults[Permission.Camera];
                }

            }
            if(storageStatus != PermissionStatus.Granted)
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
        /// Opens the Map Page, in order to select a location for the pin.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="args">Event Handler</param>
        async void SelectEventLocationOnMap(object sender, EventArgs args) {

            await Navigation.PushAsync(new MapPage(true));

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
        /// Gets the values from the name, event start time, food serve time, current user and location before sending a request to the rest api to create a new event.
        /// Still needs to check the values before allowing them to be sent to the server.
        /// </summary>
        async Task<bool> CreateEvent()
        {

            //enables the loading spinner
            loadingSpinner.IsRunning = true;

            //Create a new event object
            Event e = new Event
            {

                //Fill the event with data
                Name = name.Text,
                Campus = (Campus)campus.SelectedItem
            };

            //eventStartTime is Today + the time from the selector
            DateTime eventStartTime = DateTime.Today;
            eventStartTime += startTime.Time;
            e.StartTime = eventStartTime;

            /* JN: foodServeTime is eventStartTime + the minutes from the form
             * T Not anymore. Change FoodServeTime to be the same as start time and closing time is now  start time + minutes to add.
             */
            DateTime foodServeTime = eventStartTime;
            e.FoodServeTime = foodServeTime;

            string numOfHours = DurationPicker.SelectedItem.ToString().Substring(0, 1);
            string numOfMinutes = DurationPicker.SelectedItem.ToString().Substring(2, 2);
            double foodServeTimer = Double.Parse(numOfHours) * 60 + Double.Parse(numOfMinutes);

            DateTime eventClosingTime = eventStartTime;
            e.closingTime = eventClosingTime.AddMinutes(foodServeTimer);
            Console.WriteLine(e.closingTime);
            e.Organiser = Int32.Parse(Preferences.Get("UserID", ""));

            // JN: The Room field
            e.Location = Building.Text + ", " + Room.Text;
            e.Latitude = latitude;
            e.Longitude = longitude;

            // JN: Send the request
            bool result = await App.RestManager.CreateNewEvent(e, base64Images, SelectedTagList, SelectedTagList.Count);

            //JW: disables the loading spinner
            loadingSpinner.IsRunning = false;

            return result;
        }


        /// <summary>
        /// Triggered when the create event button is pressed.
        /// Checks the name, campus, food serve time, room, and building to make sure they are not empty and attempts to create an event.
        /// The user is then navigated back to the default home page.
        /// </summary>
        async void ClickedCreateEvent()
        {
            string error = "";
            // JN: Validate each field
            if (String.IsNullOrWhiteSpace(name.Text))
            {
                error += "Please enter an event name.\n";
            }

            if (DurationPicker.SelectedIndex == 0)
            {
                error += "Please select an event duration\n";
            }

            if (campus.SelectedIndex == -1)
            {
                error += "Please select a campus.\n";
            }

            if (String.IsNullOrWhiteSpace(Room.Text))
            {
                error += "Please enter a room.\n";
            }

            if (String.IsNullOrWhiteSpace(Building.Text))
            {
                error += "Please enter a building.";
            }

            if (error == "") {
                // JN: Create the event
                if (await CreateEvent())
                {
                    // JN: Force Mainpage as we don't want this page to be available anywhere else
                    App.Current.MainPage = new MainPage();
                }
                else {
                    //TE: Could not save the event
                    await DisplayAlert("Alert", "Error saving the event", "Close");

                }
            }
            else
            {
                // JN: Display an alert containing any errors we have collected
                await DisplayAlert("Alert", error, "Close");
            }
		}

        /// <summary>
        /// Async function called as soon as the page loads, populating the picker campuses and setting primary campus data
        /// </summary>
        async void RetrieveCampusList()
        {
            int campusListCounter = 0;

            //JW: Updates primary campus data
            await App.RestManager.RetrievePrimaryCampus(Preferences.Get("UserID", ""));

            campusList = await App.RestManager.RetrieveCampusList();
            campus.ItemsSource = campusList;

            //JW: sets the default picker campus as the user's primary campus
            foreach (var campusName in campusList)
            {
                if (Preferences.Get("PrimaryCampus", "") == campusName.ToString())
                {
                    campus.SelectedIndex = campusListCounter;
                }

                campusListCounter++;
            }
        }

        /// <summary>
        /// Async function called as soon as the page loads, populating the picker tags
        /// </summary>
        async void RetrieveTagList()
        {
            selectTags.ItemsSource = await App.RestManager.RetrieveTagList();
        }

        /// <summary>
        /// Obtains the tag name from the switch, adding it to an array list to send off to the db
        /// </summary>
        /// <param name="sender">Object-Sender that passes required information to the method</param>
        /// <param name="args">Toggle handler</param>
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
        /// Shows/hides the tooltips control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHideTips(object sender, EventArgs e)
        {
            showTips = !showTips;

            eventNameToolTip.IsVisible = showTips;
            timePickerToolTip.IsVisible = showTips;
            durationPickerToolTip.IsVisible = showTips;
            campusPickerToolTip.IsVisible = showTips;
            roomToolTip.IsVisible = showTips;
            buildingToolTip.IsVisible = showTips;
            locationToolTip.IsVisible = showTips;
            tagSelectToolTip.IsVisible = showTips;
            cameraToolTip.IsVisible = showTips;
        }

        /// <summary>
        /// Obtains the users primary campus and campus coordinates, and updates their session values.
        /// </summary>
        /// <param name="userId">The unique identification number of the user.</param>
        async void RetrievePrimaryCampus(string userId)
        {
            await App.RestManager.RetrievePrimaryCampus(userId);
        }
    }
}
