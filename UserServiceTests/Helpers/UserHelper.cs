using Newtonsoft.Json;

namespace UserServiceTests.Helpers
{
    public static class UserHelper
    {
        private static HttpClient client;

        public static void SetHttpClient(HttpClient httpClient)
        {
            client = httpClient;
        }
        public static async Task<int> CreateUser(string firstName, string lastName)
        {
            HttpRequestMessage createRequest = CreateRegisterRequestHelper.CreateRegisterUserRequest(firstName, lastName);
            HttpResponseMessage createResponse = await client.SendAsync(createRequest);
            string createContent = await createResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<int>(createContent);
        }
        public static async Task<bool> GetUserStatus(int userId)
        {
            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage getStatusResponse = await client.GetAsync(getUserStatusUri);
            return JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());
        }

        public static async Task<bool> DeleteUserAsync(int userId)
        {
            string deleteUserUrl = $"https://userservice-uat.azurewebsites.net/Register/DeleteUser?userId={userId}";
            HttpResponseMessage response = await client.DeleteAsync(deleteUserUrl);
            return response.IsSuccessStatusCode;
        }
    }
}
