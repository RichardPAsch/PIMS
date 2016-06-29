using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FluentNHibernate.Conventions;
using FluentNHibernate.Utils;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.Repositories;
using PIMS.Core.Models.ViewModels;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{
	[RoutePrefix("api/Asset/{ticker}/Income")]
	//[Authorize] // temp comment for Fiddler testing 3.22.16
	public class IncomeController : ApiController
	{
		private static IGenericRepository<Asset> _repositoryAsset;
		private static IGenericRepository<Investor> _repositoryInvestor;
		private static IGenericRepository<Income> _repository;
		private readonly IPimsIdentityService _identityService;
		private IList<RevenueYtdVm> _tempListing = new List<RevenueYtdVm>();
		private int _counter = 1;
		private decimal _runningYtdTotal;
		private decimal[] _incomeHistory;


		#region Development Notes:
			// XLS* = "RevenueAndAverages" "F-Done" = Fiddler tested Ok
			// TODO: - (C)reate   DONE 
			// TODO:   (R)etreive-Search : query conditions:
			//              ------------- existing SQL-based queries -----------------------------------------
			//              1. Show TOTAL revenue for a month, or any timerange, on all Assets - //TODO: F-DONE GetRevenueTotalForAllAssetsByDates()
			//              2. Show all Assets with (Q)uarterly payments - //TODO: RE-EVAL need for this!
			//              3. Show ALL REVENUE by SELECTED ASSET - (yr,month,ticker,amt) to date. - //TODO: F-DONE GetIncomeByAsset()
			//              4. Show assets with highest payments to date.//TODO: replaced by #11
			//              5. Show assets based on yields and frequency. - //TODO: DONE via Profile controller
			//              6. Show total revenue TO DATE, ordered by amount received - //TODO: DONE
			//              7. Show all REVENUE per Asset with payment count, avg.payment, and FREQUENCY
			//              8. Show monthly revenue for each asset by dates (YTD by default). - //TODO: F-DONE GetAssetRevenueHistoryByDates()
			//              9. Show ALL ASSETS and their descriptions - //TODO: DONE via Profile controller
			//             10. Show AVERAGE MONTHLY REVENUE to date  - //TODO: obsolete
			//             11. Show total revenue for each Asset based on any date range; grouped by income frequency &
			//                 ordered by amount received, in descending order. //TODO: F-DONE GetRevenueForAllAssetsByDatesGroupedByFrequency()
			//              ------------- Non-SQL based -------------------------------
			//             12. Show calendar YTD average of all income received (see XLS*)  - //TODO: F-DONE GetYtdAverageRevenue()
			//             13. Show rolling YTD 3-mos average of all income received (see XLS*) - //TODO: F-DONE GetRevenueYtdAndRollingAverages()
			//             14. Show total revenue for last 3 months (see 9/25/14 Activity Summary design)- //TODO: F-DONE GetRevenueTotalForAllAssetsByDates()
			//             15. Show YTD total revenue received (see 9/25/14 Activity Summary design) - //TODO: F-DONE GetRevenueTotalForAllAssetsByDates()
			//              ---------------------------------------------------------------------------------------------------------------------------------
			//             12. Show last 3 months revenue  - //TODO: F-Done GetRevenueLast3Months()
			// TODO:   (U)pdate - DONE
			// TODO:   (D)elete - DONE
			// TODO: ModelState.IsValid checks with Position controller - POSTs  - DONE
			// TODO: Allow for date range as part of (R)etreive query - DONE

		#endregion


		public IncomeController(IPimsIdentityService identitySvc, IGenericRepository<Asset> repositoryAsset, IGenericRepository<Investor> repositoryInvestor, IGenericRepository<Income> repository) {
			_repositoryAsset = repositoryAsset;
			_identityService = identitySvc;
			_repositoryInvestor = repositoryInvestor;
			_repository = repository;
		}




		#region Non-Asset specific general portfolio Income query 'GET' actions:

		[HttpGet]
		[Route("~/api/Income/{startDate}/{endDate}")]
		// satisfies #1, #14, #15
		public async Task<IHttpActionResult> GetRevenueTotalForAllAssetsByDates(string startDate, string endDate)
		{
			//todo: Fiddler Ok: 5-25
			_repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
			var currentInvestor = _identityService.CurrentUser;

			if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
				return BadRequest(string.Format("Invalid start and/or end date received for Income data retreival."));

			var fromDate = DateTime.Parse(startDate);
			var toDate = DateTime.Parse(endDate);

			if (toDate < fromDate || fromDate > toDate || fromDate == toDate)
				return BadRequest(string.Format("Invalid beginning and/or end date(s) submitted for Income data retreival."));

			var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	  .SelectMany(a => a.Revenue)
																	  .Where(i => i.DateRecvd >= fromDate && i.DateRecvd <= toDate)
																	  .AsQueryable());

			if (matchingIncome.Any())
				return Ok(matchingIncome.Sum(i => i.Actual));

			return BadRequest(string.Format("No Income found matching dates: {0} to {1} ", startDate, endDate));

		}


		[HttpGet]
		[Route("~/api/Income/YTDAverage")]
		// satisfies #12
		public async Task<IHttpActionResult> GetYtdAverageRevenue()
		{
			//todo: Fiddler Ok: 5-26
			var currentInvestor = _identityService.CurrentUser;
			var fromDate = new DateTime(DateTime.UtcNow.Year - 1, 1, 1);
			var toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));

			var revenueListing = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim())) 
																	   .AsQueryable()
																	   .SelectMany(a => a.Revenue).Where(r => r.DateRecvd >= fromDate  &&
																											  r.DateRecvd <= toDate));

			var ytdAvg = double.Parse(revenueListing.Average(i => i.Actual).ToString(CultureInfo.InvariantCulture));
			if (revenueListing.Any())
				return Ok(Math.Round(ytdAvg, 2));

			return BadRequest(string.Format("No YTD Income found for : {0} ", currentInvestor));
		}

		[HttpGet]
		[Route("~/api/Income/last3Months")]
		// satisfies #13
		public async Task<IHttpActionResult> GetRevenueLast3Months()
		{
			//todo: Fiddler Ok: 6-10
			var currentInvestor = _identityService.CurrentUser;
			DateTime dateBeg;
			DateTime dateEnd;
			string lastDayOfMonth;

			if (DateTime.Now.Month >= 1 && DateTime.Now.Month <= 3) {
				lastDayOfMonth = DateTime.DaysInMonth(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month).ToString(CultureInfo.InvariantCulture);
				dateBeg = DateTime.Parse(DateTime.Now.AddMonths(-3).Month + "/1/" + DateTime.Now.AddMonths(-3).Year);
				dateEnd = DateTime.Parse(DateTime.Now.AddMonths(-1).Month + "/" + lastDayOfMonth + "/" + DateTime.Now.AddMonths(-1).Year);

			} else {
				lastDayOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month).ToString(CultureInfo.InvariantCulture);
				dateBeg = DateTime.Parse(DateTime.Now.AddMonths(-3).Month + "/1/" + DateTime.Now.Year);
				dateEnd = DateTime.Parse(DateTime.Now.AddMonths(-1).Month + "/" + lastDayOfMonth + "/" + DateTime.Now.Year);
			}

			var revenueLast3Months = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																		   .SelectMany(a => a.Revenue)
																		   .AsQueryable()
																		   .Where(r => r.DateRecvd >= dateBeg && r.DateRecvd <= dateEnd)
																		   .GroupBy (a => a.DateRecvd.Month,
																						  a => a.Actual,
																						  (groupkey, revenueTotal) => new {
																													  incomeMonth = groupkey,
																													  incomeAmt = CalculateMonthlyTotalRevenue(revenueTotal)
																										  }));

			IList<RevenueLast3MonthsVm> prior3MonthsRevenue = revenueLast3Months.Select(record => new RevenueLast3MonthsVm {
				RevenueMonth = record.incomeMonth,
				RevenueAmount = record.incomeAmt
			}).ToList();

			if (!prior3MonthsRevenue.Any())
				return BadRequest(string.Format("No Income found for prior 3 months."));
			
			var prior3MonthsRevenueOrdered = prior3MonthsRevenue.OrderBy(i => i.RevenueMonth);
			return Ok(prior3MonthsRevenueOrdered.AsQueryable());
		}


		[HttpGet]
		[Route("~/api/Income/Averages")]
		// satisfies #13
		public async Task<IHttpActionResult> GetRevenueYtdAndRollingAverages()
		{
			//todo: Fiddler Ok: 5-30
			var currentInvestor = _identityService.CurrentUser;
			var revenueListing = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	   .AsQueryable()
																	   .SelectMany(a => a.Revenue)
																	   .Where(r => r.DateRecvd >= DateTime.Parse("1/1/" + DateTime.Now.Year))
																	   .OrderBy(r => r.DateRecvd));
																		//.GroupBy(i => i.DateRecvd.Month) // groupKey
																		//        );
																		//.GroupBy(i => i.DateRecvd.Month, // groupKey
																		//         i => i.Actual,
																		//            (groupKey, revenueTotal) => new RevenueYtdVm
																		//                     {
																		//                        MonthRecvd = groupKey,
																		//                        AmountRecvd = revenueTotal.Count()
																		//  ** Unresolved Nh ERROR : Count() - ok, Sum() - not ok. **
																		//                refer to : https://nhibernate.jira.com/browse/NH-3681
																		//                Error    : "Query Source could not be identified":
																		//                            ItemName = <generated>_1, 
																		//                            ItemType = System.Decimal, 
																		//                            Expression = from Decimal <generated>_1 in [asset]
																		//                         */
																		//                     }
																		//        )
																		//.OrderBy(r => r.MonthRecvd));
																		//);

			var ytdAveragesListing = CalculateRevenueTotals(revenueListing);

			_incomeHistory = new decimal[ytdAveragesListing.Count()];
			ytdAveragesListing.ToList().ForEach(CalculateAverages);
			//_counter = 1;
			//_runningYtdTotal = 0;
			//_incomeHistory = new decimal[0];

		   if (ytdAveragesListing.Any())
				return Ok(ytdAveragesListing);
			
			return BadRequest(string.Format("No Income found for calculating revenue averages. "));
		}


		[HttpGet]
		[Route("~/api/Income/Freq/{begDate}/{endDate}")]
		// satisfies #11
		public async Task<IHttpActionResult> GetRevenueForAllAssetsByDatesGroupedByFrequency(string begDate, string endDate)
		{
			//todo: Fiddler Ok: 6-8-15
			var currentInvestor = _identityService.CurrentUser;

			if (string.IsNullOrWhiteSpace(begDate) || string.IsNullOrWhiteSpace(endDate))
				return BadRequest(string.Format("Invalid, or missing required date range information."));

			var fromDate = Convert.ToDateTime(begDate);
			var toDate = Convert.ToDateTime(endDate);

			if (toDate < fromDate || fromDate > toDate || fromDate == toDate)
				return BadRequest(string.Format("Invalid beginning and/or end date(s) submitted for Income retreival."));

			var filteredIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	   .SelectMany(a => a.Revenue).Where(r => r.DateRecvd  >= fromDate &&
																											  r.DateRecvd <= toDate)
																	   .AsQueryable()
																	   .GroupBy(i => new { i.IncomeAsset.Profile.DividendFreq, i.IncomeAsset.Profile.TickerSymbol },
																				i => i.Actual,
																				(groupKey, revenueAmounts) => new {
																					Frequency = groupKey.DividendFreq,
																					Ticker = groupKey.TickerSymbol,
																					// ReSharper disable once PossibleMultipleEnumeration
																					MonthlyAmount = CalculateMonthlyTotalRevenue(revenueAmounts),
																					// ReSharper disable once PossibleMultipleEnumeration
																					AvgMonthlyAmount = CalculateMonthlyAvgRevenue(revenueAmounts, fromDate, toDate)
																				})
													   );
			// Bypass Nh "Unable to implement method" exception, due to CalculateMonthly... method references.
			IList<RevenuePymtFreqVm> filteredIncomeResults = filteredIncome.Select(record => new RevenuePymtFreqVm
																						  {
																							  IncomeFrequency = record.Frequency, 
																							  TickerSymbol = record.Ticker, 
																							  MonthlyRevenue = record.MonthlyAmount, 
																							  AvgMonthlyRevenue = record.AvgMonthlyAmount
																						  }).ToList();

			var filteredIncomeResultsSorted = filteredIncomeResults.OrderBy(r => r.TickerSymbol);
			if (filteredIncomeResultsSorted.Any())
				return Ok(filteredIncomeResultsSorted.AsQueryable());

			return BadRequest(string.Format("Error retreiving revenue data, or no revenue found matching dates: {0} to {1} ", fromDate, toDate));
		}


		[HttpGet]
		[Route("~/api/Income/All/{dateFrom?}/{dateTo?}")]
		// satisfies #8
		public async Task<IHttpActionResult> GetAssetRevenueHistoryByDates(string dateFrom = "", string dateTo = "")
		{
			//todo: Fiddler Ok: 6-8-15
			var currentInvestor = _identityService.CurrentUser;
			DateTime fromDate;
			DateTime toDate;

			// YTD by default.
			if (!string.IsNullOrWhiteSpace(dateFrom) && !string.IsNullOrWhiteSpace(dateTo)) {
				fromDate = Convert.ToDateTime(dateFrom);
				toDate = Convert.ToDateTime(dateTo);
			} else {
				fromDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
				toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));
			}


			var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	   .SelectMany(a => a.Revenue).Where(r => r.DateRecvd  >= fromDate &&
																											  r.DateRecvd <= toDate)
																	   .AsQueryable()
																	   .Select(i => new  {
																		   TickerSymbol = ParseUrlForTicker(i.Url),
																		   DateReceived = i.DateRecvd,
																		   AmountRecvd = i.Actual
																	   })); 
																	
			IList<AssetRevenueVm> projectedMatchingIncome = matchingIncome.Select(record => new AssetRevenueVm {
																									Ticker = record.TickerSymbol,
																									AmountReceived = record.AmountRecvd,
																									DateReceived = record.DateReceived.ToShortDateString()
			}).ToList();
			var projectedMatchingIncomeOrdered = projectedMatchingIncome.OrderBy(i => i.Ticker).ThenByDescending(i => i.DateReceived);

			if (projectedMatchingIncomeOrdered.Any())
				return Ok(projectedMatchingIncomeOrdered.AsQueryable());

			return BadRequest(string.Format("No Income found matching dates: {0} to {1} ", dateFrom, dateTo));
		}
		

		//[HttpGet]
		//[Route("")]
		//// satisfies #3
		//public async Task<IHttpActionResult> GetSingleAssetRevenueForLastTwoYears(string ticker)
		//{
		//    _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
		//    var currentInvestor = _identityService.CurrentUser;

		//    var fromDate = Convert.ToDateTime(DateTime.UtcNow.AddYears(-2).ToString("d"));
		//    var toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));

		//    var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim() &&
		//                                                                              a.Profile.TickerSymbol.Contains(ticker.Trim()))
		//                                                               .AsQueryable()
		//                                                               .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
		//                                                                                                      Convert.ToDateTime(r.DateRecvd) <= toDate)
		//                                                               .Select( i => new AssetRevenueVm
		//                                                                       {
		//                                                                           Ticker = ParseUrlForTicker(i.Url),
		//                                                                           AmountReceived = i.Actual,
		//                                                                           DateReceived = i.DateRecvd
		//                                                                       })
		//                                                               .OrderByDescending(i => Convert.ToDateTime(i.DateReceived))
		//                                                                );

		//    if (matchingIncome.Any())
		//        return Ok(matchingIncome);

		//    return BadRequest(string.Format("No Income found for the last 2 years "));

		//}
		

		//[HttpGet]
		//[Route("~/api/Income")]
		//// satisfies #6
		//public async Task<IHttpActionResult> GetTotalRevenueAndPymtFreqByAssetToDate([FromUri] bool completeRevenue)
		//{
		//    var currentInvestor = _identityService.CurrentUser;
		//    var fromDate = new DateTime(1990, 1, 1);
		//    var toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));

		//    // Inner sequence.
		//    var revenueTotals = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
		//                                                               .AsQueryable()
		//                                                               .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
		//                                                                                                      Convert.ToDateTime(r.DateRecvd) <= toDate)
		//                                                               .GroupBy(i => i.AssetId, 
		//                                                                        i => i.Actual,
		//                                                                        (groupKey, revenueTotal) => new
		//                                                                                                    {
		//                                                                                                        Key = groupKey,
		//                                                                                                        revenueSum = revenueTotal.Sum()
		//                                                                                                     }) 
		//                                              );

		//    // Outer seguence.
		//    var allAssets = await Task.FromResult(_repositoryAsset.Retreive(x => x.AssetId != default(Guid)));
		//    var assetResults = allAssets.Join(revenueTotals, asset => asset.AssetId, y => y.Key,
		//                                                                          (asset, y) => new TotalRevenueAndPymtFreqByAssetVm
		//                                                                                    {
		//                                                                                        Ticker = asset.Profile.TickerSymbol, 
		//                                                                                        TotalIncome = y.revenueSum,
		//                                                                                        IncomeFrequency = asset.Profile.DividendFreq,
		//                                                                                        DatePurchased = asset.Positions.First().PurchaseDate
		//                                                                                    }
		//                                     )
		//                                 .OrderByDescending(vm => vm.TotalIncome);


		//    if (assetResults.Any())
		//        return Ok(assetResults);

		//    return BadRequest(string.Format("Unable to fetch Asset revenue. "));

		//}
		


		//[HttpGet]
		//[Route("~/api/Profile/All")]
		//// satisfies #5, #9
		//public async Task<IHttpActionResult> GetProfileInfoForAssets()
		//{
		//    // Includes Assets missing some or all dividend info.
		//    var currentInvestor = _identityService.CurrentUser;
		//    var filteredProfiles = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
		//                                                               .AsQueryable()
		//                                                               .Where(a => a.Profile.AssetId != default(Guid))
		//                                                               .Select(p => new AssetProfilesVm {
		//                                                                   Ticker = p.Profile.TickerSymbol,
		//                                                                   TickerDescription = p.Profile.TickerDescription,
		//                                                                   DividendYield = (decimal)p.Profile.DividendYield,
		//                                                                   DividendRate = p.Profile.Price
		//                                                               })
		//                                                               .OrderBy(p => p.Ticker));

		//    if (filteredProfiles.Any())
		//        return Ok(filteredProfiles);

		//    return BadRequest(string.Format("Unable to retreive asset dividend data. "));
		//}



		#endregion






		[HttpGet]
		[Route("{accountType?}")]
		public async Task<IHttpActionResult> GetIncomeByAsset(string ticker, string accountType = "")
		{
			_repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
			var currentInvestor = _identityService.CurrentUser;

			// Fiddler debugging
			if (currentInvestor == null)
				currentInvestor = "rpasch2@rpclassics.net";

			IQueryable<AssetRevenueVm> matchingIncome = null;

			if (accountType.IsEmpty())
			{     
				// TODO: RETEST Income/Position CRUD operations as result of new Income/Position NH mappings.
				matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
																					  a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																		   .SelectMany(a => a.Revenue)
																		   .AsQueryable()
																		   .OrderByDescending(x => x.DateRecvd)
																		   .AsQueryable()
																		   .Select(r => new AssetRevenueVm {
																			   Ticker = ticker,
																			   AccountType = r.IncomePosition.Account.AccountTypeDesc,
																			   DateReceived = r.DateRecvd.ToShortDateString(),
																			   AmountReceived = r.Actual
																		   }));
				
			} else {
				matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
																					  a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	  .AsQueryable()
																	  .SelectMany(a => a.Revenue).Where(r => r.IncomePosition.Account.AccountTypeDesc.Trim() == accountType.Trim())
																	  .Select(r => new AssetRevenueVm {
																		  Ticker = ticker,
																		  AccountType = r.IncomePosition.Account.AccountTypeDesc,
																		  DateReceived = r.DateRecvd.ToShortDateString(),
																		  AmountReceived = r.Actual
																	  })
																	  .OrderByDescending(r => r.AccountType));
			}

			if (matchingIncome != null)
				return Ok(matchingIncome);

			return BadRequest(accountType == ""
				? string.Format("No Income found matching Asset: {0} ", ticker)
				: string.Format("No Income found matching Asset: {0} under Account: {1}", ticker, accountType.Trim()));

	
		}


		[HttpGet]
		[Route("~/api/Income/{year}/{month}/{placeholder}")]
		public async Task<IHttpActionResult> GetIncomeByYearAndMonth(string year, string month, string placeholder) {
			// placeholder only used as differentiator to similiar route pattern in GetIncomeByAssetAndDates().
			_repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
			if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(month))
				return BadRequest(string.Format("Invalid year and/or month received, for monthly Income data retreival."));

			var currentInvestor = Utilities.GetInvestor(_identityService);

			var begDate = month + "/1/" + year;
			var endDate = month + "/" + Utilities.GetDaysInMonth(int.Parse(year), int.Parse(month)) + "/" + year;
			var incomeRecords = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	  .SelectMany(a => a.Revenue).Where(r => r.DateRecvd >= DateTime.Parse(begDate) &&
																											 r.DateRecvd <= DateTime.Parse(endDate))
																	  .AsQueryable());

			var monthlyTotal = incomeRecords.Select(i => new RevenueMonthAndYearVm {
															RevenueAmount = decimal.Parse(incomeRecords.Sum(r => r.Actual).ToString("##,###.##")),
															RevenueMonth = int.Parse(month),
															RevenueYear = int.Parse(year)})
											.ToList().Skip(incomeRecords.Count()-1);

			if (incomeRecords.Any())
				return Ok(monthlyTotal);

			return BadRequest(string.Format("No Income found for year: {0} and month: {1}", year, month));
		}


		[HttpGet]
		[Route("~/api/Income/{startDate}/{endDate}")]
		public async Task<IHttpActionResult> GetIncomeByAssetAndDates(string ticker, string startDate, string endDate)
		{
			_repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
			if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
				return BadRequest(string.Format("Invalid start and/or end date received for Income data retreival."));

			var fromDate = DateTime.Parse(startDate);
			var toDate = DateTime.Parse(endDate);

			if(toDate < fromDate || fromDate > toDate)
				return BadRequest(string.Format("Invalid beginning and/or end date(s) submitted for Income data retreival."));

			var currentInvestor = _identityService.CurrentUser;

			var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
																					  a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	   .SelectMany(a => a.Revenue).Where(r => r.DateRecvd >= fromDate && r.DateRecvd <= toDate)
																	   .OrderByDescending(r => r.DateRecvd)
																	   .AsQueryable()
																	   .Select(r => new AssetRevenueVm
																							{
																								Ticker = ticker,
																								AccountType = r.IncomePosition.Account.AccountTypeDesc,
																								DateReceived = r.DateRecvd.ToShortDateString(),
																								AmountReceived = r.Actual
																							 }));

			if (matchingIncome.Any())
				return Ok(matchingIncome);

			return BadRequest(string.Format("No Income found matching Asset: {0}, with dates: {1} to {2} ", ticker, startDate, endDate));

		}


		[HttpGet]
		[Route("{accountType}/{startDate}/{endDate}")]
		public async Task<IHttpActionResult> GetIncomeByAssetAndAccountAndDates(string ticker, string accountType, string startDate, string endDate)
		{
			if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate) || string.IsNullOrWhiteSpace(accountType))
				return BadRequest(string.Format("Missing date(s) and/or AccountType for Income data retreival."));

			_repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
			if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
				return BadRequest(string.Format("Invalid start and/or end date received for Income data retreival."));

			var fromDate = DateTime.Parse(startDate);
			var toDate = DateTime.Parse(endDate);

			if (toDate < fromDate || fromDate > toDate)
				return BadRequest(string.Format("Invalid beginning and/or end date(s) submitted for Income data retreival."));

			var currentInvestor = _identityService.CurrentUser;

			var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
																					  a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	   .SelectMany(a => a.Revenue).Where(r => r.DateRecvd >= fromDate && 
																											  r.DateRecvd <= toDate &&
																											  r.IncomePosition.Account.AccountTypeDesc.Trim() == accountType.Trim())
																	   .OrderByDescending(r => r.DateRecvd)
																	   .AsQueryable()
																	   .Select(r => new AssetRevenueVm {
																		   Ticker = ticker,
																		   AccountType = r.IncomePosition.Account.AccountTypeDesc,
																		   DateReceived = r.DateRecvd.ToShortDateString(),
																		   AmountReceived = r.Actual
																	   }));
		   
			if (matchingIncome.Any())
				return Ok(matchingIncome);

			return BadRequest(string.Format("No Income found matching Asset: {0}, Account: {1}, with income for dates: {2} to {3} ", ticker, accountType, startDate, endDate));

		}
		


		[HttpPost]
		[Route("")]
		public async Task<IHttpActionResult> CreateNewIncome2([FromBody]IncomeVm incomeData)
		{
			if (!ModelState.IsValid) {
				return ResponseMessage(new HttpResponseMessage {
					StatusCode = HttpStatusCode.BadRequest,
					ReasonPhrase = "Invalid data received for new Income creation. " + ModelState.Keys.First()
				});
			}

			var currentInvestorId = Utilities.GetInvestorId(_repositoryInvestor, _identityService.CurrentUser);

			if (ControllerContext.Request != null)
				incomeData.Url = ControllerContext.Request.RequestUri.AbsoluteUri;
			
			var ticker = ParseUrlForTicker(incomeData.Url);

			// Asset should have already been persisted, whether as part of new Asset creation, or created previously.
			// Created Income, whether first entry or adding, will ALWAYS be associated with a required Position-AccountType.
			var matchingPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker.ToUpper().Trim() &&
																						a.InvestorId == currentInvestorId)
																		 .SelectMany(a => a.Positions)
																		 .Where(p => p.Account.AccountTypeDesc.Trim() == incomeData.AcctType.Trim())
																		 .AsQueryable());
			
			if (!matchingPosition.Any())
				return BadRequest(string.Format("No matching Position-Account Type ({0}) found to record income against, for Asset {1}  ", 
																				incomeData.AcctType.Trim().ToUpper(), 
																				ticker));

			incomeData.AssetId = matchingPosition.First().PositionAssetId;
			incomeData.ReferencedPositionId = matchingPosition.First().PositionId;
			

			if (matchingPosition.First().PositionIncomes.Any())
			{
				var existingIncome = matchingPosition.First().PositionIncomes.AsQueryable();
				if (existingIncome.Any()) {
					// Ignore any possible timestamps while looping thru existing income.
					if (Enumerable.Any(existingIncome, record => record.DateRecvd.Month == DateTime.Parse(incomeData.DateReceived.ToString()).Month &&
																 record.DateRecvd.Day == DateTime.Parse(incomeData.DateReceived.ToString()).Day &&
																 record.DateRecvd.Year == DateTime.Parse(incomeData.DateReceived.ToString()).Year)) {
						return ResponseMessage(new HttpResponseMessage {
							StatusCode = HttpStatusCode.Conflict,
							ReasonPhrase = "Duplicate income found for AssetId: "
												+ incomeData.AssetId
												+ " recorded on "
												+ string.Format("{0:M/dd/yyyy}", incomeData.DateReceived)
						});
					}
				}
			}

		
			var newLocation = incomeData.Url;
			var newIncome = MapVmToIncome(incomeData);
			var isCreated = await Task.FromResult(_repository.Create(newIncome));
			if (!isCreated)
				return BadRequest(string.Format("Unable to add Income for: {0} ", ticker.ToUpper()));

			return Created(newLocation + "/" + newIncome.IncomeId, newIncome);  
		}


		//[System.Web.Http.HttpPut]
		[HttpPatch]
		[Route("~/api/Income/{incomeId}")]
		public async Task<IHttpActionResult> UpdateIncome([FromBody]IncomeVm editedIncome, Guid incomeId)
		{
			if (!ModelState.IsValid || !IsValidDate(editedIncome.DateReceived.ToString())){
				return ResponseMessage(new HttpResponseMessage {
														StatusCode = HttpStatusCode.BadRequest,
														ReasonPhrase = "Invalid data received for Income update."
													});
			}

			var currentInvestor = _identityService.CurrentUser;
			var ticker = ParseUrlForTicker(ControllerContext.Request.RequestUri.AbsoluteUri);
			var existingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																	   .AsQueryable()
																	   .SelectMany(a => a.Revenue).Where(i => i.IncomeId == incomeId));

			if (!existingIncome.Any())
				return BadRequest(string.Format("No matching Income found to update, for {0}", ticker ));

			// Satisfy FK constraints.
			editedIncome.AssetId = existingIncome.First().AssetId;
			editedIncome.ReferencedPositionId = existingIncome.First().IncomePositionId;

			editedIncome.Url = string.IsNullOrEmpty(existingIncome.First().Url) 
				? ControllerContext.Request.RequestUri.ToString() 
				: existingIncome.First().Url;

			// Partial Income replacement.
			var isUpdated = await Task.FromResult(_repository.Update(MapVmToIncomeEdits(editedIncome), incomeId));
			if (!isUpdated)
				return BadRequest(string.Format("Unable to edit Income for Asset {0}",  ticker));

			return Ok(editedIncome);
		}


		[HttpDelete]
		[Route("~/api/Income/{incomeId}")]
		public async Task<IHttpActionResult> DeleteIncome(Guid incomeId)
		{
			var currentInvestor = _identityService.CurrentUser;
			var ticker = ParseUrlForTicker(ControllerContext.Request.RequestUri.AbsoluteUri);
			var selectedIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																				   .AsQueryable()
																				   .SelectMany(a => a.Revenue).Where(i => i.IncomeId == incomeId));

			if (!selectedIncome.Any())
				return BadRequest(string.Format("No matching Income Id found to delete for {0}", ticker));

			var isDeleted = await Task.FromResult(_repository.Delete(incomeId));

			return isDeleted 
				? Ok("Successfully deleted Income: " + incomeId) 
				: (IHttpActionResult)BadRequest("Error: unable to delete Income: " + incomeId);
		}

		/* old code
						[HttpGet]
						[Route("~/api/Income/{year}/{month}/{placeholder}")]
						public async Task<IHttpActionResult> GetIncomeByYearAndMonth(string year, string month, string placeholder)
						{
							// placeholder only used as differentiator to similiar route pattern in GetIncomeByAssetAndDates().
							_repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
							if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(month))
								return BadRequest(string.Format("Invalid year and/or month received, for monthly Income data retreival."));

							var currentInvestor = Utilities.GetInvestor(_identityService);

							var begDate = month +  "/1/" + year;
							var endDate = month + "/" + Utilities.GetDaysInMonth(int.Parse(year), int.Parse(month)) +"/" + year;
							var incomeRecords = await Task.FromResult(_repositoryAsset.Retreive(a =>  a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
																					  .SelectMany(a => a.Revenue).Where(r => r.DateRecvd >= DateTime.Parse(begDate) &&
																															 r.DateRecvd <= DateTime.Parse(endDate))
																					  .AsQueryable());

							IList<RevenueMonthAndYearVm> monthlyTotal = incomeRecords.Select(i => new RevenueMonthAndYearVm
																								 {
																									 RevenueAmount = decimal.Parse(incomeRecords.Sum(r => r.Actual).ToString("##,###.##")),
																									 RevenueMonth = int.Parse(month),
																									 RevenueYear = int.Parse(year)
																								 }).ToList();
			
							if (incomeRecords.Any())
								return Ok(monthlyTotal.Skip(1));

							return BadRequest(string.Format("No Income found for year: {0} and month: {1}", year, month));
						}
		*/




		#region Helpers

		public static string ParseUrlForTicker(string urlToParse) {
				var pos1 = urlToParse.IndexOf("Asset/", StringComparison.Ordinal) + 6;
				var pos2 = urlToParse.IndexOf("/Income", StringComparison.Ordinal); // Position or Income
				return urlToParse.Substring(pos1, pos2 - pos1);
			}


			private static bool IsValidDate(string dateToCheck) {
				// ReSharper disable once SimplifyConditionalTernaryExpression
				return Convert.ToDateTime(dateToCheck) > DateTime.UtcNow ? false : true;
			}


			private static IQueryable<RevenueYtdVm> CalculateRevenueTotals(IQueryable<Income> recvdIncome)
			{
				IList<RevenueYtdVm> averages = new List<RevenueYtdVm>();
				var currentMonth = 0;
				var total = 0M;
				var counter = 0;
				
				foreach (var income in recvdIncome)
				{
					if (currentMonth != DateTime.Parse(income.DateRecvd.ToString(CultureInfo.InvariantCulture)).Month)
					{
						// Last record for currently processed month.
						if (total > 0)
						{
							averages.Add(new RevenueYtdVm
										 {
											 AmountRecvd = total,
											 MonthRecvd = currentMonth
										 });
							total = 0M;
						}
					}
				  
					currentMonth = DateTime.Parse(income.DateRecvd.ToString(CultureInfo.InvariantCulture)).Month;
					total += income.Actual;
					counter++;

					// Add last record.
					if (counter == recvdIncome.Count())
					{
						averages.Add(new RevenueYtdVm {
							AmountRecvd = total,
							MonthRecvd = DateTime.Parse(income.DateRecvd.ToString(CultureInfo.InvariantCulture)).Month
						});
					}

				}

				return averages.AsQueryable();
			}


			private void CalculateAverages(RevenueYtdVm item) {
				// YTD & 3Mos rolling averages.
				_runningYtdTotal += item.AmountRecvd;
				_incomeHistory[_counter - 1] = item.AmountRecvd;
				item.YtdAverage = Math.Round(_runningYtdTotal / _counter, 2);
				if (_counter >= 3) {
					item.Rolling3MonthAverage = Math.Round((item.AmountRecvd + _incomeHistory[_counter - 2] + _incomeHistory[_counter - 3]) / 3, 2);
				}
				_tempListing.Add(item);
				_counter += 1;
			}


			private static Income MapVmToIncome(IncomeVm sourceVm) {
				return new Income {
					Account = sourceVm.AcctType,
					Actual = sourceVm.AmountRecvd,
					DateRecvd = sourceVm.DateReceived.HasValue ? (DateTime) sourceVm.DateReceived : DateTime.Now,
					AssetId = sourceVm.AssetId,
					Projected = sourceVm.AmountProjected,
					LastUpdate = DateTime.Now,
					IncomePositionId = sourceVm.ReferencedPositionId,
					Url = sourceVm.Url
				};
			}

			private static Income MapVmToIncomeEdits(IncomeVm sourceVm)
			{
				return new Income {
					Actual = sourceVm.AmountRecvd,
					AssetId = sourceVm.AssetId,
					IncomePositionId = sourceVm.ReferencedPositionId,
					DateRecvd = sourceVm.DateReceived.HasValue ? (DateTime)sourceVm.DateReceived : default(DateTime),
					LastUpdate = DateTime.Now,
					Url = sourceVm.Url
				};
			}


			private static decimal CalculateMonthlyTotalRevenue(IEnumerable<decimal> amounts)
			{
				return amounts.Sum();
			}


			private static decimal CalculateMonthlyAvgRevenue(IEnumerable<decimal> monthlyAmts, DateTime start, DateTime stop)
			{
				// We're only interested in date differences in MONTHS, disregarding date timespans.
				var monthsOfRevenue = Math.Abs(Math.Round(start.Subtract(stop).Days / (365.25 / 12)));
				return monthlyAmts.Sum() / int.Parse(monthsOfRevenue.ToString(CultureInfo.InvariantCulture));
			}



		#endregion

	   

	   

	}
}

