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
//using Microsoft.Office.Interop.Excel;
using System.Web.Http;
using System.Web.Http.Results;
using FluentNHibernate.Conventions;
using FluentNHibernate.Utils;
using Microsoft.AspNet.Identity;
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


        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> ProcessImportFile([FromBody] string importFileUrl)
        {
            // Verify investor login via email addr.
            _currentInvestor = _identityService.CurrentUser;
            if (_currentInvestor == null)
            {
                //return BadRequest("Import aborted; Investor login required."); 

                // un-comment for Fiddler testing
                _currentInvestor = "rpasch@rpclassics.net";
                //importFileUrl = @"C:\Downloads\FidelityXLS\2017SEP_RevenueTemplateTEST.xlsx";      // REVENUE data 
                importFileUrl = @"C:\Downloads\FidelityXLS\Portfolio_PositionsTEST_Fidelity.xlsx"; // PORTFOLIO data
            }

            ParsePortfolioSpreadsheet(importFileUrl);





            // TODO: 10.26.17 - Will need to run ParseFile() to extract PositionsCreated[0].LoggedInInvestor; defer following lines:
            //var existingInvestor = await Task.FromResult(_repositoryInvestor.Retreive(i => i.EMailAddr.Trim() == dataFile.EMailAddr.Trim()));
            //if (!existingInvestor.Any())
            //    return ResponseMessage(new HttpResponseMessage {
            //        StatusCode = HttpStatusCode.Conflict,
            //        ReasonPhrase = "Invalid or unregistered investor found."
            //    });



            //// TODO: Re-evaluate need for URL link.
            //// URL - location at which content (Investor info) has been created, will be available via Pims client.
            //dataFile.Url = "http://localhost/Pims.Client/App/Layout/#/";

            //var isCreated = await Task.FromResult(_repository.Create(dataFile));
            //if (!isCreated) return BadRequest("Unable to create/register Investor :  " + dataFile.FirstName.Trim()
            //                                  + " " + dataFile.MiddleInitial
            //                                  + " " + dataFile.LastName.Trim()
            //);

            //return Created(dataFile.Url, dataFile);

            return null;
        }



        private static void ParsePortfolioSpreadsheet(string filePath)
        {
            var positionsListing = new List<Position>();
            //var profileCtrl = new ProfileController(_repositoryProfile);
            //var investorCtrl = new InvestorController(_repositoryInvestor);
            List<Asset> aasetsToUpdate;
            List<AssetCreationVm> assetsToCreate;
            var assetCtrl = new AssetController(_repositoryAsset, _identityService, _repositoryInvestor);
            List<Position> newPositions;
            List<Income> newRevenue;

            try
            {
                var lastTickerProcessed = string.Empty;
                var importFile = new FileInfo(filePath);
                using (var package = new ExcelPackage(importFile))
                {
                    var workSheet = package.Workbook.Worksheets[1];
                    var totalRows = workSheet.Dimension.End.Row;
                    var totalColumns = workSheet.Dimension.End.Column;

                    // Ignore row 1 (column headings).
                    for (var rowNum = 2; rowNum <= totalRows; rowNum++)
                    {
                        // Args: Cells[fromRow, fromCol, toRow, toCol]
                        var row = workSheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        var enumerableCells = row as string[] ?? row.ToArray();

                        // Asset update or creation ?
                        var responseAsset = assetCtrl.GetByTicker(enumerableCells.ElementAt(1).Trim());
                        //var responseAsset = assetCtrl.GetByTicker("CSQ"); TESTING
                        if (responseAsset.Result.ToString().IndexOf("Bad", StringComparison.Ordinal) > 0)
                        {
                            // Create due to expected "BadRequest" - no asset found for logged in investor.
                            var newAsset = new AssetCreationVm();
                            newAsset.AssetTicker = enumerableCells.ElementAt(1);
                            newAsset.AssetDescription = enumerableCells.ElementAt(2);
                            newAsset.ProfileToCreate = InitializeProfile(newAsset.AssetTicker.Trim());
                            if (lastTickerProcessed.Trim() != enumerableCells.ElementAt(1))
                                newAsset.PositionsCreated = InitializePositions(new List<PositionVm>(), enumerableCells);
                            else
                                newAsset.PositionsCreated = InitializePositions(newAsset.PositionsCreated, enumerableCells);

                            
                            // TODO: test via Fiddler on 11.24.17


                            //var newAsset = new Asset {AssetId = Guid.NewGuid()};
                            //// No way of knowing Asset classification during initialization, therefore stamp with default 'common stock'.
                            //// TODO: Allow user to modify Asset classification!
                            //newAsset.AssetClassId = new Guid("6215631d-5788-4718-a1d0-a2fc00a5b1a7");
                            //var loggedInInvestor = new Investor
                            //                       {
                            //                           InvestorId =
                            //                               Utilities.GetInvestorId(_repositoryInvestor,
                            //                                   _currentInvestor)
                            //                       };
                            //loggedInInvestor.AspNetUsersId = Guid.Parse(Utilities
                            //    .GetAspNetUserId(_repositoryInvestor, loggedInInvestor.InvestorId).ToString());


                        }
                        else
                        {
                            // Update
                            var y = 3;
                        }



                        // Bypass Profile & new asset/ticker initialization if processing the same ticker, but held in a different account.
                        if (lastTickerProcessed.Trim() != enumerableCells.ElementAt(1))
                        {
                            positionsListing = new List<Position>();
                            //assetModel = new Asset
                            //             {
                            //                 AssetTicker = enumerableCells.ElementAt(1)
                            //                 AssetDescription = enumerableCells.ElementAt(2),
                            //                 AssetClassification = "TBD",    // e.g., PFD
                            //                 ProfileToCreate = profileCtrl.GetProfileByTicker(enumerableCells.ElementAt(1).ToString())
                            //             };
                        }

                        //var positionVm = new PositionVm();
                        //positionVm.PreEditPositionAccount = enumerableCells.ElementAt(0);
                        //positionVm.PostEditPositionAccount = enumerableCells.ElementAt(0);
                        //positionVm.Status = "A";
                        //positionVm.Qty = int.Parse(enumerableCells.ElementAt(3));
                        //positionVm.DateOfPurchase = null;
                        //positionVm.DatePositionAdded = null;
                        //positionVm.LastUpdate = DateTime.Now;
                        //positionVm.LoggedInInvestor = _currentInvestor.Trim();

                        //var acctTypeVm = new AccountTypeVm();
                        //acctTypeVm.AccountTypeDesc = enumerableCells.ElementAt(0);
                        //acctTypeVm.Url = string.Empty;
                        //positionVm.ReferencedAccount = acctTypeVm;

                        //var trxVm = new TransactionVm();
                        //trxVm.PositionId = Guid.NewGuid();
                        //trxVm.TransactionId = Guid.NewGuid();
                        //trxVm.TransactionEvent = "C"; // Create
                        //trxVm.Units = int.Parse(enumerableCells.ElementAt(3));
                        //trxVm.MktPrice = decimal.Parse(enumerableCells.ElementAt(4));
                        //trxVm.Fees = 0;
                        //trxVm.Valuation = Utilities.CalculateValuation(decimal.Parse(enumerableCells.ElementAt(4)), int.Parse(enumerableCells.ElementAt(3)));
                        //trxVm.CostBasis = Utilities.CalculateCostBasis(0, trxVm.Valuation);
                        //trxVm.UnitCost = Utilities.CalculateUnitCost(trxVm.CostBasis, int.Parse(enumerableCells.ElementAt(3)));
                        //trxVm.DateCreated = DateTime.Now;
                        //positionVm.ReferencedTransaction = trxVm;

                        //positionsListing.Add(positionVm);
                        //assetModel.PositionsCreated = positionsListing;

                        //lastTickerProcessed = assetModel.AssetTicker;
                        //positionVm = null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }





        }


        private static ProfileVm InitializeProfile(string ticker)
        {
            // TODO: no hard-coded localhost
            var client = new HttpClient { BaseAddress = new Uri("http://localhost/")};
            var response = client.GetAsync("Pims.Web.Api/api/Profile/" + ticker).Result;

            return !response.IsSuccessStatusCode ? null : response.Content.ReadAsAsync<ProfileVm>().Result;
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
                DateOfPurchase = null,
                DatePositionAdded = null,
                LastUpdate = DateTime.Now,
                Url = "",
                LoggedInInvestor = _identityService.CurrentUser,
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
                                            DateCreated = DateTime.Now
                                        }
            };
            
            newPositions.Add(newPosition);
            return newPositions;
        }


    }
}