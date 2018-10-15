using FoodFinder.Models;
using FoodFinder.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.FirebasePushNotification;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace FoodFinder.Data
{
    /// <summary>
    /// Rest Service that implements the functions from the IRestService Interface
    /// </summary>
    public class RestService : IRestService
    {

        /// <summary>
        /// The class we use to send http requests to the server.
        /// </summary>
        HttpClient client;

        /// <summary>
        /// The Server Address
        /// </summary>
        //TODO: Change to the Sustainability Teams Deployment Server
        readonly string serverAddress = "https://joebyaaronneil.space";

        /// <summary>
        /// Rest Service Constructor Function
        /// </summary>
        public RestService()
        {
            client = new HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        /// <summary>
        /// Calls fucntion to Refresh events in the event list. This is an asynchronous function so the application can still be used while this is running. Basic multithreading.
        /// Returns an observable collection(list that can trigger events basically) that contains events.
        /// </summary>
        /// <returns>Observable Collection of Events</returns>
        public async Task<ObservableCollection<Event>> RefreshEventsAsync()
        {

            /*TE: This needs to be changed to where the php file is located after being merged.*/
            var uri = new Uri(string.Format(serverAddress + "/api/LoadAllEvents.php"));

            /*TE: Observable collection to populate.*/
            ObservableCollection<Event> events = new ObservableCollection<Event>();

            try
            {
                Console.WriteLine("Preparing REST");

                var postData = new Dictionary<string, string> {
                    { "userId" ,Preferences.Get("UserID", "")}
                };              

                var content = new FormUrlEncodedContent(postData);

                /*TE: We need to send the user's id*/
                var response = await client.PostAsync(uri, content);

                /*TE: If the response was successful,*/
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");

                    /*TE: Convert this to a dynamic JObject. Since we are returning the array of all of the events, we broke the parsing to a dictionary.*/
                    var JSONresponse = JObject.Parse(responseString);

                    Console.WriteLine(JSONresponse);

                   if (JSONresponse["wasSuccessful"].ToString() == "1")
                   {   
                        /*TE: Need to serialize the JObject events back to a string so we can create the event objects with it. Little messy but it works.*/
                        events = JsonConvert.DeserializeObject<ObservableCollection<Event>>(JsonConvert.SerializeObject(JSONresponse["events"]));                        
                   }
                   else
                   {
                       Console.WriteLine("No events returned"+ JSONresponse["message"].ToString());
                   }
                }
            }
            catch (Exception e)
            {
                /*TE: This will pop out in the Visual Studio console.*/
                Console.WriteLine("ERROR {0}", e.Message);
            }
            return events;
        }

        /// <summary>
        /// Sends email and password strings to the web api, which will check for any
        /// matching credentials and send a json reply back w/ the appropriate information.
        /// </summary>
        /// <param name="email">A string containing the email address passed through from the login page</param>
        /// <param name="password">A string containing the plain text password passed through from the login page</param>
        /// <returns>Returns a string that is displayed to the user in an alert. Does Not display if the login is successful since the user is redirected immeadiately.</returns>
        public async Task<string> LoginAsync(string email, string password)
        {
            var uri = new Uri(string.Format(serverAddress + "/api/login.php"));

            //JW: set firebase id
            var firebaseId = CrossFirebasePushNotification.Current.Token.ToString();

            //JW: email and password will be sent in (in json) plain text unless server is using ssl
            var authData = new Dictionary<string, string> {
                    { "EmailAddress" , email },
                    { "Password" , password },
                    { "FirebaseID", firebaseId }
            };

            try
            {
                Console.WriteLine("Sending REST");

                var content = new FormUrlEncodedContent(authData);

                //JW: send content (authdata) to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    // JA: Get the response as JSON
                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    // JA: Check if the request found a valid account matching the provided details
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                    // JA: A valid account was found matching the provided details
                    if (successOutput == "1")
                    {
                        Console.WriteLine("Login: Success");

                        // JA: Get the required fields from the JSON response
                        JSONresponse.TryGetValue("id", out var idOutput);
                        JSONresponse.TryGetValue("permission", out var permissionOutput);
                        JSONresponse.TryGetValue("primaryCampus", out var primaryCampusOutput);
                        JSONresponse.TryGetValue("campusLongitude", out var primaryCampusLongitudeOutput);
                        JSONresponse.TryGetValue("campusLatitude", out var primaryCampusLatitudeOutput);
                        JSONresponse.TryGetValue("activated", out var activatedOutput);
                        JSONresponse.TryGetValue("isDeleted", out var isDeletedOutput);

                        // JA: Save the user's details to the settings (session)
                        Preferences.Set("UserID", idOutput);
                        Preferences.Set("PermissionLevel", permissionOutput);
                        Preferences.Set("PrimaryCampus", primaryCampusOutput);
                        Preferences.Set("PrimaryCampusLongitude", primaryCampusLongitudeOutput);
                        Preferences.Set("PrimaryCampusLatitude", primaryCampusLatitudeOutput);
                        Preferences.Set("Email", email);

                        //JW: set hardcoded session values for permission levels (general user, event organiser, sus team)
                        Preferences.Set("GENERAL_USER", 1);
                        Preferences.Set("EVENT_ORGANISER", 2);
                        Preferences.Set("SUSTAINABILITY_TEAM", 3);

                        // JA: Navigate to the main page of the application
                        if (isDeletedOutput == "0")
                        {
                            if (activatedOutput == "0")
                            {
                                return "user not activated";
                            }
                            else
                            {
                                App.Current.MainPage = new Views.MainPage();
                                return "";
                            }
                        }
                        else
                        {
                            return "This user has been deleted";
                        }
                    }
                    // JA: No valid accounts were found matching the provided details
                    else
                    {
                        Console.WriteLine("Login: No valid accounts found");
                        JSONresponse.TryGetValue("message", out var message);
                        return message;
                    }
                }
                else
                {
                    Console.WriteLine("Login: Error contacting server");
                    return "Error contacting server";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
                return "Could Not Send Request";
            }
        }

        /// <summary>
        /// This function sends aa request to the server to sign up a new user.
        /// </summary>
        /// <param name="email">The users email address</param>
        /// <param name="password">This is the ne users password</param>
        /// <param name="campus">This is the campus the user is signing up from</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task<string> SignupAsync(string email, string password, string campus)
        {
            // JA: TODO Change location once the api is in the web portal's root
            var uri = new Uri(string.Format(serverAddress + "/api/createUser.php"));

            // JA: Variable for the general user permission level
            var generalUser = "1";

            //JA: Email, password, and campus will be sent in JSON unless server is using ssl
            var authData = new Dictionary<string, string> {
                    { "EmailAddress" , email },
                    { "Password" , password },
                    { "PrimaryCampus" , campus },
                    { "PermissionLevel" , generalUser }
            };

            try
            {
                Console.WriteLine("Sending REST");

                var content = new FormUrlEncodedContent(authData);

                //JA: Send content (authdata) to uri via POST
                var response = await client.PostAsync(uri, content);

                //JA: Triggered if the api resonds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    // JA: Get the response as JSON
                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    // JA: Check if the request created a valid account with the provided details
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                    // JA: A valid account was created with the provided details
                    if (successOutput == "1")
                    {
                        //--TA sets the Sessions campus for location primary campus service.
                        Preferences.Set("PrimaryCampus", campus.ToString());
                        await SendSignupEmailAsync(email);
                        return "Signup Successful";
                    }
                    // JA: Couldn't create a valid account with the provided details
                    else
                    {
                        JSONresponse.TryGetValue("message", out var responseMessage);
                        return responseMessage;
                    }
                }
                else
                {
                    return "Could not connect to server";
                }
            }
            catch (Exception e)
            {
                return "ERROR {0}" + e.Message;
            }
        }

        /// <summary>
        /// Sends a request to the server to save a new event.
        /// </summary>
        /// <param name="newEvent">Saves the event object</param>
        /// <param name="eventImages">Collection of the event's iamges</param>
        /// <param name="eventTags">Collection of the event's tags</param>
        /// <param name="selectedTagCount">number of selected tags</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task<Boolean> SaveEventAsync(Event newEvent, ObservableCollection<string> eventImages, ArrayList eventTags, int selectedTagCount)
        {
            Console.WriteLine("SaveEventAsync: Begin");

            // JN: Events API
            var uri = new Uri(string.Format(serverAddress + "/api/CreateEvent.php"));

            try
            {
                // JN: Make POST Data
                // TE: Added the closing event parameter
                var postData = new Dictionary<string, string> {
                    { "Name" , newEvent.Name },
                    { "Campus" , newEvent.Campus.Campus_ID.ToString() },
                    { "StartTime" , newEvent.DateTimeToSQLFormat(newEvent.StartTime) },
                    { "FoodServeTime" , newEvent.DateTimeToSQLFormat(newEvent.FoodServeTime) },
                    { "ClosingTime" , newEvent.DateTimeToSQLFormat(newEvent.closingTime) },
                    { "Organiser" , newEvent.Organiser.ToString() },
                    { "LocatedIn" , newEvent.Location },
                    { "Latitude" , newEvent.Latitude.ToString() },
                    { "Longitude" , newEvent.Longitude.ToString() }

                };

                foreach (var pair in postData)
                {
                    Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
                }

                var content = new FormUrlEncodedContent(postData);

                // JN: Send request and store response
                var response = await client.PostAsync(uri, content);

                // JN: Wait for the response
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString);

                    /*TE: We decode the returned json string into our dictionary.*/
                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    /*TE: We check to see if the pass word update was a success*/
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);
                    Console.WriteLine(JSONresponse);

                    /*TE: Is a success.*/
                    if (successOutput == "1")
                    {

                        // Get the new event's ID
                        JSONresponse.TryGetValue("data", out var eventID);

                        // JA: Save all event images
                        foreach (string i in eventImages)
                        {
                            await SaveEventImageAsync(i, eventID.ToString());
                        }

                        await SaveEventTagsAsync(eventID, eventTags, selectedTagCount);

                        await SendEventEmailAsync(newEvent, eventID);
                        JSONresponse.TryGetValue("serverMessage", out var serverMessage);
                        return true;
                    }
                    else
                    {

                        JSONresponse.TryGetValue("serverMessage", out var serverMessage);
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
                return false;

            }
        }

        /// <summary>
        /// Saves an event's image to the database
        /// </summary>
        /// <param name="eventImage">The image to be saved</param>
        /// <param name="eventID">The associated event's ID</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task SaveEventImageAsync(string eventImage, string eventID)
        {

            var uri = new Uri(string.Format(serverAddress + "/api/SaveEventImage.php"));

            try
            {
                //JA: Make POST Data
                var postData = new Dictionary<string, string> {
                    { "Data" , eventImage },
                    { "Event" , eventID }
                };

                var content = new FormUrlEncodedContent(postData);

                //JA: Send request and store response
                var response = await client.PostAsync(uri, content);

                //JA: Wait for the response
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// Loads a list of an event's images
        /// </summary>
        /// <param name="eventID">Event ID for the images</param>
        /// <returns>An ObservableCollection of images as base64Strings</returns>
        public async Task<ObservableCollection<EventImage>> LoadEventImageListAsync(int eventID)
        {

            var uri = new Uri(string.Format(serverAddress + "/api/LoadEventImage.php"));

            ObservableCollection<EventImage> imageList = new ObservableCollection<EventImage>();

            try
            {
                //JA: Made POST Data
                var postData = new Dictionary<string, string> {
                    { "Event" , eventID.ToString() }
                };

                var content = new FormUrlEncodedContent(postData);

                //JA: Send request and store response
                var response = await client.PostAsync(uri, content);

                //JA: Wait for the response
                var responseString = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(responseString);

                imageList = JsonConvert.DeserializeObject<ObservableCollection<EventImage>>(responseString);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }

            return imageList;
        }

        
        /// <summary>
        /// Sends email of event details
        /// </summary>
        /// <param name="EmailEvent">The event which details will be emailed.</param>
        /// <param name="eventID">Unique Event ID</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task SendEventEmailAsync(Event EmailEvent, string eventID)
        {
            var uri = new Uri(string.Format(serverAddress + "/api/sendEventsEmail.php"));

            try
            {
                // JN: Make POST Data
                // TE: Added the closing event parameter
                var postData = new Dictionary<string, string> {
                    { "Name" , EmailEvent.Name },
                    { "Campus" , EmailEvent.Campus.Campus_ID.ToString() },
                    { "StartTime" , EmailEvent.DateTimeToSQLFormat(EmailEvent.StartTime) },
                    { "FoodServeTime" , EmailEvent.DateTimeToSQLFormat(EmailEvent.FoodServeTime) },
                    { "ClosingTime" , EmailEvent.DateTimeToSQLFormat(EmailEvent.closingTime) },
                    { "Organiser" , EmailEvent.Organiser.ToString() },
                    { "LocatedIn" , EmailEvent.Location },
                    { "EventID" , eventID.ToString() }

                };

                var content = new FormUrlEncodedContent(postData);

                //JA: Send request and store response
                var response = await client.PostAsync(uri, content);

                //JA: Wait for the response
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// Asynchronous Task for creating an Event Organiser privleges request reason
        /// </summary>
        /// <param name="userID">ID of the user making the request</param>
        /// <param name="emailAddress">User email making the request</param>
        /// <param name="reason">Reason for the request</param>
        public async Task CreateRequestAsync(int userID, string emailAddress, string reason)
        {
            var uri = new Uri(string.Format(serverAddress + "/api/appRequestManagement.php"));

            try
            {
                //JH: Email address and the request reason that will be sent in JSON
                var postData = new Dictionary<string, string> {
                    { "User_ID", userID.ToString() },
                    { "RequestReason", reason }
                };

                var content = new FormUrlEncodedContent(postData);

                // JA: Send content (postData) to uri vis POST
                var response = await client.PostAsync(uri, content);

                // JA: Triggered if the api responds
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    // JA: Send the request to all Sustainability Team users
                    await App.RestManager.SendRequestEmail(emailAddress, reason);
                }
            }
            catch (Exception e)
            {
                // JH: This will pop out in the Visual Studio console.
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        
        /// <summary>
        /// Sends a Request to the Server to update the User's Password.
        /// </summary>
        /// <param name="password">The current Password.</param>
        /// <param name="newPassword">The users new password.</param>
        /// <param name="id">ID of the user who wants the password changed.</param>
        /// <returns>Returns a string array. First string is a string boolean. The second is a message to the user.
        /// The reason for a string array is as follows:
        /// Boolean: False if the update failed due to server error or more importantly, incorect entry of current password.
        ///          True if the update succeeded. We need to know this so we can clear the password fields which we do not want to do if the update failed.
        /// The message: Informs the user the update happened successfully, they entered the wrong password or there was a server error.
        ///
        /// Need to return a boolean otherwise we can not differentiate between a success or failure message and clear fields etc when we need to.
        /// Message needs to be returned so the user knew they entered the wrong password. If we didn't need to tell them this I would just return a boolean. (Success update, Server error).
        /// </returns>
        public async Task<string[]> ChangePasswordAsync(string password, string newPassword, string id)
        {
            Console.WriteLine("ChangePasswordAsync: Begin");
            var uri = new Uri(string.Format(serverAddress + "/api/UpdatePassword.php"));

            /*TE: The json string we send to the server.*/
            var passwordData = new Dictionary<string, string> {
                { "Password" , password } ,
                { "newPassword", newPassword},
                { "id", id }
            };

            try
            {
                Console.WriteLine("Sending Request");

                var content = new FormUrlEncodedContent(passwordData);

                var response = await client.PostAsync(uri, content);

                /*TE: If the server responds, we the process the result to see if the password was successfully changed or not.*/
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(responseString);

                    /*TE: We decode the returned json string into our dictionary.*/
                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    /*TE: We check to see if the pass word update was a success*/
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                    /*TE: Is a success.*/
                    if (successOutput == "1")
                    {
                        /*TE: This should always read that the password was changed*/
                        JSONresponse.TryGetValue("serverMessage", out var serverMessage);
                        return new string[] { "true", serverMessage };
                    }
                    else
                    {
                        /*TE: This will return a short error message to the user. eg. Current password was incorrect.*/
                        JSONresponse.TryGetValue("serverMessage", out var serverMessage);
                        return new string[] { "false", serverMessage };
                    }
                }
                else
                {
                    /*TE: We orignally had an error with the php file. This return a failed status.*/
                    return new string[] { "true", "Error Contacting Server" };
                }
            }
            catch (Exception e)
            {
                return new string[] { "true", "Error Running" + e.Message.ToString() };
            }
        }

        /// <summary>
        /// This function sends a http request to the webserver to update a specific event. Mostly duplication of Joebys event creation and Jordans Response handling code tweaked a little.
        /// </summary>
        /// <param name="updatedEvent">The Event to be Updated</param>
        /// <param name="eventTags">The Tags associated with the event to be updated.</param>
        /// <param name="selectedTagCount">The Number of Tags associated with the event</param>
        /// <param name="eventImages">The images associated with the event</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task<bool> UpdateEventAsync(Event updatedEvent, ArrayList eventTags, int selectedTagCount, ObservableCollection<String> eventImages)
        {
            var uri = new Uri(string.Format(serverAddress + "/api/UpdateEvent.php"));
            try
            {
                var updateEventData = new Dictionary<string, string> {
                    { "Event_ID", updatedEvent.Id.ToString()},
                    { "Name" , updatedEvent.Name },
                    { "Campus" , updatedEvent.Campus.Campus_ID.ToString() },
                    { "StartTime" , updatedEvent.DateTimeToSQLFormat(updatedEvent.StartTime) },
                    { "FoodServeTime" , updatedEvent.DateTimeToSQLFormat(updatedEvent.StartTime) },
                    { "ClosingTime" , updatedEvent.DateTimeToSQLFormat(updatedEvent.closingTime) },
                    { "Organiser" , updatedEvent.Organiser.ToString() },
                    { "Latitude" , updatedEvent.Latitude.ToString() },
                    { "Longitude" , updatedEvent.Longitude.ToString() },
                    { "LocatedIn" , updatedEvent.Location }
                };

                var content = new FormUrlEncodedContent(updateEventData);

                var response = await client.PostAsync(uri, content);

                /*TE: If the response was successful handle the response data.*/
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);


                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    /*TE: See if the server updated the event or not.*/
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                    /*TE: Event update was a success if successOutput == 1*/
                    if (successOutput == "1")
                    {
                        // JA: Save all event images
                        foreach (string i in eventImages)
                        {
                            await SaveEventImageAsync(i, updatedEvent.Id.ToString());
                        }

                        await SaveEventTagsAsync(updatedEvent.Id.ToString(), eventTags, selectedTagCount);
                        /*TE: Return message from server. This will be a success message*/
                        JSONresponse.TryGetValue("message", out var messageOutput);
                        Console.WriteLine(messageOutput);
                        return true;
                    }
                    else
                    {
                        /*TE: Return message from server. This will be an error message*/
                        JSONresponse.TryGetValue("message", out var messageOutput);
                        Console.WriteLine(messageOutput);
                        return false;
                    }
                }
                else
                {
                    /*TE: This will usually be a networking, server or the php file issue.*/
                    Console.WriteLine("Server Connection Failed");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// This function takes all the campuses the user selects from the preferences screen
        /// </summary>
        /// <param name="primaryCampus">A string containing the name of the user's selected primary campus</param>
        /// <param name="secondaryCampuses">An arraylist containing the names of each of the user's selected secondary campuses</param>
        /// <param name="id">A string containing the user's unique ID</param>
        /// <param name="numOfSecondaryCampuses">The numerical value of how many secondary campuses the user selected; makes it easier for the web portal to extract them from the dict in which the data is sent</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task SaveCampus(string primaryCampus, ArrayList secondaryCampuses, string id, int numOfSecondaryCampuses)
        {
            Console.WriteLine("SaveCampus: Begin");
            var uri = new Uri(string.Format(serverAddress + "/api/SaveCampus.php"));

            //JW: dictionary that contains all of the data passed through to this function
            var CampusSubscriptions = new Dictionary<String, String> {
                { "PrimaryCampus", primaryCampus } ,
                { "id", id },
                { "NumOfSecondaryCampuses", numOfSecondaryCampuses.ToString() }
            };

            //JW: for loop that adds all of the secondary campuses into the dictionary
            for (int i = 0; i < numOfSecondaryCampuses; i++)
            {
                CampusSubscriptions.Add("SecondaryCampus" + i, secondaryCampuses[i].ToString());
            }

            //JW: used to test if secondary campuses are stored correctly in the dictionary
            foreach (var pair in CampusSubscriptions)
            {
                Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
            }

            try
            {
                Console.WriteLine("Sending REST");

                var content = new FormUrlEncodedContent(CampusSubscriptions);

                //JW: send content (CampusSubscriptions) to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    // JA: Get the response as JSON
                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    // JW: Check if data was successfully sent to the db
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                    if (successOutput == "1")
                    {
                        Console.WriteLine("Successfully saved the campuses to the db");

                        // JA: Get the required fields from the JSON response
                        JSONresponse.TryGetValue("primaryCampusLongitude", out var primaryCampusLongitudeOutput);
                        JSONresponse.TryGetValue("primaryCampusLatitude", out var primaryCampusLatitudeOutput);

                        Console.WriteLine("Longitude: " + primaryCampusLongitudeOutput);
                        Console.WriteLine("Latitude: " + primaryCampusLatitudeOutput);

                        //JW: Update the session data to use the new campus coordinates
                        Preferences.Set("PrimaryCampusLongitude", primaryCampusLongitudeOutput);
                        Preferences.Set("PrimaryCampusLatitude", primaryCampusLatitudeOutput);
                    }
                    else
                    {
                        Console.WriteLine("Server was unable to save the data to the db");
                    }
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// Queries the database for all the campuses
        /// </summary>
        /// <returns> returns an OnservableCollection containing all the campuses in the database</returns>
        public async Task<ObservableCollection<Campus>> RetrieveCampusList()
        {
            Console.WriteLine("RetrieveCampusList: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/sendCampus.php"));

            ObservableCollection<Campus> campusList = new ObservableCollection<Campus>();

            try
            {
                Console.WriteLine("Sending REST");

                //JW: send uri a request via GET
                var response = await client.GetAsync(uri);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    campusList = JsonConvert.DeserializeObject<ObservableCollection<Campus>>(responseString);
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }

            return campusList;
        }

        /// <summary>
        /// Async function that obtains the campuses that a user has subscribed to and returns them in an observable collection
        /// </summary>
        /// <param name="userId">The user's unique id</param>
        /// <returns>A response message, or an error message if an exception is encountered, or returns the SubscribedCampuses</returns>
        public async Task<ObservableCollection<Campus>> RetrieveSubscribedCampuses(string userId)
        {
            Console.WriteLine("RetrieveSubscribedCampuses: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/RetrieveSubscribedCampuses.php"));

            var id = new Dictionary<string, string> {
                { "id", userId }
            };

            ObservableCollection<Campus> subscribedCampuses = new ObservableCollection<Campus>();

            try
            {
                Console.WriteLine("Sending REST");

                var content = new FormUrlEncodedContent(id);

                //JW: send content (CampusSubscriptions) to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    subscribedCampuses = JsonConvert.DeserializeObject<ObservableCollection<Campus>>(responseString);
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }

            return subscribedCampuses;
        }

        /// <summary>
        /// Async function that sends the request information to all Sustainability Team users
        /// </summary>
        /// <param name="emailAddress">User email making the request</param>
        /// <param name="reason">Reason for the request</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task SendRequestEmail(string emailAddress, string reason)
        {
            var uri = new Uri(string.Format(serverAddress + "/api/sendRequestEmail.php"));

            try
            {
                //Kim: The dictionary contains the user's email address and request reason
                var postData = new Dictionary<string, string> {
                    { "EmailAddress" , emailAddress },
                    { "RequestReason" , reason }
                };

                var content = new FormUrlEncodedContent(postData);

                // JA: Send content (postData) to uri vis POST
                var response = await client.PostAsync(uri, content);

                // JA: Triggered if the api responds
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

       
        /// <summary>
        /// Send email via Json for Forgot Password Feature
        /// </summary>
        /// <param name="email">Email of the User to send Password Reset</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task<string> ForgotPasswordAsync(string email)
        {
            var uri = new Uri(string.Format(serverAddress + "/api/ForgotPassword.php"));

            //Kim: email will be sent in json
            var authData = new Dictionary<string, string> {
                    { "EmailAddress" , email }
            };

            try
            {
                Console.WriteLine("Sending REST");

                var content = new FormUrlEncodedContent(authData);

                //JW: send content (authdata) to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);
                    return "success to connect";

                }
                else
                {
                    Console.WriteLine("Login: Error contacting server");
                    return "Error contacting server";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
                return "Could Not Send Request";
            }
        }

        /// <summary>
        /// Retrieves the tag list.
        /// </summary>
        /// <returns>The tag list.</returns>
        public async Task<ObservableCollection<Tag>> RetrieveTagList()
        {
            Console.WriteLine("RetrieveTagList: Begin");
            var uri = new Uri(string.Format(serverAddress + "/api/sendTag.php"));

            ObservableCollection<Tag> tagList = new ObservableCollection<Tag>();

            try
            {
                Console.WriteLine("Sending REST");

                //JW: send uri a request via GET
                var response = await client.GetAsync(uri);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    tagList = JsonConvert.DeserializeObject<ObservableCollection<Tag>>(responseString);
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }

            return tagList;
        }

        /// <summary>
        /// Saves the selected tags from either the create event or update event functions.
        /// </summary>
        /// <param name="eventID">The Events Unique ID</param>
        /// <param name="eventTags">The List of Tags assosciated with the Event</param>
        /// <param name="selectedTagCount">Number of Tags assosciated with the event</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task SaveEventTagsAsync(string eventID, ArrayList eventTags, int selectedTagCount)
        {

            var uri = new Uri(string.Format(serverAddress + "/api/SaveEventTags.php"));

            try
            {
                //EDH: Make POST Data
                var eventTagsData = new Dictionary<string, string> {
                    { "Event" , eventID.ToString() },
                    { "TagCount" , selectedTagCount.ToString() }
                };

                //EDH: For loop that adds all of the selected tags into the dictionary
                for (int i = 0; i < selectedTagCount; i++)
                {
                    eventTagsData.Add("SelectedTags" + i, eventTags[i].ToString());
                }

                //EDH: Used to test if the selected tags are stored correctly in the dictionary
                foreach (var pair in eventTagsData)
                {
                    Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
                }

                try
                {
                    var content = new FormUrlEncodedContent(eventTagsData);

                    //EDH: Send request and store response
                    var response = await client.PostAsync(uri, content);

                    //EDH: Triggered if the api responds with success or failure
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Got response");
                        Console.WriteLine(responseString);

                        // JA: Get the response as JSON
                        var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                        // JW: Check if data was successfully sent to the db
                        JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                        if (successOutput == "1")
                        {
                            Console.WriteLine("Successfully saved the tags to the db");
                        }
                        else
                        {
                            Console.WriteLine("Server was unable to save the data to the db");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to connect to the server");
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// Update email and push notification notification states in the database
        /// </summary>
        /// <param name="email">Email notification state</param>
        /// <param name="push">Push notification state</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task NotificationToggle(string email, string push)
        {
            Console.WriteLine("NotificationToggle: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/NotificationToggle.php"));

            try
            {
                //Kim: the dictionary contains user id and Email toggle state
                var postData = new Dictionary<string, string> {
                   { "User_ID" , Preferences.Get("UserID", "") },
                   { "EmailSwitch" , email },
                   { "PushSwitch", push }
                };

                //JW: used to test if states are stored correctly in the dictionary
                foreach (var pair in postData)
                {
                    Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
                }

                var content = new FormUrlEncodedContent(postData);

                var response = await client.PostAsync(uri, content);

                // JA: Triggered if the api responds
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// Async function that obtains the user's selected notification types from the db
        /// </summary>
        /// <param name="userId">The user's unique id</param>
        /// <returns>A response message, or an error message if an exception is encountered.</returns>
        public async Task<ObservableCollection<NotificationType>> NotificationTypeSettings(string userId)
        {
            Console.WriteLine("NotificationTypeSettings: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/LoadNotificationSettings.php"));

            var id = new Dictionary<string, string> {
                { "id", userId }
            };

            ObservableCollection<NotificationType> notificationSettings = new ObservableCollection<NotificationType>();

            try
            {
                Console.WriteLine("Sending REST");

                var content = new FormUrlEncodedContent(id);

                //JW: send content to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    notificationSettings = JsonConvert.DeserializeObject<ObservableCollection<NotificationType>>(responseString);
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }

            return notificationSettings;
        }

        /// <summary>
        /// This function sends a request to the server everytime an events details are viewed by a user so we can record their interest in an event.
        /// </summary>
        /// <param name="eventId">The event being viewed</param>
        /// <param name="userId">The user viewing the event</param>
        /// <returns></returns>
        public async Task RegisterEventInterestAsync(int eventId, string userId) {

            Console.WriteLine("Registering Event Interest!");

            /*TE: The php file uri that we send the POST request to on the server.*/
            var uri = new Uri(string.Format(serverAddress + "/api/RegisterEventInterest.php"));

            /*TE: Prepare data to send to server*/
            var interestRequestContent = new Dictionary<string, string> {
                { "eventId", eventId.ToString()},
                { "userId",userId}
            };

            try
            {
                Console.WriteLine("Sending REST Request to server");

                var content = new FormUrlEncodedContent(interestRequestContent);

                //TE: Send request to server.
                var response = await client.PostAsync(uri, content);

                //TE: Check the reponse to see if it was successful or not.
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// Sends a request to the server to close an event
        /// </summary>
        /// <param name="eventToClose">The event as user is closing</param>
        /// <returns>
        /// Returns a string array. First variable is a string boolean and the second is a message for the user.
        /// The boolean allows us to direct the user to another page since the operation was a success or not etc.
        /// Since we have to display multiple messages to the user such as Event Closed, or Event already closed, I am returning a string message as well as a boolean.
        /// </returns>
        public async Task<string[]> CloseEventAsync(Event eventToClose) {

            Console.WriteLine("CloseEventAsync: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/OrganiserCloseEvent.php"));

            /*TE: Try to send the http request*/
            try
            {
                /*TE: We only need the event Id to be sent. No other checks needed at the moment as the application checks to make sure only authorised users have access to "Close Event"*/
                var postData = new Dictionary<string, string> {
                    { "EventId" , eventToClose.Id.ToString() }
                };

                var content = new FormUrlEncodedContent(postData);

                var response = await client.PostAsync(uri, content);

                /*TE: We reached the server and got a response*/
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString);

                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    /*TE: Check if it was successful*/
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);
                    Console.WriteLine(JSONresponse);

                    /*TE: We return true if the update suceeded and false if it did not.*/
                    if (successOutput == "1")
                    {
                        JSONresponse.TryGetValue("message", out var resultMessage);
                        Console.WriteLine(resultMessage);
                        return new string[] { "true", resultMessage.ToString() };
                    }
                    else
                    {
                        JSONresponse.TryGetValue("message", out var resultMessage);
                        Console.WriteLine(resultMessage);
                        return new string[] { "false", "Erorr closing event" };
                    }
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                    return new string[] { "false", "Error contacting server" };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
                return new string[] { "false", "Could not send request" };
            }
        }

        /// <summary>
        /// Sends a request to the server to update an events SuggestedToClose field. This alerts the event organiser to the fact the event might need closing prematurely.
        /// </summary>
        /// <param name="eventToClose">The event as user is suggesting be closed</param>
        /// <returns>
        /// Returns a string array. First variable is a string boolean and the second is a message for the user.
        /// The boolean allows us to direct the user to another page since the operation was a success or not etc.
        /// Since we have to display multiple messages to the user such as Event Closed, or suggestion already sent, Or suggestion has been saved, I am returning a string message as well as a boolean.
        /// </returns>
        public async Task<string[]> SuggestEventClosureAsync(Event eventToClose)
        {

            Console.WriteLine("Suggesting Event be Closed: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/SuggestClosingEvent.php"));

            /*TE: Try to send the http request*/
            try
            {
                /*TE: Closer is the person closing the event.*/
                var postData = new Dictionary<string, string> {
                    { "EventId" , eventToClose.Id.ToString() },
                    { "Closer" , Preferences.Get("UserID", "") },
                    { "OrganiserId" , eventToClose.Organiser.ToString() },
                    { "EventName" , eventToClose.Name }
                };

                var content = new FormUrlEncodedContent(postData);

                var response = await client.PostAsync(uri, content);

                /*TE: We reached the server and got a response*/
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString);

                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    /*TE: Check if it was successful*/
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);
                    Console.WriteLine(JSONresponse);

                    /*TE: We return true if the update suceeded and false if it did not.*/
                    if (successOutput == "1")
                    {
                        JSONresponse.TryGetValue("message", out var resultMessage);
                        Console.WriteLine(resultMessage);
                        return new string[] { "true", "The Event Organiser has been notified!\n\nThanks for your help!" };
                    }
                    else
                    {
                        JSONresponse.TryGetValue("message", out var resultMessage);
                        Console.WriteLine(resultMessage);
                        return new string[] { "false", "Error processing suggestion" };
                    }
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                    return new string[] { "false", "Error contacting server" };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
                return new string[] { "false", "Could not send request" };
            }
        }

        /// <summary>
        /// Retrives all the information about the user and then updated them in the preferences.
        /// </summary>
        public async Task UpdateUserPermissionLevel()
        {
            var currentUser = Preferences.Get("UserID", "-1");
            var uri = new Uri(string.Format(serverAddress + "/api/UserPermissionLevelCheck.php?id=" + currentUser));
            if (currentUser == "-1")
            {
                Console.WriteLine("No user was logged in, tried to update the user permission level.");
            } else {
                try
                {
                    Console.WriteLine("Sending REST");

                    //JW: send uri a request via GET
                    var response = await client.GetAsync(uri);

                    //JW: triggered if the api responds with success or failure
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                        // JA: Check if the request found a valid account matching the provided details
                        JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                        if (successOutput == "1")
                        {
                            JSONresponse.TryGetValue("HasPermissionLevel", out var permissionLevel);
                            JSONresponse.TryGetValue("IsDeleted", out var isDeleted);
                            JSONresponse.TryGetValue("Activated", out var isActivated);

                            if (isDeleted == "1") {
                                // Perform force logout
                                ForceLogoutWithAlert("Your account has been deleted. Please signup again in order to keep using the application.");
                            }
                           
                            if (isActivated == "0")
                            {
                                ForceLogoutWithAlert("Your account has been deactivated.");
                            }

                            Preferences.Set("PermissionLevel", permissionLevel);
                        }
                        else
                        {
                            Console.WriteLine("Response was not able to be used. WasSuccessful 0");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to connect to the server");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR {0}", e.Message);
                }
            }
        }

        private async void ForceLogoutWithAlert(String message)
        {
            await Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
            Xamarin.Forms.Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        /// <summary>
        /// Async function that send session email and verification code for comparison
        /// </summary>
        /// <param name="email">session email</param>
        /// <param name="verificationCode">verification code</param>
        /// <returns>If verification code matches return true, if verification code doesn't match return false </returns>
        public async Task<Boolean> SendVerificationCodeAsync(string email, string verificationCode)
        {
            Console.WriteLine("Send Verification Code Async: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/SendVerificationCode.php"));
            try
            {
                var id = new Dictionary<String, String> {
                    { "Email", email },
                    { "VerificationCode", verificationCode}
                };

                Console.WriteLine(email + verificationCode);

                var content = new FormUrlEncodedContent(id);

                //JW: send content to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);

                    /*TE: We decode the return ed json string into our dictionary.*/
                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    /*TE: We check to see if the pass word update was a success*/
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);
                    Console.WriteLine(JSONresponse);

                    //Kim: save match value
                    if (successOutput == "1")
                    {

                        JSONresponse.TryGetValue("serverMessage", out var serverMessage);
                        return true;
                    }
                    else
                    {

                        JSONresponse.TryGetValue("serverMessage", out var serverMessage);
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// This function sends verification code to the user
        /// </summary>
        /// <param name="email">Session email</param>
        public async Task SendSignupEmailAsync(string email)
        {
            Console.WriteLine("Send Verification Code Async: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/SendSignupEmail.php"));
            try
            {
                var id = new Dictionary<String, String> {
                    { "Email", email }
                };

                var content = new FormUrlEncodedContent(id);

                //JW: send content to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }

        /// <summary>
        /// This function sends a request to the server and allows us to save a users feedback and suggestions that they have provided for the app.
        /// </summary>
        /// <param name="feedbackMessage">The feedback string provided by the user that is sent to the database</param>
        /// <param name="userId">The id of the user sending the feedback</param>
        /// <returns>Returns true if feedback saved successfully, false if it didn't</returns>
        public async Task<Boolean> SendFeedbackAsync(string feedbackMessage, int userId)
        {
            Console.WriteLine("Suggesting Event be Closed: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/SubmitFeedback.php"));

            /*TE: Try to send the http request*/
            try
            {
                /*TE: Send userId to idnetify feedback and the actual feedback*/
                var postData = new Dictionary<string, string> {
                    { "Feedback" , feedbackMessage},
                    { "UserId" , userId.ToString() }
                };

                var content = new FormUrlEncodedContent(postData);

                var response = await client.PostAsync(uri, content);

                /*TE: We reached the server and got a response*/
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseString);

                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    /*TE: Check if it was successful*/
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);
                    Console.WriteLine(JSONresponse);

                    /*TE: We return true if the feedback was saved successfully and false if it was not.*/
                    if (successOutput == "1")
                    {
                        JSONresponse.TryGetValue("message", out var resultMessage);
                        Console.WriteLine(resultMessage);
                        return true;
                    }
                    else
                    {
                        JSONresponse.TryGetValue("message", out var resultMessage);
                        Console.WriteLine(resultMessage);
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Obtains the users primary campus and campus coordinates, and updates their session values.
        /// </summary>
        /// <param name="userId">The user's unique id</param>
        public async Task RetrievePrimaryCampus(string userId)
        {
            Console.WriteLine("RetrievePrimaryCampus: Begin");

            var uri = new Uri(string.Format(serverAddress + "/api/RetrievePrimaryCampus.php"));

            var id = new Dictionary<string, string> {
                { "id", userId }
            };

            try
            {
                Console.WriteLine("Sending REST");

                var content = new FormUrlEncodedContent(id);

                //JW: send content (CampusSubscriptions) to uri via POST
                var response = await client.PostAsync(uri, content);

                //JW: triggered if the api responds with success or failure
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Got response");
                    Console.WriteLine(responseString);


                    // JA: Get the response as JSON
                    var JSONresponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    // JW: Check if the request was successful
                    JSONresponse.TryGetValue("wasSuccessful", out var successOutput);

                    if (successOutput == "1")
                    {
                        Console.WriteLine("Retrieve primary campus data: Success");

                        // JA: Get the required fields from the JSON response
                        JSONresponse.TryGetValue("primaryCampus", out var primaryCampusOutput);
                        JSONresponse.TryGetValue("longitude", out var primaryCampusLongitudeOutput);
                        JSONresponse.TryGetValue("latitude", out var primaryCampusLatitudeOutput);

                        Preferences.Set("PrimaryCampus", primaryCampusOutput);
                        Preferences.Set("PrimaryCampusLongitude", primaryCampusLongitudeOutput);
                        Preferences.Set("PrimaryCampusLatitude", primaryCampusLatitudeOutput);
                    }
                    else
                    {
                        Console.WriteLine("Retrieve primary campus data: Fail");
                        JSONresponse.TryGetValue("message", out var message);
                        Console.WriteLine(message);
                    }
                }
                else
                {
                    Console.WriteLine("Unable to connect to the server");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e.Message);
            }
        }
    }
}