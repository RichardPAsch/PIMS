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
        private Mock<InMemoryPositionRepository> _mockRepo;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api/Asset/<ticker>/Position";
        private Mock<PimsIdentityService> _mockIdentitySvc;
        private Mock<InMemoryAssetRepository> _mockRepoAsset;


        //----------- Menu BUSINESS RULES : per Investor ---------------------------------------------------------------------------------
        //  Asset/Create - One or more Positions can be created client-side, and POSTed as an aggregate at time of initial Asset creation.
        //                 See VerifyAsset.
        //  Position/Create - Adds new Position to existing Asset.
        //  Position/Retreive-Update - Adds to or modifies existing Position. Each Position is uniquely identified by its' Account type.
        //  Position/Delete - Removes appropriate Position(s) per Asset.
        //
        //---------------------------------------------------------------------------------------------------------------------------------

        // TODO: (C)reate - done,  (R)etreive - done 12/10
       
        [SetUp]
        public void Init()
        {
            _mockRepo = new Mock<InMemoryPositionRepository>();
            _mockIdentitySvc = new Mock<PimsIdentityService>();
            _mockRepoAsset = new Mock<InMemoryAssetRepository>();
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_a_single_fake_position_for_an_Asset_via_ticker_symbol_and_account() {

            // Arrange - SUT
            _ctrl = new PositionController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object)
            {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Position/Roth-IRA") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var assetPosition = await _ctrl.GetPositionByAccount("AAPL", "Roth-IRA") as OkNegotiatedContentResult<IQueryable<Position>>;


            // Assert
            Assert.IsNotNull(assetPosition);
            Assert.That(assetPosition.Content.Count(), Is.EqualTo(1));
            Assert.That(assetPosition.Content.First().Account.AccountTypeDesc, Is.EqualTo("Roth-IRA"));
          
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_all_fake_positions_for_an_Asset_via_ticker_symbol() {

            // Arrange - SUT
            _ctrl = new PositionController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
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
        public async Task Controller_can_not_POST_a_new_duplicate_fake_Position_for_a_given_users_asset() {

            // Arrange - SUT
            _ctrl = new PositionController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM/Position") },
                                    Configuration = new HttpConfiguration()
            };

            var newPosition = new Position
                                {
                                    Url = "http://localhost/PIMS.Web.Api/api/Asset/IBM/Position",
                                    PositionId = Guid.NewGuid(), 
                                    PurchaseDate = DateTime.UtcNow.ToString("d"), 
                                    Quantity = 109,
                                    UnitCost = 162.99M,
                                    LastUpdate = DateTime.UtcNow.ToString("g"),
                                    Account = new AccountType
                                            {
                                                Url = "http://localhost/PIMS.Web.Api/api/Asset/AAPL/Position/Account/IRRA",
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
        public async Task Controller_can_POST_an_additional_fake_Position_for_an_existing_asset() {

            // Arrange - SUT
            _ctrl = new PositionController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR/Position") },
                Configuration = new HttpConfiguration()
            };

            var newAcctId = Guid.NewGuid();
            var newPosition = new Position {
                Url = "http://localhost/PIMS.Web.Api/api/Asset/VNR/Position/Roth-IRA",
                PositionId = Guid.NewGuid(),
                PurchaseDate = DateTime.UtcNow.ToString("d"),
                LastUpdate = DateTime.UtcNow.ToString("g"),
                Quantity = 500,
                UnitCost = 17.05M,
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
        public async Task Controller_can_POST_a_new_fake_Position_as_part_of_the_new_asset_creation_process()
        {
            // Arrange -  
            _ctrl = new PositionController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoAsset.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/PFE/Position?newAsset=true") },
                Configuration = new HttpConfiguration()
            };

            var newAcctId = Guid.NewGuid();
            var newPosition = new Position {
                Url = "http://localhost/PIMS.Web.Api/api/Asset/PFE/Position/ML-CMA",
                PositionId = Guid.NewGuid(),
                PurchaseDate = DateTime.UtcNow.ToString("d"),
                LastUpdate = DateTime.UtcNow.ToString("g"),
                Quantity = 250,
                UnitCost = 31.86M,
                Account = new AccountType {
                    Url = "http://localhost/PIMS.Web.Api/api/Asset/PFE/Position/Account/" + newAcctId,
                    AccountTypeDesc = "ML-CMA",
                    KeyId = Guid.NewGuid()
                }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newPosition);

            // Act
            var positionActionResult = await _ctrl.CreateNewPosition(newPosition, true) as CreatedNegotiatedContentResult<Position>;


            // Assert
            Assert.IsNotNull(positionActionResult);
            Assert.That(positionActionResult.Content.Account.AccountTypeDesc, Is.EqualTo("ML-CMA"));
            Assert.That(positionActionResult.Location.AbsoluteUri, Is.EqualTo("http://localhost/PIMS.Web.Api/api/Asset/PFE/Position/ML-CMA"));
            Assert.That(positionActionResult.Content.Quantity, Is.EqualTo(250));

        }

        



    }
}

