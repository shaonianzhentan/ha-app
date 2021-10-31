using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(HaApp.Droid.DeviceService))]
namespace HaApp.Droid
{
    public class DeviceService : IDevice
    {
        public int GetScreenBrightness()
        {
            return Settings.System.GetInt(Android.App.Application.Context.ContentResolver, Settings.System.ScreenBrightness);
        }

        public void SetScreenBrightness(int brightness)
        {
            Settings.System.PutInt(Android.App.Application.Context.ContentResolver, Settings.System.ScreenBrightness, brightness);
        }
    }
}