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
            WalletHelper.SetHttpClient(client);
            userId = await UserHelper.CreateUser("Walt", "Smith");
        }
        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }

        [Test]
        public async Task T1_WalletService_GetBalance_NewUser_ReturnsNotActiveUser500()
        {            
            HttpResponseMessage response = await client.GetAsync($"https://walletservice-uat.azurewebsites.net/Balance/GetBalance?userId={userId}");
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(responseBody, Is.EqualTo("not active user"));
        }
        [Test]
        public async Task T2_WalletService_GetBalance_NonExistingUser_ReturnsNotActiveUser500()
        {
            int nonExistingUserId = 0;
            HttpResponseMessage response = await WalletHelper.GetBalance(nonExistingUserId);
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(responseBody, Is.EqualTo("not active user"));
        }
        [Test]
        public async Task T3_WalletService_GetBalance_NoTransactionsActiveUser_ReturnsBalanceZero()
        {
            await UserHelper.SetUserStatus(userId, true);
            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo("0"));
        }
        [Test]
        [TestCase(30, "30.0")]
        [TestCase(20.1, "20.1")]
        [TestCase(1000.01, "1000.01")]
        public async Task T4_WalletService_GetBalance_IsCharged_ReturnsCorrectBalances(decimal amountCharged, string expectedBalance)
        {
            await UserHelper.SetUserStatus(userId, true);
            await ChargeRequest.Charge(userId, amountCharged);

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        [Test]
        public async Task T5_WalletService_GetBalance_MultipleTransactionsCharged_ReturnsCorrectBalance()
        {
            await UserHelper.SetUserStatus(userId, true);
            decimal[] amountsCharged = { 10, 20.5m, 30, -15m };

            foreach (decimal amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "45.5";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        
    }
}