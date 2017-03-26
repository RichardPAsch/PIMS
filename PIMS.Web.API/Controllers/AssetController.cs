using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using FluentNHibernate.Conventions;
using NHibernate.Linq;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;
using PIMS.Data.Repositories;
using PIMS.Core.Security;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Asset")]
    //[Authorize] // temp comment for Fiddler testing 3.22.16
    public class AssetController : ApiController
    {
        private readonly IGenericRepository<Asset> _repository;
        private readonly IPimsIdentityService _identityService;
        private readonly IGenericRepository<Investor> _repositoryInvestor;
        private readonly IGenericRepository<AssetClass> _repositoryAssetClass;
        private readonly IGenericRepository<Profile> _repositoryProfile;
        private readonly IGenericRepository<AccountType> _repositoryAccountType;
        private readonly IGenericRepository<Position> _repositoryPosition;
        private readonly IPositionEditsRepository<Position> _repositoryEdits;
        private readonly IGenericRepository<Income> _repositoryIncome;
        private const string DefaultDisplayType = "detail";
        private string _currentInvestor;


        public AssetController(IGenericRepository<Asset> repository, IPimsIdentityService identityService,
                                                                     IGenericRepository<Investor> repositoryInvestor,
                                                                     IGenericRepository<AssetClass> repositoryAssetClass,
                                                                     IGenericRepository<Profile> repositoryProfile,
                                                                     IGenericRepository<AccountType> repositoryAccountType,
                                                                     IGenericRepository<Position> repositoryPosition,
                                                                     IPositionEditsRepository<Position> repositoryEdits,
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
            _repositoryEdits = repositoryEdits;
        }



        [HttpGet]
        [Route("~/api/Assets/{status}")]
        public async Task<IHttpActionResult> GetAssetSummaries(string status)
        {
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "rpasch2@rpclassics.net";

            var assetSummary = await Task.FromResult(_repository.RetreiveAll()
                .Where(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                .Select(a => new
                             {
                                 AvailablePositions = a.Positions,
                                 AvailableProfile = a.Profile,
                                 AssetClassifications = a.AssetClass
                             })
                .SelectMany(p => p.AvailablePositions)
                .Where(p => p.Status == status)
                .Select(x => new AssetSummaryQueryVm
                             {
                                 TickerSymbol = x.PositionAsset.Profile.TickerSymbol,
                                 TickerDescription = x.PositionAsset.Profile.TickerDescription,
                                 AssetClassification = x.PositionAsset.AssetClass.Description
                             })

                .AsQueryable()
                );

            // TODO: Temporary workaround for NHibernate bug re: "GroupBy". Fixed in v4.1.0,
            // TODO: however, deferring upgrade (v.3.3.1) for fear of breaking changes.
            assetSummary = CheckForDuplicateTickers(assetSummary);
                //.GroupBy(x => x.TickerSymbol)
                //.Select(x => x.First())
                //);
           
            return Ok(assetSummary);                         
        }



        [HttpGet]
        [Route("{tickerSymbol}", Name = "AssetsByTicker")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/VNR
        public async Task<IHttpActionResult> GetByTicker(string tickerSymbol)
        {
            //TODO: Fiddler ok 6-16-15
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "rpasch2@rpclassics.net";
            
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
            var urlForProfile = _repositoryInvestor.UrlAddress.Remove(_repositoryInvestor.UrlAddress.IndexOf("/A", 0, System.StringComparison.Ordinal));
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
                                                                        ProfileUrl = urlForProfile + "/Profile/" + a.Profile.TickerSymbol.Trim().ToUpper(),
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
            _currentInvestor = _identityService.CurrentUser;

            if (_currentInvestor == null)
                _currentInvestor = "joeblow@yahoo.com";

            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();

            // Confirm investors' registration for idempotent operations.
            var registeredInvestor = await Task.FromResult(_repositoryInvestor.Retreive(u => u.EMailAddr.Trim() == _currentInvestor.Trim()));
            if (!registeredInvestor.Any())
                return BadRequest("Unable to create new Asset, no registration found for investor : " + _currentInvestor);

            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid/Incomplete required 'ModelState' data received for new Asset creation."
                });
            }

            // Reconfirm required Position data exist.
            if (!submittedAsset.PositionsCreated.Any())
                return BadRequest("Unable to create new Asset, no Position data found.");



            // * ACCOUNT TYPE.*
            var acctTypeCtrl = new AccountTypeController(_repositoryAccountType, _repository, _identityService, _repositoryInvestor);
            var existingAcctTypes = await acctTypeCtrl.GetAllAccounts() as OkNegotiatedContentResult<IList<AccountTypeVm>>;
            if(existingAcctTypes == null)
                return BadRequest("Unable to retreive required AccountType data for Asset creation.");


            var duplicateCheck = GetByTicker(submittedAsset.AssetTicker.Trim());
            if(duplicateCheck.Result.ToString().Contains("OkNegotiatedContent"))
                return ResponseMessage(new HttpResponseMessage {
                                                        StatusCode = HttpStatusCode.Conflict,
                                                        ReasonPhrase = "No Asset created; duplicate Asset found for " + submittedAsset.AssetTicker.Trim()
                });

            submittedAsset.AssetInvestorId = registeredInvestor.First().InvestorId.ToString();                      // Required entry.
            submittedAsset.AssetClassificationId = await Task.FromResult(_repositoryAssetClass.Retreive(ac => ac.Description.Trim() == submittedAsset.AssetClassification.Trim())
                                                                                              .AsQueryable()
                                                                                              .First()
                                                                                              .KeyId.ToString());   // Required entry.



            // * PROFILE.*
            // Submited Asset must contain the following minimum Profile information, per client validation checks.
            var profileLastUpdate = Convert.ToDateTime(submittedAsset.ProfileToCreate.LastUpdate) ;
            
            if(submittedAsset.ProfileToCreate.TickerSymbol.IsEmpty() || 
               submittedAsset.ProfileToCreate.TickerDescription.IsEmpty() ||
               submittedAsset.ProfileToCreate.Price == 0 || 
               profileLastUpdate == DateTime.MinValue ||  // unassigned
               submittedAsset.ProfileToCreate.Url.IsEmpty())
                    return BadRequest("Asset creation aborted: minimum Profile data [ticker,tickerDesc,Price,lastUpDate, or Url] is missing or invalid.");

            var existingProfile = await Task.FromResult(_repositoryProfile.Retreive(p => p.TickerSymbol.Trim() == submittedAsset.AssetTicker.Trim())
                                                                          .AsQueryable());

            var profileCtrl = new ProfileController(_repositoryProfile);
            if (existingProfile.Any())
            {
                if (existingProfile.First().LastUpdate >= DateTime.Now.AddHours(-24))
                    submittedAsset.ProfileToCreate.ProfileId = existingProfile.First().ProfileId;
                else
                {
                    var updatedResponse = await profileCtrl.UpdateProfile(submittedAsset.ProfileToCreate) as OkNegotiatedContentResult<string>;
                    // We'll cancel Asset creation here, as out-of-date Profile data would render inaccurate income projections, should the user
                    // choose to use these projections real-time.
                    if (updatedResponse == null || !updatedResponse.Content.Contains("successfully"))
                        return BadRequest("Asset creation aborted: unable to update Profile data.");
                }
            }
            else
            {
                var createdProfile = await profileCtrl.CreateNewProfile(submittedAsset.ProfileToCreate) as CreatedNegotiatedContentResult<Profile>;
                if (createdProfile == null)
                    return BadRequest("Error creating new Profile.");

                submittedAsset.ProfileToCreate.ProfileId = createdProfile.Content.ProfileId;
            }



            // * ASSET.*
            var newAsset = await SaveAssetAndGetId(submittedAsset) as CreatedNegotiatedContentResult<Asset>;;
            if (newAsset == null)
                return BadRequest("Error creating new Asset or AssetId.");

            submittedAsset.AssetIdentification = newAsset.Content.AssetId.ToString();



            
            // * POSITION(S).*
            var positionCtrl = new PositionController(_identityService, _repository, _repositoryInvestor, _repositoryPosition, _repositoryAccountType, _repositoryEdits);
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
                {
                    // Rollback Asset creation (via NH Cascade).
                    var deleteResponse = await DeleteAsset(submittedAsset.AssetTicker.Trim()) as OkNegotiatedContentResult<string>;
                    if(deleteResponse == null || deleteResponse.Content.Contains("Error"))
                        return BadRequest("Asset-Position creation aborted due to Asset rollback error for bad AccountType: " 
                                                                    + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim().ToUpper()); 

                    return BadRequest("Asset-Position creation aborted due to error retreiving AccountType for: " 
                                                                    + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim().ToUpper()); 
                }
                    

                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedAccount.KeyId = positionAcctTypeId.First();                // Required entry.
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedAssetId = new Guid(submittedAsset.AssetIdentification);    // Required entry.
                submittedAsset.PositionsCreated.ElementAt(pos).Url = Utilities.GetBaseUrl(_repositoryInvestor.UrlAddress)
                                                                     + "Asset/"
                                                                     + submittedAsset.AssetTicker.Trim()
                                                                     + "/Position/"
                                                                     + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim();

                var createdPosition = await positionCtrl.CreateNewPosition(submittedAsset.PositionsCreated.ElementAt(pos)) as CreatedNegotiatedContentResult<Position>;
                if (createdPosition == null)
                {
                    // Rollback Asset creation.
                    var deleteResponse = await DeleteAsset(submittedAsset.AssetTicker.Trim()) as OkNegotiatedContentResult<string>;
                    if (deleteResponse == null || deleteResponse.Content.Contains("Error"))
                        return BadRequest("Asset-Position creation aborted due to Asset rollback error for Position: " 
                                                                    + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim().ToUpper());

                    return BadRequest("Asset-Position creation aborted due to error creating Position for : " 
                                                                    + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim().ToUpper());

                }
                
                submittedAsset.PositionsCreated.ElementAt(pos).CreatedPositionId = createdPosition.Content.PositionId; 
            }



            // * INCOME (optional). *
            if (submittedAsset.RevenueCreated == null) 
                return ResponseMessage(new HttpResponseMessage {
                                              StatusCode = HttpStatusCode.Created,
                                              ReasonPhrase = "Asset created w/o submitted Income - affiliated Profile, and Position(s) recorded."
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
                    return BadRequest("Error creating new Income record(s). Please resubmit Income.");
            }

            return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.Created,
                ReasonPhrase = "Asset created - affiliated Profile, Position(s), and Income recorded."
            });

        }


        [HttpPut]
        [HttpPatch]
        [Route("{existingTicker?}")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/IBM  [Asset Summary edits]
        public async Task<IHttpActionResult> UpdateByTicker([FromBody] AssetSummaryVm assetViewModel, string existingTicker)
        {
            var currentInvestor = _identityService.CurrentUser;
            var matchingAsset = await Task.FromResult(_repository.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim())
                                                                             && a.Profile.TickerSymbol.ToUpper().Trim() == existingTicker.ToUpper().Trim())
                                                                 .AsQueryable());

            if (!matchingAsset.Any())
                return BadRequest(string.Format("Unable to find asset {0} for {1}.", existingTicker, currentInvestor));

            bool isUpdated;
           
           // Map any changes & send to repository.
            var updatedAsset = ModelParser.ParseAssetForUpdates(assetViewModel, matchingAsset.First(), out isUpdated);
            var isPersisted = _repository.Update(updatedAsset, updatedAsset.AssetId);

            if (isUpdated && isPersisted)
                return Ok("Asset update(s) successful for " + existingTicker.Trim().ToUpper());
            
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

            var assetToRemove = await Task.FromResult(_repository.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()) 
                                                                             && a.Profile.TickerSymbol.Trim() == tickerSymbol.Trim())
                                                                  .AsQueryable());

            if (!assetToRemove.Any())
                return BadRequest(string.Format("No assets found matching {0} for {1}", tickerSymbol.ToUpper(), currentInvestor));

            
            if (_repository.Delete(assetToRemove.First().AssetId))
                return Ok(string.Format("Asset {0} with related Position[s] & Revenue, successfully removed", tickerSymbol));

            return BadRequest(string.Format("Error removing asset: {0}", tickerSymbol));
        }




        #region Helpers

            private static Asset MapVmToAsset(AssetCreationVm sourceVm)
            {
                return new Asset {
                    InvestorId = new Guid(sourceVm.AssetInvestorId),
                    AssetClassId = new Guid(sourceVm.AssetClassificationId),
                    ProfileId = sourceVm.ProfileToCreate.ProfileId,
                    AssetId = sourceVm.AssetIdentification == null ? Guid.NewGuid() : new Guid(sourceVm.AssetIdentification),
                    LastUpdate = DateTime.Now
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


            private static IQueryable<AssetSummaryQueryVm> CheckForDuplicateTickers(IEnumerable<AssetSummaryQueryVm> sourceCollection)
            {
                var previousTicker = string.Empty;
                var sourceCollection2 = new List<AssetSummaryQueryVm>();
                sourceCollection = sourceCollection.ToList().OrderBy(p => p.TickerSymbol);

                foreach (var asset in sourceCollection)
                {
                    if (asset.TickerSymbol.Trim() != previousTicker.Trim())
                    {
                        sourceCollection2.Add(new AssetSummaryQueryVm
                                                {
                                                    TickerSymbol = asset.TickerSymbol,
                                                    TickerDescription = asset.TickerDescription,
                                                    AssetClassification = asset.AssetClassification
                                                }
                                             );
                    }
                    previousTicker = asset.TickerSymbol;
                }

                return sourceCollection2.AsQueryable();
            }


        #endregion
        





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
