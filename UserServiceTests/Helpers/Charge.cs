using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserServiceTests.Models;

namespace UserServiceTests.Helpers
{
    public static class ChargeRequest
    {
        public static async Task<Guid> Charge(int userId, decimal amount)
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
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                Guid transactionId = JsonConvert.DeserializeObject<Guid>(responseContent);

                return transactionId;
            }
        }
    }
}
