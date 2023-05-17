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
        public async Task T1_UserService_RegisterUser_WithEmptyFields_StatusCodeIs200()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("", "");
            
            HttpResponseMessage response = await client.SendAsync(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task T2_UserService_RegisterUser_WithEmptyFields_ResponseIsIdGreaterThan0()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("", "");

            HttpResponseMessage response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            int newUserId = JsonConvert.DeserializeObject<int>(content);

            Assert.That(newUserId, Is.GreaterThan(0));
        }
        
    }
}