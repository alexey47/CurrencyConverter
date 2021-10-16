using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Models
{
    public class Currency
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        [JsonPropertyName("NumCode")]
        public string NumCode { get; set; }

        [JsonPropertyName("CharCode")]
        public string CharCode { get; set; }

        [JsonPropertyName("Nominal")]
        public int Nominal { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Value")]
        public decimal Value { get; set; }

        [JsonPropertyName("Previous")]
        public decimal Previous { get; set; }
    }
    
    public class CurrencyRates
    {
        public CurrencyRates()
        {
            
        }
        public CurrencyRates(CurrencyRates currencyRates)
        {
            Date = currencyRates.Date;
            PreviousDate = currencyRates.PreviousDate;
            PreviousUrl = currencyRates.PreviousUrl;
            Timestamp = currencyRates.Timestamp;
            Currencies = new ObservableCollection<Currency>(currencyRates.Currencies);
            Currencies.Insert(0, new Currency
            {
                Id = null,
                NumCode = null,
                CharCode = "RUB",
                Nominal = 1,
                Name = "Российский рубль",
                Value = 1,
                Previous = 1
            });

            foreach (var currency in Currencies)
            {
                currency.Value /= currency.Nominal;
            }
        }

        [JsonPropertyName("Date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("PreviousDate")]
        public DateTime PreviousDate { get; set; }

        [JsonPropertyName("PreviousURL")]
        public string PreviousUrl { get; set; }

        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("Valute")]
        [JsonConverter(typeof(CurrencyCollectionConverter))]
        public ObservableCollection<Currency> Currencies { get; set; }
    }

    public class CurrencyCollectionConverter : JsonConverter<ObservableCollection<Currency>>
    {
        public override ObservableCollection<Currency> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var collection = new ObservableCollection<Currency>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return collection;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }
                
                collection.Add(JsonSerializer.Deserialize<Currency>(ref reader));
            }

            return collection;
        }

        public override void Write(Utf8JsonWriter writer, ObservableCollection<Currency> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var currency in value)
            {
                writer.WriteString(currency.CharCode, JsonSerializer.Serialize(currency));
            }
            writer.WriteEndObject();
        }
    }
}
