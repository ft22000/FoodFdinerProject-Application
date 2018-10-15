using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using Plugin.FirebasePushNotification;
using UIKit;

namespace FoodFinder.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
            global::Xamarin.Forms.Forms.Init();
			CarouselView.FormsPlugin.iOS.CarouselViewRenderer.Init();
			LoadApplication(new App());

			/*
             * JW: Initialises the firebase plugin on iOS devices.
             * For further information, see: https://github.com/CrossGeeks/FirebasePushNotificationPlugin/blob/master/docs/GettingStarted.md#ios-initialization
             */
			FirebasePushNotificationManager.Initialize(options, true);

			return base.FinishedLaunching(app, options);
		}

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			System.Console.WriteLine("Registered for remote notifications!" + deviceToken);
			FirebasePushNotificationManager.DidRegisterRemoteNotifications(deviceToken);
		}

		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			System.Console.WriteLine("Failed registered for remote notifications!" + error.DebugDescription);
			FirebasePushNotificationManager.RemoteNotificationRegistrationFailed(error);

		}
		// To receive notifications in foregroung on iOS 9 and below.
		// To receive notifications in background in any iOS version
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			// If you are receiving a notification message while your app is in the background,
			// this callback will not be fired 'till the user taps on the notification launching the application.

			// If you disable method swizzling, you'll need to call this method. 
			// This lets FCM track message delivery and analytics, which is performed
			// automatically with method swizzling enabled.
			FirebasePushNotificationManager.DidReceiveMessage(userInfo);
			// Do your magic to handle the notification data
			System.Console.WriteLine(userInfo);
		}

		public override void OnActivated(UIApplication uiApplication)
		{
			FirebasePushNotificationManager.Connect();

		}
		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
			FirebasePushNotificationManager.Disconnect();
		}
	}
}
