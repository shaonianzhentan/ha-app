using Newtonsoft.Json;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HaApp.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        string mqttText = string.Empty;
        public string MqttText
        {
            get { return mqttText; }
            set { SetProperty(ref mqttText, value); }
        }

        string buttonText = "保存";
        public string ButtonText
        {
            get { return buttonText; }
            set { SetProperty(ref buttonText, value); }
        }

        public AboutViewModel()
        {
            Title = "关于";
            OpenWebCommand = new Command(async () =>
            {
                if (!string.IsNullOrWhiteSpace(mqttText) && ValidateIPAddress(mqttText))
                {
                    await SecureStorage.SetAsync("mqtt", mqttText);
                    this.ConnectMQTT();
                }
                else
                {
                    ButtonText = "验证失败，重试";
                    // DependencyService.Get<IToast>().ShortAlert("请输入");
                }
            });

            new Action(async ()=> {
                MqttText = await SecureStorage.GetAsync("mqtt");
                this.ConnectMQTT();
            }).Invoke();

            this.StartHttpServer();
        }
        public ICommand OpenWebCommand { get; }


        public void StartHttpServer()
        {
            HttpListener httpListenner = new HttpListener();
            httpListenner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            string url = $"http://{MqttHA.GetIPAddress()}:8124/";
            httpListenner.Prefixes.Add(url);
            httpListenner.Start();
            new System.Threading.Thread(new ThreadStart(delegate
            {
                try
                {
                    while (true)
                    {
                        try
                        {
                            HttpListenerContext context = httpListenner.GetContext();
                            HttpListenerRequest request = context.Request;
                            HttpListenerResponse response = context.Response;
                            string path = request.Url.LocalPath;

                            string mqtt = request.QueryString.Get("mqtt");
                            if (!string.IsNullOrEmpty(mqtt))
                            {
                                if (ValidateIPAddress(mqtt))
                                {
                                    MainThread.BeginInvokeOnMainThread(async () => {
                                        await SecureStorage.SetAsync("mqtt", mqtt);
                                        MqttText = mqtt;
                                        this.ConnectMQTT();
                                    });
                                }
                            }
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict.Add("url_path", path);
                            dict.Add("update_time", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            string responseString = JsonConvert.SerializeObject(dict);
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                            //对客户端输出相应信息.
                            response.ContentType = "application/json; charset=utf-8";
                            response.ContentLength64 = buffer.Length;
                            System.IO.Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            //关闭输出流，释放相应资源
                            output.Close();
                        }
                        catch (System.Exception ex)
                        {
                            System.Console.WriteLine(ex);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    // httpListenner.Stop();
                }
            })).Start();
        }

        public bool ValidateIPAddress(string ipAddress)
        {
            Regex validipregex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
            return (ipAddress != "" && validipregex.IsMatch(ipAddress.Trim())) ? true : false;
        }

        void ConnectMQTT()
        {
            if (!string.IsNullOrWhiteSpace(MqttText) && ValidateIPAddress(MqttText))
            {
                ButtonText = "连接成功";
                string ip = MqttText;
                string port = "1883";

                MqttHA ha = new MqttHA(MqttText, port, "", "", new global::MqttDevice()
                {
                    identifiers = DeviceInfo.Model + DeviceInfo.Idiom,
                    manufacturer = DeviceInfo.Manufacturer,
                    model = MqttHA.GetIPAddress(),
                    name = DeviceInfo.Name,
                    sw_version = AppInfo.VersionString
                });
                if (!ha.TcpClientCheck(ip, int.Parse(port)))
                {
                    ButtonText = "连接MQTT服务失败！远程服务未开启";
                    return;
                }
                MqttDevice deivce = new MqttDevice(ha);
            }
        } 
    }
}