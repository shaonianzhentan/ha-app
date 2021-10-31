using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(HaApp.UWP.ToastService))]
namespace HaApp.UWP
{
    public class ToastService : IToast
    {
        public void LongAlert(string message)
        {
            
        }

        public void ShortAlert(string message)
        {
            var t = Windows.UI.Notifications.ToastTemplateType.ToastText02;
            //在模板添加xml要的标题
            var content = Windows.UI.Notifications.ToastNotificationManager.GetTemplateContent(t);
            //需要using Windows.Data.Xml.Dom;
            var xml = content.GetElementsByTagName("text");
            xml[0].AppendChild(content.CreateTextNode("通知"));
            xml[1].AppendChild(content.CreateTextNode("小文本"));
            //需要using Windows.UI.Notifications;
            Windows.UI.Notifications.ToastNotification toast = new ToastNotification(content);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
