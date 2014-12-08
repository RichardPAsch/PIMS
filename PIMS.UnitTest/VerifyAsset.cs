using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using PIMS.Core.Models.ViewModels;
using PIMS.Core.Security;
using PIMS.Data.FakeRepositories;
using PIMS.Web.Api.Controllers;
using Moq;
using PIMS.Core.Models;


namespace PIMS.UnitTest
{
    [TestFixture]
    public class VerifyAsset
    {
        private AssetController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api/Asset"; // 10/28 - correct ??
        private Mock<InMemoryAssetRepository> _mockRepo;
        private Mock<PimsIdentityService> _mockIdentitySvc;
        private Mock<InMemoryInvestorRepository> _mockRepoInvestor;


         /* -- Developer notes --
          SetUp() is used to instruct the repository how to behave under different scenarios, when testing behavior.
          The controller dictates what data it needs from the repository; the controller passes constraint(s) for data it needs from the repository.
          The repository uses controller's directive(s) in fetching correct data.
          
          Using HttpClient for WebApi access doesn't allow for debugging controller and repository source!
          Therefore : initialize controller via request & configuration attributes
                    : make RPC-like calls to controller, as recommended by MSDN
                    : test WebApi access via Fiddler also. 
         */


        //----------- Menu BUSINESS RULES ----------------------------------------------------------------------------------------------
        // * Asset/Retreive sub-menu *
        //      - Asset/Retreive/Summary - Will allow for summary view of : 
        //                                      a) all assets, e.g., [.../api/Asset?displayType=summary], OR
        //                                      b) a single single asset via ticker e.g., 
        //                                                      [.../api/Asset/VNR] , OR
        //                                                      [.../api/Asset/VNR?displayType=summary]
        //                               - Will allow for edits
        //
        //     - Asset/Retreive/Detail   - Will allow for full object graph Asset view (default = detail) of : 
        //                                      a) all assets, e.g., 
        //                                                      [.../api/Asset?displayType=detail], OR
        //                                                      [.../api/Asset]
        //                                      b) each subordinate Asset component, e,g, Profile, accessible via URL link
        //                               - Will not allow for edits
        //
        // * Asset/Delete menu - relatively rare occurrence, as most investors will opt for "inactive" status on Assets *
        //      - removes all related Income, Position(s), Profile, and AccountType(s)
        //
        // * Asset/Create menu - One or more Positions may be created at time of initial asset POSTing.
        //----------------------------------------------------------------------------------------------------------------------------


        [SetUp]
        public void Init() {
            _mockRepo = new Mock<InMemoryAssetRepository>();
            _mockIdentitySvc = new Mock<PimsIdentityService>();
            _mockRepoInvestor = new Mock<InMemoryInvestorRepository>();
        }

       

        // TODO: 11-20-14+ :  1) RETEST ALL of a)VerifyAsset - DONE & b)VerifyThatNHibernateCan ,   
        // TODO               2) implement Income collection for Asset - DONE & create/test new test case(s), - DONE
        // TODO               3) finish DELETE tests, -DONE
        // TODO               4) test all via browser/Fiddler, - DONE (before Income changes); after Income changes
        // TODO               5) check URLS generated - DONE
        // TODO               6) do we need whole "Investor" association as part of Asset ? YES, may need in UI 'Profile' data
        // TODO               6) do we need whole "AssetClass" association as part of Asset ? 

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_summary_info_for_All_Fake_Assets_for_an_investor() {

            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                        {
                            Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset?displayType=summary") },
                            Configuration = new HttpConfiguration()
                        }; 
            
            // Act 
            var assetListing = await _ctrl.GetAll("summary") as OkNegotiatedContentResult<IQueryable<AssetSummaryVm>>;
            

            // Assert
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(assetListing.Content.ElementAt(0).CurrentInvestor == _mockIdentitySvc.Object.CurrentUser);
            Assert.IsTrue(assetListing.Content.ElementAt(1).CurrentInvestor == _mockIdentitySvc.Object.CurrentUser);
           
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_detail_info_for_All_Fake_Assets_for_an_investor_using_displayType_queryString() {

            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset?displayType=detail") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var assetListing = await _ctrl.GetAll("detail") as OkNegotiatedContentResult<IQueryable<Asset>>;


            // Assert
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(assetListing.Content.ElementAt(0).Investor.LastName == _mockIdentitySvc.Object.CurrentUser);
            Assert.IsTrue(assetListing.Content.ElementAt(1).Investor.LastName == _mockIdentitySvc.Object.CurrentUser);

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_detail_info_for_All_Fake_Assets_for_an_investor_with_no_queryString() {

            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset") },
                    Configuration = new HttpConfiguration()
            };

            // Act 
            var assetListing = await _ctrl.GetAll("detail") as OkNegotiatedContentResult<IQueryable<Asset>>;


            // Assert
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(assetListing.Content.ElementAt(0).Investor.LastName == _mockIdentitySvc.Object.CurrentUser);
            Assert.IsTrue(assetListing.Content.ElementAt(1).Investor.LastName == _mockIdentitySvc.Object.CurrentUser);

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_fake_detail_ticker_info_for_an_investor_with_verification_of_revenue_and_positions() 
        {
            // Arrange - default displayType = detail.
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var assetListing = await _ctrl.GetByTicker("IBM") as OkNegotiatedContentResult<IQueryable<Asset>>;


            // Assert
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.EqualTo(1));
            Assert.That(assetListing.Content.First().Positions.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(assetListing.Content.First().Revenue.Count, Is.GreaterThanOrEqualTo(3));
            Assert.IsTrue(assetListing.Content.ElementAt(0).Investor.LastName == _mockIdentitySvc.Object.CurrentUser);
            
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_summary_or_detail_info_for_a_fake_Asset_for_an_investor_via_ticker_symbol() {

            // Arrange - SUT
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                        {
                            Request = new HttpRequestMessage{RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM?displayType=summary")},
                            Configuration = new HttpConfiguration()
                        }; 

            // Act 
            // Display type can only be "Summary", via AssetSummaryVm.
            var assetListing = await _ctrl.GetByTicker("VNR", "summary") as OkNegotiatedContentResult<IQueryable<AssetSummaryVm>>;


            // Assert
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.EqualTo(1));
            Assert.That(assetListing.Content.First().TickerSymbol, Is.EqualTo("VNR"));
            Assert.IsTrue(assetListing.Content.First().DividendFrequency == "Q");
            Assert.IsTrue(assetListing.Content.First().CurrentInvestor == _mockIdentitySvc.Object.CurrentUser);
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_not_GET_a_single_Fake_Asset_for_an_investor_using_an_invalid_ticker_symbol() {

            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNRX") },
                    Configuration = new HttpConfiguration()
            };

            // Act 
            //var assetListing = await _ctrl.GetByTicker("VNR") as OkNegotiatedContentResult<IQueryable<Asset>>;
            var assetListing = await _ctrl.DeleteAsset("VNRX") as OkNegotiatedContentResult<IQueryable<AssetSummaryVm>>;

            // Assert
            Assert.IsNull(assetListing);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_summary_info_for_all_Fake_Assets_for_an_investor_sorted_by_AssetClass()
        {
            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset?sortBy=assetClass") },
                                    Configuration = new HttpConfiguration()
            }; 

            // Act 
            var assetListing = await _ctrl.GetAndSortByOther("assetClass") as OkNegotiatedContentResult<IQueryable<AssetSummaryVm>>;


            // Assert
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(assetListing.Content.ElementAt(0).CurrentInvestor == _mockIdentitySvc.Object.CurrentUser);
            Assert.That(assetListing.Content.First().AssetClassification.ToUpper().Trim(),
                                    Is.LessThan(assetListing.Content.Last().AssetClassification.ToUpper().Trim()));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_summary_info_for_all_Fake_Assets_for_an_investor_sorted_by_AccountType() 
        {
            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                                            {
                                                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset?sortBy=acctType") },
                                                Configuration = new HttpConfiguration()
                                            }; 

            // Act 
            var assetListing = await _ctrl.GetAndSortByOther("acctType") as OkNegotiatedContentResult<IQueryable<AssetSummaryVm>>;
            
            
            // Assert
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.GreaterThanOrEqualTo(2));
            Assert.IsTrue(assetListing.Content.ElementAt(0).CurrentInvestor == _mockIdentitySvc.Object.CurrentUser);
            Assert.That(assetListing.Content.ElementAt(0).AccountType.ToUpper().Trim(),
                                     Is.LessThan(assetListing.Content.ElementAt(1).AccountType.ToUpper().Trim()));
            Assert.IsTrue(assetListing.Content.First().CurrentInvestor == _mockIdentitySvc.Object.CurrentUser);

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_summary_info_for_all_Fake_Assets_for_an_investor_sorted_by_date_Income_received()
        {
            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset?sortBy=dateRecvd") },
                                    Configuration = new HttpConfiguration()
            }; 

            // Act 
            var assetListing = await _ctrl.GetAndSortByOther("dateRecvd") as OkNegotiatedContentResult<IQueryable<AssetSummaryVm>>;

            // Assert : income by desc order and within YTD
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.GreaterThanOrEqualTo(1));
            Assert.That(assetListing.Content.First().DateRecvd, Is.GreaterThanOrEqualTo(assetListing.Content.Last().DateRecvd));

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_GET_summary_info_for_all_Fake_Assets_for_an_investor_sorted_by_Income_received()
        {
            // Arrange
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset?sortBy=income") },
                                    Configuration = new HttpConfiguration()
            };

            // Act 
            var assetListing = await _ctrl.GetAndSortByOther("income") as OkNegotiatedContentResult<IQueryable<AssetSummaryVm>>;

            // Assert : income by desc order and within YTD
            Assert.IsNotNull(assetListing);
            Assert.That(assetListing.Content.Count(), Is.GreaterThan(1));
            Assert.That(assetListing.Content.First().IncomeRecvd, Is.GreaterThanOrEqualTo(assetListing.Content.First().IncomeRecvd));
        }
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_POST_a_Fake_Asset_excluding_Income_data()
        {
            // Arrange 
            var newAsset = new Asset
            {
                AssetId = Guid.NewGuid(),
                AssetClass = new AssetClass { Code = "PFD", Description = "Preferred Stock", KeyId = Guid.NewGuid() },
                Positions = new List<Position>
                    {new Position
                                {
                                    PositionId = Guid.NewGuid(),
                                    PurchaseDate = DateTime.UtcNow.ToString("d"),
                                    Quantity = 50,
                                    UnitCost = 122.93M,
                                    Account = new AccountType
                                    {   AccountTypeDesc = "Roth-IRA",
                                        KeyId = Guid.NewGuid()
                                    },
                                    LastUpdate = DateTime.UtcNow.AddMonths(-5).ToString("d")
                                }
                    },
                Profile = new Profile {
                    DividendFreq = "M",
                    DividendRate = .05932M,
                    DividendYield = 6.1M,
                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 14).ToString("d"),
                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 18).ToString("d"),
                    EarningsPerShare = 6.21M,
                    LastUpdate = DateTime.UtcNow.ToString("g"),
                    PE_Ratio = 11.06M,
                    ProfileId = Guid.NewGuid(),
                    TickerDescription = "Intel Inc.",
                    TickerSymbol = "INTC"
                }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newAsset);

            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                        {
                            Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset") },
                            Configuration = new HttpConfiguration()
                        };

            // Act 
            var actionResult = await _ctrl.CreateNewAsset(newAsset) as CreatedNegotiatedContentResult<Asset>;

            // Assert
            Assert.IsTrue(actionResult != null);
            Assert.That(actionResult.Location.AbsoluteUri, Is.StringContaining("api/Asset/INTC"));
            Assert.That(actionResult.Content.Profile.TickerSymbol.ToUpper(), Is.EqualTo("INTC"));
            Assert.IsTrue(actionResult.Content.Revenue == null);
            if (actionResult.Content.Revenue == null) return;
            Assert.That(actionResult.Content.Revenue.Count, Is.EqualTo(1));
            Assert.That(actionResult.Content.Positions.First().Account.AccountTypeDesc, Is.EqualTo("Roth-IRA"));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_POST_a_new_Fake_Asset_containing_a_minimum_of_just_required_Profile_and_Position_data() 
        {
            // Arrange - only 1 Position created for this test.
            var newAsset = new Asset {
                AssetId = Guid.NewGuid(),
                AssetClass = new AssetClass { Code = "ETF", Description = "Exchange-traded fund", KeyId = Guid.NewGuid() },
                Positions = new List<Position>
                                        {new Position
                                            {
                                                PositionId = Guid.NewGuid(),
                                                PurchaseDate = DateTime.UtcNow.AddMonths(-2).ToString("d"),
                                                Quantity = 606,
                                                UnitCost = 22.86M,
                                                Account = new AccountType
                                                {   AccountTypeDesc = "Roth-IRA",
                                                    KeyId = Guid.NewGuid()
                                                },
                                                LastUpdate = DateTime.UtcNow.AddMonths(-5).ToString("d") 
                                            }
                                        },
                Profile = new Profile {
                    DividendFreq = "M",
                    DividendRate = .1563M,
                    DividendYield = 8.55M,
                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 16).ToString("d"),
                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 19).ToString("d"),
                    EarningsPerShare = 11.21M,
                    LastUpdate = DateTime.UtcNow.ToString("g"),
                    PE_Ratio = 19.61M,
                    ProfileId = Guid.NewGuid(),
                    TickerDescription = "Pimco Dynamic Credit Income",
                    TickerSymbol = "PCI"
                }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newAsset);

            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset") },
                Configuration = new HttpConfiguration()
            };

            // Act 
            var actionResult = await _ctrl.CreateNewAsset(newAsset) as CreatedNegotiatedContentResult<Asset>;

            // Assert
            Assert.IsTrue(actionResult != null);
            Assert.That(actionResult.Location.AbsoluteUri, Is.StringContaining("api/Asset/PCI"));
            Assert.That(actionResult.Content.Profile.TickerSymbol.ToUpper(), Is.EqualTo("PCI"));
            Assert.IsTrue(actionResult.Content.Revenue == null);
            if (actionResult.Content.Revenue == null) return;
            Assert.That(actionResult.Content.Revenue.Count, Is.EqualTo(1));
            Assert.That(actionResult.Content.Positions.First().Account.AccountTypeDesc, Is.EqualTo("Roth-IRA"));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_POST_a_new_Fake_Asset_with_verification_of_generated_URLs()
        {
            // Arrange - Mimics how a new Asset would be POSTed via a client.
            var newAsset = new Asset {
                AssetId = Guid.NewGuid(),
                AssetClass = new AssetClass { Code = "MLP", Description = "Master Limited Partnership", KeyId = Guid.NewGuid() },
                Positions = new List<Position>
                                {new Position
                                    {
                                        PositionId = Guid.NewGuid(),
                                        PurchaseDate = DateTime.UtcNow.ToString("d"),
                                        Quantity = 453,
                                        UnitCost = 50.64M,
                                        Account = new AccountType
                                        {   AccountTypeDesc = "Roth-IRA",
                                            KeyId = Guid.NewGuid()
                                        },
                                        LastUpdate = DateTime.UtcNow.ToString("d"),
                                    },
                                    new Position
                                    {
                                        PositionId = Guid.NewGuid(),
                                        PurchaseDate = DateTime.UtcNow.AddDays(-10).ToString("d"),
                                        Quantity = 200,
                                        UnitCost = 48.90M,
                                        Account = new AccountType
                                        {   AccountTypeDesc = "ML-CMA",
                                            KeyId = Guid.NewGuid()
                                        },
                                        LastUpdate = DateTime.UtcNow.AddDays(-10).ToString("d"),
                                    }
                                },
                Profile = new Profile {
                    DividendFreq = "Q",
                    DividendRate = 3.962M,
                    DividendYield = 7.05M,
                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 12).ToString("d"),
                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 21).ToString("d"),
                    EarningsPerShare = 19.50M,
                    LastUpdate = DateTime.UtcNow.AddDays(-10).ToString("d"),
                    PE_Ratio = 9.61M,
                    ProfileId = Guid.NewGuid(),
                    TickerDescription = "Energy Transfer Partners",
                    TickerSymbol = "ETP"
                },
                Revenue = new List<Income>
                          {
                              new Income
                              {
                                  IncomeId = Guid.NewGuid(),
                                  Account = "Roth-IRA",
                                  Actual = 87.88M,
                                  DateRecvd = DateTime.UtcNow.AddMonths(-10).ToString("g"),
                                  Projected = 90.11M
                              },
                              new Income
                              {
                                  IncomeId = Guid.NewGuid(),
                                  Account = "ML-CMA",
                                  Actual = 87.88M,
                                  DateRecvd = DateTime.UtcNow.AddMonths(-5).ToString("g"),
                                  Projected = 90.11M
                              },
                              new Income
                              {
                                  IncomeId = Guid.NewGuid(),
                                  Account = "Roth-IRA",
                                  Actual = 88.88M,
                                  DateRecvd = DateTime.UtcNow.AddMonths(-3).ToString("g"),
                                  Projected = 90.11M
                              }
                          }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newAsset);

            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                        Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset") },
                        Configuration = new HttpConfiguration()
            };
            

            // Act 
            var actionResult = await _ctrl.CreateNewAsset(newAsset) as CreatedNegotiatedContentResult<Asset>;

            // Assert
            Assert.IsTrue(actionResult != null);
            Assert.That(actionResult.Location.AbsoluteUri, Is.StringContaining("api/Asset/ETP"));
            Assert.IsTrue(actionResult.Content.Investor.LastName == "Asch");
            Assert.IsTrue(String.Equals(actionResult.Content.Investor.Url, "http://localhost/Pims.Web.Api/api/Investor/RichardPAsch", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(String.Equals(actionResult.Content.AssetClass.Url, "http://localhost/Pims.Web.Api/api/AssetClass/MLP", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(actionResult.Content.Positions.First().Url, Contains.Substring("/api/Position/"));
            Assert.That(actionResult.Content.Profile.Url.ToUpper(), Is.EqualTo("http://localhost/Pims.Web.Api/api/Profile/ETP".ToUpper()));
            if (actionResult.Content.Revenue == null) return;
            Assert.That(actionResult.Content.Revenue.First().Url, Contains.Substring("/api/Income/"));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_not_POST_a_new_Fake_Asset_containing_missing_required_Profile_and_or_Position_data()
        {
            // Arrange 
            var newAsset = new Asset {
                AssetId = Guid.NewGuid(),
                AssetClass = new AssetClass { Code = "ETF", Description = "Exchange-traded fund", KeyId = Guid.NewGuid() },
                Positions = new List<Position>
                                        {new Position
                                            {
                                                PositionId = Guid.NewGuid(),
                                                PurchaseDate = DateTime.UtcNow.AddMonths(-2).ToString("d"),
                                                Quantity = 0,
                                                UnitCost = 22.86M,
                                                Account = new AccountType
                                                {   AccountTypeDesc = "Roth-IRA",
                                                    KeyId = Guid.NewGuid()
                                                },
                                                LastUpdate = DateTime.UtcNow.AddMonths(-5).ToString("d") // vs PurchaseDate ?
                                            }
                                        },
                Profile = new Profile {
                    DividendFreq = "M",
                    DividendRate = 0,
                    DividendYield = 8.55M,
                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 16).ToString("d"),
                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 19).ToString("d"),
                    EarningsPerShare = 11.21M,
                    LastUpdate = DateTime.UtcNow.ToString("g"),
                    PE_Ratio = 19.61M,
                    ProfileId = Guid.NewGuid(),
                    TickerDescription = "Pimco Dynamic Credit Income",
                    TickerSymbol = "PCI"
                }
            };

            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newAsset);

            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset") }, 
                Configuration = new HttpConfiguration()
            };

            // Mimic HttpRequest via browser or Fiddler. The ASP.NET MVC Framework validates any data passed to 
            // the controller action that is executing.
            _ctrl.ModelState.AddModelError("ProfileDivRate", "Rate can't be 0");

            // Act 
            var actionResult = await _ctrl.CreateNewAsset(newAsset) as CreatedNegotiatedContentResult<Asset>;

            // Assert
            Assert.IsTrue(actionResult == null);
           
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_POST_a_Fake_Asset_including_Income_data()
        {
            // Arrange - URLs generated by server (ctrl) in integration testing.
            // "Investor" to be initialized via controller, based on currently logged in user.
            // New POST would only create 1 position, new positions added via Update.
            var newAsset = new Asset {
                Url = "http://localhost/Pims.Web.Api/api/Asset/CTX",
                AssetId = Guid.NewGuid(),
                AssetClass = new AssetClass {
                    Url = "http://localhost/Pims.Web.Api/api/AssetClass/PFD",
                    Code = "PFD",
                    Description = "Preferred Stock",
                    KeyId = Guid.NewGuid()
                },
                Positions = new List<Position>
                        {
                            new Position
                                {
                                    PositionId = Guid.NewGuid(),
                                    PurchaseDate = DateTime.UtcNow.ToString("d"),
                                    Quantity = 280,
                                    UnitCost = 72.13M,
                                    Url =  UrlBase + "/CTX/Position",
                                    Account = new AccountType
                                    {   AccountTypeDesc = "CMA",
                                        KeyId = Guid.NewGuid()
                                    },
                                    LastUpdate = DateTime.UtcNow.ToString("d"),
                                },
                            new Position
                                {
                                    PositionId = Guid.NewGuid(),
                                    PurchaseDate = DateTime.UtcNow.AddMonths(-3).ToString("d"),
                                    Quantity = 70,
                                    UnitCost = 79.00M,
                                    Url =  UrlBase + "/CTX/Position",
                                    Account = new AccountType
                                    {   AccountTypeDesc = "ROTH-IRA",
                                        KeyId = Guid.NewGuid()
                                    },
                                    LastUpdate = DateTime.UtcNow.AddMonths(-1).ToString("d"),
                                }
                        },
                Revenue = new List<Income> 
                {
                    new Income
                    {
                        Actual = 79.05M,
                        DateRecvd = DateTime.UtcNow.ToString("g"),
                        IncomeId = Guid.NewGuid(),
                        Projected = 126.11M,
                        Url = UrlBase + "/CTX/Income" ,
                        Account = "Roth-IRA"
                    }
                   
                },
                Profile = new Profile {
                    DividendFreq = "S",
                    DividendRate = .05932M,
                    DividendYield = 6.1M,
                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 14).ToString("d"),
                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 18).ToString("d"),
                    EarningsPerShare = 6.21M,
                    LastUpdate = DateTime.UtcNow.ToString("g"),
                    PE_Ratio = 11.06M,
                    ProfileId = Guid.NewGuid(),
                    TickerDescription = "Qwest Inc.",
                    TickerSymbol = "CTX"
                }
            };


            //var debugJsonString = TestHelpers.ObjectToJson(newAsset);
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                        {
                            Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset") },
                            Configuration = new HttpConfiguration()
                        };

            // Act 
            var actionResult = await _ctrl.CreateNewAsset(newAsset) as CreatedNegotiatedContentResult<Asset>;

            // Assert
            Assert.IsTrue(actionResult != null);
            Assert.That(actionResult.Location.AbsoluteUri, Is.StringContaining("api/Asset/CTX"));
            Assert.That(actionResult.Content.Profile.TickerSymbol.ToUpper(), Is.EqualTo("CTX"));
            Assert.IsTrue(actionResult.Content.Revenue.First().Actual == 79.05M);
            Assert.That(actionResult.Content.Revenue.Count, Is.EqualTo(1));
            Assert.That(actionResult.Content.Positions[0].Account.AccountTypeDesc, Is.EqualTo("CMA"));
            Assert.That(actionResult.Content.Positions[1].Account.AccountTypeDesc, Is.EqualTo("ROTH-IRA"));

        }

        [Test]
        //// ReSharper disable once InconsistentNaming
        public async void Controller_Can_not_POST_a_duplicate_Fake_Asset()
        {
            // Arrange - duplicate = same ticker, investor, and account type.
            var newAsset = new Asset
                    {
                        Url = "http://localhost/Pims.Web.Api/api/Asset/IBM",
                        AssetId = Guid.NewGuid(),
                        Positions = new List<Position>
                                                {new Position
                                                 {
                                                     PositionId = Guid.NewGuid(),
                                                     PurchaseDate = DateTime.UtcNow.ToString("d"),
                                                     Quantity = 40,
                                                     UnitCost = 7.10M,
                                                     Url =  UrlBase + "/IBM/Position",
                                                     Account = new AccountType
                                                        {   AccountTypeDesc = "ROTH-IRA",
                                                            KeyId = Guid.NewGuid()
                                                        },
                                                     LastUpdate = DateTime.UtcNow.AddMonths(-7).ToString("g")
                                                 }},
                        Profile = new Profile
                                {
                                    DividendFreq = "Q",
                                    DividendYield = 4.9M,
                                    ExDividendDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10).ToString("d"),
                                    DividendPayDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 19).ToString("d"),
                                    EarningsPerShare = 3.21M,
                                    LastUpdate = DateTime.UtcNow.AddDays(-4).ToString("g"),
                                    PE_Ratio = 15.66M,
                                    ProfileId = Guid.NewGuid(),
                                    TickerDescription = "International Business Machines",
                                    TickerSymbol = "IBM",
                                    Url = "",
                                    DividendRate = 1.097M
                                }
                    };


            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                            {
                                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset") },
                                Configuration = new HttpConfiguration()
                            };

            // Act 
            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newAsset);
            var actionResult = await _ctrl.CreateNewAsset(newAsset) as CreatedNegotiatedContentResult<Asset>;
           

            // Assert
            Assert.IsTrue(actionResult == null);
           
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_PUT_update_a_Fake_Asset_with_AccountType_and_Quantity_and_DateRecvd_changes()
        {
            // Arrange - "assetViewModel" maps to http content, while ticker maps to "existingTicker" in ctrl.
            var dateRecvd = DateTime.UtcNow;
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                                                {
                                                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR") },
                                                    Configuration = new HttpConfiguration()
                                                };

            var editedAsset = new AssetSummaryVm
                                      {
                                          AccountTypePreEdit = "IRRA",
                                          AccountType = "Roth-IRA",
                                          Quantity = 500,
                                          DateRecvd = dateRecvd.AddDays(2).ToString("g")
                                      };
            //var debugTest = TestHelpers.ObjectToJson(editedAsset);

            // Act - "UpdateByTicker()" allows for PUT/PATCH updates.
            var updatedAsset = await _ctrl.UpdateByTicker(editedAsset, "VNR") as OkNegotiatedContentResult<Asset>;


            // Assert
            Assert.IsTrue(updatedAsset != null);
            Assert.That(editedAsset.Quantity, Is.EqualTo(updatedAsset.Content.Positions.First().Quantity));
            Assert.That(updatedAsset.Content.Positions.First().Account.AccountTypeDesc.ToUpper(), Is.EqualTo("ROTH-IRA"));
            Assert.That(updatedAsset.Content.Revenue.First().DateRecvd, Is.EqualTo(editedAsset.DateRecvd));
            Assert.That(updatedAsset.Content.Positions.First().Quantity, Is.EqualTo(editedAsset.Quantity));
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Controller_Can_not_PUT_update_a_Fake_Asset_with_a_bad_AccountType()
        {
            // Arrange 
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object)
                                                {
                                                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR") },
                                                    Configuration = new HttpConfiguration()
                                                };

            // AccountTypePreEdit value initialized by server and sent unmodified by the client.
            var editedAsset = new AssetSummaryVm {AccountTypePreEdit  = "IRRA", AccountType = ""};

            // Act 
            //var debugTest = TestHelpers.ObjectToJson(editedAsset);
            var updatedAsset = await _ctrl.UpdateByTicker(editedAsset, "VNR") as BadRequestResult;


            // Assert
            Assert.IsTrue(updatedAsset == null);
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Controller_Can_DELETE_a_Fake_Asset_for_an_investor() {

            // Arrange - see above business rules
            _ctrl = new AssetController(_mockRepo.Object, _mockIdentitySvc.Object, _mockRepoInvestor.Object) {
                    Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL") },
                    Configuration = new HttpConfiguration()
            };

            // Act 
            var response = await _ctrl.DeleteAsset("AAPL") as OkNegotiatedContentResult<IQueryable<Asset>>;
            

            // Assert
            Assert.IsNotNull(response);


        }

        






    }
}
