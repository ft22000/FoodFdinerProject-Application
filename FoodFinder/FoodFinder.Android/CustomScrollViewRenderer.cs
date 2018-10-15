using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using FoodFinder.Droid;
using Android.Content;

[assembly: ExportRenderer(typeof(ScrollView), typeof(CustomScrollViewRenderer))]
namespace FoodFinder.Droid
{
    /// <summary>
    /// Extends the ScrollView renderer in Xamarin.Forms. This allows customisation of the appearance and behaviour of the ScrollView.
    /// The primary addition over the standard renderer is the ability to toggle the visability of the ScrollBar.
    /// </summary>
    class CustomScrollViewRenderer : ScrollViewRenderer
    {
        /// <summary>
        /// Constructor for the custom renderer.
        /// </summary>
        /// <param name="context">The content to be rendered.</param>
        public CustomScrollViewRenderer(Context context) : base(context)
        {
        }

        /// <summary>
        /// Event handler for when an element is changed.
        /// </summary>
        /// <param name="e">The element to be changed.</param>
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            if (e.OldElement != null)
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;

            e.NewElement.PropertyChanged += OnElementPropertyChanged;
        }

        /// <summary>
        /// Event handler for when an element's property is changed.
        /// </summary>
        /// <param name="sender">The object triggering this function.</param>
        /// <param name="e">The element to be changed.</param>
        protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ScrollBarFadeDuration = 1000;
            ScrollBarDefaultDelayBeforeFade = 0;
        }
    }
}