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



namespace PIMS.UnitTest
{
    [TestFixture]
    public class VerifyAccountType
    {
        private Mock<PimsIdentityService> _mockIdentitySvc;
        private Mock<InMemoryAccountTypeRepository> _mockRepoAcctType;
        private Mock<InMemoryAssetRepository> _mockRepoAsset;
        private AccountTypeController _ctrl;
        

        //----------- BUSINESS RULES [and menu] : per Investor ------------------------------------------------------------------------------
        // Account Type is investor-specific, as each investor may have their own personalized types, not impacting other investor types.
        //
        // Account Type/Edit - displays a grid listing all available account types for edit(s) via (R)etreive. All edits will
        //                     globally affect ALL referencing Asset-Positions belonging to the investor. - TODO: Deferred, N/A
        //
        // (C)reate - There is NO explicit menu item for the addition of account types per se; instead, new account types may be added per investor via
        //              1) new Asset creation process, as new account type is mapped to an Investors' Position creation;
        //                 addition of account type takes place ONLY if unique & does not already exist.
        //                                        - OR -
        //              2) Position/Create (or Position/Edit-Retreive) menu option - allowing for new addition, or modifcation of.
        //
        // (R)etreive - 1) all existing Account types available for an Investor, including any unique types - TODO: DONE
        //                  a) if none, user can enter an account type (via "Add") during asset creation in UI, This will be added real-time
        //                  b) if incomplete, user can "Add" another account type to existing drop down selection(s) in UI.
        //              2) all existing Account types for an Asset - TODO: DONE
        //              3) all available lookup account types for populating dropdown controls; removes need for Edit - TODO: DONE
        // (U)pdate -  defined as changing the description, e.g., 'RothIRA' to 'Roth-IRA'. Implemented via POSITION ctrl - TODO: DONE
        // (D)elete -  There is NO explicit menu option for removing account types, as they are tighly coupled to Position(s). Removal
        //             of an account type requires modifying an associated 'Position' to use an alternate account type, and is handled
        //             via Position controller. - TODO: N/A here
        //
        //---------------------------------------------------------------------------------------------------------------------------------


        [SetUp]
        public void Init() {
            _mockIdentitySvc = new Mock<PimsIdentityService>();
            _mockRepoAcctType = new Mock<InMemoryAccountTypeRepository>();
            _mockRepoAsset = new Mock<InMemoryAssetRepository>();
        }


        //[Test]
        [Obsolete]
        // 4-10-15: Commented due to inappropriate use of in-memory data vin controller!
        //public async Task Controller_can_GET_all_available_lookup_account_types() 
        //{
        //    // Arrange  
        //    _ctrl = new AccountTypeController(_mockRepoAcctType.Object, _mockRepoAsset.Object, _mockIdentitySvc.Object) {
        //                            Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/AccountType?lookUps=true") },
        //                            Configuration = new HttpConfiguration()
        //    };

        //    // Act 
        //    var lookupAccts = await _ctrl.GetLookUpAccounts() as OkNegotiatedContentResult<IQueryable<string>>;


        //    // Assert
        //    Assert.IsNotNull(lookupAccts);
        //    Assert.That(lookupAccts.Content.Contains("Roth-IRA"));
        //    Assert.That(lookupAccts.Content.All(s => s != string.Empty));
        //    Assert.That(lookupAccts.Content.Count(), Is.GreaterThanOrEqualTo(3));
        //}
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_GET_all_entered_account_types_for_an_investor() 
        {
            // Arrange  
            _ctrl = new AccountTypeController(_mockRepoAcctType.Object, _mockRepoAsset.Object, _mockIdentitySvc.Object, null) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/AccountType/none") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var accountTypes = await _ctrl.GetAllAccountsForInvestor("none") as OkNegotiatedContentResult<IQueryable<string>>;


            // Assert
            Assert.IsNotNull(accountTypes);
            Assert.That(accountTypes.Content.Contains("Roth-IRA"));
            Assert.That(accountTypes.Content.All(s => s != string.Empty));
            Assert.That(accountTypes.Content.Count(), Is.GreaterThanOrEqualTo(3));
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_GET_all_available_account_types_by_Asset_for_an_investor()
        {
            // Arrange  
            _ctrl = new AccountTypeController(_mockRepoAcctType.Object, _mockRepoAsset.Object, _mockIdentitySvc.Object, null) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/AccountType/VNR") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var accountTypes = await _ctrl.GetAllAccountsForInvestor("VNR") as OkNegotiatedContentResult<IQueryable<string>>;


            // Assert
            Assert.IsNotNull(accountTypes);
            Assert.That(accountTypes.Content.Contains("ML-CMA"));
            Assert.That(accountTypes.Content.All(s => s != string.Empty));
            Assert.That(accountTypes.Content.Count(), Is.GreaterThanOrEqualTo(1));
        }


    }
}
