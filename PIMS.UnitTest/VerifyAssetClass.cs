using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using NUnit.Framework;
using PIMS.Data.FakeRepositories;
using PIMS.Web.Api.Controllers;
using Moq;
using PIMS.Core.Models;



namespace PIMS.UnitTest
{
    [TestFixture]
    //public class Authentication
    public class VerifyAssetClass
    {
        private AssetClassController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api/AssetClass";
        private Mock<InMemoryAssetClassRepository> _mockRepo;
        private int _randomNumber;

        /*
          SetUp() is used to instruct the repository how to behave under different scenarios, when testing behavior.
          don't need Setup(), as we're manually passing all fake data to the controller (via DI in PROD)
          and the controller dictates what data it needs from the repository.
          controller passes constraint(s) for data it needs to repository
          repository uses controller's directive(s) in fetching correct data
        */


        [SetUp]
        public void Init() {
            _mockRepo = new Mock<InMemoryAssetClassRepository>();
            _randomNumber = DateTime.Now.Hour + DateTime.Now.Minute;
        }



        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_POST_a_Fake_Classification() {
            // Arrange
            _ctrl = new AssetClassController(_mockRepo.Object);

            var newClassification = new AssetClass {
                KeyId = Guid.NewGuid(),
                Code = "T" + _randomNumber,
                Description = "TEST AssetClass" + "T" + _randomNumber
            };

            // Act 
            var actionResult = await _ctrl.CreateNewAssetClass(newClassification)
                                                        as CreatedNegotiatedContentResult<AssetClass>;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.That(newClassification.Description, Is.StringStarting("TEST"));
            Assert.That(actionResult.Location.AbsoluteUri, Is.SamePath(UrlBase + "/" + newClassification.Code));
            // TODO: modified (4/25) since corrected in VerifyProfile unit test. - Absolute vs. Relative URL in header location.
            //Assert.AreEqual(result.Headers.Location, UrlBase + "/AssetClass/" + newClassification.Code);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_All_Fake_Classifications() {
            // Arrange
            _ctrl = new AssetClassController(_mockRepo.Object); // SUT

            // Act
            var classifications = await _ctrl.GetAll();

            // Assert
            Assert.GreaterOrEqual(classifications.Count(), 4);
            Assert.IsNotNull(classifications.First());
            Assert.IsTrue(classifications.First().Code == "CS");
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_GET_a_single_Fake_classification_for_a_valid_code() {
            // Arrange - Need to supply context-related properties of the request, to avoid Http error.
            //TestHelper.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);

            // Act
            var result = await _ctrl.GetByCode("ETF") as OkNegotiatedContentResult<AssetClass>;


            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Content.Code, Is.EqualTo("ETF"));

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_can_Update_PUT_a_single_fake_classification_description_via_AssetClass() {

            // Arrange
            //TestHelper.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);
            var testDescEnding = DateTime.Now.Hour.ToString(CultureInfo.InvariantCulture) + DateTime.Now.Minute.ToString(CultureInfo.InvariantCulture);
            var updatedAc = new AssetClass {
                KeyId = new Guid("0afa2f02-a7cb-4bdb-a022-a3c700fd3ea1"),
                Code = "test24",
                Description = "Updated with: " + testDescEnding
            };

            // Act
            var result = await _ctrl.UpdateAssetClass(updatedAc, updatedAc.Code) as OkNegotiatedContentResult<AssetClass>;


            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Content.Description, Is.StringContaining(testDescEnding.ToString(CultureInfo.InvariantCulture)));

        }



        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public void Controller_Cannot_POST_a_duplicate_Fake_Classification() {

        //    // Arrange
        //    _ctrl = new AssetClassController(_mockRepo.Object);

        //    var newClassification = new AssetClass {
        //        KeyId = Guid.NewGuid(),
        //        Code = "ETF",
        //        Description = "Exchange Traded Fund"
        //    };


        //    // Act
        //    var result = _ctrl.Post(newClassification, TestHelpers.GetHttpRequestMessage(
        //                                                                        HttpMethod.Post,
        //                                                                        UrlBase + "/AssetClass",
        //                                                                        _ctrl,
        //                                                                        "AssetClassRoute",
        //                                                                        "api/{controller}/{Code}",
        //                                                                        new { Code = RouteParameter.Optional }
        //                                                                    ));

        //    var jsonResult = result.Content.ReadAsStringAsync().Result;
        //    var classification = JsonConvert.DeserializeObject<AssetClass>(jsonResult);


        //    // Assert
        //    Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
        //    Assert.IsNullOrEmpty(classification.Code);

        //}



        //[Test]s
        //// ReSharper disable once InconsistentNaming
        //public void Controller_cannot_GET_a_single_Fake_classification_for_an_invalid_code() {

        //    // Arrange - Need to supply context-related properties of the request, to avoid Http error.
        //    var request = TestHelpers.GetHttpRequestMessage();
        //    _ctrl = new AssetClassController(_mockRepo.Object);

        //    // Act
        //    var myClass = _ctrl.Get(request, "RPA");

        //    // Assert
        //    Assert.IsTrue(myClass.StatusCode == HttpStatusCode.NotFound);
        //}





        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public void Controller_cannot_Update_PUT_a_single_invalid_fake_classification() {

        //    // Arrange
        //    var request = TestHelpers.GetHttpRequestMessage();
        //    _ctrl = new AssetClassController(_mockRepo.Object);
        //    var fakeAssetClass = new AssetClass { KeyId = Guid.NewGuid(), Code = "FRE", Description = "junk" };


        //    // Act
        //    var respMsg = _ctrl.UpdateAssetClass(fakeAssetClass, request);


        //    // Assert
        //    Assert.IsTrue(respMsg.StatusCode == HttpStatusCode.NotFound);

        //}


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_can_DELETE_a_single_fake_classification() {

            // Arrange
            //TestHelper.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);

            // Act
            // OkNegotiatedContentResult<> enables content access!
            var deleteResult = await _ctrl.Delete(new Guid("9a6e794a-9455-4468-b915-1e465a05a3ac")); // PFD


            // Assert
            Assert.That(deleteResult.ToString(), Is.StringContaining("Ok"));
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_cannot_DELETE_a_bad_fake_classification() {

            // Arrange
            //TestHelper.GetHttpRequestMessage();
            _ctrl = new AssetClassController(_mockRepo.Object);

            // Act
            // OkNegotiatedContentResult<> enables content access!
            var deleteResult = await _ctrl.Delete(new Guid("9a6e794a-9455-4468-b915-1e465a05a3ad")); // PFD


            // Assert
            Assert.That(deleteResult.ToString(), Is.StringContaining("Bad"));
        }


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public void Controller_cannot_DELETE_a_single_invalid_fake_classification() {

        //    // Arrange
        //    var request = TestHelpers.GetHttpRequestMessage();
        //    _ctrl = new AssetClassController(_mockRepo.Object);

        //    // Act
        //    var result = _ctrl.Delete(request, new Guid() );

        //    // Assert
        //    Assert.IsTrue(result.StatusCode == HttpStatusCode.NotFound);
        //}









    }
}
