using System;
using System.ComponentModel;
using CurrencyConverter.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CurrencyConverter.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            DatePicker.MaximumDate = DateTime.Now;
            DatePicker.MinimumDate = new DateTime(1991, 12, 26);
        }
    }
}