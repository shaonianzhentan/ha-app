using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Xamarin.Essentials;
using Xamarin.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HaApp
{
    public class MqttDevice
    {
        MqttHA mqttHA { get; set; }
        Dictionary<string, string> dictScreen;
        Dictionary<string, string> dictLightSensor;
        Dictionary<string, string> dictBattery;

        DelayAction delayAction = new DelayAction();

        public MqttDevice(MqttHA mqtt)
        {
            this.mqttHA = mqtt;
            this.Connect();
        }

        void Connect()
        {
            mqttHA.Connect(mqttEvent =>
            {
                this.PublishConfig();
                // 自动发现
                mqttHA.Subscribe("homeassistant/status", (payload) =>
                {
                    if (payload == "online")
                    {
                        this.PublishConfig();
                    }
                });
                // 屏幕亮度
                int brightness = 200;
                mqttHA.Subscribe(dictScreen["command"], (string payload) =>
                {
                    var device = DependencyService.Get<IDevice>();
                    if (payload == "OFF")
                    {
                        brightness = device.GetScreenBrightness();
                        device.SetScreenBrightness(1);
                    }
                    else
                    {
                        // 如果亮度最低，则设置                        
                        if (device.GetScreenBrightness() == 1)
                        {
                            device.SetScreenBrightness(brightness);
                        }
                    }
                    this.PublishInfo();
                });
                mqttHA.Subscribe(dictScreen["brightness_command"], (string payload) =>
                {
                    DependencyService.Get<IDevice>().SetScreenBrightness(Convert.ToInt32(payload));
                    this.PublishInfo();
                });

                mqttHA.Subscribe(mqttHA.ip + "/app", async (string payload) =>
                {
                    var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                    var p = deserializer.Deserialize<Dictionary<string, string>>(payload);
                    if (p.ContainsKey("clipboard"))
                    {
                        await Clipboard.SetTextAsync(p["clipboard"]);
                    }

                    if (p.ContainsKey("url"))
                    {
                        string url = p["url"];
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Shell.Current.GoToAsync($"//BrowserPage?url={HttpUtility.UrlEncode(url)}");
                        });
                    }

                    if (p.ContainsKey("tts"))
                    {
                        await TextToSpeech.SpeakAsync(p["tts"]);
                    }
                });

                this.PublishInfo();

            }, (disEvent) =>
            {
                // log("连接中断了");
            });
        }

        void PublishConfig()
        {
            var idiom = DeviceInfo.Idiom;
            switch (idiom.ToString())
            {
                case "Phone":
                    dictLightSensor = mqttHA.ConfigSensor("光照传感器", "lx", "illuminance");
                    dictScreen = mqttHA.ConfigLight("我的手机");
                    dictBattery = mqttHA.ConfigSensor("手机电量", "%", "battery");
                    break;
                case "Tablet":
                    dictScreen = mqttHA.ConfigLight("我的平板");
                    dictBattery = mqttHA.ConfigSensor("平板电量", "%", "battery");
                    break;
                case "Desktop":
                    dictScreen = mqttHA.ConfigLight("我的电脑");
                    dictBattery = mqttHA.ConfigSensor("电脑电量", "%", "battery");
                    break;
                case "TV":
                    break;
                case "Watch":
                    break;
            }
        }

        void PublishInfo()
        {
            delayAction.Delay(3000, null, async () =>
            {
                // 如果未连接，则检测端口重新连接
                if (!mqttHA.mqttClient.IsConnected)
                {
                    string ip = await SecureStorage.GetAsync("mqtt");
                    // 如果端口打开，则进行重连
                    if (mqttHA.TcpClientCheck(ip, 1883))
                    {
                        this.Connect();
                    }
                    return;
                }

                var device = DependencyService.Get<IDevice>();
                // 屏幕亮度
                mqttHA.Publish(dictScreen["brightness"], device.GetScreenBrightness().ToString());
                // 电量
                mqttHA.Publish(dictBattery["state"], (Battery.ChargeLevel * 100).ToString("F0"));
                Dictionary<string, string> dictBatteryAttributes = new Dictionary<string, string>();
                // 充电状态
                string[] batteryState = new string[] { "Unknown", "Charging", "Discharging", "Full", "NotCharging", "NotPresent" };
                dictBatteryAttributes.Add("battery_state", batteryState[(int)Battery.State]);
                // 电源状态
                string[] PowerSource = new string[] { "Unknown", "Battery", "AC", "Usb", "Wireless" };
                dictBatteryAttributes.Add("power_source", PowerSource[(int)Battery.PowerSource]);
                // 低功耗节能模式
                string[] EnergySaverStatus = new string[] { "Unknown", "On", "Off" };
                dictBatteryAttributes.Add("energy_saver_status", EnergySaverStatus[(int)Battery.EnergySaverStatus]);
                mqttHA.PublishJson(dictBattery["attributes"], dictBatteryAttributes);
            });
        }
    }
}
