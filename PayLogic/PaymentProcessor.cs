using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2.PayLogic
{
    public class PaymentProcessor
    {
        private readonly string publicKey;
        private readonly string privateKey;

        public event EventHandler PaymentSuccess;
        public event EventHandler<string> PaymentFailure;

        public PaymentProcessor(string publicKey, string privateKey)
        {
            this.publicKey = publicKey;
            this.privateKey = privateKey;
        }

        /// <summary>
        /// Створює URL для оплати LiqPay
        /// </summary>
        public async Task<string> CreatePaymentAsync(decimal amount, string description)
        {
            try
            {
                // JSON payload
                string json = $"{{\"public_key\":\"{publicKey}\"," +
                              $"\"version\":\"3\"," +
                              $"\"action\":\"pay\"," +
                              $"\"amount\":{amount}," +      // число, а не рядок
                              $"\"currency\":\"UAH\"," +
                              $"\"description\":\"{description}\"}}";

                // Base64 WebSafe
                string data = Base64WebSafe(json);
                string signature = Base64WebSafe(SignSHA1(privateKey + data + privateKey));

                // Валідний URL для LiqPay
                string payUrl = $"https://www.liqpay.ua/api/3/checkout?data={Uri.EscapeDataString(data)}&signature={Uri.EscapeDataString(signature)}";

                PaymentSuccess?.Invoke(this, EventArgs.Empty);
                return payUrl;
            }
            catch (Exception ex)
            {
                PaymentFailure?.Invoke(this, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Підпис SHA1
        /// </summary>
        private static byte[] SignSHA1(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        /// <summary>
        /// Base64 WebSafe (без "=" і з заміною символів)
        /// </summary>
        private static string Base64WebSafe(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
                          .TrimEnd('=')
                          .Replace('+', '-')
                          .Replace('/', '_');
        }

        /// <summary>
        /// Base64 WebSafe для байтів (для підпису)
        /// </summary>
        private static string Base64WebSafe(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                          .TrimEnd('=')
                          .Replace('+', '-')
                          .Replace('/', '_');
        }
    }
}
