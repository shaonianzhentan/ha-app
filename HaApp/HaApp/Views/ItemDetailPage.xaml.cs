using HaApp.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace HaApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}