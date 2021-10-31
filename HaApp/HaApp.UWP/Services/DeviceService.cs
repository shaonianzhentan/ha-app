using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(HaApp.UWP.DeviceService))]
namespace HaApp.UWP
{
    public class DeviceService : IDevice
    {
        public int GetScreenBrightness()
        {
            return 255;
        }

        public void SetScreenBrightness(int brightness)
        {

        }
    }
}
