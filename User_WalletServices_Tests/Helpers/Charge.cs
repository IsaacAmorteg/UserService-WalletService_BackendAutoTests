using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UserServiceTests.Models;

namespace UserServiceTests.Helpers
{
    public class ChargeResult
    {
        public Guid TransactionId { get; set; }
        public HttpResponseMessage Response { get; set; }
        public string Message { get; set; }
    }

    public static class ChargeRequest
    {
        
        public static async Task<ChargeResult> Charge(int userId, double amount)
        {
            WalletServiceChargeRequest requestBody = new WalletServiceChargeRequest()
            {
                userId = userId,
                amount = amount
            };

            string jsonRequest = JsonConvert.SerializeObject(requestBody);
            StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://walletservice-uat.azurewebsites.net/Balance/Charge"),
                    Content = content
                };

                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Guid transactionId = JsonConvert.DeserializeObject<Guid>(responseContent);
                    return new ChargeResult
                    {
                        TransactionId = transactionId,
                        Response = response
                    };
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return new ChargeResult
                    {
                        TransactionId = Guid.Empty,
                        Response = response,
                        Message = responseContent
                    };
                }
                else
                {
                    throw new Exception($"Unexpected status code received: {response.StatusCode}");
                }
            }
        }
        public static string FormatChargeMessage(decimal currentBalance, double chargeAmount)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            return $"User have '{currentBalance.ToString(culture)}', you try to charge '{chargeAmount.ToString("0.0", culture)}'.";
        }
    }
}