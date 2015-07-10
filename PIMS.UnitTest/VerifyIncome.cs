//using System;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Web.Http;
//using System.Web.Http.Results;
//using NUnit.Framework;
//using PIMS.Core.Models;
//using PIMS.Core.Security;
//using PIMS.Data.FakeRepositories;
//using PIMS.Web.Api.Controllers;
//using Moq;
//using PIMS.Core.Models.ViewModels;


//namespace PIMS.UnitTest
//{
//    [TestFixture]
//    public class VerifyIncome
//    {
//        private IncomeController _ctrl;
//        private Mock<PimsIdentityService> _mockIdentitySvc;
//        private Mock<InMemoryAssetRepository> _mockRepoAsset;
//        private Mock<InMemoryInvestorRepository> _mockRepoInvestor;


//        //----------- Menu BUSINESS RULES : per Investor ---------------------------------------------------------------------------------
//        //  (Asset/Create) -> Allows for recording an Income event during initial Asset creation, and does NOT involve Income controller.
//        //                 -> New income info created client-side & handled as a composite entity during NHibernate Asset POSTing trx.
//        //
//        //  Income/Create -> Allows for addition of one or more Income events to an existing Asset.
//        //                -> Duplicate: same DateRecvd and Account.
//        //                -> Client will be restricted to using only existing/(entered) Asset account(s) during Income creation333
//        //
//        //  Income/Retreive-Search -> Retreives existing INCOME HISTORY based on any number of search query conditions. Resulting output
//        //                            fields will vary, and be read-only by design, to minimize potential editing error(s).
//        //
//        //  Income/Retreive-Edit -> Allows for individual Asset income review and/or edits, based on the last 6 months history. Each
//        //                          income grid record is uniquely identified by its' account type & date income was received.
//        //
//        //  Income/Delete -> Removes appropriate Income(s) per Asset.
//        //
//        //---------------------------------------------------------------------------------------------------------------------------------


//        [SetUp]
//        public void Init() {
//            _mockIdentitySvc = new Mock<PimsIdentityService>();
//            _mockRepoAsset = new Mock<InMemoryAssetRepository>();
//            _mockRepoInvestor = new Mock<InMemoryInvestorRepository>();
//        }



//        #region Non-asset specific tests
//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_by_dates() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Income/4-1-2014/4-30-2014") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var byDatesRevenue = await _ctrl.GetRevenueTotalForAllAssetsByDates("4-1-2014", "4-30-2014") as OkNegotiatedContentResult<decimal>;


//            // Assert
//            Assert.IsNotNull(byDatesRevenue);
//            Assert.That(byDatesRevenue.Content, Is.EqualTo(273.02));
//            Assert.That(byDatesRevenue.Content, Is.TypeOf<decimal>());
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_averages_for_YTD() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Income/Averages") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var averagesRevenue = await _ctrl.GetRevenueYtdMonthlyAverages() as OkNegotiatedContentResult<IOrderedQueryable<RevenueYtdVm>>;


//            // Assert
//            Assert.IsNotNull(averagesRevenue);
//            Assert.That(averagesRevenue.Content.Count(), Is.GreaterThanOrEqualTo(4)); // Minimum needed for adequate test.
//            var last3MosTotal = averagesRevenue.Content.ElementAt(3).AmountRecvd + averagesRevenue.Content.ElementAt(2).AmountRecvd +
//                                     averagesRevenue.Content.ElementAt(1).AmountRecvd;
//            var runningTotal = averagesRevenue.Content.ElementAt(3).AmountRecvd + averagesRevenue.Content.ElementAt(2).AmountRecvd +
//                                     averagesRevenue.Content.ElementAt(1).AmountRecvd + averagesRevenue.Content.ElementAt(0).AmountRecvd;
//            Assert.That(averagesRevenue.Content.ElementAt(3).Rolling3MonthAverage, Is.EqualTo(Math.Round(last3MosTotal / 3, 2)));
//            Assert.That(averagesRevenue.Content.ElementAt(3).YtdAverage, Is.EqualTo(Math.Round(runningTotal / 4, 2)));
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_for_an_asset_for_the_last_2_years_with_no_wildcard_ticker_designation() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var revenueByAsset = await _ctrl.GetSingleAssetRevenueForLastTwoYears("VNR") as OkNegotiatedContentResult<IOrderedQueryable<AssetRevenueVm>>;


//            // Assert
//            Assert.IsNotNull(revenueByAsset);
//            Assert.That(revenueByAsset.Content.All(r => r.Ticker.Contains("VNR")));

//        }

//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_for_an_asset_for_the_last_2_years_with_a_wildcard_ticker_designation() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VN/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var revenueByAsset = await _ctrl.GetSingleAssetRevenueForLastTwoYears("VN") as OkNegotiatedContentResult<IOrderedQueryable<AssetRevenueVm>>;


//            // Assert
//            Assert.IsNotNull(revenueByAsset);
//            Assert.That(revenueByAsset.Content.All(r => r.Ticker.Contains("VN")));

//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_total_revenue_and_payment_freq_by_asset_ToDate() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Income?completeRevenue=true") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act - [dates used: 1-1-1990 -> current]
//            var totalRevenueInfo = await _ctrl.GetTotalRevenueAndPymtFreqByAssetToDate(true)
//                                                    as OkNegotiatedContentResult<IOrderedQueryable<TotalRevenueAndPymtFreqByAssetVm>>;

//            // Assert
//            Assert.IsNotNull(totalRevenueInfo);
//            Assert.That(totalRevenueInfo.Content.Count(), Is.GreaterThanOrEqualTo(3));
//            Assert.That(totalRevenueInfo.Content.ElementAt(0).DatePurchased, Is.GreaterThan(default(DateTime).ToString("d")));
//            Assert.That(totalRevenueInfo.Content.ElementAt(0).TotalIncome, Is.GreaterThan(0));
//            Assert.That(totalRevenueInfo.Content, Is.Ordered.Descending.By("TotalIncome"));
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_Total_Revenue_for_all_Assets_with_frequency() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Income/Freq/1-1-2014/12-31-2014") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act
//            var revenueFreqInfo = await _ctrl.GetRevenueForAllAssetsBasedOnDateRangeAndGroupedByFrequency("1-1-2014", "12-31-2014")
//                                                                        as OkNegotiatedContentResult<IOrderedQueryable<RevenuePymtFreqVm>>;

//            // Assert
//            Assert.IsNotNull(revenueFreqInfo);
//            Assert.That(revenueFreqInfo.Content.Where(a => a.IncomeFrequency != ""), Is.Ordered.By("IncomeFrequency"));
//            Assert.IsTrue(revenueFreqInfo.Content.All(i => i.TotalRecvd > 0));
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_does_not_GET_any_revenue_history_using_default_YTD_criteria() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var assetRevenueHx = await _ctrl.GetRevenueHistoryForEachAssetByDates() as OkNegotiatedContentResult<IOrderedQueryable<AssetRevenueVm>>;


//            // Assert
//            Assert.IsNull(assetRevenueHx); // no 2015 revenue

//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_does_GET_all_revenue_history_using_last_6_months_as_criteria() {
//            // TODO: re-eval need to use [FromUri], when we can differentiate method calls via differing URLs, as is done here
//            // TODO: to differentiate call from GetRevenueTotalForAllAssetsByDates().
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Income/All/6-1-2014/12-31-2014") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act - 1st boolean arg used 
//            var assetRevenueHx = await _ctrl.GetRevenueHistoryForEachAssetByDates("6-1-2014", "12-31-2014")
//                                                                as OkNegotiatedContentResult<IOrderedQueryable<AssetRevenueVm>>;

//            // Assert
//            Assert.IsNotNull(assetRevenueHx); // 2014 revenue
//            Assert.IsTrue(assetRevenueHx.Content.Count(a => a.Ticker == "IBM").Equals(2));
//            Assert.That(assetRevenueHx.Content.Where(a => a.Ticker == "IBM"), Is.Ordered.By("Ticker"));
//            Assert.That(assetRevenueHx.Content.Where(a => a.Ticker == "IBM"), Is.Ordered.Descending.By("DateReceived"));
//        }

//        #endregion






//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_for_an_Asset_via_ticker_symbol_and_account() {

//            // Arrange - SUT
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM/Income/Roth-IRA") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var assetRevenue = await _ctrl.GetIncomeByAsset("IBM", "Roth-IRA") as OkNegotiatedContentResult<IOrderedQueryable<Income>>;


//            // Assert
//            Assert.IsNotNull(assetRevenue);
//            Assert.That(assetRevenue.Content.Count(), Is.GreaterThanOrEqualTo(2));
//            Assert.That(assetRevenue.Content.All(i => i.Account == "Roth-IRA"), Is.True);
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_for_an_Asset_via_ticker_account_and_dateRecvd_criteria() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM/Income/Roth-IRA/4-17-2014/6-30-2014") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var assetRevenue = await _ctrl.GetIncomeByAssetAndAccountAndDates("IBM", "Roth-IRA", "4-17-2014", "6-30-2014") as OkNegotiatedContentResult<IOrderedQueryable<Income>>;


//            // Assert
//            Assert.IsNotNull(assetRevenue);
//            Assert.That(assetRevenue.Content.Count(), Is.EqualTo(1));
//            Assert.That(assetRevenue.Content.All(i => i.Account == "Roth-IRA"), Is.True);
//            Assert.That(assetRevenue.Content.All(i => i.Url.Contains("IBM")));
//        }

//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_for_an_Asset_via_dateRecvd_criteria() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Income/10-30-2013/10-30-2014") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var assetRevenue = await _ctrl.GetIncomeByAssetAndDates("AAPL", "10-30-2013", "10-30-2014") as OkNegotiatedContentResult<IOrderedQueryable<Income>>;


//            // Assert
//            Assert.IsNotNull(assetRevenue);
//            Assert.That(assetRevenue.Content.Count(), Is.EqualTo(2));
//            Assert.That(assetRevenue.Content.All(i => i.Url.Contains("AAPL")));
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_GET_all_fake_revenue_for_an_Asset_via_ticker_symbol() {

//            // Arrange - SUT
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var assetRevenue = await _ctrl.GetIncomeByAsset("IBM") as OkNegotiatedContentResult<IOrderedQueryable<Income>>;


//            // Assert
//            Assert.IsNotNull(assetRevenue);
//            Assert.That(assetRevenue.Content.Count(), Is.GreaterThanOrEqualTo(3));
//            Assert.That(assetRevenue.Content.First().Actual, Is.EqualTo(60.99));
//            Assert.That(assetRevenue.Content.All(i => i.Actual > 0), Is.True);

//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_not_GET_fake_revenue_for_an_Asset_via_a_nonexistent_ticker_symbol() {

//            // Arrange - SUT
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/XYZ/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var assetRevenue = await _ctrl.GetIncomeByAsset("XYZ") as OkNegotiatedContentResult<IOrderedQueryable<Income>>;


//            // Assert
//            Assert.IsNull(assetRevenue);

//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_Can_not_GET_fake_revenue_for_an_valid_Asset_but_with_an_incorrect_account() {

//            // Arrange - SUT
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/VNR/Income/Roth-IRA") },
//                Configuration = new HttpConfiguration()
//            };

//            // Act 
//            var assetRevenue = await _ctrl.GetIncomeByAsset("VNR", "Roth-IRA") as OkNegotiatedContentResult<IOrderedQueryable<Income>>;


//            // Assert
//            Assert.IsNull(assetRevenue);

//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_POST_a_new_fake_Income_payment_for_an_existing_asset() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            // Mimics data sent from client.
//            var newIncome = new Income {
//                Url = "",
//                AssetId = new Guid("216d1d40-e63c-4fc8-bee9-0dddf343c0aa"),
//                Account = "IRRA",
//                DateRecvd = DateTime.UtcNow.ToString("d"),
//                Actual = 58.29M
//            };


//            // Act
//            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newIncome);
//            var incomeActionResult = await _ctrl.CreateNewIncome(newIncome) as CreatedNegotiatedContentResult<Income>;


//            // Assert
//            Assert.IsNotNull(incomeActionResult);
//            Assert.That(incomeActionResult.Content, Is.InstanceOf<Income>());
//            Assert.That(incomeActionResult.Location.AbsoluteUri, Is.EqualTo("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Income/" + newIncome.IncomeId));
//            Assert.That(incomeActionResult.Content.DateRecvd, Is.AtLeast(DateTime.UtcNow.AddMinutes(-3).ToString("d")));
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_not_POST_a_new_fake_duplicate_Income_payment_for_an_existing_asset() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            //  Duplicate pay date & Account.
//            var newIncome = new Income {
//                Url = "",
//                AssetId = new Guid("55224b18-5777-48b1-a9a1-28fb74d385f3"),
//                Account = "Roth-IRA",
//                DateRecvd = new DateTime(2014, 4, 17).ToString("d"),
//                Actual = 58.29M
//            };


//            // Act
//            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newIncome);
//            var incomeActionResult = await _ctrl.CreateNewIncome(newIncome) as CreatedNegotiatedContentResult<Income>;


//            // Assert
//            Assert.IsNull(incomeActionResult);

//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_POST_a_new_fake_Income_payment_for_an_existing_asset_with_the_same_paydate() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Income") },
//                Configuration = new HttpConfiguration()
//            };

//            // Mimics data sent from client.
//            var newIncome = new Income {
//                Url = "",
//                AssetId = new Guid("216d1d40-e63c-4fc8-bee9-0dddf343c0aa"),
//                Account = "IRRA",
//                DateRecvd = new DateTime(2013, 12, 19).ToString("d"),
//                Actual = 52.56M
//            };


//            // Act
//            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newIncome);
//            var incomeActionResult = await _ctrl.CreateNewIncome(newIncome) as CreatedNegotiatedContentResult<Income>;


//            // Assert
//            Assert.IsNotNull(incomeActionResult);
//            Assert.That(incomeActionResult.Content, Is.InstanceOf<Income>());
//            Assert.That(incomeActionResult.Location.AbsoluteUri, Is.EqualTo("http://localhost/PIMS.Web.Api/api/Asset/AAPL/Income/" + newIncome.IncomeId));
//            Assert.That(incomeActionResult.Content.DateRecvd, Is.EqualTo(new DateTime(2013, 12, 19).ToString("d")));
//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_PUT_update_a_fake_Income_for_an_existing_asset() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f") },
//                Configuration = new HttpConfiguration()
//            };

//            // First 3 fields: read-only & received from client.
//            var editedIncome = new Income {
//                Url = "http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f",
//                IncomeId = new Guid("a142835a-2eac-4070-84dd-844bc2ad3a4f"),
//                AssetId = new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9"),
//                Account = "ML-CMA",
//                Actual = 220.91M,                                   // adjusted
//                DateRecvd = new DateTime(2014, 3, 30).ToString("d")   // adjusted
//            };


//            // Act
//            //var debugJsonForFiddler = TestHelpers.ObjectToJson(editedIncome);
//            var incomeActionResult = await _ctrl.UpdateIncome(editedIncome, editedIncome.IncomeId) as OkNegotiatedContentResult<Income>;


//            // Assert
//            Assert.IsNotNull(incomeActionResult);
//            Assert.That(incomeActionResult.Content.DateRecvd, Is.EqualTo(editedIncome.DateRecvd));
//            Assert.That(incomeActionResult.Content.Actual, Is.EqualTo(220.91));

//        }


//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_not_PUT_update_a_fake_Income_for_an_existing_asset_with_a_future_payment_date() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f") },
//                Configuration = new HttpConfiguration()
//            };

//            // First 3 fields: read-only & received from client.
//            var editedIncome = new Income {
//                Url = "http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f",
//                IncomeId = new Guid("a142835a-2eac-4070-84dd-844bc2ad3a4f"),
//                AssetId = new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9"),
//                Account = "ML-CMA",
//                Actual = 216.91M,
//                DateRecvd = new DateTime(2027, 3, 30).ToString("d")   // invalid future date
//            };


//            // Act
//            //var debugJsonForFiddler = TestHelpers.ObjectToJson(editedIncome);
//            var incomeActionResult = await _ctrl.UpdateIncome(editedIncome, editedIncome.IncomeId) as OkNegotiatedContentResult<Income>;


//            // Assert
//            Assert.IsNull(incomeActionResult);

//        }



//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_not_PUT_update_a_fake_Income_for_an_existing_asset_with_an_empty_payment_date() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f") },
//                Configuration = new HttpConfiguration()
//            };

//            // Simulate normal HTTP request model binding validations, utilizing data annotations on models.
//            _ctrl.ModelState.AddModelError("dateRecvd", "");

//            var editedIncome = new Income {
//                Url = "http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f",
//                IncomeId = new Guid("a142835a-2eac-4070-84dd-844bc2ad3a4f"),
//                AssetId = new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9"),
//                Account = "ML-CMA",
//                Actual = 216.91M,
//                DateRecvd = ""
//            };


//            // Act
//            //var debugJsonForFiddler = TestHelpers.ObjectToJson(editedIncome);
//            var incomeActionResult = await _ctrl.UpdateIncome(editedIncome, editedIncome.IncomeId) as OkNegotiatedContentResult<Income>;


//            // Assert
//            Assert.IsNull(incomeActionResult);

//        }



//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_not_PUT_update_a_fake_Income_for_an_existing_asset_with_an_invalid_income_amount() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f") },
//                Configuration = new HttpConfiguration()
//            };

//            // Simulate normal HTTP request model binding validations, utilizing data annotations on models.
//            _ctrl.ModelState.AddModelError("actual", "Invalid amount");

//            // First 3 fields: read-only & received from client.
//            var editedIncome = new Income {
//                Url = "http://localhost/Pims.Web.Api/api/Asset/VNR/Income/a142835a-2eac-4070-84dd-844bc2ad3a4f",
//                IncomeId = new Guid("a142835a-2eac-4070-84dd-844bc2ad3a4f"),
//                AssetId = new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9"),
//                Account = "ML-CMA",
//                Actual = 0,
//                DateRecvd = new DateTime(2015, 1, 03).ToString("d")
//            };


//            // Act
//            //var debugJsonForFiddler = TestHelpers.ObjectToJson(editedIncome);
//            var incomeActionResult = await _ctrl.UpdateIncome(editedIncome, editedIncome.IncomeId) as OkNegotiatedContentResult<Income>;

//            // Assert
//            Assert.IsNull(incomeActionResult);
//        }



//        [Test]
//        // ReSharper disable once InconsistentNaming
//        public async Task Controller_can_DELETE_a_fake_Income_for_an_existing_asset() {
//            // Arrange 
//            _ctrl = new IncomeController(_mockIdentitySvc.Object, _mockRepoAsset.Object, _mockRepoInvestor.Object) {
//                Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost/PIMS.Web.Api/api/Asset/IBM/Income/120fdca2-4704-4db5-b63f-cea29a9ffeee") },
//                Configuration = new HttpConfiguration()
//            };


//            // Act
//            var deleteResult = await _ctrl.DeleteIncome(new Guid("120fdca2-4704-4db5-b63f-cea29a9ffeee")) as OkResult;


//            // Assert
//            Assert.IsNull(deleteResult);

//        }


//    }
//}


