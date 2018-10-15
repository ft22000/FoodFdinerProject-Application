using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using Xamarin.Essentials;
using System.Collections.ObjectModel;

namespace FoodFinder
{
    /// <summary>
    /// The Master Page gets and returns the list view.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterPage : ContentPage
    {
        /// <summary>
        /// Gets and Returns the listView used by the hamburger menu.
        /// </summary>
        public ListView ListView { get { return listView; } }
        ListView listView;
        private ObservableCollection<MasterPageItem> masterPageItems;
        private bool showCreateEventOption = false;

        /// <summary>
        /// Default Constructor function, also inits the Master Page and hamburger menu. A different menu is created depending on the permission level
        /// of the use.
        /// </summary>
        public MasterPage()
        {
            // Initialise the list and component
            StackLayout headerStackLayout;
            masterPageItems = new ObservableCollection<MasterPageItem>();
            InitializeComponent();

            // Add items to the master detail page, one for each of the pages that we need to be able to access
            //masterPageItems.Add(new MasterPageItem { Title = "Food Finder", Name = "Title", IconSource = "icon.png"});
            masterPageItems.Add(new MasterPageItem { Title = "Map", Name = "Maps Button", IconSource = "mapIcon.png", TargetType = typeof(Views.MapPage) });
            masterPageItems.Add(new MasterPageItem { Title = "Events", Name = "EventsButton", IconSource = "eventIcon.png", TargetType = typeof(Views.EventListPage) });



            masterPageItems.Add(new MasterPageItem { Title = "Preferences", Name = "PreferencesButton", IconSource = "preferencesIcon.png", TargetType = typeof(Views.PreferencePage) });
            masterPageItems.Add(new MasterPageItem { Title = "About", Name = "AboutButton", IconSource = "aboutIcon.png", TargetType = typeof(Views.AboutPage) });
            masterPageItems.Add(new MasterPageItem { Title = "Feedback", Name = "LeaveFeedback", IconSource = "feedback.png", TargetType = typeof(Views.FeedbackPage) });
            masterPageItems.Add(new MasterPageItem { Title = "Logout", Name = "Logout", IconSource = "logoutIcon.png", TargetType = typeof(Views.LoginPage) });

            CheckPermissionLevel();

            // JA: Checks if the current user already has event organiser permissions or higher
            if (showCreateEventOption)
            {
                AddCreateEventButtonToList();
            }

            headerStackLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(5,10),
            };

            headerStackLayout.Children.Add(new Image { Source = "icon.png", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, WidthRequest = 50 } );
            headerStackLayout.Children.Add(new Label { Text = "Food Finder", HorizontalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold ,VerticalOptions = LayoutOptions.Center });

            // Create the list view.
            listView = new ListView
            {

                ItemsSource = masterPageItems,
                Header = headerStackLayout,
                ItemTemplate = new DataTemplate(() =>
               {
                   var grid = new Grid { Padding = new Thickness(5, 10) };
                   grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
                   grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                   var image = new Image();
                   image.SetBinding(Image.SourceProperty, "IconSource");
                   var label = new Label { VerticalOptions = LayoutOptions.FillAndExpand };
                   label.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof (Label));
                   label.SetBinding(Label.TextProperty, "Title");

                   grid.Children.Add(image);
                   grid.Children.Add(label, 1, 0);

                   return new ViewCell { View = grid };
               }),
                SeparatorVisibility = SeparatorVisibility.Default,
                Footer = ""
            };

            Icon = "hamburger.png";
            Title = "Food Finder";
            Content = new StackLayout
            {
                Children = { listView }
            };
        }

        /// <summary>
        /// Checks and updates whether the menu should display the create event option for the current user.
        /// </summary>
        private void CheckPermissionLevel()
        {
            if (Int32.Parse(Preferences.Get("PermissionLevel", "")) > Preferences.Get("GENERAL_USER", 1))
            {
                showCreateEventOption = true;
            }
            else
            {
                showCreateEventOption = false;
            }
        }

        /// <summary>
        /// Checks if the permission level of the user has changed, and if it has handles the addition or removal of the Create Event button from the collection.
        /// </summary>
        public void UpdateShowCreateEventOption()
        {
            var currentState = showCreateEventOption;

            // This method call will update the value of the showCreateEventOption variable
            CheckPermissionLevel();

            // Check if they have changed
            if (currentState != showCreateEventOption)
            {
                if (showCreateEventOption)
                {
                     AddCreateEventButtonToList();
                }
                else
                {
                    RemoveCreateEventButtonFromList();
                }
            }
        }

        /// <summary>
        /// Adds the create event button to the observable collection in the correct position.
        /// </summary>
        public void AddCreateEventButtonToList()
        {
            var indexForAdd = -1;

            for (int i = 0; i < masterPageItems.Count; i++)
            {
                if (masterPageItems[i].Title == "Events")
                {
                    indexForAdd = i;
                    break;
                }
            }
            if (indexForAdd < masterPageItems.Count-1 && indexForAdd != -1)
            {
                indexForAdd++;
                masterPageItems.Insert(indexForAdd ,new MasterPageItem { Title = "Create Event", Name = "EventsCreationButton", IconSource = "addEvent.png", TargetType = typeof(Views.EventCreationPage) });
            }
        }

        /// <summary>
        /// Removes the create event button from the observable collection, where ever it may be.
        /// </summary>
        public void RemoveCreateEventButtonFromList()
        {
            var index = -1;
            for (int i = 0; i < masterPageItems.Count; i++)
            {
                if (masterPageItems[i].Title == "Create Event")
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0){
                masterPageItems.RemoveAt(index);
            }
        }
    }
}
