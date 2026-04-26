using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FinUchetClient.Services
{
    public class CurrencyInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public double RateToRub { get; set; }
    }

    public class CurrencyService
    {
        private readonly HttpClient _client;
        private Dictionary<string, double> _exchangeRates;

        // Курсы валют к рублю (можно обновлять через API)
        private readonly Dictionary<string, CurrencyInfo> _currencies = new()
        {
            { "RUB", new CurrencyInfo { Code = "RUB", Name = "Российский рубль", Symbol = "₽", RateToRub = 1.0 } },
            { "USD", new CurrencyInfo { Code = "USD", Name = "Доллар США", Symbol = "$", RateToRub = 91.5 } },
            { "EUR", new CurrencyInfo { Code = "EUR", Name = "Евро", Symbol = "€", RateToRub = 99.2 } },
            { "KZT", new CurrencyInfo { Code = "KZT", Name = "Казахстанский тенге", Symbol = "₸", RateToRub = 0.20 } },
            { "GBP", new CurrencyInfo { Code = "GBP", Name = "Фунт стерлингов", Symbol = "£", RateToRub = 116.5 } },
            { "CNY", new CurrencyInfo { Code = "CNY", Name = "Китайский юань", Symbol = "¥", RateToRub = 12.8 } },
            { "TRY", new CurrencyInfo { Code = "TRY", Name = "Турецкая лира", Symbol = "₺", RateToRub = 2.8 } },
            { "AED", new CurrencyInfo { Code = "AED", Name = "Дирхам ОАЭ", Symbol = "د.إ", RateToRub = 24.9 } },
            { "BYN", new CurrencyInfo { Code = "BYN", Name = "Белорусский рубль", Symbol = "Br", RateToRub = 28.1 } },
            { "UAH", new CurrencyInfo { Code = "UAH", Name = "Украинская гривна", Symbol = "₴", RateToRub = 2.2 } }
        };

        public CurrencyService()
        {
            _client = new HttpClient();
            _exchangeRates = new Dictionary<string, double>();
        }

        public List<CurrencyInfo> GetAllCurrencies()
        {
            return new List<CurrencyInfo>(_currencies.Values);
        }

        public async Task UpdateRatesAsync()
        {
            try
            {
                // Используем бесплатный API для получения актуальных курсов
                var response = await _client.GetAsync("https://api.exchangerate-api.com/v4/latest/RUB");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(json);

                    foreach (var currency in _currencies.Values)
                    {
                        if (currency.Code != "RUB")
                        {
                            double rate = data.rates[currency.Code];
                            currency.RateToRub = rate;
                        }
                    }
                }
            }
            catch
            {
                // Если API недоступен, используем запасные курсы
                // (Оставляем текущие значения)
            }
        }

        public double Convert(double amountRub, string targetCurrency)
        {
            if (_currencies.ContainsKey(targetCurrency) && targetCurrency != "RUB")
            {
                return amountRub / _currencies[targetCurrency].RateToRub;
            }
            return amountRub;
        }

        public double ConvertFromCurrency(double amount, string sourceCurrency, string targetCurrency)
        {
            if (sourceCurrency == targetCurrency)
                return amount;

            // Сначала конвертируем в рубли
            double inRub = amount;
            if (sourceCurrency != "RUB")
            {
                inRub = amount * _currencies[sourceCurrency].RateToRub;
            }

            // Затем из рублей в целевую валюту
            return Convert(inRub, targetCurrency);
        }

        public string FormatCurrency(double amount, string currencyCode)
        {
            if (_currencies.ContainsKey(currencyCode))
            {
                return $"{amount:N2} {_currencies[currencyCode].Symbol}";
            }
            return $"{amount:N2}";
        }

        public CurrencyInfo GetCurrency(string code)
        {
            return _currencies.ContainsKey(code) ? _currencies[code] : null;
        }
    }
}