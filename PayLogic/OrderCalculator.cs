using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;

namespace WinFormsApp2.PayLogic
{
    public class OrderCalculator
    {
        private Dictionary<string, decimal> servicePrices;

        public OrderCalculator()
        {
            LoadPricesFromDatabase();
        }

        private void LoadPricesFromDatabase()
        {
            servicePrices = new Dictionary<string, decimal>();

            try
            {
                string dbPath = "app.db";
                if (!System.IO.File.Exists(dbPath))
                    throw new Exception($"Файл бази даних не знайдено: {dbPath}");

                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    string sql = "SELECT name, price FROM Services";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["name"]?.ToString() ?? "";
                            string priceStr = reader["price"]?.ToString() ?? "0";

                            if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                                servicePrices[name] = price;
                            else
                                servicePrices[name] = 0m;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Не вдалося завантажити ціни", ex);
            }
        }

        public decimal GetPrice(string serviceName)
        {
            return servicePrices.TryGetValue(serviceName, out decimal price) ? price : 0m;
        }

        public decimal CalculateTotal(IEnumerable<string> selectedServices)
        {
            if (selectedServices == null)
                return 0m;

            return selectedServices.Sum(s => GetPrice(s));
        }

        public IReadOnlyDictionary<string, decimal> GetAllServices()
        {
            return new Dictionary<string, decimal>(servicePrices); // повертаємо копію
        }
    }
}
