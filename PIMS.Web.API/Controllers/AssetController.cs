using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;
using PIMS.Data.Repositories;
using PIMS.Core.Security;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Asset")]
    public class AssetController : ApiController
    {
        private readonly IGenericRepository<Asset> _repository;
        private readonly IPimsIdentityService _identityService;
        private readonly IGenericRepository<Investor> _repositoryInvestor;
        private readonly IGenericRepository<AssetClass> _repositoryAssetClass;
        private readonly IGenericRepository<Profile> _repositoryProfile;
        private readonly IGenericRepository<AccountType> _repositoryAccountType;
        private readonly IGenericRepository<Position> _repositoryPosition;
        private readonly IGenericRepository<Income> _repositoryIncome;
        private const string DefaultDisplayType = "detail";
        private string _currentInvestor;


        public AssetController(IGenericRepository<Asset> repository, IPimsIdentityService identityService,
                                                                     IGenericRepository<Investor> repositoryInvestor,
                                                                     IGenericRepository<AssetClass> repositoryAssetClass,
                                                                     IGenericRepository<Profile> repositoryProfile,
                                                                     IGenericRepository<AccountType> repositoryAccountType,
                                                                     IGenericRepository<Position> repositoryPosition,
                                                                     IGenericRepository<Income> repositoryIncome )
        {
            _repository = repository;
            _identityService = identityService;
            _repositoryInvestor = repositoryInvestor;
            _repositoryAssetClass = repositoryAssetClass;
            _repositoryProfile = repositoryProfile;
            _repositoryAccountType = repositoryAccountType;
            _repositoryPosition = repositoryPosition;
            _repositoryIncome = repositoryIncome;
        }



        [HttpGet]
        [Route("{tickerSymbol}", Name = "AssetsByTicker")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/VNR
        public async Task<IHttpActionResult> GetByTicker(string tickerSymbol)
        {
            //TODO: Fiddler ok 6-16-15
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            var assetSummary = await Task.FromResult(_repository.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim())
                                                                            && a.Profile.TickerSymbol.Trim() == tickerSymbol.Trim())
                                                                .AsQueryable());
            if(!assetSummary.Any())
                return BadRequest("No Asset found matching " + tickerSymbol.Trim() + " for Investor " + currentInvestor.Trim());

            IList<AssetSummaryVm> customizedAssetSummary = new List<AssetSummaryVm>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var assetRec in assetSummary) {
                customizedAssetSummary.Add(new AssetSummaryVm {
                                                    AccountTypePostEdit = assetRec.Positions.First().Account.AccountTypeDesc,
                                                    AccountTypePreEdit = assetRec.Positions.First().Account.AccountTypeDesc,
                                                    AssetClassification = assetRec.AssetClass.Code,
                                                    DividendFrequency = assetRec.Profile.DividendFreq,
                                                    IncomeRecvd = assetRec.Revenue.Any() ? assetRec.Revenue.First().Actual : 0,
                                                    Quantity = assetRec.Positions.First().Quantity,
                                                    UnitPrice = assetRec.Positions.First().MarketPrice,
                                                    TickerSymbol = assetRec.Profile.TickerSymbol,
                                                    DateRecvd = assetRec.Revenue.Any() ? assetRec.Revenue.First().DateRecvd : DateTime.Parse("1/1/1900"),
                                                    TickerSymbolDescription = assetRec.Profile.TickerDescription,
                                                    CurrentInvestor = assetRec.Investor.EMailAddr
                                                });
            }

            return Ok(customizedAssetSummary.AsQueryable());
        }


        [HttpGet]
        [Route("{displayType?}")]
        public async Task<IHttpActionResult> GetAll([FromUri] string displayType=DefaultDisplayType)
        {
            //TODO: Fiddler ok 6-15-15
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            if (displayType == "detail")
            {
                var assetDetails = await Task.FromResult(_repository.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                                                                    .OrderBy(item => item.Profile.TickerSymbol)
                                                                    .AsQueryable()
                                                                    .Select(a => new AssetDetailVm {
                                                                        TickerSymbol = a.Profile.TickerSymbol,
                                                                        AssetInvestor = a.Investor.FirstName  +" " + a.Investor.MiddleInitial + " " + a.Investor.LastName,
                                                                        AssetClassification = a.AssetClass.Code,
                                                                        ProfileUrl = _repositoryInvestor.UrlAddress + "/" + a.Profile.TickerSymbol.Trim() + "/Profile",
                                                                        PositionsUrl = _repositoryInvestor.UrlAddress + "/" +  a.Profile.TickerSymbol.Trim() + "/Position",
                                                                        RevenueUrl = _repositoryInvestor.UrlAddress + "/" + a.Profile.TickerSymbol.Trim() + "/Income"
                                                                    }));

                if (assetDetails.Any())
                    return Ok(assetDetails.AsQueryable());
                
                return BadRequest("Unable to retreive Asset details for: " + currentInvestor);
            }


            var assetSummary = await Task.FromResult(_repository.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                                                                .OrderBy(item => item.Profile.TickerSymbol)
                                                                .AsQueryable());

            // Workaround for: "exceptionMessage=Exception of type 'Antlr.Runtime.NoViableAltException' was thrown..." ?
            IList<AssetSummaryVm> customizedAssetSummary = new List<AssetSummaryVm>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var assetRec in assetSummary)
            {
                customizedAssetSummary.Add(new AssetSummaryVm
                                           {
                                               AccountTypePostEdit = assetRec.Positions.First().Account.AccountTypeDesc,
                                               AccountTypePreEdit = assetRec.Positions.First().Account.AccountTypeDesc,
                                               AssetClassification = assetRec.AssetClass.Code,
                                               DividendFrequency = assetRec.Profile.DividendFreq,
                                               IncomeRecvd = assetRec.Revenue.Any() ? assetRec.Revenue.First().Actual : 0,
                                               Quantity = assetRec.Positions.First().Quantity,
                                               UnitPrice = assetRec.Positions.First().MarketPrice,
                                               TickerSymbol = assetRec.Profile.TickerSymbol,
                                               DateRecvd = assetRec.Revenue.Any() ? assetRec.Revenue.First().DateRecvd : DateTime.Parse("1/1/1900"),
                                               TickerSymbolDescription = assetRec.Profile.TickerDescription,
                                               CurrentInvestor = assetRec.Investor.EMailAddr
                                           });
            }

            if (customizedAssetSummary.Any())
                return Ok(customizedAssetSummary.AsQueryable());

            return BadRequest("Unable to retreive Asset summary for: " + currentInvestor);
        }

     
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateNewAsset([FromBody] AssetCreationVm submittedAsset)
        {
            
            //TODO: WIP 6-18-15 
            _currentInvestor = _identityService.CurrentUser; 
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();

            // Confirm investors' registration for idempotent operations.
            var registeredInvestor = await Task.FromResult(_repositoryInvestor.Retreive(u => u.EMailAddr.Trim() == _currentInvestor.Trim()));
            if (!registeredInvestor.Any())
                return BadRequest("Unable to create new Asset, no registration found for investor : " + _currentInvestor);

            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid/Incomplete data received for new Asset creation."});
            }

            // Reconfirm required Position data exist.
            if (!submittedAsset.PositionsCreated.Any())
                return BadRequest("Unable to create new Asset, no Position data found.");

            // ACCOUNT TYPE.
            var acctTypeCtrl = new AccountTypeController(_repositoryAccountType, _repository, _identityService);
            var existingAcctTypes = await acctTypeCtrl.GetAllAccounts() as OkNegotiatedContentResult<IList<AccountTypeVm>>;
            if(existingAcctTypes == null)
                return BadRequest("Unable to retreive required AccountType data (id) for Asset creation.");


            var duplicateCheck = GetByTicker(submittedAsset.AssetTicker.Trim());
            if(duplicateCheck.Result.ToString().Contains("OkNegotiatedContent"))
                return ResponseMessage(new HttpResponseMessage {
                                                        StatusCode = HttpStatusCode.Conflict,
                                                        ReasonPhrase = "No Asset created; duplicate Asset found for " + submittedAsset.AssetTicker.Trim()
                });

            submittedAsset.AssetInvestorId = registeredInvestor.First().InvestorId.ToString();
            submittedAsset.AssetClassificationId = await Task.FromResult(_repositoryAssetClass.Retreive(ac => ac.Code.Trim() == submittedAsset.AssetClassification.Trim())
                                                                                              .AsQueryable()
                                                                                              .First()
                                                                                              .KeyId.ToString());

            // PROFILE.
            var existingProfile = await Task.FromResult(_repositoryProfile.Retreive(p => p.TickerSymbol.Trim() == submittedAsset.AssetTicker.Trim())
                                                                          .AsQueryable());
            if (existingProfile.Any())
                submittedAsset.AssetProfileId = existingProfile.First().ProfileId.ToString();
            else
            {
                var profileCtrl = new ProfileController(_repositoryProfile);
                var newProfileVm = new ProfileVm
                                      {
                                          TickerSymbol = submittedAsset.AssetTicker.Trim(),
                                          TickerDescription = submittedAsset.AssetDescription.Trim(),
                                          DividendRate = submittedAsset.DividendPerShare,
                                          DividendYield = submittedAsset.DividendYield,
                                          DividendFreq = !string.IsNullOrWhiteSpace(submittedAsset.DividendFrequency)
                                              ? submittedAsset.DividendFrequency
                                              : "U", // (U)nkown
                                          EarningsPerShare = submittedAsset.EarningsPerShare > 0
                                              ? submittedAsset.EarningsPerShare
                                              : 0,
                                          PE_Ratio = submittedAsset.PriceEarningsRatio > 0
                                              ? submittedAsset.PriceEarningsRatio
                                              : 0,
                                          LastUpdate = DateTime.Now,
                                          ExDividendDate = submittedAsset.ExDividendDate,
                                          DividendPayDate = submittedAsset.DividendPayDate,
                                          Url = Utilities.GetBaseUrl( _repositoryInvestor.UrlAddress) + "Profile/",
                                          Price = submittedAsset.PricePerShare > 0
                                              ? submittedAsset.PricePerShare
                                              : 0
                                      };

                var createdProfile = await profileCtrl.CreateNewProfile(newProfileVm) as CreatedNegotiatedContentResult<Profile>;
                if (createdProfile == null)
                    return BadRequest("Error creating new Profile.");

                submittedAsset.AssetProfileId = createdProfile.Content.ProfileId.ToString();
            }


            // ASSET.
            var newAsset = await SaveAssetAndGetId(submittedAsset) as CreatedNegotiatedContentResult<Asset>;;
            if (newAsset == null)
                return BadRequest("Error creating new Asset or AssetId.");

            submittedAsset.AssetIdentification = newAsset.Content.AssetId.ToString();

            
            // POSITION(S).
            var positionCtrl = new PositionController(_identityService, _repository, _repositoryInvestor, _repositoryPosition, _repositoryAccountType);
            for(var pos = 0; pos < submittedAsset.PositionsCreated.Count; pos++)
            {
                // ReSharper disable once AccessToModifiedClosure
                var positionAcctTypeId = existingAcctTypes.Content.Where(at => at.AccountTypeDesc.Trim().ToUpper() == submittedAsset.PositionsCreated.ElementAt(pos)
                                                                                                                          .PreEditPositionAccount
                                                                                                                          .ToUpper()
                                                                                                                          .Trim())
                                                                                                                          .AsQueryable()
                                                                                                                          .Select(at => at.KeyId);

                if (!positionAcctTypeId.Any())
                    return BadRequest("Position creation aborted, error retreiving AccountType for: " + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim().ToUpper());

                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedAccount.KeyId = positionAcctTypeId.First();
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedAssetId = new Guid(submittedAsset.AssetIdentification); 
                submittedAsset.PositionsCreated.ElementAt(pos).Url = Utilities.GetBaseUrl(_repositoryInvestor.UrlAddress)
                                                                     + "Asset/"
                                                                     + submittedAsset.AssetTicker.Trim()
                                                                     + "/Position/"
                                                                     + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim();

                var createdPosition = await positionCtrl.CreateNewPosition(submittedAsset.PositionsCreated.ElementAt(pos)) as CreatedNegotiatedContentResult<Position>;
                if (createdPosition == null)
                    return BadRequest("Error creating new Position(s).");
                
                submittedAsset.PositionsCreated.ElementAt(pos).CreatedPositionId = createdPosition.Content.PositionId; // added 7-4-15
            }


            // INCOME (optional).
            if (!submittedAsset.RevenueCreated.Any()) 
                return ResponseMessage(new HttpResponseMessage {
                                              StatusCode = HttpStatusCode.Created,
                                              ReasonPhrase = "Asset created - affiliated Profile, and Position(s) recorded."
            });

            var incomeCtrl = new IncomeController(_identityService, _repository, _repositoryInvestor, _repositoryIncome);
            foreach (var incomeRecord in submittedAsset.RevenueCreated)
            {
                incomeRecord.Url = Utilities.GetBaseUrl(_repositoryInvestor.UrlAddress)
                                   + "Asset/"
                                   + submittedAsset.AssetTicker.Trim()
                                   + "/Income/";
               
                var createdIncome = await incomeCtrl.CreateNewIncome2(incomeRecord) as CreatedNegotiatedContentResult<Income>;
                if (createdIncome == null)
                    return BadRequest("Error creating new Income record(s).");
            }

            return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.Created,
                ReasonPhrase = "Asset created - affiliated Profile, Position(s), and Income recorded."
            });

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



        #region Helpers

            private Asset InitializeUrls(Asset assetToUpdate) {
                // Initialize with appropriate URLs.
                assetToUpdate.Url = _repositoryInvestor.UrlAddress + "/" + assetToUpdate.Profile.TickerSymbol.ToUpper().Trim();
                assetToUpdate.Investor.Url = _repositoryInvestor.UrlAddress.Replace("Asset",
                                                        "Investor/" + assetToUpdate.Investor.FirstName.Trim()
                                                        + assetToUpdate.Investor.MiddleInitial.Trim()
                                                        + assetToUpdate.Investor.LastName.Trim());
                assetToUpdate.AssetClass.Url = _repositoryInvestor.UrlAddress.Replace("Asset",
                                                         "AssetClass/" + assetToUpdate.AssetClass.LastUpdate.Trim().ToUpper());
                assetToUpdate.Profile.Url = _repositoryInvestor.UrlAddress.Replace("Asset",
                                                        "Profile/" + assetToUpdate.Profile.TickerSymbol.Trim().ToUpper());
                foreach (var position in assetToUpdate.Positions)
                    position.Url = assetToUpdate.Url + "/Position/" + position.Account.AccountTypeDesc.Trim();

                if (assetToUpdate.Revenue == null) return assetToUpdate;
                foreach (var subitem in assetToUpdate.Revenue)
                    subitem.Url = _repositoryInvestor.UrlAddress.Replace("Asset", "Income/" + Guid.NewGuid());

                return assetToUpdate;
            }
        
            private void InitializePositionsWithInvestorInfo(ref IList<Position> positionsToUpdate) {
                foreach (var position in positionsToUpdate)
                    position.InvestorKey = _currentInvestor.Trim();
            }
        
            private static bool ContainsDuplicates<T>(ref IList<T> collectionToCheck) {
                switch (typeof(T).Name) {
                    case "Position":
                        var positionData = collectionToCheck as IList<Position>;
                        return positionData != null && positionData.GroupBy(p => new { p.Account.AccountTypeDesc }).Any(p => p.Skip(1).Any());
                    case "Income":
                        var incomeData = collectionToCheck as IList<Income>;
                        return incomeData != null && incomeData.GroupBy(i => new { i.Account, i.DateRecvd }).Any(i => i.Skip(1).Any());
                }

                return false;
            }

            private static Asset MapVmToAsset(AssetCreationVm sourceVm)
            {
                return new Asset {
                    InvestorId = new Guid(sourceVm.AssetInvestorId),
                    AssetClassId = new Guid(sourceVm.AssetClassificationId),
                    ProfileId = new Guid(sourceVm.AssetProfileId),
                    AssetId = sourceVm.AssetIdentification == null ? new Guid() : new Guid(sourceVm.AssetIdentification),
                    LastUpdate = DateTime.Now
                };
            }

            private static AccountType MapVmToAccountType(AccountTypeVm sourceVm)
            {
                return new AccountType
                       {
                           //PositionRefId = sourceVm.PositionRefId,
                           AccountTypeDesc = sourceVm.AccountTypeDesc,
                           Url = ""
                       };
            }

            private async Task<IHttpActionResult> SaveAssetAndGetId(AssetCreationVm assetToSave)
            {
                var createdAsset = MapVmToAsset(assetToSave);
                var isCreated = await Task.FromResult(_repository.Create(createdAsset));
             
                if (!isCreated) return BadRequest("Unable to create new Asset for :  " + assetToSave.AssetTicker.Trim());

                //TODO: Use routeData for URL.
               return Created("http://localhost/Pims.Web.Api/api/Asset/", createdAsset);
            }



        #endregion
        




        // TODO: for Position/Update ?
        //private static bool CheckExistingPositionsAgainstNewPositions(IQueryable<Position> currentPositions, IQueryable<Position> newPositions)
        //{
        //    foreach (var positionToAdd in newPositions.Where(positionToAdd => Enumerable.Any(currentPositions)))
        //    {
        //        return currentPositions.Count(a => a.Account.AccountTypeDesc == positionToAdd.Account.AccountTypeDesc) >= 1;
        //    }
        //}


        #region Obsolete - handled via UI/AngularJs
        //[HttpGet]
        //[Route("")]
        //// Ex: http://localhost/Pims.Web.Api/api/Asset?sortBy=assetClass
        //public async Task<IHttpActionResult> GetAndSortByOther([FromUri]string sortBy)
        //{
        //   _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
        //    var currentInvestor = _identityService.CurrentUser;

        //    IQueryable<Asset> assets;
        //    IQueryable<AssetSummaryVm> assetSummary = null;

        //    switch (sortBy.Trim().ToUpper()) {
        //        case "ASSETCLASS": {
        //                assets = await Task.FromResult(_repository.Retreive(a => a.Investor
        //                                                                          .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
        //                                                           .OrderBy(item => item.AssetClass.LastUpdate).AsQueryable());

        //                assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
        //                                                                        AccountTypePostEdit = a.Revenue.First().Account,
        //                                                                        AssetClassification = a.AssetClass.LastUpdate,
        //                                                                        DividendFrequency = a.Profile.DividendFreq,
        //                                                                        IncomeRecvd = a.Revenue.First().Actual,
        //                                                                        Quantity = a.Positions.First().Quantity,
        //                                                                        UnitPrice = a.Positions.First().MarketPrice,
        //                                                                        TickerSymbol = a.Profile.TickerSymbol,
        //                                                                        DateRecvd = a.Revenue.First().DateRecvd,
        //                                                                        TickerSymbolDescription = a.Profile.TickerDescription,
        //                                                                        CurrentInvestor = a.Investor.LastName
        //                                                                });
        //                break;
        //            }
        //        case "ACCTTYPE": {
        //                assets = await Task.FromResult(_repository.Retreive(a => a.Investor
        //                                                                          .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
        //                                                          .OrderBy(item => item.Positions.First().Account.AccountTypeDesc).AsQueryable());

        //                assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
        //                                                                            AccountTypePostEdit = a.Revenue.First().Account,
        //                                                                            AssetClassification = a.AssetClass.LastUpdate,
        //                                                                            DividendFrequency = a.Profile.DividendFreq,
        //                                                                            IncomeRecvd = a.Revenue.First().Actual,
        //                                                                            Quantity = a.Positions.First().Quantity,
        //                                                                            UnitPrice = a.Positions.First().MarketPrice,
        //                                                                            TickerSymbol = a.Profile.TickerSymbol,
        //                                                                            DateRecvd = a.Revenue.First().DateRecvd,
        //                                                                            TickerSymbolDescription = a.Profile.TickerDescription,
        //                                                                            CurrentInvestor = a.Investor.LastName
        //                                                                    });
        //                break;
        //            }
        //        case "INCOME": {

        //                assets = await Task.FromResult(_repository.Retreive(a => a.Investor
        //                                                                          .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
        //                                                          .OrderByDescending(assetList => assetList.Revenue.First().Actual).AsQueryable());

        //                assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
        //                                                                        AccountTypePostEdit = a.Revenue.First().Account,
        //                                                                        AssetClassification = a.AssetClass.LastUpdate,
        //                                                                        DividendFrequency = a.Profile.DividendFreq,
        //                                                                        IncomeRecvd = a.Revenue.First().Actual,
        //                                                                        Quantity = a.Positions.First().Quantity,
        //                                                                        UnitPrice = a.Positions.First().MarketPrice,
        //                                                                        TickerSymbol = a.Profile.TickerSymbol,
        //                                                                        DateRecvd = a.Revenue.First().DateRecvd,
        //                                                                        TickerSymbolDescription = a.Profile.TickerDescription,
        //                                                                        CurrentInvestor = a.Investor.LastName
        //                                                                    });
        //                break;
        //            }
        //        case "DATERECVD": {
        //                //var withinLast12Months = DateTime.Now.AddYears(-1);
        //                assets = await Task.FromResult(_repository.Retreive(a => a.Investor
        //                                                                          .LastName.ToUpper().Trim() == currentInvestor.Trim().ToUpper())
        //                                                          .OrderByDescending(assetList => assetList.Revenue.First().DateRecvd).AsQueryable());

        //                assetSummary = assets.AsQueryable().Select(a => new AssetSummaryVm {
        //                                                                        AccountTypePostEdit = a.Revenue.First().Account,
        //                                                                        AssetClassification = a.AssetClass.LastUpdate,
        //                                                                        DividendFrequency = a.Profile.DividendFreq,
        //                                                                        IncomeRecvd = a.Revenue.First().Actual,
        //                                                                        Quantity = a.Positions.First().Quantity,
        //                                                                        UnitPrice = a.Positions.First().MarketPrice,
        //                                                                        TickerSymbol = a.Profile.TickerSymbol,
        //                                                                        DateRecvd = a.Revenue.First().DateRecvd,
        //                                                                        TickerSymbolDescription = a.Profile.TickerDescription,
        //                                                                        CurrentInvestor = a.Investor.LastName
        //                                                                    });
        //                break;
        //            }

        //    }

        //    if (assetSummary != null)
        //        return Ok(assetSummary);

        //    return BadRequest("Unable to retreive Assets for: " + currentInvestor);
        //}

        #endregion

    }
}
