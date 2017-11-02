using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.Repositories;
using PIMS.Web.Api.Controllers;


namespace PIMS.IntegrationTest
{
    [TestFixture]
    public class VerifyAccountTypeController
    {
        private ISessionFactory _nhSessionFactory;
        private AccountTypeController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api";
        readonly IGenericRepository<AccountType> _repository;
        readonly IGenericRepository<Asset> _repositoryAsset;
        private readonly IPimsIdentityService _identityService;

        public VerifyAccountTypeController(IPimsIdentityService identityService)
        {
            _identityService = identityService;
        }

        public VerifyAccountTypeController()
        {
            _nhSessionFactory = NhDatabaseConfiguration.CreateSessionFactory();
            _repository = new AccountTypeRepository(_nhSessionFactory);
            _repositoryAsset = new AssetRepository(_nhSessionFactory);
            _identityService = new PimsIdentityService();
        }

        public void Init()
        {}


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_All_Account_Types()
        {
            // Arrange
            _ctrl = new AccountTypeController(_repository, _repositoryAsset, _identityService, null) {
                Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AccountType") },
                Configuration = new HttpConfiguration()
            };
               

            // Act
            var acctTypes = await _ctrl.GetAllAccounts();
            
            // Assert
            Assert.IsNotNull(acctTypes);
          
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_an_AccountType_By_Description()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                var resp = await client.GetAsync(UrlBase + "/AccountType/IRA").ConfigureAwait(false);

                // Act
                var acctType = await resp.Content.ReadAsAsync<AccountType>();

                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNull(acctType);
                Assert.IsTrue(acctType.AccountTypeDesc == "IRA");
            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_POST_a_new_AccountType() {

            using (var client = new HttpClient())
            {
                // Arrange
                var newAccountType = new AccountType
                                     {
                                         KeyId = Guid.NewGuid(),
                                         AccountTypeDesc = "ATN" + DateTime.Now.Hour + DateTime.Now.Minute.ToString(CultureInfo.InvariantCulture)
                                     };
               

                // Act
                var response = await client.PostAsJsonAsync(UrlBase + "/AccountType", newAccountType);

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);

            }
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_not_POST_a_duplicate_AccountType() {

            using (var client = new HttpClient())
            {
                // Arrange
                var newAccountType = new AccountType 
                {
                    KeyId = new Guid("18d97297-bfe8-4746-9e2a-be9bc2c0e9e4"),
                    AccountTypeDesc = "ROTH-IRA"
                };


                // Act
                var response = await client.PostAsJsonAsync(UrlBase + "/AccountType", newAccountType);

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Conflict);

            }
        }



        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_PUT_Update_an_AccountType() {

            using (var client = new HttpClient())
            {
                // Arrange
                var updatedAccountType = new AccountType
                                    {
                                        KeyId = new Guid("91d865a6-5446-45b7-bedc-a3c300e8f0f6"),
                                        AccountTypeDesc = "ATU" + DateTime.Now.Hour + DateTime.Now.Minute.ToString(CultureInfo.InvariantCulture)
                                    };


                // Act // Error: "Method not allowed"; statuscode = 405 [Sol: Check routing attribute on action !]
                var response =  await client.PutAsJsonAsync(UrlBase + "/AccountType", updatedAccountType);
      

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);

            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        // Admin only
        public async void Can_DELETE_an_AccountType() {

            using (var client = new HttpClient())
            {
                // Arrange
                var acctTypeToDelete = new AccountType
                {
                    KeyId = Guid.NewGuid(),
                    AccountTypeDesc = "ATD" + DateTime.Now.Hour + DateTime.Now.Minute.ToString(CultureInfo.InvariantCulture)
                };
                await client.PostAsJsonAsync(UrlBase + "/AccountType", acctTypeToDelete);

                // Act
                var reqUrl = UrlBase + "/AccountType/" + acctTypeToDelete.AccountTypeDesc.Trim();
                var response = await client.DeleteAsync(reqUrl);

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);  // 204 status code

            }
        }


   }


 }




