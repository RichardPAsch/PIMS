using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FluentNHibernate.Conventions;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;
using PIMS.Core.Security;
using PIMS.Data.Repositories;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/PositionTransactions")]
    public class TransactionController : ApiController
    {
        private readonly IGenericRepository<Transaction> _repository;
        private readonly IPimsIdentityService _identityService;
        private static IGenericRepository<Investor> _repositoryInvestor;
        private static IGenericRepository<Asset> _repositoryAsset;


        public TransactionController(IGenericRepository<Transaction> repository, IPimsIdentityService identitySvc, IGenericRepository<Investor> repositoryInvestor, IGenericRepository<Asset> repositoryAsset)
        {
            _repository = repository;
            _identityService = identitySvc;
            _repositoryInvestor = repositoryInvestor;
            _repositoryAsset = repositoryAsset;
        }



        [HttpGet]
        [Route("{positionId?}")]
        // e.g. http://localhost/Pims.Web.Api/api/PositionTransactions/cac9f68c-8d64-410a-afa5-8d18b6f0ed70
        public async Task<IHttpActionResult> GetTransactionsForPosition(Guid positionId)
        {
            _repository.UrlAddress = ControllerContext.Request.RequestUri.ToString();

            var matchingTrxs = await Task.FromResult(_repository.Retreive(p => p.PositionId == positionId)
                                         .Select(t => new TransactionVm {
                                                                            PositionId = t.PositionId,
                                                                            Units = t.Units,
                                                                            MktPrice = t.MktPrice,
                                                                            Fees = t.Fees,
                                                                            UnitCost = t.UnitCost,
                                                                            CostBasis = t.CostBasis,
                                                                            Valuation = t.Valuation,
                                                                            DateCreated = t.Date
                                                                        })
                                         .AsQueryable());

            if (matchingTrxs != null)
                return Ok(matchingTrxs.OrderByDescending(a => a.DateCreated));

            return BadRequest(string.Format("Error retreiving Position transactions for {0}.", positionId));

        }


        [HttpPut]
        [HttpPatch]
        [Route("~/transactionId")]
        public async Task<IHttpActionResult> UpdatePositionTransaction([FromBody] TransactionVm editedTransaction)
        {
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid Position transaction received for updating."
                });
            }

            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "rpasch2@rpclassics.net";

            var currentTrx = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                                                                   .SelectMany(a => a.Positions)
                                                                   .Where(p => p.PositionId == editedTransaction.PositionId)
                                                                   .SelectMany(t => t.PositionTransactions)
                                                                   .Where(t => t.TransactionId == editedTransaction.TransactionId)
                                                                   .AsQueryable());

            if(currentTrx.IsEmpty())
                return BadRequest(string.Format("No matching Position transaction found to update, for {0}  ", editedTransaction.TransactionId));

            currentTrx.First().Units = editedTransaction.Units;
            currentTrx.First().MktPrice = editedTransaction.MktPrice;
            currentTrx.First().Fees = editedTransaction.Fees;
            currentTrx.First().UnitCost = editedTransaction.UnitCost;
            currentTrx.First().CostBasis = editedTransaction.CostBasis;
            currentTrx.First().Valuation = editedTransaction.Valuation;


            var isUpdated = await Task.FromResult(_repository.Update(currentTrx.First(), currentTrx.First().TransactionId));
            if (!isUpdated)
                return BadRequest(string.Format("Unable to update Position transaction : {0} ", currentTrx.First().TransactionId));

            return Ok();
        }

    }

}