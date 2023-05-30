using Newtonsoft.Json;
using NUnit.Framework;
using System.Globalization;
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
        public async Task T1_WalletService_GetBalance_NewUser_ReturnsNotActiveUser500() //Negative Path
        {
            HttpResponseMessage response = await client.GetAsync($"https://walletservice-uat.azurewebsites.net/Balance/GetBalance?userId={userId}");
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(responseBody, Is.EqualTo("not active user"));
        }
        [Test]
        public async Task T2_WalletService_GetBalance_NonExistingUser_ReturnsNotActiveUser500() //Negative Path
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
        [TestCase(10, "10.0")]
        [TestCase(0.01, "0.01")]
        [TestCase(9999999.99, "9999999.99")]
        [TestCase(10000000, "10000000.0")]
        public async Task T4_WalletService_GetBalance_IsCharged_ReturnsCorrectBalances(double amountCharged, string expectedBalance)
        {
            await UserHelper.SetUserStatus(userId, true);
            await ChargeRequest.Charge(userId, amountCharged);

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        [Test]
        [TestCase(-10, "0")]
        [TestCase(-0.01, "0")]
        [TestCase(-9999999.99, "0")]
        [TestCase(-10000000.01, "0")]
        public async Task T4_4_WalletService_GetBalance_IsChargedNonSufficientFunds_ReturnsBalanceZero(double amountCharged, string expectedBalance)
        {
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult result = await ChargeRequest.Charge(userId, amountCharged);
            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        [Test]

        public async Task T5_WalletService_GetBalance_MultipleTransactionsCharged_ReturnsCorrectBalance()
        {
            await UserHelper.SetUserStatus(userId, true);
            double[] amountsCharged = { 10, 20.5, 30, -15 };

            foreach (double amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "45.5";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        [Test]
        public async Task T6_WalletService_GetBalance_MultipleTransactionsCharged_ReturnsBalanceZero()
        {
            await UserHelper.SetUserStatus(userId, true);
            double[] amountsCharged = { 10, 20.5, 30, -10, -20, -30, -0.5 };

            foreach (double amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "0.0";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        [Test]
        public async Task T7_WalletService_GetBalance_MultipleTransactionsCharged_ReturnsBalanceZeroDecimalOne()
        {
            await UserHelper.SetUserStatus(userId, true);
            double[] amountsCharged = { 10, 20.5, 30, -10, -20, -30, -0.4 };

            foreach (double amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "0.1";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        [Test]
        public async Task T8_WalletService_GetBalance_MultipleTransactionsChargedNonSufficientFunds_BalanceIsLastBalanceWithOKChargePerformed()
        {
            await UserHelper.SetUserStatus(userId, true);
            double[] amountsCharged = { 10, 20.5, 30, -10, -20, -30, -0.4, -0.8, -9999999.91 };


            foreach (double amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "0.1";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }

        [Test]
        public async Task T9_WalletService_GetBalance_NoTransactionsChargeNegativeBalance_Return200ZeroBalance()
        {
            await UserHelper.SetUserStatus(userId, true);
            double chargeAmount = -100;

            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);
            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "0";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }

        [Test]
        public async Task T10_WalletService_GetBalance_MultipleTransactionsCharged_OverallBalance9999999_99()
        {
            await UserHelper.SetUserStatus(userId, true);
            double[] amountsCharged = { 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 1000000, 999999.99 };

            foreach (double amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "9999999.99";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }

        [Test]
        public async Task T11_WalletService_GetBalance_MultipleTransactionsCharged_OverallBalance10000000()
        {
            await UserHelper.SetUserStatus(userId, true);
            double[] amountsCharged = { 1000000, -1000000, 10000000 };

            foreach (double amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "10000000.0";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }
        [Test]

        public async Task T12_WalletService_GetBalance_MultipleTransactionsStatusToInactive_ReturnsCorrectBalance()
        {
            await UserHelper.SetUserStatus(userId, true);
            double[] amountsCharged = { 10, 20.5, 30, -15 };

            foreach (double amount in amountsCharged)
            {
                await ChargeRequest.Charge(userId, amount);
            }

            HttpResponseMessage response = await WalletHelper.GetBalance(userId);
            string responseBody = await response.Content.ReadAsStringAsync();
            string expectedBalance = "45.5";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));

            await UserHelper.SetUserStatus(userId, false);
            bool statusAfterFalse = await UserHelper.GetUserStatus(userId);
            Assert.IsFalse(statusAfterFalse);

            await WalletHelper.GetBalance(userId);
            responseBody = await response.Content.ReadAsStringAsync();
            expectedBalance = "45.5";

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseBody, Is.EqualTo(expectedBalance));
        }

        [Test]
        [TestCase(10, 5)]
        [TestCase(10000.01, 526.2)]
        [TestCase(23450.65, 5)]
        public async Task T13_WalletService_Charge_PositiveBalanceAndPositiveCharge_ReturnsTransactionIdCode200(double initialBalance, double chargeAmount)
        {
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult initBalanceResult = await ChargeRequest.Charge(userId, initialBalance);
            Assert.That(initBalanceResult.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.TransactionId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        [TestCase(-10)]
        [TestCase(-1000)]
        public async Task T14_WalletService_Charge_ZeroBalanceAndNegativeCharge_ReturnsTransactionIdEmptyCode500AndBodyMessage(double chargeAmount)
        {
            await UserHelper.SetUserStatus(userId, true);
            HttpResponseMessage balanceResponse = await WalletHelper.GetBalance(userId);
            string balanceResponseBody = await balanceResponse.Content.ReadAsStringAsync();
            decimal currentBalance = decimal.Parse(balanceResponseBody);

            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(result.TransactionId, Is.EqualTo(Guid.Empty));

            string expectedMessage = ChargeRequest.FormatChargeMessage(currentBalance, chargeAmount);
            Assert.That(result.Message, Is.EqualTo(expectedMessage));

        }

        [Test]
        [TestCase(-10, -5)]
        [TestCase(-1000, -300.30)]
        [TestCase(-0.5, -0.1)]
        public async Task T15_WalletService_Charge_NegativeBalanceAndNegativeCharge_ReturnsTransactionIdEmptyCode500AndBodyMessage(double initialBalance, double chargeAmount)
        {

            await UserHelper.SetUserStatus(userId, true);
            ChargeResult initChargeResult = await ChargeRequest.Charge(userId, initialBalance);
            Assert.That(initChargeResult.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            HttpResponseMessage balanceResponse = await WalletHelper.GetBalance(userId);
            string balanceResponseBody = await balanceResponse.Content.ReadAsStringAsync();
            decimal currentBalance = decimal.Parse(balanceResponseBody);

            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(result.TransactionId, Is.EqualTo(Guid.Empty));

            string expectedMessage = ChargeRequest.FormatChargeMessage(currentBalance, chargeAmount);
            Assert.That(result.Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCase(10, -5)]
        [TestCase(1000, -300.30)]
        [TestCase(1000, -0.01)]
        [TestCase(0.5, -0.1)]
        public async Task T16_WalletService_Charge_PositiveBalanceNegativeCharge_ReturnsTransactionIDStatusCode200(double initialBalance, double chargeAmount)
        {
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult initChargeResult = await ChargeRequest.Charge(userId, initialBalance);
            Assert.That(initChargeResult.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.TransactionId, Is.Not.EqualTo(Guid.Empty));
        }
        [Test]
        [TestCase(5)]
        [TestCase(0.01)]
        //[TestCase(10000000.01)] Create a Negative test for this one
        //[TestCase(0.001)] Create a Negative test for this one
        public async Task T17_WalletService_Charge_ZeroBalancePositiveCharge_ReturnsTransactionIDStatusCode200(double chargeAmount)
        {
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.TransactionId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        [TestCase(145)]
        [TestCase(-23.5)]
        public async Task T18_WalletService_Charge_InactiveUserCharge_ReturnsTransactionIdEmptyCode500AndBodyMessage(double chargeAmount)
        {
            bool userStatus = await UserHelper.GetUserStatus(userId);
            Assert.IsFalse(userStatus);

            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(result.TransactionId, Is.EqualTo(Guid.Empty));
            Assert.That(result.Message, Is.EqualTo("not active user"));
        }
        [Test]
        [TestCase(145)]
        [TestCase(-23.5)]
        public async Task T19_WalletService_Charge_NonExistingUser_ReturnsTransactionIdEmptyCode500AndBodyMessage(double chargeAmount)
        {
            int nonExistingUserId = 0;
            ChargeResult result = await ChargeRequest.Charge(nonExistingUserId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(result.TransactionId, Is.EqualTo(Guid.Empty));
            Assert.That(result.Message, Is.EqualTo("not active user"));
        }
        [Test]
        [TestCase(10)]
        [TestCase(3410)]
        [TestCase(11030.2)]
        public async Task T20_WalletService_Charge_PositiveBalanceExceedNegativeBalance_ReturnsTransactionIdEmptyCode500AndBodyMessage(double initialBalance)
        {
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult result = await ChargeRequest.Charge(userId, initialBalance);
            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ChargeResult resultAfterSecondCharge = await ChargeRequest.Charge(userId, (-initialBalance - 0.01));
            Assert.That(resultAfterSecondCharge.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(resultAfterSecondCharge.TransactionId, Is.EqualTo(Guid.Empty));

        }
        [Test]        
        public async Task T21_WalletService_Charge_PositiveBalanceExceedBalanceCharge_ReturnsTransactionIdEmptyCode500AndBodyMessage()
        {
            double initialBalance = 100;
            double chargeAmount = -100.01;
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult result = await ChargeRequest.Charge(userId, initialBalance);
            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            HttpResponseMessage balanceResponse = await WalletHelper.GetBalance(userId);
            string balanceResponseBody = await balanceResponse.Content.ReadAsStringAsync();           


            ChargeResult resultAfterSecondCharge = await ChargeRequest.Charge(userId, chargeAmount);
            Assert.That(resultAfterSecondCharge.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(resultAfterSecondCharge.TransactionId, Is.EqualTo(Guid.Empty));

            CultureInfo culture = CultureInfo.InvariantCulture;
            Assert.That(resultAfterSecondCharge.Message, Is.EqualTo($"User have '{balanceResponseBody}', you try to charge '{chargeAmount.ToString("0.00", culture)}'."));
        }

        [Test]
        [TestCase(10000000.01)]
        [TestCase(999999999.35)]

        public async Task T22_WalletService_Charge_ZeroBalancePositiveCharge_ReturnsTransactionIDStatusCode200(double chargeAmount)
        {
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(result.TransactionId, Is.EqualTo(Guid.Empty));

            CultureInfo culture = CultureInfo.InvariantCulture;           
            Assert.That(result.Message, Is.EqualTo($"After this charge balance could be '{chargeAmount.ToString("0.00", culture)}', maximum user balance is '10000000'"));
        }

        [Test]
        [TestCase(0.001)]
        [TestCase(210.011)]
        public async Task T23_WalletService_Charge_ZeroBalancePositiveCharge_ReturnsTransactionIDStatusCode200(double chargeAmount)
        {
            await UserHelper.SetUserStatus(userId, true);
            ChargeResult result = await ChargeRequest.Charge(userId, chargeAmount);

            HttpResponseMessage balanceResponse = await WalletHelper.GetBalance(userId);
            string balanceResponseBody = await balanceResponse.Content.ReadAsStringAsync();

            Assert.That(result.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(result.TransactionId, Is.EqualTo(Guid.Empty));

            Assert.That(result.Message, Is.EqualTo("Amount value must have precision 2 numbers after dot"));
            
        }

    }
}