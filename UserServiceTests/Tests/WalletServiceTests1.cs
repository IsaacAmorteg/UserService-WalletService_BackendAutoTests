using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using UserServiceTests.Helpers;

namespace WalletServiceTests
{
    [TestFixture]
    public class WalletServiceTests
    {        
        private HttpClient client;
        private int userId;
        [SetUp]
        public async Task SetupAsync()
        {
            client = new HttpClient();
            UserHelper.SetHttpClient(client);
            userId = await UserHelper.CreateUser("Walt", "Smith");
        }
        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }

        [Test]
        public async Task T1_WalletService_GetBalance_NewUser_ReturnsNotActiveUser()
        {            
            HttpResponseMessage response = await client.GetAsync($"https://walletservice-uat.azurewebsites.net/Balance/GetBalance?userId={userId}");
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(responseBody, Is.EqualTo("not active user"));
        }
    }
}