using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.FakeRepositories;
using PIMS.Web.Api.Controllers;
using Moq;


namespace PIMS.UnitTest
{
    [TestFixture]
    public class VerifyPosition
    {
        private PositionController _ctrl;
        private Mock<PimsIdentityService> _mockIdentitySvc;
        private Mock<InMemoryAssetRepository> _mockRepoAsset;
        


        //----------- Menu BUSINESS RULES : per Investor ---------------------------------------------------------------------------------
        //  Asset/Create -> One or more Positions can be created client-side, and POSTed as an aggregate at time of initial Asset creation.
        //                 See VerifyAsset.
        //  Position/Create -> Adds new Position to existing Asset.
        //  Position/Retreive-Edit -> Adds to or modifies existing Position. Each Position is uniquely identified by its' Account type.
        //                           - 'Purchase Date' : Read-Only, set during Position creation
        //                           - 'Last Update'   : Read-Only, updated whenever any changes in Position are made
        //                           - 'Market Price'  : Read/Write, income projections based on this current market-adjusted rate
        //                           - 'Quantity'      : Read/Write, adjusted total 
        //                           - 'Account'       : Read/Write, allows for modifying existing account type; if consolidating with
        //                                               an existing account type:
        //                                                  a. Qty = total of summations
        //                                                  b. Unit Cost = market value at time of consolidation
        //                                                  c. Purchase Date = Date of consolidation
        //                                                  d. existing Position to be removed
        //
        //  Position/Delete -> Removes appropriate Position(s) per Asset, e.g., as per Account type consolidation or explicit Position
        //                     removal.
        //
        //---------------------------------------------------------------------------------------------------------------------------------

        
        [SetUp]
        public void Init()
        {
            _mockIdentitySvc = new Mock<PimsIdentityService>();
            _mockRepoAsset = new Mock<InMemoryAssetRepository>();
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_a_single_fake_position_for_an_Asset_via_ticker_symbol_and_account() {

            // Arrange - SUT  _mockRepo.Object,
            _ctrl = new PositionController( _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Position/Roth-IRA") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var assetPosition = await _ctrl.GetPositionByAccount("IBM", "ML-CMA") as OkNegotiatedContentResult<IQueryable<Position>>;


            // Assert
            Assert.IsNotNull(assetPosition);
            Assert.That(assetPosition.Content.Count(), Is.EqualTo(1));
            Assert.That(assetPosition.Content.First().Account.AccountTypeDesc, Is.EqualTo("ML-CMA"));
            Assert.That(assetPosition.Content.First().Url, Is.EqualTo("http://localhost/Pims.Web.Api/api/Asset/IBM/Position/ML-CMA"));
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_a_single_fake_Account_for_a_Position_via_an_Account_key() {

            // Arrange  
            _ctrl = new PositionController( _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/33f4b62f-bcd4-4d5f-8b2d-373d628a5dfc") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var positionAccountType = await _ctrl.GetAccountByAccountKey(new Guid("33f4b62f-bcd4-4d5f-8b2d-373d628a5dfc")) as OkNegotiatedContentResult<AccountType>;


            // Assert
            Assert.IsNotNull(positionAccountType);
            Assert.That(positionAccountType.Content.KeyId, Is.EqualTo(new Guid("33f4b62f-bcd4-4d5f-8b2d-373d628a5dfc")));
        }

        
        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_all_fake_positions_for_an_Asset_via_ticker_symbol() {

            // Arrange 
            _ctrl = new PositionController( _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Position") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var assetPositions = await _ctrl.GetPositionsByAsset("AAPL") as OkNegotiatedContentResult<IQueryable<Position>>;


            // Assert
            Assert.IsNotNull(assetPositions);
            Assert.That(assetPositions.Content.Count(), Is.GreaterThanOrEqualTo(3));
            Assert.That(assetPositions.Content.Last().Account.AccountTypeDesc, Is.EqualTo("Roth-IRA"));
           
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_not_POST_a_new_fake_duplicate_Position_for_an_existing_asset() {

            // Arrange 
            _ctrl = new PositionController( _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM/Position") },
                                    Configuration = new HttpConfiguration()
            };

            var newPosition = new Position
                                {
                                    Url = "http://localhost/PIMS.Web.Api/api/Asset/IBM/Position/Roth-IRA",
                                    PositionId = Guid.NewGuid(), 
                                    PurchaseDate = DateTime.UtcNow.ToString("d"), 
                                    Quantity = 109,
                                    MarketPrice = 162.99M,
                                    LastUpdate = DateTime.UtcNow.ToString("g"),
                                    Account = new AccountType
                                            {
                                                Url = "http://localhost/PIMS.Web.Api/api/Asset/IBM/Position/Account/" + Guid.NewGuid(),
                                                AccountTypeDesc = "Roth-IRA",
                                                KeyId = Guid.NewGuid()
                                            }
                                };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newPosition);

            // Act
            var assetPosition = await _ctrl.CreateNewPosition(newPosition) as OkNegotiatedContentResult<IQueryable<Position>>;


            // Assert
            Assert.IsNull(assetPosition);
          
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_POST_a_new_fake_Position_for_an_existing_asset() {

            // Arrange 
            _ctrl = new PositionController( _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR/Position") },
                Configuration = new HttpConfiguration()
            };

            var newAcctId = Guid.NewGuid();
            var newPosition = new Position {
                Url = "http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/Roth-IRA",
                PositionId = Guid.NewGuid(),
                PurchaseDate = DateTime.UtcNow.AddDays(-13).ToString("d"),
                LastUpdate = DateTime.UtcNow.ToString("g"),
                Quantity = 500,
                MarketPrice = 15.22M,
                Account = new AccountType {
                    Url = "http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/Account/" + newAcctId,
                    AccountTypeDesc = "Roth-IRA",
                    KeyId = Guid.NewGuid()
                }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newPosition);

            // Act
            var positionActionResult = await _ctrl.CreateNewPosition(newPosition) as CreatedNegotiatedContentResult<Position>;
           

            // Assert
            Assert.IsNotNull(positionActionResult);
            Assert.That(positionActionResult.Content.Account.AccountTypeDesc, Is.EqualTo("Roth-IRA"));
            Assert.That(positionActionResult.Location.AbsoluteUri, Is.EqualTo("http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/Roth-IRA"));
            Assert.That(positionActionResult.Content.Quantity, Is.EqualTo(500));

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_DELETE_a_fake_Position_for_an_existing_asset()
        {
            // Arrange 
            _ctrl = new PositionController( _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/2a64098f-e408-45ac-969f-6cfe566ef249") },
                Configuration = new HttpConfiguration()
            };


            // Act
            var deleteResult = await _ctrl.DeletePosition(new Guid("2a64098f-e408-45ac-969f-6cfe566ef249")) as OkNegotiatedContentResult<string>;


            // Assert
            Assert.IsNotNull(deleteResult);
            Assert.IsTrue(deleteResult.Content == "deleted");
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_PUT_update_a_fake_Position_for_an_existing_asset() 
        {
            // Arrange 
            _ctrl = new PositionController( _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/ML-CMA") },
                Configuration = new HttpConfiguration()
            };

            // Updated Position - example: increased holding.
            var newAcctId = Guid.NewGuid();
            var updatedPosition = new Position {
                Url = "http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/ML-CMA",
                PositionId = Guid.NewGuid(),
                PurchaseDate = DateTime.UtcNow.AddMonths(-14).ToString("d"),
                InvestorKey = "Asch",
                LastUpdate = DateTime.UtcNow.ToString("g"),
                Quantity = 1000,        // new total
                MarketPrice = 16.88M,   // new unit price based on current market conditions
                Account = new AccountType {
                    Url = "http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/Account/" + newAcctId,
                    AccountTypeDesc = "ML-CMA",
                    KeyId = Guid.NewGuid()
                }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(updatedPosition);

            // Act
            var positionActionResult = await _ctrl.UpdatePositionsByAsset(updatedPosition) as OkNegotiatedContentResult<Position>;

            
            // Assert
            Assert.IsNotNull(positionActionResult);
            Assert.That(positionActionResult.Content.Url, Is.EqualTo("http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/ML-CMA"));
            Assert.That(positionActionResult.Content.Quantity, Is.EqualTo(1000));

        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_can_PUT_update_a_fake_Position_via_account_type_consolidation_for_an_existing_asset()
        {
 
            // Arrange 
            _ctrl = new PositionController(_mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Position/IRRA") },
                Configuration = new HttpConfiguration()
            };

            // merge Positions:  converting IRRA -> Roth-IRA example
            var updatedPosition = new Position {
                Url = "http://localhost/PIMS.Web.Api/api/Asset/AAPL/Position/IRRA",
                PositionId = new Guid("6172313a-20d1-47b6-9a7e-de1fe80a9cc8"), 
                PurchaseDate = DateTime.UtcNow.ToString("d"),
                InvestorKey = "Asch",
                LastUpdate = DateTime.UtcNow.ToString("g"),
                Quantity = 50,          
                MarketPrice = 118.96M,   // new unit price based on current market conditions- per Yahoo Finance
                Account = new AccountType {
                    Url = "http://localhost/PIMS.Web.Api/api/Asset/AAPL/Position/Account/98228ef0-c3f1-43b1-b640-9c84e13bb99b",
                    AccountTypeDesc = "Roth-IRA",  // new account type
                    KeyId = new Guid("98228ef0-c3f1-43b1-b640-9c84e13bb99b")
                }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(updatedPosition);

            // Act
            var positionActionResult = await _ctrl.UpdatePositionsByAsset(updatedPosition) as OkNegotiatedContentResult<IQueryable<Position>>;


            // Assert
            Assert.IsNotNull(positionActionResult);
            Assert.That(positionActionResult.Content.First().Url, Is.EqualTo("http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Roth-IRA"));

        }

    }
}

