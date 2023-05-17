using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;

namespace UserServicePositiveTests
{
    public class UserServicePositiveTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task T1_UserService_RegisterUser_WithEmptyFields_StatusCodeIs200()
        {
            HttpClient client = new HttpClient();

            UserServiceRegisterNewUserRequest requestBody = new UserServiceRegisterNewUserRequest()
            {
                firstName = " ",
                lastName = " "
            };

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri("https://userservice-uat.azurewebsites.net/Register/RegisterNewUser"),
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task T2_UserService_RegisterUser_WithEmptyFields_ResponseIsIdGreaterThan0()
        {
            HttpClient client = new HttpClient();

            UserServiceRegisterNewUserRequest requestBody = new UserServiceRegisterNewUserRequest()
            {
                firstName = " ",
                lastName = " "
            };

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri("https://userservice-uat.azurewebsites.net/Register/RegisterNewUser"),
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            int newUserId = JsonConvert.DeserializeObject<int>(content);

            Assert.That(newUserId, Is.GreaterThan(0));
        }
    }
    public class UserServiceRegisterNewUserRequest
    {
        public string? firstName;
        public string? lastName;
    }
}