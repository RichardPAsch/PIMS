using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;
using PIMS.Data.Repositories;
using PIMS.Core.Security;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Asset")]
    public class AssetController : ApiController
    {
        private const string UrlBase = "http://localhost/PIMS.Web.Api/api";
        private readonly IGenericRepository<Asset> _repository;
        private readonly IPimsIdentityService _identityService;
        private readonly IGenericRepository<Investor> _repositoryInvestor;
        private readonly string _urlLink = string.Empty;
        private const string DefaultDisplayType = "detail";


        public AssetController(IGenericRepository<Asset> repository, IPimsIdentityService identityService, IGenericRepository<Investor> repositoryInvestor  )
        {
            _repository = repository;
            _identityService = identityService;
            _repositoryInvestor = repositoryInvestor;
        }



        [HttpGet]
        [Route("{tickerSymbol?}", Name = "AssetsByTicker")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/VNR
        public async Task<IHttpActionResult> GetByTicker(string tickerSymbol, [FromUri] string displayType=DefaultDisplayType)
        {
            _repository.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            if (displayType == "summary")
            {
                var editedAsset = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                              .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper() &&
                                                                        a.Profile.TickerSymbol.ToUpper().Trim() == tickerSymbol.ToUpper().Trim())
                                                                        .AsQueryable()
                                                                        .Select(a => new AssetSummaryVm {
                                                                            AccountType = a.Revenue.First().Account,
                                                                            AssetClassification = a.AssetClass.Code,
                                                                            DividendFrequency = a.Profile.DividendFreq,
                                                                            IncomeRecvd = a.Revenue.First().Actual,
                                                                            Quantity = a.Positions.First().Quantity,
                                                                            UnitPrice = a.Positions.First().UnitCost,
                                                                            TickerSymbol = a.Profile.TickerSymbol,
                                                                            DateRecvd = a.Revenue.First().DateRecvd,
                                                                            TickerSymbolDescription = a.Profile.TickerDescription,
                                                                            CurrentInvestor = a.Investor.LastName
                                                                        })
                                                        );
                if (editedAsset.Any())
                    return Ok(editedAsset);
            }

            var editedAssetDetail = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                              .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper() &&
                                                                        a.Profile.TickerSymbol.ToUpper().Trim() == tickerSymbol.ToUpper().Trim())
                                                                        .AsQueryable()
                                                        );
            if (editedAssetDetail.Any())
                return Ok(editedAssetDetail);
 

            return BadRequest(string.Format("No Assets found matching {0} for investor {1}", tickerSymbol.ToUpper(), currentInvestor.ToUpper()));
        }


        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll([FromUri] string displayType=DefaultDisplayType)
        {
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            //IQueryable<object> assets; returns Null in Unit test ?
            if (displayType == "detail") {
                var assets = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                              .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
                                                                              .OrderBy(item => item.Profile.TickerSymbol)
                                                              .AsQueryable());
                if (assets != null)
                    return Ok(assets);

                return BadRequest("Unable to retreive Asset details for: " + currentInvestor);
            } 
            else
            {
                var assets = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                         .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
                                                                         .OrderBy(item => item.Profile.TickerSymbol)
                                                                         .AsQueryable()
                                                                         .Select(a => new AssetSummaryVm {
                                                                             AccountType = a.Revenue.First().Account,
                                                                             AssetClassification = a.AssetClass.Code,
                                                                             DividendFrequency = a.Profile.DividendFreq,
                                                                             IncomeRecvd = a.Revenue.First().Actual,
                                                                             Quantity = a.Positions.First().Quantity,
                                                                             UnitPrice = a.Positions.First().UnitCost,
                                                                             TickerSymbol = a.Profile.TickerSymbol,
                                                                             DateRecvd = a.Revenue.First().DateRecvd,
                                                                             TickerSymbolDescription = a.Profile.TickerDescription,
                                                                             CurrentInvestor = a.Investor.LastName
                                                                         })
                                                                         );
                if (assets != null)
                    return Ok(assets);

                return BadRequest("Unable to retreive Asset summary for: " + currentInvestor);
            }
            
        }


        [HttpGet]
        [Route("")]
        // Ex: http://localhost/Pims.Web.Api/api/Asset?sortBy=assetClass
        public async Task<IHttpActionResult> GetAndSortByOther([FromUri]string sortBy)
        {
           _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            IQueryable<Asset> assets;
            IQueryable<AssetSummaryVm> assetSummary = null;

            switch (sortBy.Trim().ToUpper()) {
                case "ASSETCLASS": {
                        assets = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                                  .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
                                                                   .OrderBy(item => item.AssetClass.Code).AsQueryable());

                        assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
                                                                                AccountType = a.Revenue.First().Account,
                                                                                AssetClassification = a.AssetClass.Code,
                                                                                DividendFrequency = a.Profile.DividendFreq,
                                                                                IncomeRecvd = a.Revenue.First().Actual,
                                                                                Quantity = a.Positions.First().Quantity,
                                                                                UnitPrice = a.Positions.First().UnitCost,
                                                                                TickerSymbol = a.Profile.TickerSymbol,
                                                                                DateRecvd = a.Revenue.First().DateRecvd,
                                                                                TickerSymbolDescription = a.Profile.TickerDescription,
                                                                                CurrentInvestor = a.Investor.LastName
                                                                        });
                        break;
                    }
                case "ACCTTYPE": {
                        assets = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                                  .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
                                                                  .OrderBy(item => item.Positions.First().Account.AccountTypeDesc).AsQueryable());

                        assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
                                                                                    AccountType = a.Revenue.First().Account,
                                                                                    AssetClassification = a.AssetClass.Code,
                                                                                    DividendFrequency = a.Profile.DividendFreq,
                                                                                    IncomeRecvd = a.Revenue.First().Actual,
                                                                                    Quantity = a.Positions.First().Quantity,
                                                                                    UnitPrice = a.Positions.First().UnitCost,
                                                                                    TickerSymbol = a.Profile.TickerSymbol,
                                                                                    DateRecvd = a.Revenue.First().DateRecvd,
                                                                                    TickerSymbolDescription = a.Profile.TickerDescription,
                                                                                    CurrentInvestor = a.Investor.LastName
                                                                            });
                        break;
                    }
                case "INCOME": {

                        assets = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                                  .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
                                                                  .OrderByDescending(assetList => assetList.Revenue.First().Actual).AsQueryable());

                        assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
                                                                                AccountType = a.Revenue.First().Account,
                                                                                AssetClassification = a.AssetClass.Code,
                                                                                DividendFrequency = a.Profile.DividendFreq,
                                                                                IncomeRecvd = a.Revenue.First().Actual,
                                                                                Quantity = a.Positions.First().Quantity,
                                                                                UnitPrice = a.Positions.First().UnitCost,
                                                                                TickerSymbol = a.Profile.TickerSymbol,
                                                                                DateRecvd = a.Revenue.First().DateRecvd,
                                                                                TickerSymbolDescription = a.Profile.TickerDescription,
                                                                                CurrentInvestor = a.Investor.LastName
                                                                            });
                        break;
                    }
                case "DATERECVD": {
                        //var withinLast12Months = DateTime.Now.AddYears(-1);
                        assets = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                                  .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
                                                                  .OrderByDescending(assetList => assetList.Revenue.First().DateRecvd).AsQueryable());

                        assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
                                                                                AccountType = a.Revenue.First().Account,
                                                                                AssetClassification = a.AssetClass.Code,
                                                                                DividendFrequency = a.Profile.DividendFreq,
                                                                                IncomeRecvd = a.Revenue.First().Actual,
                                                                                Quantity = a.Positions.First().Quantity,
                                                                                UnitPrice = a.Positions.First().UnitCost,
                                                                                TickerSymbol = a.Profile.TickerSymbol,
                                                                                DateRecvd = a.Revenue.First().DateRecvd,
                                                                                TickerSymbolDescription = a.Profile.TickerDescription,
                                                                                CurrentInvestor = a.Investor.LastName
                                                                            });
                        break;
                    }

            }

            if (assetSummary != null)
                return Ok(assetSummary);
           
            return BadRequest("Unable to retreive Assets for: " + currentInvestor);
        }


        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateNewAsset([FromBody] Asset newAsset)
        {
            var currentInvestor = _identityService.CurrentUser; 
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString(); 


            // Contained "Income" component may equal NULL, denoting deferred use during Asset POSTing. ModelState
            // will continue to be valid.
            if (!ModelState.IsValid)
            {
                return ResponseMessage(new HttpResponseMessage
                                        {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid data received for Asset creation."
                                        });
            }


            // Confirm investors' registration for idempotent operation. Authenticated & Authorized (via tokens) investor data appended
            // for database use as needed.
            var registeredInvestor = _repositoryInvestor.Retreive(u => u.LastName.Trim() == currentInvestor.Trim()).SingleOrDefault();
            if (registeredInvestor == null)
                return BadRequest("Invalid operation, no registration found for investor : " + currentInvestor);

            newAsset.Investor = registeredInvestor; // TODO: Entire object needed ? Re-eval during integration tests. For investor Profile info ?


            // Check for duplicate entry via: ticker, user, and account type.
            var newAssetTicker = newAsset.Profile.TickerSymbol.ToUpper().Trim();
            var newAssetAcctType = newAsset.Positions.First().Account.AccountTypeDesc.Trim().ToUpper();
            var duplicateAssets = await Task.FromResult(_repository.Retreive(asset => asset.Investor
                                                                                           .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper()
                                                                                             && asset.Profile.TickerSymbol.ToUpper().Trim() == newAssetTicker
                                                                                             && asset.Positions.First().Account.AccountTypeDesc.Trim().ToUpper() == newAssetAcctType)
                                                                   .AsQueryable());
            
            if (duplicateAssets.Any()) {
                return ResponseMessage(new HttpResponseMessage 
                        {
                            StatusCode = HttpStatusCode.Conflict,
                            ReasonPhrase = string.Format("Duplicate Asset found for {0}, with account type of {1} ", newAssetTicker, newAssetAcctType)
                        });
            }

            newAsset = InitializeUrls(newAsset);
           
            var isCreated = await Task<bool>.Factory.StartNew(() => _repository.Create(newAsset));
            var newLocation = _repositoryInvestor.UrlAddress + "/" + newAsset.Profile.TickerSymbol.Trim();
            _repository.UrlAddress = newLocation;

            if (isCreated)
                return Created(newLocation, newAsset); // 201 status code


            return BadRequest("Unable to create new Asset for : " + newAsset.Profile.TickerSymbol);
        }


        [HttpPut]
        [HttpPatch]
        [Route("{existingTicker?}")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/VNR
        public async Task<IHttpActionResult> UpdateByTicker([FromBody] AssetSummaryVm assetViewModel, string existingTicker)
        {
            var currentInvestor = _identityService.CurrentUser;
            var foundAssets = await Task.FromResult(_repository.Retreive(a => a.Investor
                                                                               .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper() &&
                                                                                      a.Profile.TickerSymbol.ToUpper().Trim() == existingTicker.ToUpper().Trim())
                                                               .AsQueryable());

            if (!foundAssets.Any())
                return BadRequest(string.Format("Unable to find asset {0} for {1}.", existingTicker, currentInvestor));

            bool isUpdated;
            var assetToUpdate = foundAssets.First(a => a.Profile.TickerSymbol.ToUpper().Trim() == existingTicker.ToUpper().Trim()
                                                                                               && a.Positions
                                                                                                   .FirstOrDefault()
                                                                                                   .Account
                                                                                                   .AccountTypeDesc == assetViewModel.AccountTypePreEdit);

            var updatedAsset = ModelParser.ParseAsset(assetViewModel, assetToUpdate, out isUpdated);

           
            if (isUpdated)
                return Ok(updatedAsset);


            return BadRequest(string.Format("No update(s) occurred regarding asset {0} for investor {1}; invalid data edit(s).", 
                existingTicker.ToUpper(), currentInvestor.ToUpper()));
           
        }


        [HttpDelete]
        [Route("{tickerSymbol?}")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/VNR
        public async Task<IHttpActionResult> DeleteAsset(string tickerSymbol)
        {
            _repository.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            // Validate asset to be removed belongs to currently logged in investor.
            var assetsToRemove = await Task.FromResult(_repository.Retreive(a => a.Investor.LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper() &&
                                                            String.Equals(a.Profile.TickerSymbol.Trim(), tickerSymbol.Trim(), StringComparison.CurrentCultureIgnoreCase))
                                           .AsQueryable()
                                                      );

            if (!assetsToRemove.Any())
                return BadRequest(string.Format("No matching assets found for investor {0}", currentInvestor.ToUpper()));

            if (_repository.Delete(assetsToRemove.First().AssetId))
                return Ok(assetsToRemove);

            return BadRequest(string.Format("Error removing asset: {0}", tickerSymbol));
        }



        private Asset InitializeUrls(Asset assetToUpdate)
        {
            // Initialize with appropriate URLs.
            assetToUpdate.Url = _repositoryInvestor.UrlAddress + "/" + assetToUpdate.Profile.TickerSymbol.ToUpper().Trim();
            assetToUpdate.Investor.Url = _repositoryInvestor.UrlAddress.Replace("Asset",
                                                    "Investor/" + assetToUpdate.Investor.FirstName.Trim()
                                                    + assetToUpdate.Investor.MiddleInitial.Trim()
                                                    + assetToUpdate.Investor.LastName.Trim());
            assetToUpdate.AssetClass.Url = _repositoryInvestor.UrlAddress.Replace("Asset",
                                                     "AssetClass/" + assetToUpdate.AssetClass.Code.Trim().ToUpper());
            assetToUpdate.Profile.Url = _repositoryInvestor.UrlAddress.Replace("Asset",
                                                    "Profile/" + assetToUpdate.Profile.TickerSymbol.Trim().ToUpper());
            foreach (var subitem in assetToUpdate.Positions)
                subitem.Url = _repositoryInvestor.UrlAddress.Replace("Asset", "Position/" + Guid.NewGuid());
            if (assetToUpdate.Revenue == null) return assetToUpdate;
            foreach (var subitem in assetToUpdate.Revenue)
                subitem.Url = _repositoryInvestor.UrlAddress.Replace("Asset", "Income/" + Guid.NewGuid());

            return assetToUpdate;

        }



        



    }
}
