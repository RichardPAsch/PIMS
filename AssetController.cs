﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using FluentNHibernate.Conventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly IAssetTypeEditsRepository<Asset> _repositoryAssetType;
        private readonly IPimsIdentityService _identityService;
        private readonly IGenericRepository<Investor> _repositoryInvestor;
        private readonly IGenericRepository<AssetClass> _repositoryAssetClass;
        private readonly IGenericRepository<Profile> _repositoryProfile;
        private readonly IGenericRepository<AccountType> _repositoryAccountType;
        private readonly IGenericRepository<Position> _repositoryPosition;
        private readonly IPositionEditsRepository<Position> _repositoryEdits;
        private readonly IGenericRepository<Income> _repositoryIncome;
        private readonly IGenericRepository<Transaction> _repositoryTransaction;
        private readonly ITransactionEditsRepository<Transaction> _repositoryTransactionEdits;
        private const string DefaultDisplayType = "detail";
        private string _currentInvestor;


        public AssetController(IGenericRepository<Asset> repository, IPimsIdentityService identityService, IGenericRepository<Investor> repositoryInvestor )
        {
            // Only used by ImportFileController.ParsePortfolioSpreadsheet().
            _repository = repository;
            _identityService = identityService;
            _repositoryInvestor = repositoryInvestor;
        }


        public AssetController(IGenericRepository<Asset> repository, IPimsIdentityService identityService,
                                                                     IGenericRepository<Investor> repositoryInvestor,
                                                                     IGenericRepository<AssetClass> repositoryAssetClass,
                                                                     IGenericRepository<Profile> repositoryProfile,
                                                                     IGenericRepository<AccountType> repositoryAccountType,
                                                                     IGenericRepository<Position> repositoryPosition,
                                                                     IPositionEditsRepository<Position> repositoryEdits,
                                                                     IGenericRepository<Income> repositoryIncome,
                                                                     IGenericRepository<Transaction> repositoryTransaction,
                                                                     ITransactionEditsRepository<Transaction> repositoryTransactionEdits,
                                                                     IAssetTypeEditsRepository<Asset> repositoryAssetType
                              )
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
            _repositoryTransaction = repositoryTransaction;
            _repositoryTransactionEdits = repositoryTransactionEdits;
            _repositoryAssetType = repositoryAssetType;
        }



        [HttpGet]
        [Route("~/api/Assets/{status}")]
        public async Task<IHttpActionResult> GetAssetSummaries(string status)
        {
            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "rpasch@rpclassics.net";

            var assetSummary = await Task.FromResult(_repository.RetreiveAll()
                .Where(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                .Select(a => new {
                    AvailablePositions = a.Positions,
                    AvailableProfile = a.Profile,
                    AssetClassifications = a.AssetClass
                })
                .SelectMany(p => p.AvailablePositions)
                .Where(p => p.Status == char.Parse(status))
                .Select(x => new AssetSummaryQueryVm {
                    TickerSymbol = x.PositionAsset.Profile.TickerSymbol,
                    TickerDescription = x.PositionAsset.Profile.TickerDescription,
                    AssetClassification = x.PositionAsset.AssetClass.Description,
                    ProfileId = x.PositionAsset.ProfileId.ToString(),
                    InvestorId = x.PositionAsset.InvestorId.ToString(),
                    AssetId = x.PositionAssetId.ToString(),
                    DistFreq = x.PositionAsset.Profile.DividendFreq
                })

                .AsQueryable()
                );

            // TODO: Temporary workaround for NHibernate bug re: "GroupBy". Fixed in v4.1.0,
            // TODO: however, deferring upgrade (v.3.3.1) for fear of breaking changes.
            assetSummary = CheckForDuplicateTickers(assetSummary);

           
            return Ok(assetSummary);                         
        }


        [HttpGet]
        [Route("~/api/AllAssets/{investorId}")]
        public async Task<IHttpActionResult> GetByInvestorAllAssets(Guid investorId)
        {
            IQueryable<Asset> existingInvestorAssets;
            try
            {
                existingInvestorAssets = await Task.FromResult(_repository.Retreive(a => a.InvestorId == investorId));
            }
            catch (Exception ex) {
                return BadRequest("Error retreiving assets for investor due to: " + ex.Message);
            }

            if (existingInvestorAssets == null || !existingInvestorAssets.Any())
                return BadRequest("Error retreiving assets for investor.");

            var cachedInvestorAssets = new List<AssetIncomeVm>();
            foreach (var asset in existingInvestorAssets)
            {
                var positionsTally = asset.Positions.Count;
                var investorAsset = new AssetIncomeVm();
                investorAsset.RevenueTickerSymbol = asset.Profile.TickerSymbol;
                investorAsset.RevenueAssetId = asset.AssetId;
                investorAsset.RevenuePositionId = asset.Positions.First().PositionId;
                investorAsset.RevenueAccount = asset.Positions.First().Account.AccountTypeDesc;

                cachedInvestorAssets.Add(investorAsset);

                if (positionsTally <= 1) continue;

                // If applicable, returned sequence should contain one element for EACH position/account combination, e.g., CSQ-IRA, CSQ-CMA.
                for (var pos = 1; pos < positionsTally; pos++)
                {
                    investorAsset = new AssetIncomeVm();
                    investorAsset.RevenueTickerSymbol = asset.Profile.TickerSymbol;
                    investorAsset.RevenueAssetId = asset.AssetId;
                    investorAsset.RevenuePositionId = asset.Positions.ElementAt(pos).PositionId; // 0-based
                    investorAsset.RevenueAccount = asset.Positions.ElementAt(pos).Account.AccountTypeDesc;

                    cachedInvestorAssets.Add(investorAsset);
                }
            }

            return Ok(cachedInvestorAssets);
        }


        [HttpGet]
        [Route("~/api/Asset/{tickerSymbol}/Account/{assetAccount}")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/CHW/Account/CMA
        public bool GetByTickerAndAccount(string tickerSymbol, string assetAccount)
        {
            //_repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "rpasch@rpclassics.net";  // temp login for Fiddler TESTING
                //currentInvestor = "maryblow@yahoo.com";
            
            var positionInfo =  Task.FromResult(_repository.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim())
                                                                            && a.Profile.TickerSymbol.Trim() == tickerSymbol.Trim())
                                                            .SelectMany(a => a.Positions).Where(p => p.Account.AccountTypeDesc == assetAccount)
                                                            .AsQueryable());

            return positionInfo.Result.Any();
        }


        [HttpGet]
        [Route("{tickerSymbol}", Name = "AssetsByTicker")]
        // Ex. http://localhost/Pims.Web.Api/api/Asset/VNR
        public async Task<IHttpActionResult> GetByTicker(string tickerSymbol)
        {
           
            //TODO: Fiddler ok 6-16-15
            //_repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "rpasch@rpclassics.net";  // temp login for Fiddler TESTING
                //currentInvestor = "maryblow@yahoo.com";
            
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
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid model state data for Asset creation."
                });
            }
            
            _currentInvestor = submittedAsset.PositionsCreated[0].LoggedInInvestor.Trim();
            if (_currentInvestor == null)
                _currentInvestor = "rpasch@rpclassics.net"; // for portfolio initialization TESTING via Fiddler.
                //_currentInvestor = "joeblow@yahoo.com";

            _repositoryInvestor.UrlAddress = ControllerContext.Request.RequestUri.ToString();

            // Confirm investors' registration for idempotent operations.
            var registeredInvestor = await Task.FromResult(_repositoryInvestor.Retreive(u => u.EMailAddr.Trim() == _currentInvestor.Trim()));
            if (!registeredInvestor.Any())
                return BadRequest("Unable to create new Asset, no registration found for investor : " + _currentInvestor);

           
            // Reconfirm required Position data exist.
            if (!submittedAsset.PositionsCreated.Any())
                return BadRequest("Unable to create new Asset, no Position data found.");



            // * ACCOUNT TYPE.*
            var acctTypeCtrl = new AccountTypeController(_repositoryAccountType, _repository, _identityService, _repositoryInvestor);
            var existingAcctTypes = await acctTypeCtrl.GetAllAccounts() as OkNegotiatedContentResult<IList<AccountTypeVm>>;
            if(existingAcctTypes == null)
                return BadRequest("Unable to retreive required AccountType data for Asset creation.");


            var isDuplicate = GetByTickerAndAccount(submittedAsset.AssetTicker.Trim().ToUpper(), submittedAsset.PositionsCreated.First().ReferencedAccount.ToString());
            if (isDuplicate)
            {
                return ResponseMessage(new HttpResponseMessage
                                       {
                                           StatusCode = HttpStatusCode.Conflict,
                                           ReasonPhrase = "No Asset created; duplicate Asset found for " + 
                                               submittedAsset.AssetTicker.Trim() + "\nin Account " + submittedAsset.PositionsCreated.First().ReferencedAccount
                                        });
            }
          
            submittedAsset.AssetInvestorId = registeredInvestor.First().InvestorId.ToString();                      // Required entry.
            submittedAsset.AssetClassificationId = await Task.FromResult(_repositoryAssetClass.Retreive(ac => ac.Code.Trim() == submittedAsset.AssetClassification.Trim())
                                                                                              .AsQueryable()
                                                                                              .First()
                                                                                              .KeyId.ToString());   // Required entry.



            // * PROFILE.*
            // Submited Asset must contain the following minimum Profile information, per client validation checks.
            var profileLastUpdate = Convert.ToDateTime(submittedAsset.ProfileToCreate.LastUpdate) ;
            if(profileLastUpdate == DateTime.MinValue)
                submittedAsset.ProfileToCreate.LastUpdate = DateTime.Today;
            
            if(submittedAsset.ProfileToCreate.TickerSymbol.IsEmpty() || submittedAsset.ProfileToCreate.TickerDescription.IsEmpty() || submittedAsset.ProfileToCreate.Price == 0) 
                return BadRequest("Asset creation aborted: minimum Profile data [ticker,tickerDesc,Price,or lastUpDate] is missing or invalid.");


            var existingProfile = await Task.FromResult(_repositoryProfile.Retreive(p => p.TickerSymbol.Trim() == submittedAsset.AssetTicker.Trim())
                                                                          .AsQueryable());

            var profileCtrl = new ProfileController(_repositoryProfile, null);
            if (existingProfile.Any())
            {
                if (existingProfile.First().LastUpdate <= DateTime.Now.AddHours(-72))
                {
                    submittedAsset.ProfileToCreate.ProfileId = existingProfile.First().ProfileId;
                    var updatedResponse = await profileCtrl.UpdateProfile(submittedAsset.ProfileToCreate) as OkNegotiatedContentResult<string>;
                    // We'll cancel Asset creation here, as out-of-date Profile data would render inaccurate income projections, should the user
                    // choose to use these projections real-time.
                    if (updatedResponse == null || !updatedResponse.Content.Contains("successfully"))
                        return BadRequest("Asset creation aborted: unable to update Profile data.");  
                }
            }
            else
            {
                // Indicate that we're creating a Profile as part of Asset/Position creation process.
                submittedAsset.ProfileToCreate.CreatedBy = "XLSX";
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



            
            // * POSITION(S) - TRANSACTION.*
            var positionCtrl = new PositionController(_identityService, _repository, _repositoryInvestor, _repositoryPosition, _repositoryAccountType, _repositoryEdits);

            // BUG: 11.30.17 - reverify loop iteration for position count > 1
            for (var pos = 0; pos < submittedAsset.PositionsCreated.Count; pos++)
            {
                // Parse account in validating allowable account only, (e.g., CMA-TRUST -> CMA) when importing data.
                var parsedAccountType = Utilities.ParseAccountTypeFromDescription(submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.ToUpper().Trim()) 
                                     ?? submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.ToUpper().Trim();

                var positionAcctTypeId = existingAcctTypes.Content.Where(at => at.AccountTypeDesc.Trim().ToUpper() == parsedAccountType)
                                                                  .AsQueryable()
                                                                  .Select(at => at.KeyId);

                if (!positionAcctTypeId.Any())
                {
                    // Rollback Asset creation (via NH Cascade) if problem with account type.
                    var deleteResponse = await DeleteAsset(submittedAsset.AssetTicker.Trim()) as OkNegotiatedContentResult<string>;
                    if (deleteResponse == null || deleteResponse.Content.Contains("Error"))
                        return BadRequest("Asset-Position creation aborted due to Asset rollback error for bad or missing AccountType: "
                            + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim().ToUpper());

                    return BadRequest("Asset-Position creation aborted due to error retreiving AccountType for: "
                                      + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim()
                                          .ToUpper());
                }

                submittedAsset.PositionsCreated.ElementAt(pos).LoggedInInvestor = _currentInvestor;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedAccount.KeyId = new Guid(positionAcctTypeId.First().ToString()); // Required entry.
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedAssetId = new Guid(submittedAsset.AssetIdentification); // Required entry.
                submittedAsset.PositionsCreated.ElementAt(pos).Url =
                    Utilities.GetBaseUrl(_repositoryInvestor.UrlAddress)
                    + "Asset/"
                    + submittedAsset.AssetTicker.Trim()
                    + "/Position/"
                    + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim();

                // BUG: verify SECOND position is actually saved
                var createdPosition = await positionCtrl.CreateNewPosition(submittedAsset.PositionsCreated.ElementAt(pos)) as CreatedNegotiatedContentResult<Position>;
                if (createdPosition == null)
                {
                    // Rollback Asset creation.
                    var deleteResponse = await DeleteAsset(submittedAsset.AssetTicker.Trim()) as OkNegotiatedContentResult<string>;
                    if (deleteResponse == null || deleteResponse.Content.Contains("Error"))
                        return BadRequest("Asset-Position creation aborted due to Asset rollback error for Position: "
                                          + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim()
                                              .ToUpper());

                    return BadRequest("Asset-Position creation aborted due to error creating Position for : "
                                      + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim()
                                          .ToUpper());
                }

                submittedAsset.PositionsCreated.ElementAt(pos).CreatedPositionId = createdPosition.Content.PositionId;
            
                // Initialize Transaction component of new Position(s); Position:Transaction = 1:1 for each new Position created. 7.12.17

                var transactionCtrl = new TransactionController(_repositoryTransaction, _identityService, _repositoryInvestor, _repository, _repositoryTransactionEdits);
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.PositionId = createdPosition.Content.PositionId;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.TransactionEvent = "B";
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.Fees = submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.Fees;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.Units = submittedAsset.PositionsCreated.ElementAt(pos).Qty;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.MktPrice = submittedAsset.ProfileToCreate.Price;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.Valuation =
                                                                            submittedAsset.PositionsCreated.ElementAt(pos).Qty *
                                                                            submittedAsset.ProfileToCreate.Price;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.CostBasis =
                                                                            submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.Valuation +
                                                                            submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.Fees;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.UnitCost =
                                                                            submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.CostBasis /
                                                                            submittedAsset.PositionsCreated.ElementAt(pos).Qty;
                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.DateCreated = DateTime.Parse(submittedAsset.PositionsCreated.ElementAt(pos).LastUpdate.ToString());


                var createdTransaction = await transactionCtrl.CreateNewTransaction(submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction) as CreatedNegotiatedContentResult<Transaction>;
                if (createdTransaction == null)
                    return BadRequest("Position-Transaction creation aborted due to error creating Transaction for Position : "
                                                                    + submittedAsset.PositionsCreated.ElementAt(pos).PreEditPositionAccount.Trim().ToUpper());

                submittedAsset.PositionsCreated.ElementAt(pos).ReferencedTransaction.TransactionId = createdTransaction.Content.TransactionId;
            }  



            // * INCOME (optional). *
            if (submittedAsset.RevenueCreated == null) 
                return ResponseMessage(new HttpResponseMessage {
                                              StatusCode = HttpStatusCode.Created,
                                              ReasonPhrase = "Asset created w/o submitted Income; affiliated Profile, Position(s), and Transaction(s) sucessfully recorded."
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
                ReasonPhrase = "Asset created, with affiliated Profile, Position(s) - Transaction(s), and Income successfully recorded."
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


        [HttpPut]
        [HttpPatch]
        [Route("~/api/AssetTypeUpdates/Asset")]
        // Ex. http://localhost/Pims.Web.Api/api/AssetTypeUpdates/Asset  
        public async Task<IHttpActionResult> UpdateByTypes([FromBody]object[] editedTypes)
        {
            var currentInvestor = _identityService.CurrentUser;
            if (string.IsNullOrEmpty(currentInvestor.Trim()))
                currentInvestor = "maryblow@yahoo.com";
                //return BadRequest(string.Format("No login credentials found for current investor"));

            
            var pendingAssetUpdates = new Asset[editedTypes.Length];// { };
            for(var i = 0; i < editedTypes.Length; i++)
            {
                var assetPendingUpdate = JsonConvert.DeserializeObject<Asset>(editedTypes[i].ToString());
                assetPendingUpdate.LastUpdate = DateTime.Now;
                pendingAssetUpdates[i] = assetPendingUpdate;
            }


            try
            {
                var isUpdated = await Task.FromResult(_repositoryAssetType.UpdateAssetTypes(pendingAssetUpdates));
                if (!isUpdated)
                    return BadRequest("Unable to update one or more asset types.");
            }
            catch (Exception e)
            {
                return BadRequest("Error updating asset type(s) due to: " + e.Data);
            }

            return Ok("Successful asset type update(s).");
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
                try
                {
                    return new Asset {
                                         InvestorId = new Guid(sourceVm.AssetInvestorId),
                                         AssetClassId = new Guid(sourceVm.AssetClassificationId),
                                         ProfileId = sourceVm.ProfileToCreate.ProfileId,
                                         AssetId = sourceVm.AssetIdentification == null ? Guid.NewGuid() : new Guid(sourceVm.AssetIdentification),
                                         LastUpdate = DateTime.Now
                                     };
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        var errMsg = e.InnerException.Message;
                    }
                }
                return null;
            }

        
            private async Task<IHttpActionResult> SaveAssetAndGetId(AssetCreationVm assetToSave)
            {
                var isCreated = false;
                var createdAsset = MapVmToAsset(assetToSave);

                try
                {
                    isCreated = await Task.FromResult(_repository.Create(createdAsset));
                }
                catch (Exception e)
                {
                    var errMsg = e.InnerException;
                    if (!isCreated)
                        return BadRequest("Unable to create new Asset for :  " + assetToSave.AssetTicker.Trim() + " due to: " + errMsg);
                }
                
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
                                                    AssetClassification = asset.AssetClassification,
                                                    ProfileId = asset.ProfileId,
                                                    InvestorId = asset.InvestorId,
                                                    AssetId = asset.AssetId,
                                                    DistFreq = asset.DistFreq
                                                }
                                             );
                    }
                    previousTicker = asset.TickerSymbol;
                }

                return sourceCollection2.AsQueryable();
            }


        #endregion
        


    }
}
