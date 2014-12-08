using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using PIMS.Core.Security;
using PIMS.Data.FakeRepositories;
using PIMS.Web.Api.Controllers;
using Moq;
using PIMS.Core.Models;



namespace PIMS.UnitTest
{
    [TestFixture]
    public class VerifyProfile
    {
        //------------- Business rules -------------------------------------------------------------
        // Profile creation (GET in this case) can only be done via new Asset creation, and if
        //   no existing Profile.
        // Profile DELETE may only succeed if Profile is not used/referenced by any Asset, and is
        //   only allowed via Asset deletion.
        // Profile updates (GET) will only occur automatically if necessary. No user edits allowed.
        // Profile data is READ-ONLY: due to potential accuracy and/or liability issues stemming from 
        //   multi-user edits. Profile data obtained via a 3rd party (Yahoo Finanace as of 12/2014)
        //   service.
        //------------------------------------------------------------------------------------------

        private ProfileController _ctrl;
        private Mock<InMemoryProfileRepository> _mockRepo;
        private Mock<PimsIdentityService> _mockIdentitySvc;
        private Mock<InMemoryAssetRepository> _mockRepoAsset;


        [SetUp]
        public void Init() {
            _mockRepo = new Mock<InMemoryProfileRepository>();
            _mockIdentitySvc = new Mock<PimsIdentityService>();
            _mockRepoAsset = new Mock<InMemoryAssetRepository>();
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_can_GET_new_Profile_data_for_a_nonexistent_fake_asset()
        {
            // Arrange 
            _ctrl = new ProfileController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/KMB") },
                Configuration = new HttpConfiguration()
            };

            // Act - "KMB" not in repository & will be created.
            var profileResult = await _ctrl.GetProfileByTicker("KMB") as OkNegotiatedContentResult<Profile>;
          

            // Assert
            Assert.IsNotNull(profileResult);
            Assert.That(profileResult.Content.TickerSymbol, Is.EqualTo("KMB"));
            Assert.IsTrue(profileResult.Content.AssetId == default(Guid));
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_can_GET_existing_fake_Profile_data_and_update_data_as_needed()
        {
            // Arrange 
            _ctrl = new ProfileController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/ETP") },
                Configuration = new HttpConfiguration()
            };

            // Act - "ETP" is in repository & will be updated as needed.
            var profileResult = await _ctrl.GetProfileByTicker("ETP") as OkNegotiatedContentResult<Profile>;


            // Assert
            Assert.IsNotNull(profileResult);
            Assert.That(profileResult.Content.TickerSymbol, Is.EqualTo("ETP"));
            Assert.That(Convert.ToDateTime(profileResult.Content.DividendPayDate), Is.Not.EqualTo(default(DateTime)));
            Assert.IsTrue(profileResult.Content.AssetId != default(Guid));
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_cannot_GET_a_single_Fake_Profile_for_a_invalid_ticker_symbol() {

            // Arrange 
            _ctrl = new ProfileController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/RPA") },
                        Configuration = new HttpConfiguration()
            };

            // Act
            var profileResult = await _ctrl.GetProfileByTicker("RPA") as OkNegotiatedContentResult<Profile>;


            // Assert
            Assert.IsNotInstanceOf<Profile>(profileResult);
        }
        
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_not_DELETE_a_fake_Profile_currently_in_use_by_another_investor()
        {
            // Arrange 
            _ctrl = new ProfileController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object)
            {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/1bb5e7bc-2f1e-4c89-b582-1c864b8c1c9b") },
                Configuration = new HttpConfiguration()
            };

            // Act  
            var result = _ctrl.Delete(new Guid("1bb5e7bc-2f1e-4c89-b582-1c864b8c1c9b"));

            // Assert
            Assert.IsTrue(result.IsCompleted == true);
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Controller_can_DELETE_a_fake_Profile_currently_not_in_use_by_another_investor() {
            // Arrange 
            _ctrl = new ProfileController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/66e5cd2f-8cec-4453-8920-af91589decd2") },
                Configuration = new HttpConfiguration()
            };

            // Act  
            var result = _ctrl.Delete(new Guid("66e5cd2f-8cec-4453-8920-af91589decd2"));
            

            // Assert
            Assert.IsTrue(result.Status == TaskStatus.RanToCompletion);
        } 



    }
}
