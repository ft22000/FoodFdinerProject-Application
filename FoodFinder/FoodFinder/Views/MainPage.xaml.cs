using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FoodFinder.Data;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace FoodFinder.Views
{
    /// <summary>
    /// The Main Page Class for the MasterDetailPage
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        /// <summary>
        /// Main Page default Constructor function, initialises all components contained within this class and the assosciated XAML code
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            //TA calls the event listener as the button is interacted it.
            MasterPage.ListView.ItemSelected += (sender, e) => OnItemSelected(sender, e);

            this.IsPresentedChanged += (sender, e) => OnStateChanged(sender, e);

        }

        /// <summary>
        /// Handles taps on the item in the Hamburger Menu.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //item is the selected item from the list(a button) -- TA
            if (e.SelectedItem is MasterPageItem item)
            {

                if (item.TargetType != null && item.TargetType != typeof(LoginPage))
                {

                    IsPresented = false;

                    //loads the page of a targetType (i.e, views: AboutPage, views: EventsListPage) -- TA
                    Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType))
                    {
                    };

                    //Retracts the Hamburger List from view -- TA

                    //resets the selected item to null -- TA
                    MasterPage.ListView.SelectedItem = null;

                }
                else if (item.TargetType == typeof(LoginPage))
                {

                    App.Current.MainPage = new NavigationPage(new LoginPage());
                }
                else
                {

                    MasterPage.ListView.SelectedItem = false;
                }
            }
            MasterPage.ListView.SelectedItem = null;
        }
        
        /// <summary>
        /// Method for responding to when the page is change to be displayed or not
        ///
        /// Updates the collection in order to ensure that the create event button is displayed at the appropriate time
        /// </summary>
        public async void OnStateChanged(object sender, EventArgs e)
        {
            await App.RestManager.UpdateUserPermissionLevel();
            MasterPage.UpdateShowCreateEventOption();
        }
    }
}
