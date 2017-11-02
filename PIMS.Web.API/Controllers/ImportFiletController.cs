using System;
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
//using DocumentFormat.OpenXml;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate.Util;
using OfficeOpenXml;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.Repositories;






namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/ImportFile")]
    public class ImportFileController : ApiController
    {
        private static ImportFileRepository _fileRepository;
        private static IGenericRepository<Investor> _repositoryInvestor;
        private readonly IPimsIdentityService _identityService;


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
            var currentInvestor = _identityService.CurrentUser;
            if (currentInvestor == null)
            {
                //return BadRequest("Import aborted; Investor login required."); 

                // un-comment for Fiddler testing
                currentInvestor = "rpasch@rpclassics.net";
                importFileUrl = @"C:\Downloads\FidelityXLS\2017SEP_RevenueTemplateTEST.xlsx";
            }

            ParseSpreadsheet(importFileUrl);
           
           



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



        private static void ParseSpreadsheet(string filePath)
        {
            try
            {
                var importFile = new FileInfo(filePath);
                using (var package = new ExcelPackage(importFile)) {
                    var workSheet = package.Workbook.Worksheets[1];
                    var totalRows = workSheet.Dimension.End.Row;
                    var totalColumns = workSheet.Dimension.End.Column;

                    var sb = new StringBuilder(); 
                    for (var rowNum = 1; rowNum <= totalRows; rowNum++) 
                    {
                        var row = workSheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                        sb.AppendLine(string.Join(",", row));
                    }
                    var len = sb.Length; // 11.1.17 - ran to completion Ok.
                }

            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            




        }


    }
}
