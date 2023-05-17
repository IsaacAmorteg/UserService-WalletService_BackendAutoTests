using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;

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
        public async Task T1_UserService_RegisterUser_WithEmptyFields_StatusCodeIs200()
        {
            HttpRequestMessage request = CreateRegisterUserRequest("", "");
            
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task T2_UserService_RegisterUser_WithEmptyFields_ResponseIsIdGreaterThan0()
        {
            HttpRequestMessage request = CreateRegisterUserRequest("", "");

            HttpResponseMessage response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            int newUserId = JsonConvert.DeserializeObject<int>(content);

            Assert.That(newUserId, Is.GreaterThan(0));
        }
        private HttpRequestMessage CreateRegisterUserRequest(string firstName, string lastName)
        {
            UserServiceRegisterNewUserRequest requestBody = new UserServiceRegisterNewUserRequest()
            {
                firstName = firstName,
                lastName = lastName
            };
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri("https://userservice-uat.azurewebsites.net/Register/RegisterNewUser"),
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };
            return request;

        }
    }
   
    public class UserServiceRegisterNewUserRequest
    {
        public string? firstName;
        public string? lastName;
    }
}