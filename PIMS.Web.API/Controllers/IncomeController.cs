using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FluentNHibernate.Conventions;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.Repositories;
using PIMS.Core.Models.ViewModels;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Asset/{ticker}/Income")]
    public class IncomeController : ApiController
    {
        private static IGenericRepository<Asset> _repositoryAsset;
        private readonly IPimsIdentityService _identityService;
        private IList<RevenueYtdVm> _tempListing = new List<RevenueYtdVm>();
        private int _counter = 1;
        private decimal _runningYtdTotal;
        private decimal[] _incomeHistory;

        #region Development Notes:
            // TODO: - (C)reate   DONE 
            // TODO:   (R)etreive-Search : query conditions:
            //              1. Show TOTAL revenue for a month, or any timerange, on all Assets - //TODO: DONE
            //              2. Show all Assets with (Q)uarterly payments - //TODO: RE-EVAL need for this!
            //              3. Show ALL REVENUE by SELECTED ASSET - (yr,month,ticker,amt) to date. - //TODO: DONE
            //              4. Show assets with highest payments to date.//TODO: replaced by #11
            //              5. Show assets based on yields and frequency. - //TODO: DONE via Profile controller
            //              6. Show total revenue TO DATE, ordered by amount received - //TODO: DONE
            //              7. Show all REVENUE per Asset with payment count, avg.payment, and FREQUENCY
            //              8. Show monthly revenue for each asset by dates (YTD by default). - //TODO: DONE 
            //              9. Show ALL ASSETS and their descriptions - //TODO: DONE via Profile controller
            //             10. Show AVERAGE MONTHLY REVENUE to date  - //TODO: obsolete
            //             11. Show total revenue for each Asset based on any date range; grouped by income frequency &
            //                 ordered by amount received, in descending order. //TODO: DONE
            //              ------------- Non-SQL based -------------------------------
            //             12. Show calendar YTD average of all income received (see XLS)  - //TODO: DONE
            //             13. Show rolling YTD 3-mos average of all income received (see XLS) - //TODO: DONE
            //             14. Show total revenue for last 3 months (see 9/25/14 Activity Summary design)- //TODO: DONE
            //             15. Show YTD total revenue received (see 9/25/14 Activity Summary design) - //TODO: DONE
            // TODO:   (U)pdate - DONE
            // TODO:   (D)elete - DONE
            // TODO: ModelState.IsValid checks with Position controller - POSTs  - DONE
            // TODO: Allow for date range as part of (R)etreive query - DONE

        #endregion
        

        public IncomeController(IPimsIdentityService identitySvc, IGenericRepository<Asset> repositoryAsset) {
            _repositoryAsset = repositoryAsset;
            _identityService = identitySvc;
        }




        #region Non-Asset specific general Income query actions:

        [HttpGet]
        [Route("~/api/Income/{startDate}/{endDate}")]
        // satisfies #1, #14, #15
        public async Task<IHttpActionResult> GetRevenueTotalForAllAssetsByDates(string startDate, string endDate)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                return BadRequest(string.Format("Invalid start and/or end date received for monthly Income data retreival."));

            // Convert & format dates.
            var fromDate = Convert.ToDateTime(startDate);
            var toDate = Convert.ToDateTime(endDate);

            var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                        );

            // do we want to project (select) into a concrete class? Needed later for multiple output values?
            if (matchingIncome.Any())
                return Ok(matchingIncome.Sum(i => i.Actual));

            return BadRequest(string.Format("No Income found matching dates: {0} to {1} ", startDate, endDate));

        }


        [HttpGet]
        [Route("~/api/Income/Averages")]
        // satisfies #12, #13
        public async Task<IHttpActionResult> GetRevenueYtdMonthlyAverages()
        {
            var currentInvestor = _identityService.CurrentUser;
            var fromDate = new DateTime(DateTime.UtcNow.Year-1, 1, 1);
            var toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));

            var revenueListing = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                       .GroupBy(i => Convert.ToDateTime(i.DateRecvd).Month, // groupKey
                                                                                i => i.Actual,
                                                                                (groupKey, revenueTotal) => new RevenueYtdVm
                                                                                                            {
                                                                                                                MonthRecvd = groupKey,
                                                                                                                AmountRecvd = revenueTotal.Sum()
                                                                                                             })
                                                                        .OrderBy(r => r.MonthRecvd)
                                                        );

            _incomeHistory = new decimal[revenueListing.Count()];
            revenueListing.ToList().ForEach(CalculateAverages);
            _counter = 1;
            _runningYtdTotal = 0;
            _incomeHistory = new decimal[0];
            
            var updatedRevenueListing = (IOrderedQueryable<RevenueYtdVm>) _tempListing.AsQueryable();

            if(updatedRevenueListing.Any())
                return Ok(updatedRevenueListing);

            return BadRequest(string.Format("No Income found for calculating revenue averages. "));
        }


        [HttpGet]
        [Route("~/api/Income/Freq/{begDate}/{endDate}")]
        // satisfies #11
        public async Task<IHttpActionResult> GetRevenueForAllAssetsBasedOnDateRangeAndGroupedByFrequency(string begDate, string endDate)
        {
            // Tough !
            var currentInvestor = _identityService.CurrentUser;

            if (string.IsNullOrWhiteSpace(begDate) || string.IsNullOrWhiteSpace(endDate))
                return BadRequest(string.Format("Invalid, or missing required date range information."));

            var fromDate = Convert.ToDateTime(begDate);
            var toDate = Convert.ToDateTime(endDate);

            var filteredIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                       .GroupBy(i => new { i.Url, i.AssetId },
                                                                                i =>  i.Actual,
                                                                                (groupKey, incomeTotal ) => new
                                                                                                            {
                                                                                                                id = groupKey.AssetId,
                                                                                                                Ticker = ParseUrlForTicker(groupKey.Url),
                                                                                                                TotalRecvd = incomeTotal.Sum()
                                                                                                            })
                                                       );

            var filteredProfiles = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                         .AsQueryable()
                                                                         .Select(a => a.Profile));

            var finalResults = filteredIncome.Join(filteredProfiles, i => i.id,
                                                                     p => p.AssetId, (income, profile) => new {
                                                                         Frequency = profile.DividendFreq,
                                                                         Ticker = profile.TickerSymbol,
                                                                         AmountRecvd = income.TotalRecvd
                                                                     })
                                             .GroupBy(res => new { res.Ticker, res.Frequency }, res => res.AmountRecvd,
                                                                                                (groupKey, totals) => new RevenuePymtFreqVm
                                                                                                                    {
                                                                                                                        TickerSymbol = groupKey.Ticker,
                                                                                                                        IncomeFrequency = groupKey.Frequency, 
                                                                                                                        TotalRecvd = totals.Sum()
                                                                                                                    }
                                                     ).OrderBy(res2 => res2.IncomeFrequency);


            if (finalResults.Any())
                return Ok(finalResults);

            return BadRequest(string.Format("Unable to retreive revenue stats, or no revenue found matching dates: {0} to {1} ", fromDate, toDate));
            
        }
        

        [HttpGet]
        [Route("~/api/Income/All/{dateFrom?}/{dateTo?}")]
        // satisfies #8
        public async Task<IHttpActionResult> GetRevenueHistoryForEachAssetByDates(string dateFrom = "", string dateTo = "")
        {
            var currentInvestor = _identityService.CurrentUser;
            DateTime fromDate;
            DateTime toDate;

            // YTD by default.
            if (!string.IsNullOrWhiteSpace(dateFrom) && !string.IsNullOrWhiteSpace(dateTo))
            {
                fromDate = Convert.ToDateTime(dateFrom);
                toDate = Convert.ToDateTime(dateTo);
            }
            else
            {
                fromDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
                toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));
            }
 
            var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                       .Select(i => new AssetRevenueVm
                                                                                    {
                                                                                        Ticker = ParseUrlForTicker(i.Url),
                                                                                        DateReceived = i.DateRecvd,
                                                                                        AmountReceived = i.Actual
                                                                                    })
                                                                              .OrderBy(i => i.Ticker).ThenByDescending(i => i.DateReceived)
                                                                        );
            if (matchingIncome.Any())
                return Ok(matchingIncome);

            return BadRequest(string.Format("No Income found matching dates: {0} to {1} ", dateFrom, dateTo));
        }
        

        [HttpGet]
        [Route("")]
        // satisfies #3
        public async Task<IHttpActionResult> GetSingleAssetRevenueForLastTwoYears(string ticker)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            var fromDate = Convert.ToDateTime(DateTime.UtcNow.AddYears(-2).ToString("d"));
            var toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));

            var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim() &&
                                                                                      a.Profile.TickerSymbol.Contains(ticker.Trim()))
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                       .Select( i => new AssetRevenueVm
                                                                               {
                                                                                   Ticker = ParseUrlForTicker(i.Url),
                                                                                   AmountReceived = i.Actual,
                                                                                   DateReceived = i.DateRecvd
                                                                               })
                                                                       .OrderByDescending(i => Convert.ToDateTime(i.DateReceived))
                                                                        );

            if (matchingIncome.Any())
                return Ok(matchingIncome);

            return BadRequest(string.Format("No Income found for the last 2 years "));

        }
        

        [HttpGet]
        [Route("~/api/Income")]
        // satisfies #6
        public async Task<IHttpActionResult> GetTotalRevenueAndPymtFreqByAssetToDate([FromUri] bool completeRevenue)
        {
            var currentInvestor = _identityService.CurrentUser;
            var fromDate = new DateTime(1990, 1, 1);
            var toDate = Convert.ToDateTime(DateTime.UtcNow.ToString("d"));

            // Inner sequence.
            var revenueTotals = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                       .GroupBy(i => i.AssetId, 
                                                                                i => i.Actual,
                                                                                (groupKey, revenueTotal) => new
                                                                                                            {
                                                                                                                Key = groupKey,
                                                                                                                revenueSum = revenueTotal.Sum()
                                                                                                             }) 
                                                      );

            // Outer seguence.
            var allAssets = await Task.FromResult(_repositoryAsset.Retreive(x => x.AssetId != default(Guid)));
            var assetResults = allAssets.Join(revenueTotals, asset => asset.AssetId, y => y.Key,
                                                                                  (asset, y) => new TotalRevenueAndPymtFreqByAssetVm
                                                                                            {
                                                                                                Ticker = asset.Profile.TickerSymbol, 
                                                                                                TotalIncome = y.revenueSum,
                                                                                                IncomeFrequency = asset.Profile.DividendFreq,
                                                                                                DatePurchased = asset.Positions.First().PurchaseDate
                                                                                            }
                                             )
                                         .OrderByDescending(vm => vm.TotalIncome);


            if (assetResults.Any())
                return Ok(assetResults);

            return BadRequest(string.Format("Unable to fetch Asset revenue. "));

        }
        


        [HttpGet]
        [Route("~/api/Profile/All")]
        // satisfies #5, #9
        public async Task<IHttpActionResult> GetProfileInfoForAssets()
        {
            // Includes Assets missing some or all dividend info.
            var currentInvestor = _identityService.CurrentUser;
            var filteredProfiles = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .Where(a => a.Profile.AssetId != default(Guid))
                                                                       .Select(p => new AssetProfilesVm {
                                                                           Ticker = p.Profile.TickerSymbol,
                                                                           TickerDescription = p.Profile.TickerDescription,
                                                                           DividendYield = (decimal)p.Profile.DividendYield,
                                                                           DividendRate = p.Profile.Price
                                                                       })
                                                                       .OrderBy(p => p.Ticker));

            if (filteredProfiles.Any())
                return Ok(filteredProfiles);

            return BadRequest(string.Format("Unable to retreive asset dividend data. "));
        }




        #endregion






        [HttpGet]
        [Route("{accountType?}")]
        public async Task<IHttpActionResult> GetIncomeByAsset(string ticker, string accountType = "")
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;
            IOrderedQueryable<Income> matchingIncome;

            if (accountType.IsEmpty())
            {
                matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
                                                                                                    a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                    .AsQueryable()
                                                                                    .SelectMany(a => a.Revenue)
                                                                                    .OrderByDescending(r => r.DateRecvd));
            }
            else
            {
                matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
                                                                                                    a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                    .AsQueryable()
                                                                                    .SelectMany(a => a.Revenue).Where(r => r.Account == accountType.Trim())
                                                                                    .OrderByDescending(r => r.DateRecvd));
            }
           

            if (matchingIncome.Any())
                return Ok(matchingIncome);

            return BadRequest(accountType == "" 
                ? string.Format("No Income found matching Asset: {0} ", ticker) 
                : string.Format("No Income found matching Asset: {0} under Account: {1}", ticker, accountType.Trim()));
        }


        [HttpGet]
        [Route("{accountType}/{startDate}/{endDate}")]
        public async Task<IHttpActionResult> GetIncomeByAssetAndAccountAndDates(string ticker, string accountType, string startDate, string endDate)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            if(string.IsNullOrWhiteSpace(accountType) || string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                return BadRequest(string.Format("Invalid account, start date, and/or end date received for Income data retreival. "));

            // Convert & format dates.
            var fromDate = Convert.ToDateTime(startDate); 
            var toDate = Convert.ToDateTime(endDate);
            
            var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
                                                                                      a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => r.Account == accountType.Trim() &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                       .OrderByDescending(r => r.DateRecvd));

            if (matchingIncome.Any())
                return Ok(matchingIncome);

            return BadRequest(string.Format("No Income found matching Asset: {0}, Account: {1}, with dates: {2} to {3} ", ticker, accountType, startDate, endDate));

        }


        [HttpGet]
        [Route("{startDate}/{endDate}")]
        public async Task<IHttpActionResult> GetIncomeByAssetAndDates(string ticker, string startDate, string endDate) 
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
                return BadRequest(string.Format("Invalid start and/or end date received for Income data retreival."));

            // Convert & format dates.
            var fromDate = Convert.ToDateTime(startDate);
            var toDate = Convert.ToDateTime(endDate);

            var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
                                                                                      a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                       .AsQueryable()
                                                                       .SelectMany(a => a.Revenue).Where(r => Convert.ToDateTime(r.DateRecvd) >= fromDate &&
                                                                                                              Convert.ToDateTime(r.DateRecvd) <= toDate)
                                                                       .OrderByDescending(r => r.DateRecvd));

            if (matchingIncome.Any())
                return Ok(matchingIncome);

            return BadRequest(string.Format("No Income found matching Asset: {0}, with dates: {1} to {2} ", ticker, startDate, endDate));

        }


        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateNewIncome([FromBody]Income incomeData)
        {
            var currentInvestor = _identityService.CurrentUser;
            var newIncomeId = Guid.NewGuid();
            if (!ModelState.IsValid) {
                    return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid data received for new Income creation."
                });
            }

            var ticker = ParseUrlForTicker(ControllerContext.Request.RequestUri.ToString());
            var newLocation = ControllerContext.Request.RequestUri.AbsoluteUri + "/" + newIncomeId;

            var matchingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker.ToUpper().Trim() &&
                                                                                                     a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                    .AsQueryable()
                                                                                    .SelectMany(a => a.Revenue.Where(i => i.AssetId  == incomeData.AssetId &&
                                                                                                                          i.Account == incomeData.Account.Trim() &&
                                                                                                                          i.DateRecvd == incomeData.DateRecvd))
                                                            );

           if (matchingIncome.Any())
                return BadRequest(string.Format("Income not saved, due to existing Income already recorded for Asset: {0} under Account {1} on {2} ",
                                                                                                                    ticker.ToUpper(), 
                                                                                                                    incomeData.Account.Trim(),
                                                                                                                    incomeData.DateRecvd));

            incomeData.Url = newLocation;
            incomeData.IncomeId = newIncomeId;
            var isCreated = await Task<bool>.Factory.StartNew(
                        () => ((IGenericAggregateRepository)_repositoryAsset).AggregateCreate<Income>(incomeData, currentInvestor, ticker));

            if (!isCreated) return BadRequest(string.Format("Unable to add Income to: {0} ", ticker.ToUpper()));

            return Created(newLocation, incomeData);
        }


        [HttpPut]
        [HttpPatch]
        [Route("{incomeId}")]
        public async Task<IHttpActionResult> UpdateIncome([FromBody]Income editedIncome, Guid incomeId)
        {

            if (!ModelState.IsValid || !IsValidDate(editedIncome.DateRecvd) )
            {
                return ResponseMessage(new HttpResponseMessage {
                                                    StatusCode = HttpStatusCode.BadRequest,
                                                    ReasonPhrase = "Invalid data received for Income update."
                });
            }


            // Replace entire Income.
            var currentInvestor = _identityService.CurrentUser;
            var ticker = ParseUrlForTicker(ControllerContext.Request.RequestUri.AbsoluteUri);
            var existingIncome = await Task.FromResult(_repositoryAsset.Retreive(a => a.AssetId  == editedIncome.AssetId &&
                                                                                      a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                     .AsQueryable()
                                                                                     .SelectMany(a => a.Revenue.Where(i => i.IncomeId == incomeId)));

            if (!existingIncome.Any())
                return BadRequest(string.Format("No matching Income found to update, for {0} under account {1} ", ticker, editedIncome.Account));

            var isUpdated = await Task<bool>.Factory.StartNew(
                           () => ((IGenericAggregateRepository)_repositoryAsset).AggregateUpdate<Income>(editedIncome, currentInvestor, ticker));

            if (!isUpdated)
                return BadRequest(string.Format("Unable to edit Income under Account : {0} for Asset {1}", editedIncome.Account, ticker));

            return Ok(editedIncome);

        }


        [HttpDelete]
        [Route("{incomeId}")]
        public async Task<IHttpActionResult> DeleteIncome(Guid incomeId)
        {
            var isDeleted = await Task<bool>.Factory.StartNew(
                                        () => ((IGenericAggregateRepository)_repositoryAsset).AggregateDelete<Income>(incomeId));

            return isDeleted 
                ? Ok("Successfully deleted Income: " + incomeId) 
                : (IHttpActionResult)BadRequest("Error: unable to delete Income: " + incomeId);

        }



        


        public static string ParseUrlForTicker(string urlToParse) {
            var pos1 = urlToParse.IndexOf("Asset/", StringComparison.Ordinal) + 6;
            var pos2 = urlToParse.IndexOf("/Income", StringComparison.Ordinal); // Position or Income
            return urlToParse.Substring(pos1, pos2 - pos1);
        }

        private static bool IsValidDate(string dateToCheck)
        {
            // ReSharper disable once SimplifyConditionalTernaryExpression
            return Convert.ToDateTime(dateToCheck) > DateTime.UtcNow ? false : true;
        }

       
        private void CalculateAverages(RevenueYtdVm item)
        {
           // YTD & 3Mos rolling averages.
            _runningYtdTotal += item.AmountRecvd;
            _incomeHistory[_counter - 1] = item.AmountRecvd;
            item.YtdAverage = Math.Round(_runningYtdTotal / _counter, 2);
            if (_counter >= 3)
            {
                item.Rolling3MonthAverage = Math.Round((item.AmountRecvd + _incomeHistory[_counter - 2] + _incomeHistory[_counter - 3]) / 3, 2); 
            }
            _tempListing.Add(item);
            _counter += 1;
        }
    }
}

