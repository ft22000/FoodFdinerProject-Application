using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace FoodFinder.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash",MainLauncher =true,NoHistory =true)]
    public class SplashActivity : AppCompatActivity
    {

        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");
            // Create your application here
        }

        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();
        }

        async void SimulateStartup()
        {
            Log.Debug(TAG, "Performing some startupwork that takes time.");
            await Task.Delay(2000); //simulate the startup work.
            Log.Debug(TAG, "Startup work is finished, starting Main Activity");
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}