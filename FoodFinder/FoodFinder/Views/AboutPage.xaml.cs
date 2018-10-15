using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using System.Reflection;

namespace FoodFinder.Views
{
    /// <additional_information>
    /// Authors: Tim Anderson, John Aslin, Eli den Hartog
    /// Created: 03/05/2018
    /// Date modified: 1/8/2018
    /// Modified by: Eli den Hartog
    /// Modify Reason: 
    /// </additional_information>

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        /// <summary>
        /// Default constructor function, initialises all components on the about page,
        /// reads in the about screen text file. So this can be changed later by the sustainability Team. 
        /// </summary>
        public AboutPage()
        {
            InitializeComponent();

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(AboutPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("FoodFinder.Resources.aboutSusTeam.txt");
            
            string text = "";
            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            aboutText.Text = text;
        }

        /// <summary>
        /// Opens the URL to the Sustainability Team Website once the button is clicked.
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void SustainabilityWebsiteLink(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://www.utas.edu.au/sustainability/"));
        }

        /// <summary>
        /// Opens the users webmail client, and preloads an email to the Sustainability Team
        /// </summary>
        /// <param name="sender">Object Data Sender</param>
        /// <param name="e">Event Handler</param>
        void SustainabilityEmailLink(object sender, EventArgs e)
        {
            string susTeamEmailAddress = "sustainability.utas@utas.edu.au";
            Device.OpenUri(new Uri($"mailto:{susTeamEmailAddress}"));
        }
    }
}