using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Moq.Language.Flow;
using Newtonsoft.Json;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Data.FakeRepositories;
using PIMS.Web.Api.Controllers;
using Moq;


// TODO: 1) test to fetch 1 classification 
/* DONE */
// TODO: 2) test to update 1 classification 
/* DONE */
// TODO: 3) test to delete 1 classification - admin only privilege to do this!
/* DONE */

// TODO: Need to set up StructureMap with correct mapping for ClassificationRepository (PROD) - in order to test via Fiddler !! Integration tests.
// TODO: 4) test to create a classification - PROD
// TODO: 5) test to fetch classifications - PROD
// TODO: 6) test to fetch 1 classification - PROD
// TODO: 7) test to update 1 classification - PROD
// TODO: 8) test to delete 1 classification - admin only privilege - PROD

// TODO: 9) refactor common code for last 3 tests(fake)



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

        // TODO: REFACTOR !!
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
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
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
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
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
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
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
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
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
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
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
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
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
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
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

            // Arrange - Boiler plate setup: mimics controller context state that run-time engine expects
            // when handling a POST; this may be unnecessary with WebApi v2.x?

            /* Request */
            var request = new HttpRequestMessage(HttpMethod.Post, UrlBase + "/AssetClass");
            var httpCfg = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = httpCfg;

            /* Controller */
            _ctrl = new AssetClassController(_mockRepo.Object) {Request = request, Configuration = httpCfg};

            /* Http configuration */
            // Use correct Route name, such that "Code" parameter maps correctly.
            httpCfg.Routes.MapHttpRoute("AssetClassRoute", "api/{controller}/{Code}", new { Code = RouteParameter.Optional });

            /* Route */
            var httpRouteData = new HttpRouteData(httpCfg.Routes["AssetClassRoute"], new HttpRouteValueDictionary(new { controller = "AssetClass" }));
            _ctrl.Request.Properties[HttpPropertyKeys.HttpRouteDataKey] = httpRouteData;

            var newClassification = new AssetClass {
                KeyId = Guid.NewGuid(),
                Code = "CEF",
                Description = "Closed-End Fund"
            };


            // Act
            var result = _ctrl.Post(newClassification, request);
            // Check data for new Classification entity.
            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var classification = JsonConvert.DeserializeObject<AssetClass>(jsonResult);


            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(classification);
            Assert.AreEqual(result.Headers.Location, "http://localhost/api/AssetClass/CEF");
            Assert.IsTrue(classification.Code == "CEF");
            Assert.IsTrue(classification.KeyId != Guid.Empty);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_Cannot_POST_a_duplicate_Fake_Classification() {

            // Arrange - Boiler plate setup: mimics controller context state that run-time engine expects
            // when handling a POST; this may be unnecessary with WebApi v2.x?

            /* Request */
            var request = new HttpRequestMessage(HttpMethod.Post, UrlBase + "/AssetClass");
            var httpCfg = new HttpConfiguration();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = httpCfg;

            /* Controller */
            _ctrl = new AssetClassController(_mockRepo.Object) { Request = request, Configuration = httpCfg };

            /* Http configuration */
            httpCfg.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{Code}", new { Code = RouteParameter.Optional });

            /* Route */
            var httpRouteData = new HttpRouteData(httpCfg.Routes["DefaultApi"], new HttpRouteValueDictionary(new { controller = "AssetClass" }));
            _ctrl.Request.Properties[HttpPropertyKeys.HttpRouteDataKey] = httpRouteData;

            var newClassification = new AssetClass {
                KeyId = Guid.NewGuid(),
                Code = "ETF",
                Description = "Exchange Traded Fund"
            };


            // Act
            var result = _ctrl.Post(newClassification, request);
            // Check data for new Classification entity.
            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var classification = JsonConvert.DeserializeObject<AssetClass>(jsonResult);


            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
            Assert.IsNullOrEmpty(classification.Code);

        }


    }
}
