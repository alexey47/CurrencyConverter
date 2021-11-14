using System;
using System.Net;
using System.Windows.Input;
using CurrencyConverter.Models;
using Xamarin.Forms;
using System.Text.Json;
using Xamarin.Essentials;

namespace CurrencyConverter.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            var date = DateTime.Parse(Preferences.Get("date", $"{DateTime.Now}"));

            CurrencyRates = GetCurrencyRates(date);

            Amount = Preferences.Get("amount", string.Empty);
            FromCurrency = JsonSerializer.Deserialize<Currency>(Preferences.Get("fromCurrency", JsonSerializer.Serialize(CurrencyRates.Currencies[0])));
            ToCurrency = JsonSerializer.Deserialize<Currency>(Preferences.Get("toCurrency", JsonSerializer.Serialize(CurrencyRates.Currencies[0])));
            Date = date;
        }

        #region Fields

        private DateTime _date;
        private Currency _fromCurrency;
        private Currency _toCurrency;
        private string _result;
        private CurrencyRates _currencyRates;
        private string _amount;

        #endregion

        #region Properties

        public CurrencyRates CurrencyRates
        {
            get => _currencyRates;
            set
            {
                _currencyRates = value;
                OnPropertyChanged(nameof(CurrencyRates));
            }
        }
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                Preferences.Set("date", $"{_date}");
                OnPropertyChanged(nameof(Date));
                GetCurrenciesRates.Execute(null);
            }
        }
        public Currency FromCurrency
        {
            get => _fromCurrency;
            set
            {
                _fromCurrency = value;
                Preferences.Set("fromCurrency", JsonSerializer.Serialize(_fromCurrency));
                OnPropertyChanged(nameof(FromCurrency));
                Calculate.Execute(null);
            }
        }
        public Currency ToCurrency
        {
            get => _toCurrency;
            set
            {
                _toCurrency = value;
                Preferences.Set("toCurrency", JsonSerializer.Serialize(_toCurrency));
                OnPropertyChanged(nameof(ToCurrency));
                Calculate.Execute(null);
            }
        }
        public string Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                Preferences.Set("amount", $"{_amount}");
                Calculate.Execute(null);
            }
        }
        public string Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        #endregion

        public ICommand Calculate
        {
            get
            {
                return new Command(() =>
                {
                    if (FromCurrency == null || ToCurrency == null || Amount == string.Empty)
                    {
                        return;
                    }

                    Result = $"{FromCurrency.Value / ToCurrency.Value * decimal.Parse(Amount):0.##}";
                });
            }
        }
        public ICommand SwapCurrencies
        {
            get
            {
                return new Command(() =>
                {
                    (FromCurrency, ToCurrency) = (ToCurrency, FromCurrency);
                    //Calculate.Execute(null);
                });
            }
        }
        public ICommand GetCurrenciesRates
        {
            get
            {
                return new Command(() =>
                {
                    var fromCurrencyCode = FromCurrency.CharCode;
                    var toCurrencyCode = ToCurrency.CharCode;

                    CurrencyRates = GetCurrencyRates(Date);

                    FromCurrency = CurrencyRates.Currencies[0];
                    ToCurrency = CurrencyRates.Currencies[0];
                    foreach (var currency in CurrencyRates.Currencies)
                    {
                        if (currency.CharCode == fromCurrencyCode)
                        {
                            FromCurrency = currency;
                        }
                        if (currency.CharCode == toCurrencyCode)
                        {
                            ToCurrency = currency;
                        }
                    }

                    Calculate.Execute(null);
                });
            }
        }

        private CurrencyRates GetCurrencyRates(DateTime date)
        {
            string json;
            if (date.Date == DateTime.Today.Date)
            {
                json = new WebClient().DownloadString($"https://www.cbr-xml-daily.ru/daily_json.js");
            }
            else
            {
                while (true)
                {
                    try
                    {
                        json = new WebClient().DownloadString($"https://www.cbr-xml-daily.ru/archive/{date.Year:d4}/{date.Month:d2}/{date.Day:d2}/daily_json.js");
                        break;
                    }
                    catch
                    {
                        date = date.AddDays(1);
                    }
                }
            }

            return new CurrencyRates(JsonSerializer.Deserialize<CurrencyRates>(json));
        }
    }
}