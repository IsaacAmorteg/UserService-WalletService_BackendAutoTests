using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserServiceTests.Helpers
{
    public static class WalletHelper
    {
        private static HttpClient client;

        public static void SetHttpClient(HttpClient httpClient)
        {
            client = httpClient;
        }
        public static async Task<HttpResponseMessage> GetBalance(int userId)
        {
            HttpResponseMessage response = await client.GetAsync($"https://walletservice-uat.azurewebsites.net/Balance/GetBalance?userId={userId}");
            return response;
        }
    }
}
