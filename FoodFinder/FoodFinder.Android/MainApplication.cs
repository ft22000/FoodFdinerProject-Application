using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.FirebasePushNotification;
using Plugin.CurrentActivity;

/* 
 * JW: This .cs file is used to initialise the firebase push notification. This will also keep the connection with firebase open while the application is closed.
 * For further information, see: https://github.com/CrossGeeks/FirebasePushNotificationPlugin/blob/master/docs/GettingStarted.md#android-initialization
 */

namespace FoodFinder.Droid
{
    [Application]
    public class MainApplication : Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            /* JW: Sets the default notification channel for your app when running Android Oreo
             * 
             *     The code examples on github put this at the bottom of the MainApplication page,
             *     however, this prevents oreo users from receiving push notifications while the 
             *     app is in the foreground. 
             *   
             *     if this is placed /before/ the initialisation, it will allow oreo users to
             *     recieve notifications while the app is in the foreground, making the notification
             *     behaviour more consistent with previous versions of android.
             */
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                //JW: Change for your default notification channel id here
                FirebasePushNotificationManager.DefaultNotificationChannelId = "DefaultChannel";

                //JW: Change for your default notification channel name here
                FirebasePushNotificationManager.DefaultNotificationChannelName = "General";
            }

            /*
             * JW: Initialise the plugin. If in debug mode, the token should be reset on each run. In release, 
             *     only one token will be generated for each device. (tokens register the device to firebase).
             */
#if DEBUG
            FirebasePushNotificationManager.Initialize(this, true);
#else
            FirebasePushNotificationManager.Initialize(this,false);
#endif

            //JW: Handle notification when app is closed here
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {


            };
            //JA: Initialised the CrossCurrentActvity plugin. This is required as part of the Xam.Plugin.Media plugin.
            CrossCurrentActivity.Current.Init(this);
        }
    }
}