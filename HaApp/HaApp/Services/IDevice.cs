using System;
using System.Collections.Generic;
using System.Text;

namespace HaApp
{
    public interface IDevice
    {
        int GetScreenBrightness();
        void SetScreenBrightness(int brightness);
    }
}
