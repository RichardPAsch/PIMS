using System;
using System.Collections.Generic;
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
        private readonly IGenericRepository<Asset> _repository;
        private readonly IPimsIdentityService _identityService;
        private readonly IGenericRepository<Investor> _repositoryInvestor;
        private const string DefaultDisplayType = "detail";
        private string _currentInvestor;


        public AssetController(IGenericRepository<Asset> repository, IPimsIdentityService identityService, IGenericRepository<Investor> repositoryInvestor)
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
                                                                            UnitPrice = a.Positions.First().MarketPrice,
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
                                                                             UnitPrice = a.Positions.First().MarketPrice,
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
                                                                                UnitPrice = a.Positions.First().MarketPrice,
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
                                                                                    UnitPrice = a.Positions.First().MarketPrice,
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
                                                                                UnitPrice = a.Positions.First().MarketPrice,
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
                                                                                UnitPrice = a.Positions.First().MarketPrice,
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
            _currentInvestor = _identityService.CurrentUser; 
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();

            // Confirm investors' registration for idempotent operation.
            var registeredInvestor = _repositoryInvestor.Retreive(u => u.LastName.Trim() == _currentInvestor.Trim()).SingleOrDefault();
            if (registeredInvestor == null)
                return BadRequest("Invalid operation, no registration found for investor : " + _currentInvestor);

            newAsset.Investor = registeredInvestor; // TODO: Entire object needed ? Re-eval during integration tests. For investor Profile info ?
           
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid/Incomplete data received for Asset creation."
                });
            }

            if(newAsset.Profile == null || newAsset.Positions == null)
                return BadRequest("No new Asset created, due to missing required Profile and/or Position data.");

            // Check against existing data. Per UI design, all Positions are created before any Income is added.
            var newAssetTicker = newAsset.Profile.TickerSymbol.ToUpper().Trim();
            var existingAsset = await Task.FromResult(_repository.Retreive(asset => asset.Investor.LastName.ToUpper().Trim() == _currentInvestor.Trim().ToUpper()
                                                                                             && asset.Profile.TickerSymbol.ToUpper().Trim() == newAssetTicker)
                                          .AsQueryable());

            if (existingAsset.Any()) {
                return ResponseMessage(new HttpResponseMessage {
                                        StatusCode = HttpStatusCode.Conflict,
                                        ReasonPhrase = string.Format("No new Asset created, due to existing Asset already found for: {0}", newAssetTicker)});
            }

            var newPositions = newAsset.Positions;
            if (ContainsDuplicates(ref newPositions))
                return BadRequest("No new Asset created, due to duplicate Position entry(ies) received.");

            // Contained "Income" aggregate component may not be initialized due to deferred use.
            if (newAsset.Revenue != null)
            {
                var newIncome = newAsset.Revenue;
                if (ContainsDuplicates(ref newIncome))
                    return BadRequest("No new Asset created, due to duplicate Income entry(ies) received.");
            }
           
            InitializePositionsWithInvestorInfo(ref newPositions);
            newAsset = InitializeUrls(newAsset);

            var isCreated = await Task<bool>.Factory.StartNew(() => _repository.Create(newAsset));
            var newLocation = _repositoryInvestor.UrlAddress + "/" + newAsset.Profile.TickerSymbol.Trim();
            _repository.UrlAddress = newLocation;

            if (isCreated)
                return Created(newLocation, newAsset); // 201 status code

            return BadRequest("Unable to create new Asset for : " + newAsset.Profile.TickerSymbol);


            //TODO: Use for Asset Position/Update ?
            //var existingPositions = existingAsset.SelectMany(p => p.Positions); 
            //var matchingPositionsFound = CheckExistingPositionsAgainstNewPositions(existingPositions, newAsset.Positions.AsQueryable());
            //if (matchingPositionsFound)
            //    return BadRequest("Matching existing Position(s) found, Asset not created");


            //var existingIncome = existingAsset.SelectMany(p => p.Revenue).Where(r => r.DateRecvd == new DateTime(2014, 07, 10, 18, 09, 0).ToString("g"));
            //var x = existingPositions.Count();
            //var y = existingIncome.Count();

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
            // Use Position/Account to match selected individual asset.
            var selectedAssetToUpdate = foundAssets.FirstOrDefault(a => a.Profile.TickerSymbol.ToUpper().Trim() == existingTicker.ToUpper().Trim()
                                                                                              && a.Positions
                                                                                                  .FirstOrDefault()
                                                                                                  .Account
                                                                                                  .AccountTypeDesc == assetViewModel.AccountTypePreEdit);
            if(selectedAssetToUpdate == null)
                return BadRequest(string.Format("No update(s) occurred; invalid data edits received for asset {0} under account {1}.",
                                                                                        existingTicker.ToUpper(), assetViewModel.AccountTypePreEdit)); 

            // Map any changes & send to repository.
            var updatedAsset = ModelParser.ParseAsset(assetViewModel, selectedAssetToUpdate, out isUpdated);
            var isPersisted = _repository.Update(updatedAsset, updatedAsset.AssetId);
            
            if (isUpdated && isPersisted)
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
            foreach (var position in assetToUpdate.Positions)
                position.Url = assetToUpdate.Url + "/Position/" + position.Account.AccountTypeDesc.Trim();

            if (assetToUpdate.Revenue == null) return assetToUpdate;
            foreach (var subitem in assetToUpdate.Revenue)
                subitem.Url = _repositoryInvestor.UrlAddress.Replace("Asset", "Income/" + Guid.NewGuid());

            return assetToUpdate;
        }


        private void InitializePositionsWithInvestorInfo(ref IList<Position> positionsToUpdate)
        {
            foreach (var position in positionsToUpdate)
                position.InvestorKey = _currentInvestor.Trim();
        }


        private static bool ContainsDuplicates<T>(ref IList<T> collectionToCheck)
        {
            switch (typeof(T).Name)
            {
                case "Position" :
                    var positionData  = collectionToCheck as IList<Position>;
                    return positionData != null && positionData.GroupBy(p => new {p.Account.AccountTypeDesc}).Any(p => p.Skip(1).Any());
                case "Income" :
                  var incomeData  = collectionToCheck as IList<Income>;
                    return incomeData != null && incomeData.GroupBy(i => new {i.Account, i.DateRecvd}).Any(i => i.Skip(1).Any());
            }

            return false;
        }


        // TODO: for Position/Update ?
        //private static bool CheckExistingPositionsAgainstNewPositions(IQueryable<Position> currentPositions, IQueryable<Position> newPositions)
        //{
        //    foreach (var positionToAdd in newPositions.Where(positionToAdd => Enumerable.Any(currentPositions)))
        //    {
        //        return currentPositions.Count(a => a.Account.AccountTypeDesc == positionToAdd.Account.AccountTypeDesc) >= 1;
        //    }
        //}

    }
}
