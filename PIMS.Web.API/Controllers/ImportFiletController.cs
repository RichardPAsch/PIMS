using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using FluentNHibernate.Conventions;
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
        private static IGenericRepository<Income> _repositoryIncome;
        private static string _serverBaseUri = string.Empty;
        private static OkNegotiatedContentResult<List<AssetIncomeVm>> _existingInvestorAssets;
        private static bool _isDuplicateIncomeData;
        private static int _totalXlsIncomeRecordsToSave = 0;
        private static string _xlsIncomeRecordsOmitted = string.Empty;
        private static string _assetsNotAddedListing = string.Empty;


        public ImportFileController(ImportFileRepository fileRepository,
            IGenericRepository<Investor> repositoryInvestor, IPimsIdentityService identityService,
            IGenericRepository<Profile> repositoryProfile, IGenericRepository<Asset> repositoryAsset,
            IGenericRepository<Income> repositoryIncome) 
        {
                                    _fileRepository = fileRepository;
                                    _repositoryInvestor = repositoryInvestor;
                                    _identityService = identityService;
                                    _repositoryProfile = repositoryProfile;
                                    _repositoryAsset = repositoryAsset;
                                    _repositoryIncome = repositoryIncome;
        }
        
  

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> ProcessImportFile([FromBody] ImportFileVm importFile)
        {
            string dataPersistenceResults;
            var importFileUrl = importFile.ImportFilePath;
            var requestUri = Request.RequestUri.AbsoluteUri;

            _serverBaseUri = Utilities.GetWebServerBaseUri(requestUri);
            
            // Verify investor login via email addr.
            _currentInvestor = _identityService.CurrentUser;
            if (_currentInvestor == null)
            {
                //return BadRequest("Import aborted; Investor login required."); 
                // un-comment during Fiddler testing
                // TODO: in Production, exit if not logged in.
                _currentInvestor = "rpasch@rpclassics.net";
            }

            if (importFile.IsRevenueData)
            {
                var assetCtrl = new AssetController(_repositoryAsset,_identityService,_repositoryInvestor);
                var investorId = Utilities.GetInvestorId(_repositoryInvestor, _currentInvestor);
                _existingInvestorAssets = await assetCtrl.GetByInvestorAllAssets(investorId) as OkNegotiatedContentResult<List<AssetIncomeVm>>;
                var portfolioRevenueToBeInserted = ParseRevenueSpreadsheet(importFileUrl);
                if(portfolioRevenueToBeInserted == null)
                    return BadRequest("Invalid XLS data submitted.");

                var revenueToBeInserted = portfolioRevenueToBeInserted as Income[] ?? portfolioRevenueToBeInserted.ToArray();
                if (!revenueToBeInserted.Any() )
                    return BadRequest("No income data saved; please check ticker symbol(s), amount(s), and/or account(s) for validity.");

                dataPersistenceResults = PersistIncomeData(revenueToBeInserted);
            }
            else
            {
                var portfolioListing = ParsePortfolioSpreadsheet(importFileUrl);
                if (portfolioListing == null)
                    return BadRequest("Error processing Position(s) in one or more accounts.");

                var portfolioAssetsToBeInserted = portfolioListing as AssetCreationVm[] ?? portfolioListing.ToArray();
                if (!portfolioAssetsToBeInserted.Any() )
                    return BadRequest("No portfolio data saved; please check XLSX spreadsheet for valid data.");

                dataPersistenceResults = PersistPortfolioData(portfolioAssetsToBeInserted); 
            }

            var responseVm = new HttpResponseVm{ ResponseMsg = dataPersistenceResults };
            return Ok(responseVm);
        }



        private static IEnumerable<Income> ParseRevenueSpreadsheet(string filePath)
        {
           var newIncomeListing = new List<Income>();

            try
            {
                var importFile = new FileInfo(filePath);

                using (var package = new ExcelPackage(importFile))
                {
                    var workSheet = package.Workbook.Worksheets[1];
                    var totalRows = workSheet.Dimension.End.Row;
                    var totalColumns = workSheet.Dimension.End.Column;
                    _xlsIncomeRecordsOmitted = string.Empty;

                    for (var rowNum = 2; rowNum <= totalRows; rowNum++)
                    {
                        // Validate XLS
                        var headerRow = workSheet.Cells[1, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        if (!IsCorrectSpreadsheetType(true, headerRow))
                            return null;

                        var row = workSheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        var enumerableCells = row as string[] ?? row.ToArray();
                        var xlsTicker = enumerableCells.ElementAt(3).Trim();
                        var xlsAccount = Utilities.ParseAccountTypeFromDescription(enumerableCells.ElementAt(1).Trim());
                        var currentXlsAsset = _existingInvestorAssets.Content.Find(a => a.RevenueTickerSymbol == xlsTicker && a.RevenueAccount == xlsAccount);

                        // Ignore: either a bad ticker symbol, or no account was found to be affiliated with this position/asset.
                        if (currentXlsAsset == null)
                        {
                            if (_xlsIncomeRecordsOmitted == string.Empty)
                                _xlsIncomeRecordsOmitted += xlsTicker;
                            else
                                _xlsIncomeRecordsOmitted += ", " + xlsTicker;

                            continue;
                        }

                        var incomeCtrl = new IncomeController(_identityService,_repositoryAsset,_repositoryInvestor,_repositoryIncome);
                        _isDuplicateIncomeData = incomeCtrl.FindIncomeDuplicates(currentXlsAsset.RevenuePositionId.ToString(), enumerableCells.ElementAt(0), enumerableCells.ElementAt(4)) 
                                                           .Result;

                        if (_isDuplicateIncomeData)
                        {
                            if(_xlsIncomeRecordsOmitted == string.Empty)
                                _xlsIncomeRecordsOmitted +=  xlsTicker;
                            else
                                _xlsIncomeRecordsOmitted += ", " + xlsTicker;
                           
                            continue;
                        }
                        var newIncome = new Income();
                        newIncome.IncomeId = Guid.NewGuid();
                        newIncome.AssetId = currentXlsAsset.RevenueAssetId;
                        newIncome.IncomePositionId = currentXlsAsset.RevenuePositionId;
                        newIncome.DateRecvd = DateTime.Parse(enumerableCells.ElementAt(0));
                        newIncome.Actual = decimal.Parse(enumerableCells.ElementAt(4));
                        newIncome.LastUpdate = DateTime.Now;

                        newIncomeListing.Add(newIncome);
                        _totalXlsIncomeRecordsToSave += 1;
                        incomeCtrl.Dispose();
                    }
                }
            }
            catch(Exception)
            {
                return null;
            }
        
            return newIncomeListing;
        }



        private static IEnumerable<AssetCreationVm> ParsePortfolioSpreadsheet(string filePath)
        {
            var assetsToCreateList = new List<AssetCreationVm>();
            var assetCtrl = new AssetController(_repositoryAsset, _identityService, _repositoryInvestor);
            _assetsNotAddedListing = string.Empty;

            try
            {
                var lastTickerProcessed = string.Empty;
                var importFile = new FileInfo(filePath);

                using (var package = new ExcelPackage(importFile))
                {
                    var workSheet = package.Workbook.Worksheets[1];
                    var totalRows = workSheet.Dimension.End.Row;
                    var totalColumns = workSheet.Dimension.End.Column;
                    var newAsset = new AssetCreationVm();

                    // Iterate XLS/CSV, ignoring column headings (row 1).
                    for (var rowNum = 2; rowNum <= totalRows; rowNum++)
                    {
                        // Validate XLS
                        var headerRow = workSheet.Cells[1, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        if (!IsCorrectSpreadsheetType(false, headerRow))
                            return null;

                        // Args: Cells[fromRow, fromCol, toRow, toCol]
                        var row = workSheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        var enumerableCells = row as string[] ?? row.ToArray();
                        var responseAsset = assetCtrl.GetByTicker(enumerableCells.ElementAt(1).Trim());

                        // Allow new asset creation for "BadRequest" response, as no existing asset should be found for logged in investor.
                        if (responseAsset.Result.ToString().IndexOf("Bad", StringComparison.Ordinal) > 0)
                        {
                            ProfileVm assetProfilePersisted = null;
                            if (lastTickerProcessed != enumerableCells.ElementAt(1).Trim()) {

                                // Profile/ticker check via Tiingo API.
                                var assetProfile = InitializeProfile(enumerableCells.ElementAt(1).Trim(), false);

                                // Profile/ticker check via database.
                                if (assetProfile == null) {
                                    assetProfilePersisted = InitializeProfile(enumerableCells.ElementAt(1).Trim(), true);
                                }

                                // Bypass saving asset if no Profile fetched, e.g., invalid ticker symbol or no custom Profile entry ?
                                if (assetProfile == null && assetProfilePersisted == null) {
                                    if (_assetsNotAddedListing == string.Empty)
                                        _assetsNotAddedListing = "[" + enumerableCells.ElementAt(1).Trim() + "]";
                                    else
                                        _assetsNotAddedListing += ",  [" + enumerableCells.ElementAt(1).Trim() + "]";

                                    continue;
                                }

                                var newPositionsToBeSaved = InitializePositions(new List<PositionVm>(), enumerableCells);
                                if (newPositionsToBeSaved == null)
                                    return null;

                                newAsset = new AssetCreationVm {
                                    AssetTicker = enumerableCells.ElementAt(1),
                                    AssetDescription = enumerableCells.ElementAt(2).Length >= 49
                                                                        ? enumerableCells.ElementAt(2).Substring(0, 49)
                                                                        : enumerableCells.ElementAt(2),
                                    AssetClassification = "TBA",
                                    AssetClassificationId = "1b42ade9-27b9-45c7-b63f-7ef97d6cad8b",
                                    // InvestorId to be initialized during asset creation.
                                    AssetInvestorId = string.Empty,
                                    ProfileToCreate = assetProfile ?? assetProfilePersisted,
                                    PositionsCreated = newPositionsToBeSaved
                                };
                                // TODO: Allow investor to assign asset classification.
                                // Investor to assign/update classification as needed, e.g. CS [common stock], via UI. ;"TBA" (aka - to be assigned)
                                lastTickerProcessed = enumerableCells.ElementAt(1).Trim();
                                assetsToCreateList.Add(newAsset);
                            }
                            else
                                // Asset header initialization bypassed.
                                assetsToCreateList.Last().PositionsCreated = InitializePositions(newAsset.PositionsCreated, enumerableCells);
                        }
                        else
                        {
                            // Capture attempted duplicate asset insertion.
                            _assetsNotAddedListing += enumerableCells.ElementAt(1).Trim() + " ,";
                            lastTickerProcessed = enumerableCells.ElementAt(1).Trim();
                            responseAsset.Dispose();
                            continue;
                        }

                        responseAsset.Dispose();
                    } // end of XLS row looping

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Portfolio population aborted, due to {0}", e.Message));
            }

            return assetsToCreateList;
        }

        

        private static List<PositionVm> InitializePositions(List<PositionVm> existingPositions, string[] currentRow)
        {
            if (existingPositions == null) return null; // || existingPositions.Count == 0) return null;
            if (currentRow == null) throw new ArgumentNullException("currentRow");

            var mktPrice = decimal.Parse(currentRow.ElementAt(4));
            var valuation = Utilities.CalculateValuation(decimal.Parse(currentRow.ElementAt(4)), decimal.Parse(currentRow.ElementAt(3)));
            decimal fees = 0;
            var costBasis = Utilities.CalculateCostBasis(fees, valuation);
            var unitCost = Utilities.CalculateUnitCost(costBasis, decimal.Parse(currentRow.ElementAt(3)));

            var newPositions = existingPositions;

            var newPosition = new PositionVm {
                PreEditPositionAccount = currentRow.ElementAt(0),
                PostEditPositionAccount = currentRow.ElementAt(0),
                Status = "A",
                Qty = decimal.Parse(currentRow.ElementAt(3)),
                UnitCost = costBasis,
                // TODO: Allow user to assign date position added.
                // Position add date will not have been assigned, therefore assign an unlikely date & allow for investor update via UI.
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
                                            Units = decimal.Parse(currentRow.ElementAt(3)),
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



        private static ProfileVm InitializeProfile(string ticker, bool isDbProfileCheck)
        {
            using (var client = new HttpClient {BaseAddress = new Uri(_serverBaseUri)})
            {
                HttpResponseMessage response = null;
                try
                {
                    if(!isDbProfileCheck)
                        response = client.GetAsync("Pims.Web.Api/api/Profile/" + ticker).Result;
                    else
                        response = client.GetAsync("Pims.Web.Api/api/Profile/persisted/" + ticker).Result;


                    if (response.IsSuccessStatusCode)
                    {
                        var profile = response.Content.ReadAsAsync<ProfileVm>().Result;
                        // Enforce the 50 char limitation on the ticker 'description' dB field.
                        if(profile.TickerDescription.Length >= 50)
                            profile.TickerDescription = profile.TickerDescription.Substring(0, 50);

                        return profile;
                    }
                }
                catch (Exception e)
                {
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return null;
        }



        private static string PersistPortfolioData(IEnumerable<AssetCreationVm> portfolioToSave)
        {
            /*
                Note: Received 'portfolioToSave' collection contains all UNIQUE valid assets to be saved; valid in the sense 
                      that each submitted asset has an associated Profile--either preexisting, or newly fetched. Collection will
                      only contain unique ticker symbols, such that a ticker/asset/position may be affiliated with multiple
                      accounts, e.g., CHW : (CMA & Roth-IRA). Positions are persisted accordingly.
            */
           
            string statusMsg;
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
                    }
                    catch (Exception e) {
                        if (e.InnerException == null) continue;
                        if (errorList.IsEmpty())
                            errorList += assetCreationVms.First().AssetTicker.Trim();
                        else
                            errorList += ", " + assetCreationVms.First().AssetTicker.Trim();
                        
                        return "Error saving asset(s) for \n" + errorList;
                    } 
                }


                if (_assetsNotAddedListing.Any())
                    statusMsg = string.Format("Portfolio initialization partially complete, with the following asset(s) omitted ( Profile ? ) : \n{0} ",
                                                                                                     _assetsNotAddedListing);
                else
                    statusMsg = "Portfolio initialization complete; \nasset(s) successfully added.";
            }
            
            return statusMsg;
        }



        private static string PersistIncomeData(IEnumerable<Income> incomeToSave)
        {
            var savedIncomeRecordCount = 0;
            var statusMsg = string.Empty;
            var errorList = string.Empty;
            var savedIncomeRecordTotal = 0.0;

            if (incomeToSave == null) throw new ArgumentNullException("incomeToSave");

            using (var client = new HttpClient { BaseAddress = new Uri(_serverBaseUri) }) {
                var revenueCollection = incomeToSave as Income[] ?? incomeToSave.ToArray();

                foreach (var incomeRecord in revenueCollection)
                {
                    try{
                        var x = client.PostAsJsonAsync("PIMS.Web.Api/api/Spreadsheet/Income", incomeRecord).Result;
                        savedIncomeRecordCount += 1;
                        savedIncomeRecordTotal += double.Parse(incomeRecord.Actual.ToString(CultureInfo.InvariantCulture));
                        statusMsg = string.Format("Income successfully recorded for {0}/{1} record(s), totaling: ${2}", 
                                                                       savedIncomeRecordCount, revenueCollection.Length, savedIncomeRecordTotal);
                        x.Dispose();
                    }
                    catch (Exception e) {
                        if (e.InnerException == null) continue;
                        if(errorList.IsEmpty())
                            errorList += incomeRecord.IncomeAsset.Profile.TickerSymbol;
                        else
                            errorList += ", " + incomeRecord.IncomeAsset.Profile.TickerSymbol;
                    }
                }


                // Return status to handle :
                //   1. err text associated with either a bad ticker or no associated account, or
                //   2. err text associated with error saving ticker data into db.
                if (errorList.IsNotEmpty() && _xlsIncomeRecordsOmitted.Any())
                    statusMsg = "Error(s) saving/recording income for: \n" + errorList 
                                                                         + "\n and unable to process income for ticker(s): \n" 
                                                                         + _xlsIncomeRecordsOmitted 
                                                                         + ".\nCheck account, amount, and/or ticker validity.";
                else if (errorList.IsEmpty() && _xlsIncomeRecordsOmitted.Any())
                    statusMsg = "Unable to process income for ticker(s): \n" + _xlsIncomeRecordsOmitted 
                                                                           + ".\n Check account, amount, and/or ticker validity.";
                if (errorList.IsNotEmpty() && _xlsIncomeRecordsOmitted.IsNotAny())
                    statusMsg = "Error(s) saving/recording income for: \n" + errorList;
                
            }

            return statusMsg;
        }



        private static bool IsCorrectSpreadsheetType(bool containsRevenueData, IEnumerable<string> xlsRow)
        {
            if (xlsRow == null) return false;
            var enumerable = xlsRow as string[] ?? xlsRow.ToArray();
            if (containsRevenueData)
                return enumerable.ElementAt(0).Trim() == "Recvd Date" &&
                       enumerable.ElementAt(1).Trim() == "Account" &&
                       enumerable.ElementAt(2).Trim() == "Description"  &&
                       enumerable.ElementAt(3).Trim() == "Symbol"  &&
                       enumerable.ElementAt(4).Trim() == "Amount";


            return enumerable.ElementAt(0).Trim() == "Account Name" &&
                   enumerable.ElementAt(1).Trim() == "Symbol" &&
                   enumerable.ElementAt(2).Trim() == "Description" &&
                   enumerable.ElementAt(3).Trim() == "Quantity" &&
                   enumerable.ElementAt(4).Trim() == "Last Price";
        }


    }

}