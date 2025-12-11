using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace WinFormsApp2.PayLogic
{
    public class OrderCalculator
    {
        private readonly Dictionary<string, decimal> servicePrices;

        public OrderCalculator()
        {
            servicePrices = LoadPricesFromJson();
        }

        private Dictionary<string, decimal> LoadPricesFromJson()
        {
            const string filePath = "services.json";

            if (!File.Exists(filePath))
            {
                var defaultPrices = new Dictionary<string, decimal>
                {
                    { "Охорона", 300 },
                    { "Доставка", 150 },
                    { "Ескорт", 2000 },
                    { "Таксі", 100 }
                };

                var json = JsonSerializer.Serialize(defaultPrices, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filePath, json);
                return defaultPrices;
            }

            string fileContent = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(fileContent)
                   ?? new Dictionary<string, decimal>();
        }

        public decimal GetPrice(string serviceName)
        {
            return servicePrices.TryGetValue(serviceName, out var price) ? price : 0m;
        }

        public decimal CalculateTotal(IEnumerable<string> selectedServices)
        {
            return selectedServices?.Sum(GetPrice) ?? 0m;
        }

        public IReadOnlyDictionary<string, decimal> GetAllServices()
        {
            return new Dictionary<string, decimal>(servicePrices);
        }
    }
}