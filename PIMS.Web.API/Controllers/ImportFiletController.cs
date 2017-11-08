using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Office.Interop.Excel;
using System.Web.Http;
using FluentNHibernate.Conventions;
using FluentNHibernate.Utils;
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
        private readonly IPimsIdentityService _identityService;
        private static string _currentInvestor;
      

        public ImportFileController(ImportFileRepository fileRepository, IGenericRepository<Investor> repositoryInvestor, IPimsIdentityService identityService)
        {
            _fileRepository = fileRepository;
            _repositoryInvestor = repositoryInvestor;
            _identityService = identityService;
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
                importFileUrl = @"C:\Downloads\FidelityXLS\Portfolio_PositionsTEST_Fidelity.xlsx";   // PORTFOLIO data
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
            AssetCreationVm assetModel = new AssetCreationVm();
            var positionsListing = new List<PositionVm>();

            try
            {
                var lastTickerProcessed = string.Empty;
                var importFile = new FileInfo(filePath);
                using (var package = new ExcelPackage(importFile)) {
                    var workSheet = package.Workbook.Worksheets[1];
                    var totalRows = workSheet.Dimension.End.Row;
                    var totalColumns = workSheet.Dimension.End.Column;

                    // Ignore row 1 (column headings).
                    for (var rowNum = 2; rowNum <= totalRows; rowNum++) 
                    {
                        // Cells[fromRow, fromCol, toRow, toCol]
                        var row = workSheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        var enumerableCells = row as string[] ?? row.ToArray();

                        // Bypass Profile & new asset/ticker initialization if processing a different account type for the same position.
                        if (lastTickerProcessed.Trim() != enumerableCells.ElementAt(1))
                        {
                            positionsListing = new List<PositionVm>();
                            assetModel = new AssetCreationVm
                                         {
                                             AssetTicker = enumerableCells.ElementAt(1),
                                             AssetDescription = enumerableCells.ElementAt(2),
                                             AssetClassification = "TBD",    // e.g., PFD
                                             ProfileToCreate = null
                                         };
                        }
                           
                        var positionVm = new PositionVm();
                        positionVm.PreEditPositionAccount = enumerableCells.ElementAt(0);
                        positionVm.PostEditPositionAccount = enumerableCells.ElementAt(0);
                        positionVm.Status = "A";
                        positionVm.Qty = int.Parse(enumerableCells.ElementAt(3));
                        positionVm.DateOfPurchase = null;
                        positionVm.DatePositionAdded = null;
                        positionVm.LastUpdate = DateTime.Now;
                        positionVm.LoggedInInvestor = _currentInvestor.Trim();

                        var acctTypeVm = new AccountTypeVm();
                        acctTypeVm.AccountTypeDesc = enumerableCells.ElementAt(0);
                        acctTypeVm.Url = string.Empty;
                        positionVm.ReferencedAccount = acctTypeVm;
                       
                        var trxVm = new TransactionVm();
                        trxVm.PositionId = Guid.NewGuid();
                        trxVm.TransactionId = Guid.NewGuid();
                        trxVm.TransactionEvent = "C"; // Create
                        trxVm.Units = int.Parse(enumerableCells.ElementAt(3));
                        trxVm.MktPrice = decimal.Parse(enumerableCells.ElementAt(4));
                        trxVm.Fees = 0;
                        trxVm.Valuation = Utilities.CalculateValuation(decimal.Parse(enumerableCells.ElementAt(4)), int.Parse(enumerableCells.ElementAt(3)));
                        trxVm.CostBasis = Utilities.CalculateCostBasis(0, trxVm.Valuation);
                        trxVm.UnitCost = Utilities.CalculateUnitCost(trxVm.CostBasis, int.Parse(enumerableCells.ElementAt(3)));
                        trxVm.DateCreated = DateTime.Now;
                        positionVm.ReferencedTransaction = trxVm;

                        positionsListing.Add(positionVm);
                        assetModel.PositionsCreated = positionsListing;

                        lastTickerProcessed = assetModel.AssetTicker;
                        positionVm = null;
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            




        }


    }
}
