using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;
using PIMS.Core.Security;
using PIMS.Data;
using PIMS.Data.Repositories;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Asset")]
    public class PositionController : ApiController
    {
        //----------- Menu BUSINESS RULES : per Investor ---------------------------------------------------------------------------------
        //  Asset/Create -> One or more Positions can be POSTed as part of an aggregate during Asset creation.
        //                  (See VerifyAsset) 
        //
        //  Position/Create       :  Adds a new Position to an existing Asset.
        //  Position/Retreive     : 
        //             Adds or modifies existing Position. Each Position is uniquely identified by its' Account type.
        //             via Position/Retreive-Edit
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
        //  DELETE -> Removes appropriate Position(s) per Asset, e.g., as per Account type consolidation or explicit Position
        //                     removal. via Position/Delete
        //
        //---------------------------------------------------------------------------------------------------------------------------------



        private static IGenericRepository<Asset> _repositoryAsset;
        private static IGenericRepository<Investor> _repositoryInvestor; 
        private readonly IPimsIdentityService _identityService;
        private static IGenericRepository<Position> _repository;
        private static IGenericRepository<AccountType> _repositoryAccountType;

        public PositionController(IPimsIdentityService identitySvc, IGenericRepository<Asset> repositoryAsset, IGenericRepository<Investor> repositoryInvestor, IGenericRepository<Position> repository, IGenericRepository<AccountType> repositoryAccountType)
        {
            _repositoryAsset = repositoryAsset;
            _identityService = identitySvc;
            _repositoryInvestor = repositoryInvestor;
            _repository = repository;
            _repositoryAccountType = repositoryAccountType;
        }


        [HttpGet]
        [Route("{tickerSymbol}/Position/{accountType}")]
        public async Task<IHttpActionResult> GetPositionByAccount(string accountType)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var tickerSymbol = ParseUrlForTicker(_repositoryAsset.UrlAddress);
            var currentInvestor = _identityService.CurrentUser;

            var matchingPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == tickerSymbol.ToUpper().Trim() &&
                                                                                        a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                                                                         .SelectMany(a => a.Positions).Where(p => p.Account.AccountTypeDesc.Trim() == accountType.Trim())
                                                                         .Select(p2 => new PositionVm {
                                                                             PostEditPositionAccount = p2.Account.AccountTypeDesc,
                                                                             PreEditPositionAccount = p2.Account.AccountTypeDesc,
                                                                             CostBasis = p2.MarketPrice,
                                                                             Qty = p2.Quantity,
                                                                             DateOfPurchase = p2.PurchaseDate,
                                                                             LastUpdate = p2.LastUpdate,
                                                                             Url = p2.Url,
                                                                             LoggedInInvestor = p2.InvestorKey
                                                                         })
                                                                         .AsQueryable());

            if (matchingPosition.Any())
                return Ok(matchingPosition);

            return BadRequest(string.Format("No Position found matching {0} under Asset: {1} ", accountType, tickerSymbol.ToUpper()));
        }


        [HttpGet]
        // Ex: http://localhost/PIMS.Web.API/api/Position/b8f464d2-9c64-49d3-b9cc-a465011574b7
        [Route("~/api/Position/{positionKeyId}")]
        public async Task<IHttpActionResult> GetPositionById(Guid positionKeyId)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            var matchingPosition = await Task.FromResult(_repository.Retreive(p => p.PositionId == positionKeyId)
                                                                    .Select(x => new PositionVm
                                                                                 {
                                                                                     PreEditPositionAccount = x.Account.AccountTypeDesc,
                                                                                     CostBasis = x.MarketPrice,
                                                                                     Qty = x.Quantity,
                                                                                     DateOfPurchase = x.PurchaseDate,
                                                                                     LastUpdate = x.LastUpdate,
                                                                                     Url = x.Url,
                                                                                     LoggedInInvestor = x.InvestorKey
                                                                                 })
                                                                    .AsQueryable());


            if (matchingPosition != null && new Guid(matchingPosition.First().LoggedInInvestor) == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                  return Ok(matchingPosition);

            return BadRequest(string.Format("No Position found matching id {0} for {1} ", positionKeyId, currentInvestor));
        }


        [HttpGet]
        [Route("{tickerSymbol}/Position")]
        public async Task<IHttpActionResult> GetPositionsByAsset(string tickerSymbol)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "rpasch2@rpclassics.net";
           
            var filteredPositions = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == tickerSymbol.ToUpper().Trim()
                                                                                      && a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                                                                          .SelectMany(p => p.Positions)
                                                                          .Select(p2 => new PositionVm
                                                                                        {
                                                                                            // TODO: 5.13.16 - reevaluate need for 1st 2 properties?
                                                                                            PreEditPositionAccount = p2.Account.AccountTypeDesc,
                                                                                            PostEditPositionAccount = p2.Account.AccountTypeDesc,
                                                                                            CostBasis = p2.MarketPrice,
                                                                                            Qty = p2.Quantity,
                                                                                            DateOfPurchase = p2.PurchaseDate,
                                                                                            LastUpdate = p2.LastUpdate,
                                                                                            Url = p2.Url,
                                                                                            LoggedInInvestor = p2.InvestorKey,
                                                                                            ReferencedAssetId = p2.PositionAsset.AssetId // added 5.17.16
                                                                                        })
                                                                          .AsQueryable());

            if (filteredPositions.Any())
                return Ok(filteredPositions);
            
            return BadRequest(string.Format("No Positions found matching {0} for investor {1} ", tickerSymbol.ToUpper(), currentInvestor.ToUpper()));
        }


        [HttpPost]
        [Route("{tickerSymbol}/Position")]
        public async Task<IHttpActionResult> CreateNewPosition([FromBody]PositionVm positionData)
        {
            if (string.IsNullOrWhiteSpace(positionData.LoggedInInvestor))
                positionData.LoggedInInvestor = _identityService.CurrentUser.Trim();


            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid data received for Position creation."
                });
            }

            var currentInvestorId = Utilities.GetInvestorId(_repositoryInvestor, _identityService.CurrentUser);

            // Account for RPC from another controller.
            if (ControllerContext.Request != null)
                positionData.Url = ControllerContext.Request.RequestUri.ToString();

            var ticker = ParseUrlForTicker(positionData.Url);
            var newLocation = positionData.Url;

            var matchingPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker.ToUpper().Trim() &&
                                                                                        a.InvestorId == currentInvestorId)
                                                                         .SelectMany(a => a.Positions).Where(p => p.Account.AccountTypeDesc.Trim() == positionData.ReferencedAccount.AccountTypeDesc.Trim())
                                                                         .AsQueryable());

            if (matchingPosition.Any())
                return BadRequest(string.Format("No Position created, Position {0} already exists for {1} ",
                                                                                                positionData.ReferencedAccount.AccountTypeDesc.Trim(),
                                                                                                ticker.ToUpper()));

            positionData.ReferencedAccount.Url = Utilities.GetBaseUrl(newLocation) + "AccountType/";

            var positionToCreate = MapVmToPosition(positionData);
            var isCreated = await Task.FromResult(_repository.Create(positionToCreate));
            if (!isCreated) return BadRequest(string.Format("Unable to add Position for: {0} ", ticker.ToUpper()));
            return Created(newLocation + positionToCreate.AcctTypeId, positionToCreate);
        }
        

        [HttpPut]
        [HttpPatch]
        [Route("{tickerSymbol}/Position")]
        public async Task<IHttpActionResult> UpdatePositionsByAsset([FromBody]PositionVm editedPosition)
        {
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                                        StatusCode = HttpStatusCode.BadRequest,
                                                        ReasonPhrase = "Invalid data received for Position update(s)."
                                                    });
            }

            var currentInvestor = _identityService.CurrentUser;
            string ticker;

            // Check for AccountType consolidation (rollover) Example: [Roth-IRA <- from IRRA].
            if(editedPosition.PreEditPositionAccount.Trim() != editedPosition.PostEditPositionAccount.Trim())
            {
                ticker = ParseUrlForTicker(editedPosition.Url);
                var newLocation = ParseForNewPositionUrl(ControllerContext.Request.RequestUri.AbsoluteUri, editedPosition.PostEditPositionAccount.Trim())
                                    + "/" + editedPosition.PostEditPositionAccount;

                var targetPosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()) &&
                                                                                          a.Profile.TickerSymbol == ticker)
                                                                    .SelectMany(a => a.Positions).Where(p => p.Account.AccountTypeDesc.Trim() == 
                                                                                                            editedPosition.PostEditPositionAccount.Trim())
                                                                    .AsQueryable());

                if (!targetPosition.Any())
                    return BadRequest(string.Format("No matching Position found to consolidate with, for {0} ", editedPosition.PostEditPositionAccount));

                var sourcePosition = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()) &&
                                                                                          a.Profile.TickerSymbol == ticker)
                                                                    .SelectMany(a => a.Positions).Where(p => p.Account.AccountTypeDesc.Trim() ==
                                                                                                            editedPosition.PreEditPositionAccount.Trim())
                                                                    .AsQueryable());

                if (!sourcePosition.Any())
                    return BadRequest(string.Format("No matching source Position found to consolidate, for {0} ", editedPosition.PreEditPositionAccount));


                // Purchase date ALWAYS reflects date of consolidation.
                targetPosition.First().PurchaseDate = DateTime.UtcNow;
                targetPosition.First().Quantity = targetPosition.First().Quantity + sourcePosition.First().Quantity;
                targetPosition.First().MarketPrice = await GetCurrentMarketPrice(ticker);
                targetPosition.First().Account = targetPosition.First().Account;
                targetPosition.First().LastUpdate = DateTime.UtcNow;
                targetPosition.First().Url = newLocation.Trim();

                editedPosition.PostEditPositionAccount = targetPosition.First().Account.AccountTypeDesc;
                editedPosition.CostBasis = targetPosition.First().MarketPrice;
                editedPosition.Qty = targetPosition.First().Quantity;
                editedPosition.Url = targetPosition.First().Url;
                editedPosition.LastUpdate = targetPosition.First().LastUpdate;

                var isUpdated = await Task.FromResult(_repository.Update(targetPosition.First(), targetPosition.First().PositionId));
                if (!isUpdated) return BadRequest(string.Format("Unable to update Position : {0} for Asset {1}",
                                                                           targetPosition.First().Account.AccountTypeDesc, ticker));

                var deleteResult = await DeletePosition(sourcePosition.First().PositionId) as OkNegotiatedContentResult<string>;
                if (deleteResult != null && deleteResult.Content == "Delete successful")
                    return Ok(editedPosition);                 // Display newly consolidated Position.

                return BadRequest("Unable to consolidate account types for " + editedPosition.PostEditPositionAccount.Trim());
            }
            else
            {
                // Replace entire Position.
                if (currentInvestor.Trim() != editedPosition.LoggedInInvestor.Trim())
                    return BadRequest("Cuurent Position does not belong to investor " + currentInvestor);

                ticker = ParseUrlForTicker(editedPosition.Url.Trim());
                _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
                var positionToUpdate = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()) &&
                                                                                            a.Profile.TickerSymbol == ticker)
                                                                    .SelectMany(a => a.Positions).Where(p => p.Account.AccountTypeDesc.Trim() ==
                                                                                                            editedPosition.PreEditPositionAccount.Trim())
                                                                    .AsQueryable());

                if (!positionToUpdate.Any())
                    return BadRequest(string.Format("No matching Position found to update, for {0} under account {1} ", 
                                                                                      ticker, editedPosition.PreEditPositionAccount.Trim()));

                // Only selected attributes are available for editing.
                positionToUpdate.First().Quantity = editedPosition.Qty;
                positionToUpdate.First().MarketPrice = editedPosition.CostBasis;


                var isUpdated = await Task.FromResult(_repository.Update(positionToUpdate.First(), positionToUpdate.First().PositionId));
                if (!isUpdated) return BadRequest(string.Format("Unable to edit Position : {0} for Asset {1}",
                                                                           positionToUpdate.First().Account.AccountTypeDesc, ticker));

                return Ok(editedPosition); 
            }

        }


        [HttpDelete]
        [Route("{tickerSymbol}/Position/{accountKey}")]
        public async Task<IHttpActionResult> DeletePosition(Guid accountKey)
        {
            var isDeleted = await Task.FromResult(_repository.Delete(accountKey));

            if (isDeleted)
                return Ok("Delete successful");

            return BadRequest(string.Format("Unable to delete Position, or id  {0} , not found", accountKey));
            
        }





        #region Helpers

            private static string ParseUrlForTicker(string urlToParse) {
                var pos1 = urlToParse.IndexOf("Asset/", StringComparison.Ordinal) + 6;
                var pos2 = urlToParse.IndexOf("/Position", StringComparison.Ordinal);
                return urlToParse.Substring(pos1, pos2 - pos1);
            }

            private static string ParseForNewPositionUrl(string urlForPosition, string accountType) {
                if (urlForPosition.IndexOf("?", StringComparison.Ordinal) <= 0) return urlForPosition;
                var pos1 = urlForPosition.IndexOf("Position?", StringComparison.Ordinal);
                return urlForPosition.Substring(0, pos1 + 8) + "/" + accountType.Trim();
            }

            private static async Task<decimal> GetCurrentMarketPrice(string tickerSymbol) {
                var currentProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerSymbol, new Profile()));
                return currentProfile == null ? 0 : currentProfile.Price;

            }

            private Position MapVmToPosition(PositionVm sourceData)
            {
                var acctTypeCtrl = new AccountTypeController(_repositoryAccountType, _repositoryAsset, _identityService, _repositoryInvestor);
                return new Position
                       {
                           // ReSharper disable once PossibleInvalidOperationException
                           PurchaseDate = (DateTime) sourceData.DateOfPurchase,
                           Quantity = sourceData.Qty,
                           MarketPrice = sourceData.CostBasis,
                           InvestorKey = Utilities.GetInvestorId(_repositoryInvestor, _identityService.CurrentUser).ToString(),
                           AcctTypeId = sourceData.ReferencedAccount.KeyId,
                           PositionAssetId = sourceData.ReferencedAssetId, 
                           LastUpdate = DateTime.Now,
                           TickerSymbol = sourceData.ReferencedTickerSymbol,
                           Account = acctTypeCtrl.MapVmToAccountType(sourceData.ReferencedAccount),
                           Url = sourceData.Url
                       };
            }

        #endregion

        

        
        
    }
}
