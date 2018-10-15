using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FoodFinder.Models;


namespace FoodFinder.Data
{

    /// <summary>
    /// IRest implements the following Database Request Functions. Acts as a wrapper for the Rest Service.
    /// </summary>
    public interface IRestService
    {
        /// <summary>
        /// Returns an observable collection (list that can trigger events basically) that contains events.
        /// </summary>
        /// <returns>An Obersvable Collection of Type Event</returns>
        Task<ObservableCollection<Event>> RefreshEventsAsync();

        /// <summary>
        /// Asynchronous Task to create a new request to become an Event Organiser.
        /// </summary>
        /// <param name="userID">ID of the user making the request</param>
        /// <param name="emailAddress">User email making the request</param>
        /// <param name="reason">Reason for the request</param>
        Task CreateRequestAsync(int userID, string emailAddress, string reason);

        /// <summary>
        /// Sends a request to the server to create a new event.
        /// </summary>
        /// <param name="newEvent">The event to be saved</param>
        /// <param name="eventImages">Collection of the event's images</param>
        /// <param name="eventTags">Collection of the event's tags</param>
        /// <param name="selectedTagCount">number of selected tags</param>
        /// <returns>Boolean</returns>
        Task<Boolean> SaveEventAsync(Event newEvent, ObservableCollection<String> eventImages, ArrayList eventTags, int selectedTagCount);

        /// <summary>
        /// Saves an event's image to the database
        /// </summary>
        /// <param name="eventImage">The image to be saved</param>
        /// <param name="eventID">The associated event's ID</param>
        /// <returns>Task</returns>
        Task SaveEventImageAsync(String eventImage, String eventID);

        /// <summary>
        /// Load a list of an event's images
        /// </summary>
        /// <param name="eventID">Event ID for the images</param>
        /// <returns>An ObservableCollection of images as base64Strings</returns>
        Task<ObservableCollection<EventImage>> LoadEventImageListAsync(int eventID);

        /// <summary>
        /// Asyncrhonous Task for Logging into the Application
        /// </summary>
        /// <param name="email">The email of the accoun to log in.</param>
        /// <param name="password">The password of the account to log in.</param>
        /// <returns>String</returns>
        ///
        Task<String> LoginAsync(string email, string password);

        /// <summary>
        /// Processes a signup request
        /// </summary>
        /// <param name="email">The email that the user wants to use to sign up. Must be a @utas.edu.au email.</param>
        /// <param name="password">The password that the user wants to use.</param>
        /// <param name="campus">The Primary campus that the user wants to set.</param>
        /// <returns>String</returns>
        Task<String> SignupAsync(string email, string password, string campus);

        /// <summary>
        /// Function to update event
        /// </summary>
        /// <param name="eventToUpdate">The event that will be updated.</param>
        /// <param name="eventTags">The descriptor tags associated with the event.</param>
        /// <param name="selectedTagCount">The number of descriptor tags. </param>
        /// <param name="eventImages">Collection of the event's images</param>
        /// <returns>Boolean</returns>
        Task<Boolean> UpdateEventAsync(Event eventToUpdate, ArrayList eventTags, int selectedTagCount, ObservableCollection<String> eventImages);

        /// <summary>
        /// The task for changing the users password.
        /// </summary>
        /// <param name="password">The users current Password</param>
        /// <param name="newPassword">The new password that the user wishes to change to.</param>
        /// <param name="id">User ID</param>
        /// <returns>String array with string boolean and string message</returns>
        Task<String[]> ChangePasswordAsync(string password, string newPassword, string id);

        /// <summary>
        /// The task for Retrieving the users subscribed campuses.
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <returns>Observable Collection of Subscribed Campuses</returns>
        Task<ObservableCollection<Campus>> RetrieveSubscribedCampuses(string userId);

        /// <summary>
        /// Task to Save the selected campuses.
        /// </summary>
        /// <param name="primaryCampus">The string to represent the users primary campus.</param>
        /// <param name="secondaryCampuses">The list of secondary campuses.</param>
        /// <param name="id">User ID</param>
        /// <param name="numOfSecondaryCampuses">The number of secondary campuses.</param>
        /// <returns>Task</returns>
        Task SaveCampus(string primaryCampus, ArrayList secondaryCampuses, string id, int numOfSecondaryCampuses);

        /// <summary>
        /// Task to retrieve the List of Campuses.
        /// </summary>
        /// <returns>Obersvable Collection of Campuses</returns>
        Task<ObservableCollection<Campus>> RetrieveCampusList();

        /// <summary>
        /// Sends the request information to all Sustainability Team users
        /// </summary>
        /// <param name="emailAddress">User email making the request</param>
        /// <param name="reason">Reason for the request</param>
        /// <returns>Task</returns>
        Task SendRequestEmail(string emailAddress, string reason);

        /// <summary>
        /// Retrieves the List of Tags
        /// </summary>
        /// <returns>An Observable collection of Type Tag List</returns>
        Task<ObservableCollection<Tag>> RetrieveTagList();

        /// <summary>
        /// Saves the Event.
        /// </summary>
        /// <param name="eventID">The Events unique ID</param>
        /// <param name="eventTags">List of Tags associated with the event.</param>
        /// <param name="selectedTagCount">Number of Selected Tags</param>
        /// <returns>Task</returns>
        Task SaveEventTagsAsync(string eventID, ArrayList eventTags, int selectedTagCount);

        /// <summary>
        /// Asynchronous Task for Forgotten Password Features
        /// </summary>
        /// <param name="email">User's Email</param>
        /// <returns>Task</returns>
        Task<String> ForgotPasswordAsync(string email);

        /// <summary>
        /// Toggles Notifications
        /// </summary>
        /// <param name="email">Contains true/false value/state of the email switch in the preferences</param>
        /// <param name="push">Contains true/false value/state of the push notification switch in the preferences</param>
        /// <returns>Task</returns>
        Task NotificationToggle(string email, string push);

        /// <summary>
        /// Ontains the users notification settings from the database
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Obersvable Collection of Notification type</returns>
        Task<ObservableCollection<NotificationType>> NotificationTypeSettings(string userId);


        /// <summary>
        /// Asynchronous Task to send Event Details via email.
        /// </summary>
        /// <param name="EmailEvent">Event to Email</param>
        /// <param name="eventID">Event's Unique Identifier.</param>
        /// <returns>Task</returns>
        Task SendEventEmailAsync(Event EmailEvent, string eventID);


        /// <summary>
        /// This sends a reqest to the server to register a user's interest in an event.
        /// </summary>
        /// <param name="eventId">Unique Event ID</param>
        /// <param name="userId">Unique User ID</param>
        /// <returns></returns>
        Task RegisterEventInterestAsync(int eventId, string userId);

        /// <summary>
        /// This sends a request to the server to close an event
        /// </summary>
        /// <param name="eventToClose">The event being closed</param>
        /// <returns>Returns a string array. First variable is a string boolean and the second is a message for the user.</returns>
        Task<String[]> CloseEventAsync(Event eventToClose);

        /// <summary>
        /// This sends a request to server to sughest an event be closed
        /// </summary>
        /// <param name="eventToClose">The event that a user is suggesting being closed</param>
        /// <returns>Returns a string array. First variable is a string boolean and the second is a message for the user.</returns>
        Task<String[]> SuggestEventClosureAsync(Event eventToClose);

        /// <summary>
        /// Checks the current users permission level against the DB
        /// </summary>
        Task UpdateUserPermissionLevel();

        /// <summary>
        /// This function sends a request to the server and allows us to save a users feedback and suggestions that they have provided for the app.
        /// </summary>
        /// <param name="feedbackMessage">The feedback string provided by the user that is sent to the database</param>
        /// <param name="userId">The id of the user sending the feedback</param>
        /// <returns>Returns true if feedback saved successfully, false if it didn't</returns>
        Task<Boolean> SendFeedbackAsync(string feedbackMessage, int userId);

        /// <summary>
        /// Asynchronous Task to send verification code to API for comparison
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="verificationCode">Verificaition code</param>
        /// <returns>If verification code matches return true, if verification code doesn't match return false</returns>
        Task<Boolean> SendVerificationCodeAsync(string email, string verificationCode);

        /// <summary>
        /// Asynchronous Task to send verification code to the  user
        /// </summary>
        /// <param name="email">The user's email address</param>
        Task SendSignupEmailAsync(string email);

        /// <summary>
        /// Obtains the users primary campus and campus coordinates, and updates their session values.
        /// </summary>
        /// <param name="userId"></param>
        Task RetrievePrimaryCampus(string userId);
    }
}
