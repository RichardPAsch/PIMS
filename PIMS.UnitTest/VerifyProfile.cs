using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using NUnit.Framework;
using PIMS.Core.Security;
using PIMS.Data.FakeRepositories;
using PIMS.Web.Api.Controllers;
using Moq;



namespace PIMS.UnitTest
{
    [TestFixture]
    public class VerifyProfile
    {
        private ProfileController _ctrl;
        private Mock<InMemoryProfileRepository> _mockRepo;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api/Asset";

        /*  Note:
         *  Test data represents either:
         *      1) Yahoo.Finance-derived, or 
         *      2) existing (db) Profile info, 
        */

        [SetUp]
        public void Init() {
            _mockRepo = new Mock<InMemoryProfileRepository>();
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_GET_a_single_Fake_Profile_for_a_valid_ticker_symbol() {

            // Arrange 
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new ProfileController(_mockRepo.Object);

            // Act
            var myProfile = _ctrl.Get(request, "HMC");
            var taskProfile = myProfile.Content.ReadAsAsync<Profile>().Result;


            // Assert
            Assert.IsNotNull(myProfile);
            Assert.IsTrue(HttpStatusCode.OK == myProfile.StatusCode);
            Assert.IsTrue(taskProfile.TickerSymbol == "HMC");
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_cannot_GET_a_single_Fake_Profile_for_a_invalid_ticker_symbol() {

            // Arrange 
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new ProfileController(_mockRepo.Object);

            // Act
            var myProfile = _ctrl.Get(request, "RPA");
            var taskProfile = myProfile.Content.ReadAsAsync<Profile>().Result;


            // Assert
            Assert.IsTrue(myProfile.StatusCode == HttpStatusCode.NotFound);
            Assert.IsNull(taskProfile.TickerSymbol);
        }
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_GET_a_single_Fake_Profile_based_on_a_valid_AssetId() {

            // Arrange 
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new ProfileController(_mockRepo.Object);

            // Act
            var myProfile = _ctrl.Get(request, new Guid("e07a582a-aec8-43b9-9cb8-faed5e5434de"));
            var taskProfile = myProfile.Content.ReadAsAsync<Profile>().Result;


            // Assert
            Assert.IsNotNull(myProfile);
            Assert.IsTrue(HttpStatusCode.OK == myProfile.StatusCode);
            Assert.IsTrue(taskProfile.TickerSymbol == "GSK");
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_POST_a_new_fake_Profile() {

            // Arrange 
            _ctrl = new ProfileController(_mockRepo.Object);
            var newProfile = new Profile() {
                    ProfileId = Guid.NewGuid(),
                    SharePrice = 692.16M,
                    DividendFreq = "S",
                    DividendYield = 3.07M,
                    DividendRate = 1.052M,
                    TickerDescription = "Google",
                    EarningsPerShare = 7.96M,
                    PE_Ratio = 19.01M,
                    LastUpdate = DateTime.Now,
                    TickerSymbol = "GOOG"
            };

            // Act
            var result = _ctrl.Post(newProfile, TestHelpers.GetHttpRequestMessage(
                                                            HttpMethod.Post,
                                                            UrlBase + "/" + newProfile.TickerSymbol + "/Profile",
                                                            _ctrl,
                                                            "ProfileRoute",
                                                            "api/Asset/{ticker}/Profile/{ProfileId}",
                                                            new { ProfileId = RouteParameter.Optional }
                                                        ));
            // Format entity.
            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var profileEntity = JsonConvert.DeserializeObject<Profile>(jsonResult);


            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(profileEntity);
            // TODO: Use relative path ?
            Assert.AreEqual(result.Headers.Location, UrlBase +  "/" + newProfile.TickerSymbol + "/Profile");
            Assert.IsTrue(profileEntity.DividendFreq == "S");
            Assert.IsTrue(profileEntity.ProfileId != Guid.Empty);
        }
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_cannot_POST_a_new_duplicate_fake_Profile() {
            
            // Arrange 
            _ctrl = new ProfileController(_mockRepo.Object);
            var newProfile = new Profile() {
                ProfileId = new Guid("e07a582a-aec8-43b9-9cb8-faed5e5434de"),
                SharePrice = 22.16M,
                DividendFreq = "M",
                DividendYield = 4.892M,
                DividendRate = .912M,
                TickerDescription = "Glaxo Smith Kline",
                EarningsPerShare = 3.46M,
                PE_Ratio = 15.17M,
                LastUpdate = DateTime.Now,
                TickerSymbol = "GSK"
            };

            // Act
            var result = _ctrl.Post(newProfile, TestHelpers.GetHttpRequestMessage(
                                                            HttpMethod.Post,
                                                            UrlBase + "/" + newProfile.TickerSymbol + "/Profile",
                                                            _ctrl,
                                                            "ProfileRoute",
                                                            "api/Asset/{ticker}/Profile/{ProfileId}",
                                                            new { ProfileId = RouteParameter.Optional }
                                                        ));
            // Format entity.
            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var profileEntity = JsonConvert.DeserializeObject<Profile>(jsonResult);


            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode); // 409 status code
            Assert.IsNotNull(profileEntity);
            Assert.IsTrue(profileEntity.ProfileId == Guid.Empty);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_Update_PUT_a_fake_Profile() {

            // Arrange - edited
            var revisedProfile = new Profile() {
                ProfileId = new Guid("dbf1590e-b99f-468d-a44e-e5e8f86c5f86"),
                SharePrice = 201.06M,
                DividendFreq = "Q",
                DividendYield = 6.79M,
                DividendRate = 1.31M,
                TickerDescription = "International Business Machines",
                EarningsPerShare = 6.81M,
                PE_Ratio = 21.07M,
                LastUpdate = DateTime.Now,
                TickerSymbol = "IBM"
            };

            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new ProfileController(_mockRepo.Object);

            // Act
            var respMsg = _ctrl.Put(revisedProfile, request);
      

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, respMsg.StatusCode);
        }
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_DELETE_a_single_fake_Profile() {
            
            // TODO - per ADMIN role only!
            // Arrange 
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new ProfileController(_mockRepo.Object);

            // Act
            var result = _ctrl.Delete(request, new Guid("e07a582a-aec8-43b9-9cb8-faed5e5434de"));

            // Assert
            Assert.IsTrue(result.StatusCode == HttpStatusCode.OK);
        } 
        
        

    }
}
