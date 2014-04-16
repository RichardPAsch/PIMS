using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Data.FakeRepositories;
using PIMS.Web.Api.Controllers;
using Moq;



namespace PIMS.UnitTest
{
    [TestFixture]
    public class VerifyProfile
    {
        private ProfileController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api";
        private Mock<InMemoryProfileRepository> _mockRepo;


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
            var myProfile = _ctrl.Get(request, "GSK");
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
            var newProfile = new Profile()
                             {
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
                                                                                UrlBase + "/Profile",
                                                                                _ctrl,
                                                                                "ProfileRoute",
                                                                                "api/{controller}/{ticker}",
                                                                                new { ticker = RouteParameter.Optional }
                                                                            ));
            // Format entity.
            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var profileEntity = JsonConvert.DeserializeObject<Profile>(jsonResult);


            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(profileEntity);
            Assert.AreEqual(result.Headers.Location, "http://localhost/api/Profile/GOOG");
            Assert.IsTrue(profileEntity.DividendFreq == "S");
            Assert.IsTrue(profileEntity.ProfileId != Guid.Empty);
        }






        /*

           [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_DELETE_a_single_fake_classification() {

            // Arrange
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);

            // Act
            var result = _ctrl.Delete(new Guid("9a6e794a-9455-4468-b915-1e465a05a3ac"), request);

            // Assert
            Assert.IsTrue(result.StatusCode == HttpStatusCode.OK);
        } 
          
          
         
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_cannot_GET_a_single_Fake_Profile_for_an_invalid_ticker() {

            // Arrange - Need to supply context-related properties of the request, to avoid Http error.
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new ProfileController(_mockRepo.Object);

            // Act
            var myProfile = _ctrl.Get(request, "X2Z");

            // Assert
            Assert.IsTrue(myProfile.StatusCode == HttpStatusCode.NotFound);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_cannot_DELETE_a_single_invalid_fake_classification() {

            // Arrange
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);

            // Act
            var result = _ctrl.Delete(new Guid(), request);

            // Assert
            Assert.IsTrue(result.StatusCode == HttpStatusCode.NotFound);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_Update_PUT_a_single_fake_classification() {

            // Arrange
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);
            const string classCode = "CS";

            // Act
            var respMsg = _ctrl.Get(request, classCode);
            var content = respMsg.Content.ReadAsStringAsync().Result;
            var testAssetClass = JsonConvert.DeserializeObject<AssetClass>(content);
            testAssetClass.Description = "Common stock/equity"; // modified 
            var respMsg2 = _ctrl.Put(testAssetClass, request);


            // Assert
            Assert.IsNotNull(respMsg2);
            Assert.AreEqual(HttpStatusCode.OK, respMsg2.StatusCode);

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_cannot_Update_PUT_a_single_invalid_fake_classification() {

            // Arrange
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);
            var fakeAssetClass = new AssetClass { KeyId = Guid.NewGuid(), Code = "FRE", Description = "junk" };


            // Act
            var respMsg = _ctrl.Put(fakeAssetClass, request);


            // Assert
            Assert.IsNotNull(respMsg);
            Assert.AreEqual(HttpStatusCode.BadRequest, respMsg.StatusCode);

        }

      
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_Cannot_POST_a_duplicate_Fake_Classification() {

            // Arrange
            _ctrl = new AssetClassController(_mockRepo.Object);

            var newClassification = new AssetClass {
                KeyId = Guid.NewGuid(),
                Code = "ETF",
                Description = "Exchange Traded Fund"
            };


            // Act
            var result = _ctrl.Post(newClassification, TestHelpers.GetHttpRequestMessage(
                                                                                HttpMethod.Post,
                                                                                UrlBase + "/AssetClass",
                                                                                _ctrl,
                                                                                "AssetClassRoute",
                                                                                "api/{controller}/{Code}",
                                                                                new { Code = RouteParameter.Optional }
                                                                            ));

            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var classification = JsonConvert.DeserializeObject<AssetClass>(jsonResult);


            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
            Assert.IsNullOrEmpty(classification.Code);

        }
        */

    }
}
