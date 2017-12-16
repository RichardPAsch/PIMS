using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Office.Interop.Excel;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using FluentNHibernate.Conventions;
using FluentNHibernate.Utils;
using Microsoft.AspNet.Identity;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Mapping;
using NHibernate.Util;
using OfficeOpenXml;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;
using PIMS.Core.Security;
using PIMS.Data.Repositories;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/ImportFile")]
    public class ImportFileController : ApiController
    {
        private static ImportFileRepository _fileRepository;
        private static IGenericRepository<Investor> _repositoryInvestor;
        private static IPimsIdentityService _identityService;
        private static string _currentInvestor;
        private static IGenericRepository<Profile> _repositoryProfile;
        private static IGenericRepository<Asset> _repositoryAsset;
        private static string _assetNotAddedListing = string.Empty;
        private static int _assetCountToSave = 0;
        private static string _serverBaseUri = string.Empty;
        private static OkNegotiatedContentResult<List<AssetIncomeVm>> _existingInvestorAssets;


        public ImportFileController(ImportFileRepository fileRepository,
            IGenericRepository<Investor> repositoryInvestor, IPimsIdentityService identityService,
            IGenericRepository<Profile> repositoryProfile, IGenericRepository<Asset> repositoryAsset)
        {
            _fileRepository = fileRepository;
            _repositoryInvestor = repositoryInvestor;
            _identityService = identityService;
            _repositoryProfile = repositoryProfile;
            _repositoryAsset = repositoryAsset;
        }

        //  public async Task<IHttpActionResult> ProcessImportFile([FromBody] string importFileUrl)
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> ProcessImportFile([FromBody] ImportFileVm importFile)
        {
            var statusResults = string.Empty;
            var importFileUrl = importFile.ImportFilePath;
            var requestUri = Request.RequestUri.AbsoluteUri;


            _serverBaseUri = Utilities.GetWebServerBaseUri(requestUri);
            
            // Verify investor login via email addr.
            _currentInvestor = _identityService.CurrentUser;
            if (_currentInvestor == null)
            {
                //return BadRequest("Import aborted; Investor login required."); 

                // un-comment during Fiddler testing
                _currentInvestor = "rpasch@rpclassics.net";
                //importFileUrl = @"C:\Downloads\FidelityXLS\2017SEP_RevenueTemplateTEST.xlsx";          // REVENUE data 
                //importFileUrl = @"C:\Downloads\FidelityXLS\Portfolio_PositionsTEST_7_Fidelity.xlsx";   // PORTFOLIO data
            }

            if (importFile.IsRevenueData)
            {
                var assetCtrl = new AssetController(_repositoryAsset,_identityService,_repositoryInvestor);
                var investorId = Utilities.GetInvestorId(_repositoryInvestor, _currentInvestor);
                _existingInvestorAssets = await assetCtrl.GetByInvestorAllAssets(investorId) as OkNegotiatedContentResult<List<AssetIncomeVm>>;
                var portfolioRevenueToBeInserted = ParseRevenueSpreadsheet(importFileUrl);
                if (portfolioRevenueToBeInserted == null)
                    return BadRequest("Error(s) recording income.");
            }
            else
            {
                var portfolioListingoToBeInserted = ParsePortfolioSpreadsheet(importFileUrl);
                var listingoToBeInserted = portfolioListingoToBeInserted as AssetCreationVm[] ?? portfolioListingoToBeInserted.ToArray();
                _assetCountToSave = listingoToBeInserted.Length;
                statusResults = PersistPortfolioData(listingoToBeInserted);
            }

            return Ok(statusResults);
        }



        private static IEnumerable<Income> ParseRevenueSpreadsheet(string filePath)
        {
            var lastTickerProcessed = string.Empty;
            var lastDateRecvdProcessed = string.Empty;
            var lastAccountProcessed = string.Empty;
            var newIncomeListing = new List<Income>();

            try
            {
                var importFile = new FileInfo(filePath);

                using (var package = new ExcelPackage(importFile))
                {
                    var workSheet = package.Workbook.Worksheets[1];
                    var totalRows = workSheet.Dimension.End.Row;
                    _assetCountToSave = totalRows;
                    var totalColumns = workSheet.Dimension.End.Column;

                    for (var rowNum = 2; rowNum <= totalRows; rowNum++)
                    {
                        var row = workSheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        var enumerableCells = row as string[] ?? row.ToArray();
                        var xlsTicker = enumerableCells.ElementAt(3).Trim();
                        var xlsAccount = Utilities.ParseAccountTypeFromDescription(enumerableCells.ElementAt(1).Trim());
                        var currentXlsAsset = _existingInvestorAssets.Content.Find(a => a.RevenueTickerSymbol == xlsTicker && a.RevenueAccount == xlsAccount);

                        // Ignore if no Position yet established for this account.
                        if (currentXlsAsset == null) continue;

                        var newIncome = new Income();
                        newIncome.IncomeId = Guid.NewGuid();
                        newIncome.AssetId = currentXlsAsset.RevenueAssetId;
                        newIncome.IncomePositionId = currentXlsAsset.RevenuePositionId;
                        newIncome.DateRecvd = DateTime.Parse(enumerableCells.ElementAt(0));
                        newIncome.Actual = decimal.Parse(enumerableCells.ElementAt(4));
                        newIncome.LastUpdate = DateTime.Now;

                        newIncomeListing.Add(newIncome);
                    }
                }
            }
            catch(Exception ex)
            {
                return null;
            }

        
            return newIncomeListing;
        }


        private static IEnumerable<AssetCreationVm> ParsePortfolioSpreadsheet(string filePath)
        {
            var assetsToCreate = new List<AssetCreationVm>();
            var assetCtrl = new AssetController(_repositoryAsset, _identityService, _repositoryInvestor);

            try
            {
                var lastTickerProcessed = string.Empty;
                var importFile = new FileInfo(filePath);

                using (var package = new ExcelPackage(importFile))
                {
                    var workSheet = package.Workbook.Worksheets[1];
                    var totalRows = workSheet.Dimension.End.Row;
                    _assetCountToSave = totalRows;
                    var totalColumns = workSheet.Dimension.End.Column;
                    var newAsset = new AssetCreationVm();

                    // Iterate XLS/CSV, ignoring column headings (row 1).
                    for (var rowNum = 2; rowNum <= totalRows; rowNum++)
                    {
                        // Args: Cells[fromRow, fromCol, toRow, toCol]
                        var row = workSheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        var enumerableCells = row as string[] ?? row.ToArray();
                        var responseAsset = assetCtrl.GetByTicker(enumerableCells.ElementAt(1).Trim());

                        //var responseAsset = assetCtrl.GetByTicker("CSQ"); TESTING

                        // New asset creation expected for "BadRequest" response, due to no existing asset found for logged in investor.
                        if (responseAsset.Result.ToString().IndexOf("Bad", StringComparison.Ordinal) > 0)
                        {
                            if (lastTickerProcessed != enumerableCells.ElementAt(1).Trim())
                            {
                                newAsset = new AssetCreationVm();
                                newAsset.AssetTicker = enumerableCells.ElementAt(1);
                                newAsset.AssetDescription = enumerableCells.ElementAt(2).Length >= 49
                                    ? enumerableCells.ElementAt(2).Substring(0, 49)
                                    : enumerableCells.ElementAt(2);
                                // TODO: Allow investor to assign asset classification.
                                // Investor to assign/update classification as needed, e.g. CS [common stock], via UI.
                                newAsset.AssetClassification = "TBA"; // aka - to be assigned
                                newAsset.AssetClassificationId = "1b42ade9-27b9-45c7-b63f-7ef97d6cad8b";
                                // InvestorId to be initialized during asset creation.
                                newAsset.AssetInvestorId = string.Empty;
                                newAsset.ProfileToCreate = InitializeProfile(newAsset.AssetTicker.Trim());
                                newAsset.PositionsCreated = InitializePositions(new List<PositionVm>(), enumerableCells);
                                lastTickerProcessed = enumerableCells.ElementAt(1).Trim();
                                assetsToCreate.Add(newAsset);
                            }
                            else
                                // Asset header initialization bypassed; processing same ticker, different account(s). Created
                                // position(s) collection passed for new position addition.
                                newAsset.PositionsCreated = InitializePositions(newAsset.PositionsCreated, enumerableCells);
                        }
                        else
                        {
                            // Capture attempted duplicate asset insertion.
                            _assetNotAddedListing += enumerableCells.ElementAt(1).Trim() + " ,";
                            lastTickerProcessed = enumerableCells.ElementAt(1).Trim();
                            responseAsset.Dispose();
                            continue;
                        }

                        //lastTickerProcessed = enumerableCells.ElementAt(1).Trim();
                        // DON'T re-Add asset if ticker is the same BUG here
                        //assetsToCreate.Add(newAsset);
                        responseAsset.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Portfolio population aborted, due to {0}", e.Message));
            }

            return assetsToCreate;
        }



        private static ProfileVm InitializeProfile(string ticker)
        {
            using (var client = new HttpClient {BaseAddress = new Uri(_serverBaseUri)})
            {
                try
                {
                    var response = client.GetAsync("Pims.Web.Api/api/Profile/" + ticker).Result;
                    return !response.IsSuccessStatusCode ? null : response.Content.ReadAsAsync<ProfileVm>().Result;
                }
                catch (Exception e)
                {
                    //var debug = 1;
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return null;
        }



        private static List<PositionVm> InitializePositions(List<PositionVm> existingPositions, string[] currentRow)
        {
            if (existingPositions == null) throw new ArgumentNullException("existingPositions");
            if (currentRow == null) throw new ArgumentNullException("currentRow");

            var mktPrice = decimal.Parse(currentRow.ElementAt(4));
            var valuation = Utilities.CalculateValuation(decimal.Parse(currentRow.ElementAt(4)), int.Parse(currentRow.ElementAt(3)));
            var fees = 0;
            var costBasis = Utilities.CalculateCostBasis(fees, valuation);
            var unitCost = Utilities.CalculateUnitCost(costBasis, int.Parse(currentRow.ElementAt(3)));

            var newPositions = existingPositions;

            var newPosition = new PositionVm {
                PreEditPositionAccount = currentRow.ElementAt(0),
                PostEditPositionAccount = currentRow.ElementAt(0),
                Status = "A",
                Qty = int.Parse(currentRow.ElementAt(3)),
                UnitCost = costBasis,
                // TODO: Allow user to assign date position added.
                // Unlikely that position add date has been assigned, therefore allow for investor update via UI.
                DateOfPurchase = new DateTime(1950,1,1),
                DatePositionAdded = null,
                LastUpdate = DateTime.Now,
                Url = "",
                LoggedInInvestor = _identityService.CurrentUser,
                ReferencedAssetId = Guid.NewGuid(),            // initialized during Asset creation
                ReferencedAccount = new AccountTypeVm
                                    {
                                        AccountTypeDesc = currentRow.ElementAt(0),
                                        KeyId = Guid.NewGuid(), // Guid for AccountType, initialized during Asset creation
                                        Url = ""
                                    },
                ReferencedTransaction = new TransactionVm
                                        {
                                            PositionId = Guid.NewGuid(),
                                            TransactionId = Guid.NewGuid(),
                                            Units = int.Parse(currentRow.ElementAt(3)),
                                            TransactionEvent = "C",
                                            MktPrice = mktPrice,
                                            Fees = fees,
                                            UnitCost = unitCost,
                                            CostBasis = costBasis,
                                            Valuation = valuation,
                                            DateCreated = DateTime.Now,
                                            DatePositionCreated = null
                                        }
            };
            
            newPositions.Add(newPosition);
            return newPositions;
        }



        private static string PersistPortfolioData(IEnumerable<AssetCreationVm> portfolioToSave)
        {
            var assetCountSaved = 0;
            var statusMsg = string.Empty;
            var errorList = string.Empty;

            if (portfolioToSave == null) throw new ArgumentNullException("portfolioToSave");
            
            using (var client = new HttpClient { BaseAddress = new Uri(_serverBaseUri) })
            {
                var assetCreationVms = portfolioToSave as AssetCreationVm[] ?? portfolioToSave.ToArray();
                foreach (var asset in assetCreationVms)
                {
                    try
                    {
                        var httpResponseMessage = client.PostAsJsonAsync("PIMS.Web.Api/api/Asset", asset).Result;
                        assetCountSaved += 1;
                        statusMsg = string.Format("Sucessfully added {0}/{1} asset(s) as part of PIMS portfolio initialization.", assetCountSaved, _assetCountToSave);
                    }
                    catch (Exception e) {
                        if (e.InnerException != null)
                        {
                            errorList += assetCreationVms.First().AssetTicker.Trim() + ", ";
                            statusMsg = "Error saving asset(s) for " + errorList;
                        }
                    } 
                }
             }
            
            return statusMsg;
        }


        private static List<object> FetchAssetProfilesForInvestor(Guid investorId)
        {
            var profileCtrl = new ProfileController(_repositoryProfile);
            var assetCtrl = new AssetController(_repositoryAsset,_identityService, _repositoryInvestor);

            var allProfiles = profileCtrl.GetAllPersistedProfiles();
            //var allAssets = assetCtrl.

            return null;
        }
        
    }
}