using System;
using System.Linq;
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
    public class VerifyAssetClass
    {
        private AssetClassController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api";
        private Mock<InMemoryAssetClassRepository> _mockRepo;

        /*
          SetUp() is used to instruct the repository how to behave under different scenarios, when testing behavior.
          don't need Setup(), as we're manually passing all fake data to the controller (via DI in PROD)
          and the controller dictates what data it needs from the repository.
          controller passes constraint(s) for data it needs to repository
          repository uses controller's directive(s) in fetching correct data
        */


        [SetUp]
        public void Init()
        {
            _mockRepo = new Mock<InMemoryAssetClassRepository>();
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_Can_GET_All_Fake_Classifications() {

            // Arrange
            _ctrl = new AssetClassController(_mockRepo.Object); // SUT


            // Act
            var result = _ctrl.Get().ToList();


            // Assert
            Assert.GreaterOrEqual(result.Count(), 4);
            Assert.IsNotNull(result.First());
            Assert.IsTrue(result.First().Code == "CS");
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_GET_a_single_Fake_classification_for_a_valid_code() {

            // Arrange - Need to supply context-related properties of the request, to avoid Http error.
            var request = TestHelpers.GetHttpRequestMessage();
           _ctrl = new AssetClassController(_mockRepo.Object);
            
            // Act
            var myClass = _ctrl.Get(request,  "ETF");

            // Assert
            Assert.IsNotNull(myClass);
            Assert.IsTrue(HttpStatusCode.OK == myClass.StatusCode);
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_GET_a_single_Fake_classification_for_a_valid_key() {

            // Arrange - Need to supply context-related properties of the request, to avoid Http error.
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);

            // Act
            var myClass = _ctrl.Get(request, new Guid("f2695700-3e14-4af0-a0d1-4da1ff2204d8"));

            // Assert
            Assert.IsNotNull(myClass);
            Assert.IsTrue(HttpStatusCode.OK == myClass.StatusCode);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_DELETE_a_single_fake_classification()
        {

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
        public void Controller_cannot_GET_a_single_Fake_classification_for_an_invalid_code() {

            // Arrange - Need to supply context-related properties of the request, to avoid Http error.
            var request = TestHelpers.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);

            // Act
            var myClass = _ctrl.Get(request, "RPA");

            // Assert
            Assert.IsTrue(myClass.StatusCode == HttpStatusCode.NotFound);
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
            var respMsg2 =_ctrl.Put(testAssetClass, request);


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
            var fakeAssetClass = new AssetClass {KeyId = Guid.NewGuid(), Code = "FRE", Description = "junk"};


            // Act
            var respMsg = _ctrl.Put(fakeAssetClass, request);


            // Assert
            Assert.IsNotNull(respMsg);
            Assert.AreEqual(HttpStatusCode.BadRequest, respMsg.StatusCode);

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_Can_POST_a_Fake_Classification() {
            
            // Arrange
            _ctrl = new AssetClassController(_mockRepo.Object);

            var newClassification = new AssetClass {
                KeyId = Guid.NewGuid(),
                Code = "C10",
                Description = "Closed-End Fund10"
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
           
            // Format entity.
            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var classification = JsonConvert.DeserializeObject<AssetClass>(jsonResult);


            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(classification);
            Assert.AreEqual(result.Headers.Location, "http://localhost/api/AssetClass/C10");
            Assert.IsTrue(classification.Code == "C10");
            Assert.IsTrue(classification.KeyId != Guid.Empty);
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


    }
}
