using System;
using System.Linq;
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
        //------------------- Menu Business rules -------------------------------------------------------------
        // During Asset creation  - 
        // Profile/Retreive-Update -> View, or update existing Profile via GET
        //                          - GET is also performed as part of new Asset creation process (if PROFILE
        //                            is not already persisted) and is returned for later persistence during
        //                            the asset creation process.
        //                          - Profile is read-only, due to potential accuracy and/or liability issues
        //                            stemming from multi-user edits.
        // Profile/DELETE -> may only succeed if Profile is not used/referenced by any Asset belonging
        //                   to any other investor.
        //
        // ** NOTE **
        // Profile data is obtained via a 3rd party (Yahoo Finanace) service as of 12/2014) and may yield
        // incomplete or unreliable data due to erroneous ticker data, etc. CUSIP numbers are unique to each
        // Asset; however, difficult to obtain. Consider alternative services (fees).
        //------------------------------------------------------------------------------------------------------

        private ProfileController _ctrl;
        private Mock<PimsIdentityService> _mockIdentitySvc;
        private Mock<InMemoryAssetRepository> _mockRepoAsset;


        [SetUp]
        public void Init() {
            _mockIdentitySvc = new Mock<PimsIdentityService>();
            _mockRepoAsset = new Mock<InMemoryAssetRepository>();
        }


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async Task Controller_Can_GET_all_fake_Profile_data()
        //{
        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object)
        //    {
        //        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile") },
        //        Configuration = new HttpConfiguration()
        //    };

        //    // Act 
        //    var profileInfo = await _ctrl.GetAllPersistedProfiles() as OkNegotiatedContentResult<IOrderedQueryable<Profile>>;


        //    // Assert
        //    Assert.IsNotNull(profileInfo);
        //    Assert.That(profileInfo.Content.All(p => p.DividendYield >= 0));
        //    Assert.That(profileInfo.Content, Is.TypeOf<EnumerableQuery<Profile>>());
        //}
        
        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Controller_can_GET_new_Profile_data_mimicking_new_fake_asset_as_part_of_the_asset_creation_process()
        //{
        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object) {
        //        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/KMB") },
        //        Configuration = new HttpConfiguration()
        //    };

        //    // Act - "KMB" not in repository & will be created. Also, mimics no db record in Production scenario.
        //    var newProfileResult = await _ctrl.GetProfileByTicker("KMB") as OkNegotiatedContentResult<Profile>;
          

        //    // Assert
        //    Assert.IsNotNull(newProfileResult);
        //    Assert.That(newProfileResult.Content.TickerSymbol, Is.EqualTo("KMB"));
        //    Assert.IsTrue(newProfileResult.Content.AssetId == default(Guid));
        //}


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Controller_can_GET_and_update_existing_fake_Profile_data_that_Is_greater_than_24hours_old_as_part_of_the_asset_creation_process()
        //{
        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object) {
        //        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/AAPL") },
        //        Configuration = new HttpConfiguration()
        //    };

        //    // Act - "ETP" is in repository & will be updated as needed.
        //    var profileResult = await _ctrl.GetProfileByTicker("AAPL") as OkNegotiatedContentResult<Profile>;


        //    // Assert
        //    Assert.IsNotNull(profileResult);
        //    Assert.That(profileResult.Content.TickerSymbol, Is.EqualTo("AAPL"));
        //    Assert.That(Convert.ToDateTime(profileResult.Content.LastUpdate), Is.AtLeast(DateTime.UtcNow.AddMinutes(-4)));
        //    Assert.IsTrue(profileResult.Content.AssetId != default(Guid));
        //}

        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Controller_will_not_GET_and_update_existing_fake_Profile_data_that_Is_less_than_24hours_old_as_part_of_the_asset_creation_process()
        //{
        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object) {
        //        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/IBM") },
        //        Configuration = new HttpConfiguration()
        //    };

        //    // Act - "ETP" is in repository & will be updated as needed.
        //    var profileResult = await _ctrl.GetProfileByTicker("IBM") as OkNegotiatedContentResult<Profile>;


        //    // Assert
        //    Assert.IsNotNull(profileResult);
        //    Assert.That(profileResult.Content.TickerSymbol, Is.EqualTo("IBM"));
        //    Assert.That(Convert.ToDateTime(profileResult.Content.LastUpdate), Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5)));
        //    Assert.IsTrue(profileResult.Content.AssetId != default(Guid));
        //}


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Controller_can_PUT_PATCH_an_existing_fake_Asset_Profile_as_part_of_the_Profile_update_process()
        //{
        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object) {
        //        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/VNR") },
        //        Configuration = new HttpConfiguration()
        //    };

        //    // Act 
        //    var updatedProfileContent = await _ctrl.GetProfileByTicker("VNR") as OkNegotiatedContentResult<Profile>;
        //    var updatedProfile = new Profile();
        //    if (updatedProfileContent != null)
        //        updatedProfile = updatedProfileContent.Content;
                
 
        //    //var debugJsonForFiddler = TestHelpers.ObjectToJson(updatedProfile);
        //    var profileResult = await _ctrl.UpdateProfile(updatedProfile) as OkNegotiatedContentResult<Profile>;


        //    // Assert - just validating sut (ctrl) works ok, no persistence done here.
        //    Assert.IsNotNull(profileResult);
        //    Assert.That(profileResult.Content.TickerSymbol, Is.EqualTo("VNR"));
        //    Assert.That(Convert.ToDateTime(profileResult.Content.DividendPayDate), Is.Not.EqualTo(default(DateTime)));
        //    Assert.IsTrue(Convert.ToDateTime(profileResult.Content.LastUpdate) >= DateTime.UtcNow.AddHours(-24));
        //}
        

        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Controller_cannot_GET_a_single_Fake_Profile_for_a_invalid_ticker_symbol() {

        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object) {
        //                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/RPA") },
        //                Configuration = new HttpConfiguration()};

        //    // Act
        //    var profileResult = await _ctrl.GetProfileByTicker("RPA") as OkNegotiatedContentResult<Profile>;


        //    // Assert
        //    Assert.IsNotInstanceOf<Profile>(profileResult);
        //}
        
        
        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public void Controller_can_not_DELETE_a_fake_Profile_currently_in_use_by_another_investor()
        //{
        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object)
        //    {
        //        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/1bb5e7bc-2f1e-4c89-b582-1c864b8c1c9b") },
        //        Configuration = new HttpConfiguration()
        //    };

        //    // Act  
        //    var result = _ctrl.Delete(new Guid("1bb5e7bc-2f1e-4c89-b582-1c864b8c1c9b"));

        //    // Assert
        //    Assert.IsTrue(result.IsCompleted == true);
        //}
        

        //[Test]
        ////[Repeat(1000)]
        //// ReSharper disable once InconsistentNaming
        //public void Controller_can_DELETE_a_fake_Profile_currently_not_in_use_by_another_investor() {
        //    // Arrange 
        //    _ctrl = new ProfileController(_mockIdentitySvc.Object, _mockRepoAsset.Object) {
        //        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Profile/66e5cd2f-8cec-4453-8920-af91589decd2") },
        //        Configuration = new HttpConfiguration()
        //    };

        //    // Act  
        //    var actionResult = _ctrl.Delete(new Guid("66e5cd2f-8cec-4453-8920-af91589decd2"));
        //    var taskResult = actionResult.Result; // forces completion of task ?
            
            
        //    // Assert
        //    Assert.That(actionResult, Is.Not.Null);
        //    Assert.IsTrue(actionResult.Status == TaskStatus.RanToCompletion);
        //} 



    }
}
