using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Asset")]
    public class PositionController : ApiController
    {
        private static IGenericRepository<Asset> _repositoryAsset;
        private readonly IPimsIdentityService _identityService;


        public PositionController( IPimsIdentityService identitySvc, IGenericRepository<Asset> repositoryAsset) {
            _repositoryAsset = repositoryAsset;
            _identityService = identitySvc;
        }

        [HttpGet]
        [Route("{tickerSymbol}/Position/{accountType}")]
        public async Task<IHttpActionResult> GetPositionByAccount(string tickerSymbol, string accountType)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            var matchingPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == tickerSymbol &&
                                                                                                     a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                     .AsQueryable()
                                                                                     .SelectMany(a => a.Positions.Where(p => p.Account.AccountTypeDesc.Trim() ==
                                                                                                        accountType.Trim()))
                                                         );

            if (matchingPosition.Any())
                return Ok(matchingPosition);


            return BadRequest(string.Format("No Position found matching {0} under Asset: {1} ", accountType, tickerSymbol.ToUpper()));

        }


        [HttpGet]
        [Route("{tickerSymbol}/Position/Account/{accountKeyId}")]
        public async Task<IHttpActionResult> GetAccountByAccountKey(Guid accountKeyId)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            var matchingAccount = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                 .AsQueryable()
                                                                                 .SelectMany(p => p.Positions.Where(x => x.Account.KeyId == accountKeyId))
                                                                                 .First().Account
                                                        );

            var currentTicker = ParseUrlForTicker(matchingAccount.Url);
            if (matchingAccount.KeyId == accountKeyId)
                return Ok(matchingAccount);


            return BadRequest(string.Format("No Position found matching Account {0} under Asset: {1} ", matchingAccount.KeyId, currentTicker.ToUpper()));

        }

        [HttpGet]
        [Route("{tickerSymbol}/Position")]
        public async Task<IHttpActionResult> GetPositionsByAsset(string tickerSymbol)
        {

            // Playing with interfaces: casting to correct interface is necessary, as injected instance (via IoC) has no reference to derived interface.
            //-----------------------------------------------------------------------------------------------------------------------------------------
            //var itfTest = (ITestProfileRepository) _repositoryAsset;
            //itfTest.TestProfileMethod();

            // ok
            //var itfTest2 = (IGenericAggregateRepository)_repositoryAsset;
            //itfTest2.AggregateRetreive<Position>(p => p.Quantity == 300);
            
            //var repo = new InMemoryAssetRepository();
            //repo.TestProfileMethod2();
            //------------------------------------------------------------------------------------------------------------------------------------------


            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            var currentAsset = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == tickerSymbol.ToUpper().Trim()
                                                                                 && a.Investor.LastName.Trim() == currentInvestor.Trim()).AsQueryable());


            var currentPositions = await Task.FromResult(currentAsset.First().Positions.OrderBy(p => p.Account.AccountTypeDesc).ToList().AsQueryable());

            if (currentPositions.Any())
                return Ok(currentPositions);


            return BadRequest(string.Format("No Positions found matching {0} for investor {1} ", tickerSymbol.ToUpper(), currentInvestor.ToUpper()));

        }


        [HttpPost]
        [Route("{tickerSymbol}/Position")]
        public async Task<IHttpActionResult> CreateNewPosition([FromBody]Position positionData)
        {
            var currentInvestor = _identityService.CurrentUser;
            positionData.InvestorKey = currentInvestor.Trim();

            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid data received for Position creation."
                                      });
            }

            var ticker = ParseUrlForTicker(ControllerContext.Request.RequestUri.ToString());

            // TODO: Investigate using route data for building links instead.
            var newLocation = ParseForNewPositionUrl(ControllerContext.Request.RequestUri.AbsoluteUri, positionData.Account.AccountTypeDesc.Trim())
                                    + "/" + positionData.Account.AccountTypeDesc;
            var matchingPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker.ToUpper().Trim() &&
                                                                                                     a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                    .AsQueryable()
                                                                                    .SelectMany(a => a.Positions.Where(p => p.Account.AccountTypeDesc.Trim() == 
                                                                                                        positionData.Account.AccountTypeDesc.Trim()))
                                                            );

            // var matchingPosition = currentAsset.SelectMany(a => a.Positions.Where(p => p.Account.AccountTypeDesc == positionData.Account.AccountTypeDesc.Trim()));
            if (matchingPosition.Any())
                return BadRequest(string.Format("No Position created, due to Position {0} already exists for {1} ",
                                                                                                positionData.Account.AccountTypeDesc.Trim(),
                                                                                                ticker.ToUpper()));

            positionData.Url = newLocation;
            var isCreated = await Task<bool>.Factory.StartNew(
                        () => ((IGenericAggregateRepository) _repositoryAsset).AggregateCreate<Position>(positionData, currentInvestor, ticker));
               
            if (!isCreated) return BadRequest(string.Format("Unable to add Position to: {0} ", ticker.ToUpper()));
            return Created(newLocation, positionData);
            
        }
        

        [HttpPut]
        [Route("{tickerSymbol}/Position/{account}")]
        public async Task<IHttpActionResult> UpdatePositionsByAsset([FromBody]Position editedPosition)
        {
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                                        StatusCode = HttpStatusCode.BadRequest,
                                                        ReasonPhrase = "Invalid data received for Position update(s)."
                                                    }
                );
            }

            var currentInvestor = _identityService.CurrentUser;
            string ticker;
            if(editedPosition.Account.AccountTypeDesc.Trim() != ParseUrlForAccount(editedPosition.Url).Trim())
            {
                ticker = ParseUrlForTicker(editedPosition.Url);

                // Consolidation. Example: [Roth-IRA <- from IRRA].
                var consolidateToPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.Investor.LastName == currentInvestor.Trim() &&
                                                                                                 a.Profile.TickerSymbol == ticker)
                                                                    .AsQueryable()
                                                                    .SelectMany(a => a.Positions.Where(p => p.Account.AccountTypeDesc.Trim()
                                                                                     == editedPosition.Account.AccountTypeDesc.Trim())));

                if (!consolidateToPosition.Any())
                    return BadRequest(string.Format("No matching Position found to consolidate with, for {0} ", editedPosition.Account.AccountTypeDesc));

                // Purchase date ALWAYS reflects date of consolidation.
                consolidateToPosition.First().PurchaseDate = DateTime.UtcNow.ToString("d");
                consolidateToPosition.First().Quantity = consolidateToPosition.First().Quantity + editedPosition.Quantity;
                consolidateToPosition.First().MarketPrice = await GetCurrentMarketPrice(ticker);
                consolidateToPosition.First().Account = editedPosition.Account;
                consolidateToPosition.First().LastUpdate = DateTime.UtcNow.ToString("d");

                var deleteResult = await DeletePosition(editedPosition.PositionId) as OkNegotiatedContentResult<string>;

                if (deleteResult != null && deleteResult.Content == "deleted")
                    return Ok(consolidateToPosition);

                return BadRequest("Unable to consolidate account types for " + consolidateToPosition.First().Account.AccountTypeDesc.Trim());
            }
            else
            {
                // Replace entire Position.
                if (currentInvestor.Trim() != editedPosition.InvestorKey.Trim())
                    return BadRequest("Cuurent Position does not belong to investor " + currentInvestor);

                ticker = ParseUrlForTicker(editedPosition.Url.Trim());
                _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
                var matchingPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker &&
                                                                                                         a.Investor.LastName.Trim() == currentInvestor.Trim())
                                                                                         .AsQueryable()
                                                                                         .SelectMany(a => a.Positions.Where(p => p.Account.AccountTypeDesc.Trim() ==
                                                                                                          editedPosition.Account.AccountTypeDesc.Trim()))
                                                             );

                if (!matchingPosition.Any())
                    return BadRequest(string.Format("No matching Position found to update, for {0} under account {1} ", ticker, editedPosition.Account.AccountTypeDesc.Trim()));



                var isUpdated = await Task<bool>.Factory.StartNew(
                               () => ((IGenericAggregateRepository)_repositoryAsset).AggregateUpdate<Position>(editedPosition, currentInvestor, ticker));

                if (!isUpdated) return BadRequest(string.Format("Unable to edit Position : {0} for Asset {1}",
                                                                           editedPosition.Account.AccountTypeDesc, ticker));

                return Ok(editedPosition); 
            }

        }


        [HttpDelete]
        [Route("{tickerSymbol}/Position/{accountKey}")]
        public async Task<IHttpActionResult> DeletePosition(Guid accountKey)
        {
            var isDeleted = await Task<bool>.Factory.StartNew(
                       () => ((IGenericAggregateRepository)_repositoryAsset).AggregateDelete<Position>(accountKey));

            return isDeleted ? Ok("deleted") : (IHttpActionResult)BadRequest("Error: unable to delete Position/Account: " + accountKey);
            
        }





        private static string ParseUrlForTicker(string urlToParse)
        {
            var pos1 = urlToParse.IndexOf("Asset/", StringComparison.Ordinal) + 6;
            var pos2 = urlToParse.IndexOf("/Position", StringComparison.Ordinal);
            return urlToParse.Substring(pos1, pos2 - pos1);
        }

        private static string ParseUrlForAccount(string urlToParse)
        {
            var pos1 = urlToParse.LastIndexOf("Position/", StringComparison.Ordinal) + 9;
            return urlToParse.Substring(pos1, urlToParse.Length - pos1);
        }


        private static string ParseForNewPositionUrl(string urlForPosition, string accountType)
        {
            if (urlForPosition.IndexOf("?", StringComparison.Ordinal) <= 0) return urlForPosition;
            var pos1 = urlForPosition.IndexOf("Position?", StringComparison.Ordinal);
            return urlForPosition.Substring(0, pos1 + 8) + "/" + accountType.Trim();
        }


        private static async Task<decimal> GetCurrentMarketPrice(string tickerSymbol)
        {
            var currentProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerSymbol, new Profile()));
            return currentProfile == null ? 0 : currentProfile.Price;

        }

        
    }
}
