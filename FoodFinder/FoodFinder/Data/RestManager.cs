using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Text;
using FoodFinder.Models;
using System.Collections;

namespace FoodFinder.Data
{
    /// <summary>
    /// This class manages communication between a class the implements IRestService Interface and other classes in the application that need to send a receive data from a database.
    /// </summary>
    public class RestManager
    {

        /// <summary>
        /// The serice will will reference when calling our methods.
        /// </summary>
        IRestService restService;

        /// <summary>
        /// Create rest manager. Using the restservice setup.
        /// </summary>
        /// <param name="service">Utilises the IRestService Wrapper</param>
        public RestManager(IRestService service)
        {
            restService = service;
        }

        /// <summary>
        /// Calls fucntion to Refresh events in the event list.
        /// Returns an observable collection (list that can trigger events basically) that contains events.
        /// </summary>
        /// <returns>An ObservableCollection of Events</returns>
        public Task<ObservableCollection<Event>> RefreshEventsAsync()
        {
            return restService.RefreshEventsAsync();
        }

        /// <summary>
        /// Sends a request to the server to create a new event.
        /// </summary>
        /// <param name="newEvent">The event to be saved</param>
        /// <param name="eventImages">Collection of the event's iamges</param>
        /// <param name="eventTags">Collection of the event's tags</param>
        /// <param name="selectedTagCount">number of selected tags</param>
        /// <returns></returns>
        public Task<Boolean> CreateNewEvent(Event newEvent, ObservableCollection<String> eventImages, ArrayList eventTags, int selectedTagCount)
        {
            return restService.SaveEventAsync(newEvent, eventImages, eventTags, selectedTagCount);
        }

        /// <summary>
        /// Saves an event's image to the database
        /// </summary>
        /// <param name="eventImage">The image to be saved</param>
        /// <param name="eventID">The associated event's ID</param>
        /// <returns></returns>
        public Task SaveNewEventImage(String eventImage, String eventID)
        {
            return restService.SaveEventImageAsync(eventImage, eventID);
        }

        /// <summary>
        /// Loads a list of an event's images
        /// </summary>
        /// <param name="eventID">Event ID for the images</param>
        /// <returns>An ObservableCollection of images as base64Strings</returns>
        public Task<ObservableCollection<EventImage>> LoadEventImageList(int eventID)
        {
            return restService.LoadEventImageListAsync(eventID);
        }

        /// <summary>
        /// Change The users password
        /// </summary>
        /// <param name="password">Users Password</param>
        /// <param name="newPassword">New Password</param>
        /// <param name="id">User ID</param>
        /// <returns>String array with string boolean and string message</returns>
        public Task<String[]> ChangePassword(string password, string newPassword, string id)
        {
            return restService.ChangePasswordAsync(password, newPassword, id);
        }

        /// <summary>
        /// Creates a new request to become an Event Organiser
        /// </summary>
        /// <param name="userID">ID of the user making the request</param>
        /// <param name="emailAddress">User email making the request</param>
        /// <param name="reason">Reason for the request</param>
        /// <returns>restService.CreateRequestAsync</returns>
        public Task CreateRequest(int userID, string emailAddress, string reason)
        {
            return restService.CreateRequestAsync(userID, emailAddress, reason);
        }

        /// <summary>
        /// Login Task
        /// </summary>
        /// <param name="email">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <returns>restService.LoginAsync</returns>
        public Task<String> Login(string email, string password)
        {
            return restService.LoginAsync(email, password);
        }

        /// <summary>
        /// Signup Task
        /// </summary>
        /// <param name="email">Email for the User to Sign up With</param>
        /// <param name="password">Password that the User wants to use.</param>
        /// <param name="campus">Users Initial Primary Campus (Of their Choosing)</param>
        /// <returns>restService.SignupAsync</returns>
        public Task<String> Signup(string email, string password, string campus)
        {
            return restService.SignupAsync(email, password, campus);
        }

        /// <summary>
        /// Fucntion to update event
        /// </summary>
        /// <param name="updateEvent">Event to be updated</param>
        /// <param name="eventTags">List of Tags assosciated with the event.</param>
        /// <param name="selectedTagCount">Number of Selected Tags</param>
        /// <param name="eventImages">Collection of the event's iamges</param>
        /// <returns>restService.UpdateEventAsync</returns>
        public Task<Boolean> UpdateEventAsync(Event updateEvent, ArrayList eventTags, int selectedTagCount, ObservableCollection<String> eventImages)
        {
            return restService.UpdateEventAsync(updateEvent, eventTags, selectedTagCount, eventImages);
        }

        /// <summary>
        /// Saves the Campuses that have been selected in the Preferences.
        /// </summary>
        /// <param name="primaryCampus">Primary Campus to be saved. </param>
        /// <param name="secondaryCampuses">List of Secondary Campuses that the user is subscribed too. </param>
        /// <param name="id">User ID</param>
        /// <param name="numOfSecondaryCampuses"> Number of Secondary Campuses.</param>
        /// <returns>restService.SaveCampus</returns>
        public Task SaveCampus(string primaryCampus, ArrayList secondaryCampuses, string id, int numOfSecondaryCampuses)
        {
            return restService.SaveCampus(primaryCampus, secondaryCampuses, id, numOfSecondaryCampuses);
        }

        /// <summary>
        /// Retrieves the subscribed campuses for the specified user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>restService.RetrieveSubscribedCampuses</returns>
        public Task<ObservableCollection<Campus>> RetrieveSubscribedCampuses(string userId)
        {
            return restService.RetrieveSubscribedCampuses(userId);
        }

        /// <summary>
        /// Retrieves the list of all campuses.
        /// </summary>
        /// <returns>Observalbe Collection (Type: List) of Campuses</returns>
        public Task<ObservableCollection<Campus>> RetrieveCampusList()
        {
            return restService.RetrieveCampusList();
        }

        /// <summary>
        /// Sends the Become an Event Organiser Reason Email.
        /// </summary>
        /// <param name="emailAddress">User email making the request</param>
        /// <param name="reason">Reason for the request</param>
        /// <returns>restService.SendRequestEmail</returns>
        public Task SendRequestEmail(string emailAddress, string reason)
        {
            return restService.SendRequestEmail(emailAddress, reason);
        }

        /// <summary>
        /// Task to Retrieve Tag List
        /// </summary>
        /// <returns>An Observable Collection of Tags</returns>
        public Task<ObservableCollection<Tag>> RetrieveTagList()
        {
            return restService.RetrieveTagList();
        }

        /// <summary>
        /// Save Tags Associated with the Event
        /// </summary>
        /// <param name="eventID">Unique Event Identifier</param>
        /// <param name="eventTags">List of Tags Assosciated with the Event</param>
        /// <param name="selectedTagCount">Number of Tags Assosciated with the Event</param>
        /// <returns>restService.SaveEventTagAsync</returns>
        public Task SaveEventTags(string eventID, ArrayList eventTags, int selectedTagCount)
        {
            return restService.SaveEventTagsAsync(eventID, eventTags, selectedTagCount);
        }

        /// <summary>
        /// Changes password if user forgot password
        /// </summary>
        /// <param name="email">Users email</param>
        /// <returns>restService.ForgotPasswordAsync</returns>
        public Task<String> ForgotPassword(string email)
        {
            return restService.ForgotPasswordAsync(email);
        }

        /// <summary>
        /// Toggles the Notification Options
        /// </summary>
        /// <param name="email">Contains true/false value/state of the email switch in the preferences</param>
        /// <param name="push">Contains true/false value/state of the push notification switch in the preferences</param>
        /// <returns>Task</returns>
        public Task NotificationToggle(string email, string push)
        {
            return restService.NotificationToggle(email, push);
        }

        /// <summary>
        /// Ontains the users notification settings from the database
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Obersvable Collection of Notification type</returns>
        public Task<ObservableCollection<NotificationType>> NotificationTypeSettings(string userId)
        {
            return restService.NotificationTypeSettings(userId);
        }

        /// <summary>
        /// Asynchronous Task to send Event Details via email.
        /// </summary>
        /// <param name="EmailEvent">Event to Email</param>
        /// <param name="eventID">Unique Event ID</param>
        /// <returns>Task</returns>
        public Task SendEventEmail(Event EmailEvent, string eventID)
        {
            return restService.SendEventEmailAsync(EmailEvent, eventID);
        }


        /// <summary>
        /// This allows us to register a users interest in an event.
        /// </summary>
        /// <param name="eventId">The event's id</param>
        /// <param name="userId">The user's id</param>
        /// <returns>Task to Register Interest in an Event</returns>
        public Task RegisterEventInterest(int eventId,string userId)
        {
            return restService.RegisterEventInterestAsync(eventId, userId);
        }

        /// <summary>
        /// Allows a user to close an event.
        /// </summary>
        /// <param name="eventToClose">The event the user is closing</param>
        /// <returns>Returns a string array. First variable is a string boolean and the second is a message for the user.</returns>
        public Task<String[]> CloseEvent(Event eventToClose) {
            return restService.CloseEventAsync(eventToClose);
        }

        /// <summary>
        /// Allows a user suggest the event be closed.
        /// </summary>
        /// <param name="eventToClose">The event the user is suggesting should be closed</param>
        /// <returns>Returns a string array. First variable is a string boolean and the second is a message for the user.</returns>
        public Task<String[]> SuggestEventClosure(Event eventToClose)
        {
            return restService.SuggestEventClosureAsync(eventToClose);
        }

        /// <summary>
        /// Retrives all the information about the user and then updated them in the preferences.
        /// </summary>
        public async Task UpdateUserPermissionLevel()
        {
            await restService.UpdateUserPermissionLevel();
        }

        /// <summary>
        /// This function sends a request to the server and allows us to save a users feedback and suggestions that they have provided for the app.
        /// </summary>
        /// <param name="feedbackMessage">The feedback string provided by the user that is sent to the database</param>
        /// <param name="userId">The id of the user sending the feedback</param>
        /// <returns>Returns true if feedback saved successfully, false if it didn't</returns>
        public Task<Boolean> SaveFeedback(string feedbackMessage, int userId) {
            return restService.SendFeedbackAsync(feedbackMessage,userId);
        }

        /// <summary>
        /// Asynchronous Task to send verification code to API for comparison
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="verificationCode">The unique verification code</param>
        /// <returns>Task</returns>
        public Task<Boolean> SendVerificationCode(string email, string verificationCode)
        {
            return restService.SendVerificationCodeAsync(email, verificationCode);
        }

        /// <summary>
        /// Obtains the users primary campus and campus coordinates, and updates their session values.
        /// </summary>
        /// <param name="userId"></param>
        public Task RetrievePrimaryCampus(string userId)
        {
            return restService.RetrievePrimaryCampus(userId);
        }

        /// <summary>
        /// Sends verification code to the user
        /// </summary>
        /// <param name="email">The user's email address</param>
        public Task SendSignupEmailAsync(string email)
        {
            return restService.SendSignupEmailAsync(email);
        }
    }
}
