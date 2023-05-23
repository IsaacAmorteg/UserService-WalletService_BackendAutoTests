using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserServicePositiveTests;

namespace UserServiceTests.Helpers
{
    public static class CreateRegisterRequestHelper
    {
        public static HttpRequestMessage CreateRegisterUserRequest(string firstName, string lastName)
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
}
