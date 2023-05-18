using Newtonsoft.Json;
using NUnit.Framework;
using System.Diagnostics.SymbolStore;
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
            UserHelper.SetHttpClient(client);
        }
        [TearDown]
        public void TearDown()
        {
            client.Dispose();
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

            bool deleteUserResult = await UserHelper.DeleteUserAsync(initialUserId);
            Assert.IsTrue(deleteUserResult);

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
        [Test]
        public async Task T10_UserService_GetUserStatus_UserExists_ReturnsFalseDefault()
        {
            HttpRequestMessage createRequest = CreateRegisterRequestHelper.CreateRegisterUserRequest("Robin", "Hood");
            HttpResponseMessage createResponse = await client.SendAsync(createRequest);

            string createContent = await createResponse.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage statusResponse = await client.GetAsync(getUserStatusUri);
            bool userStatus = JsonConvert.DeserializeObject<bool>(await statusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(userStatus);
        }
        [Test]
        public async Task T11_UserService_GetUserStatus_UserDoesNotExist_Returns500()
        {
            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId=0";
            HttpResponseMessage statusResponse = await client.GetAsync(getUserStatusUri);
            
            Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
        [Test]
        public async Task T12_UserService_GetUserStatus_StatusChangedToTrue_StatusCodeIs200AndNewStatusTrue()
        {
            HttpRequestMessage createRequest = CreateRegisterRequestHelper.CreateRegisterUserRequest("Egan", "Bernal");
            HttpResponseMessage createResponse = await client.SendAsync(createRequest);
            var content = await createResponse.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(content);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage statusResponse = await client.GetAsync(getUserStatusUri);
            bool userStatus = JsonConvert.DeserializeObject<bool>(await statusResponse.Content.ReadAsStringAsync());

            Assert.That(userStatus, Is.False);

            string setUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            HttpResponseMessage setUserStatusResponse = await client.PutAsync(setUserStatusUri, null);

            string getUserStatusUri2 = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage statusResponse2 = await client.GetAsync(getUserStatusUri2);
            bool newUserStatus = JsonConvert.DeserializeObject<bool>(await statusResponse2.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(newUserStatus, Is.True);
                Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(statusResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            });
            
        }
        [Test]
        public async Task T13_UserService_GetUserStatus_StatusChangedToFalse_StatusCodeIs200AndNewStatusFalse ()
        {
            HttpRequestMessage createRequest = CreateRegisterRequestHelper.CreateRegisterUserRequest("Miguel", "Lopez");
            HttpResponseMessage createResponse = await client.SendAsync(createRequest);
            var content = await createResponse.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(content);

            string setUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            HttpResponseMessage setUserStatusResponse = await client.PutAsync(setUserStatusUri, null);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage statusResponse = await client.GetAsync(getUserStatusUri);
            bool userStatus = JsonConvert.DeserializeObject<bool>(await statusResponse.Content.ReadAsStringAsync());

            Assert.That(userStatus, Is.True);

            string setUserStatusUriFalse = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=false";
            HttpResponseMessage setUserStatusResponseFalse = await client.PutAsync(setUserStatusUriFalse, null);

            string getUserStatusUri2 = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage statusResponse2 = await client.GetAsync(getUserStatusUri2);
            bool newUserStatus = JsonConvert.DeserializeObject<bool>(await statusResponse2.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(newUserStatus, Is.False);
                Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(statusResponse2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            });

        }
        [Test]
        public async Task T14_UserService_GetUserStatus_DeletedUserStatus_Returns500()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("Sergui", "Higuita");
            HttpResponseMessage response = await client.SendAsync(request);
            string createContent = await response.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            bool deleteUserUrl = await UserHelper.DeleteUserAsync(userId);
            Assert.IsTrue(deleteUserUrl);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage statusResponse = await client.GetAsync(getUserStatusUri);

            Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
        [Test]
        public async Task T15_UserService_SetStatus_NotExistingUserStatusChange_Status500()
        {
            string setStatusUri = "https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId=0&newStatus=true";
            HttpResponseMessage setStatusResponse = await client.PutAsync(setStatusUri, null);
            
            Assert.That(setStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
        [Test]
        public async Task T16_UserService_SetStatus_ChangeFromDefaultToTrue_StatusCodeIs200AndFinalStatusTrue()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("Dross", "Rotzank");
            HttpResponseMessage response = await client.SendAsync(request);
            string createContent = await response.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool initialStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(initialStatus);

            string setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            HttpResponseMessage setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool finalStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(setStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.IsTrue(finalStatus);
            });
        }
        [Test]
        public async Task T17_UserService_SetStatus_MultipleStatusChanges_StatusCodeIs200AndFinalStatusFalse()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("Santi", "Buitrago");
            HttpResponseMessage response = await client.SendAsync(request);
            string createContent = await response.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool initialStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(initialStatus);

            string setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            HttpResponseMessage setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool statusAfterTrue = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsTrue(statusAfterTrue);

            setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=false";
            setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool finalStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(setStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.IsFalse(finalStatus);
            });
        }
        [Test]
        public async Task T18_UserService_SetStatus_MultipleStatusChanges_StatusCodeIs200AndFinalStatusTrue()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("Santi", "Buitrago");
            HttpResponseMessage response = await client.SendAsync(request);
            string createContent = await response.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool initialStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(initialStatus);

            string setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            HttpResponseMessage setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool statusAfterTrue = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsTrue(statusAfterTrue);

            setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=false";
            setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool statusAfterFalse = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(statusAfterFalse);

            setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool finalStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.That(setStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.IsTrue(finalStatus);
            }); 
        }
        [Test]
        public async Task T19_UserService_SetStatus_FromFalseToFalse_Response200andStatusFalse()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("Primoz", "Roglic");
            HttpResponseMessage response = await client.SendAsync(request);
            string createContent = await response.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool initialStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(initialStatus);

            string setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=false";
            HttpResponseMessage setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool finalStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.IsFalse(finalStatus);
                Assert.That(setStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            });
            
        }
        [Test]
        public async Task T20_UserService_SetStatus_FromTrueToTrue_Response200andStatusTrue()
        {
            HttpRequestMessage request = CreateRegisterRequestHelper.CreateRegisterUserRequest("Primoz", "Roglic");
            HttpResponseMessage response = await client.SendAsync(request);
            string createContent = await response.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool initialStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(initialStatus);

            string setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            HttpResponseMessage setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool statusAfterTrue = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsTrue(statusAfterTrue);

            setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool finalStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.Multiple(() =>
            {
                Assert.IsTrue(finalStatus);
                Assert.That(setStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            });
        }
        [Test]
        public async Task T21_UserService_DeleteUser_NonActiveExistingUser_Returns200()
        {
            int userId = await UserHelper.CreateUser("Lance", "Armstrong");
            bool intialStatus = await UserHelper.GetUserStatus(userId);
            Assert.IsFalse(intialStatus);

            bool deleteResult = await UserHelper.DeleteUserAsync(userId);
            Assert.IsTrue(deleteResult);
        }

        [Test]
        public async Task T22_UserService_DeleteUser_ActiveExistingUser_Returns200()
        {
            HttpRequestMessage createRequest = CreateRegisterRequestHelper.CreateRegisterUserRequest("Lance", "Armstrong");
            HttpResponseMessage createResponse = await client.SendAsync(createRequest);
            string createContent = await createResponse.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(createContent);

            string getUserStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/GetUserStatus?userId={userId}";
            HttpResponseMessage getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool initialStatus = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsFalse(initialStatus);

            string setStatusUri = $"https://userservice-uat.azurewebsites.net/UserManagement/SetUserStatus?userId={userId}&newStatus=true";
            HttpResponseMessage setStatusResponse = await client.PutAsync(setStatusUri, null);
            setStatusResponse.EnsureSuccessStatusCode();

            getStatusResponse = await client.GetAsync(getUserStatusUri);
            bool statusAfterTrue = JsonConvert.DeserializeObject<bool>(await getStatusResponse.Content.ReadAsStringAsync());

            Assert.IsTrue(statusAfterTrue);

            string deleteUserUrl = $"https://userservice-uat.azurewebsites.net/Register/DeleteUser?userId={userId}";
            HttpResponseMessage deleteResponse = await client.DeleteAsync(deleteUserUrl);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        [Test]
        public async Task T23_UserService_DeleteUser_NonExistingUser_Returns500AndMessageBody()
        {
            string deleteUserUrl = $"https://userservice-uat.azurewebsites.net/Register/DeleteUser?userId=0";
            HttpResponseMessage deleteResponse = await client.DeleteAsync(deleteUserUrl);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            string responseBody = await deleteResponse.Content.ReadAsStringAsync();
            Assert.That(responseBody, Is.EqualTo("Sequence contains no elements"));
        }
        [Test]
        public async Task T24_UserService_DeleteUser_AlreadyDeletedUser_Returns500AndMessageBody()
        {
            int userId = await UserHelper.CreateUser("Michael", "Scott");
            bool deleteResult = await UserHelper.DeleteUserAsync(userId);
            Assert.IsTrue(deleteResult);

            deleteResult = await UserHelper.DeleteUserAsync(userId);
            Assert.IsFalse(deleteResult);

            HttpResponseMessage deleteResponse = await client.DeleteAsync($"https://userservice-uat.azurewebsites.net/Register/DeleteUser?userId={userId}");

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            string responseBody = await deleteResponse.Content.ReadAsStringAsync();
            Assert.That(responseBody, Is.EqualTo("Sequence contains no elements"));

        }

    }
}