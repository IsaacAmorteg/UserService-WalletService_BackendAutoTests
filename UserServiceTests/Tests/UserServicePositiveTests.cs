using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;
using UserServiceTests.Helpers;

namespace UserServicePositiveTests
{
    [TestFixture]
    public class UserServicePositiveTests
    {
        private HttpClient client;
        [SetUp]
        public void Setup()
        {
            client = new HttpClient();
        }

        [Test]
        public async Task T1_UserService_RegisterUser_WithEmptyFields_StatusCodeIs200AndIdMoreThan0()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("", "");
            
            HttpResponseMessage response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Assert.Multiple(() =>

            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(content, Is.Not.Null.And.Not.Empty);
                int newUserId = JsonConvert.DeserializeObject<int>(content);
                Assert.That(newUserId, Is.GreaterThan(0));
            });
                        
        }
        [Test]
        public async Task T2_UserService_RegisterUser_WithUpperCase_StatusCodeIs200()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("ISAAC", "AMORTEGUI");

            HttpResponseMessage response = await client.SendAsync(request);
                                                     
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));                     
        }

        [Test]
        public async Task T3_UserService_RegisterUser_FieldIsOneCharacter_StatusCodeIs200()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("A", "B");

            HttpResponseMessage response = await client.SendAsync(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task T4_UserService_RegisterUser_FieldsLengthGreaterThan100Symbols_StatusCodeIs200()
        {
            string longName = new string('A', 105);
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest(longName, longName);

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task T5_UserService_RegisterUser_RegisterTwoUsers_IdIsAutoIncremented()
        {
            HttpRequestMessage request1 = CreateRegisterRequestHelper.CreateRegisterUserRequest("Juan", "Alvarez");
            HttpRequestMessage request2 = CreateRegisterRequestHelper.CreateRegisterUserRequest("Camilo", "Suarez");

            HttpResponseMessage response1 = await client.SendAsync(request1);
            HttpResponseMessage response2 = await client.SendAsync(request2);

            var content1 = await response1.Content.ReadAsStringAsync();
            var content2 = await response2.Content.ReadAsStringAsync();

            int userId1 = JsonConvert.DeserializeObject<int>(content1);
            int userId2 = JsonConvert.DeserializeObject<int>(content2);

            Assert.That(userId2, Is.GreaterThan(userId1));

        }
        [Test]
        public async Task T6_UserService_RegisterUser_FieldsAreNull_StatusCodeIs500()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest(null, null);

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
        [Test]
        public async Task T7_UserService_RegisterUser_SpecialCharacters_StatusCodeIs200()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("Boeing-787!@", "Avianc@_1#$%");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task T8_UserService_RegisterUser_AfterUserIsDeletedAndNewUserRegistered_NewUserIdIsIncrementedByOne()
        {
            HttpRequestMessage request1 = CreateRegisterRequestHelper.CreateRegisterUserRequest("Willy", "Castro");
            HttpResponseMessage response1 = await client.SendAsync(request1);
            string createContent = await response1.Content.ReadAsStringAsync();
            int initialUserId = JsonConvert.DeserializeObject<int>(createContent);

            string deleteUserUrl = $"https://userservice-uat.azurewebsites.net/Register/DeleteUser?userId={initialUserId}";
            HttpResponseMessage deleteResponse = await client.DeleteAsync(deleteUserUrl);

            int newUserId = initialUserId + 1;
            HttpRequestMessage request2 = CreateRegisterRequestHelper.CreateRegisterUserRequest("Dean", "Smith");
            HttpResponseMessage response2 = await client.SendAsync(request2);
            string newCreateContent = await response2.Content.ReadAsStringAsync();
            int newUserCreatedId = JsonConvert.DeserializeObject<int>(newCreateContent);

            Assert.That(newUserCreatedId, Is.EqualTo(newUserId));

            
        }
        [Test]
        public async Task T9_UserService_RegisterUser_FieldAreDigits_StatusCodeIs200()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("55637346", "242345");

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}