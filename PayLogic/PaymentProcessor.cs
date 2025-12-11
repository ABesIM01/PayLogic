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

        public async Task<string> CreatePaymentAsync(decimal amount, string description)
        {
            try
            {
                string json =
                    $"{{\"public_key\":\"{publicKey}\"," +
                    $"\"version\":\"3\"," +
                    $"\"action\":\"pay\"," +
                    $"\"amount\":{amount.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"currency\":\"UAH\"," +
                    $"\"description\":\"{description}\"}}";

                string data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

                string signature = Convert.ToBase64String(
                    SignSHA1(privateKey + data + privateKey)
                );

                string url =
                    $"https://www.liqpay.ua/api/3/checkout?data={Uri.EscapeDataString(data)}&signature={Uri.EscapeDataString(signature)}";

                PaymentSuccess?.Invoke(this, EventArgs.Empty);
                return url;
            }
            catch (Exception ex)
            {
                PaymentFailure?.Invoke(this, ex.Message);
                throw;
            }
        }

        private static byte[] SignSHA1(string input)
        {
            using var sha1 = SHA1.Create();
            return sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        }
    }
}