using System;
using System.Globalization;
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
        private static ITransactionEditsRepository<Transaction> _repositoryTransaction;


        public TransactionController(IGenericRepository<Transaction> repository, IPimsIdentityService identitySvc, IGenericRepository<Investor> repositoryInvestor, IGenericRepository<Asset> repositoryAsset, ITransactionEditsRepository<Transaction> repositoryTransaction )
        {
            _repository = repository;
            _identityService = identitySvc;
            _repositoryInvestor = repositoryInvestor;
            _repositoryAsset = repositoryAsset;
            _repositoryTransaction = repositoryTransaction;
        }



        [HttpGet]
        [Route("{positionId?}")]
        // e.g. http://localhost/Pims.Web.Api/api/PositionTransactions/cac9f68c-8d64-410a-afa5-8d18b6f0ed70
        public async Task<IHttpActionResult> GetTransactionsForPosition(Guid positionId)
        {
            var currentInvestor = _identityService.CurrentUser;


            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "joeblow@yahoo.com";

            _repository.UrlAddress = ControllerContext.Request.RequestUri.ToString();

            var matchingTrxs = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                                                                     .SelectMany(a => a.Positions) 
                                                                     .Where(p => p.PositionId == positionId)
                                                                     .SelectMany(p => p.PositionTransactions)
                                                                     .Select(t => new TransactionVm {
                                                                         TransactionId = t.TransactionId,
                                                                         DatePositionCreated = t.TransactionPosition.PositionDate,
                                                                         DateCreated = t.TransactionPosition.PurchaseDate,
                                                                         TransactionEvent = t.Action,
                                                                         PositionId = positionId,
                                                                         Units = t.Units,
                                                                         MktPrice = t.MktPrice,
                                                                         Fees = t.Fees,
                                                                         UnitCost = t.UnitCost,
                                                                         CostBasis = t.CostBasis,
                                                                         Valuation = t.Valuation,
                                                                         TickerSymbol = t.TransactionPosition.PositionAsset.Profile.TickerSymbol
                                                               })
                                         .AsQueryable());

            if (matchingTrxs != null)
                return Ok(matchingTrxs.OrderByDescending(t => t.DateCreated));

            return BadRequest(string.Format("Error retreiving Position transactions for {0}.", positionId));

        }


        [HttpPut]
        [HttpPatch]
        [Route("{transactionId?}")]
        // e.g. http://localhost/Pims.Web.Api/api/PositionTransactions/cac9f68c-8d64-410a-afa5-8d18b6f0ed70
        public async Task<IHttpActionResult> UpdatePositionTransaction(Guid transactionId, [FromBody] TransactionVm editedTransaction) {

            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid transaction data received for updating."
                });
            }

            var currentInvestor = _identityService.CurrentUser;

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "joeblow@yahoo.com";

            var currentTrx = await Task.FromResult(_repositoryAsset.Retreive(a => a.InvestorId == Utilities.GetInvestorId(_repositoryInvestor, currentInvestor.Trim()))
                                                                   .SelectMany(a => a.Positions)
                                                                   .Where(p => p.PositionId == editedTransaction.PositionId)
                                                                   .SelectMany(t => t.PositionTransactions)
                                                                   .Where(t => t.TransactionId == editedTransaction.TransactionId)
                                                                   .AsQueryable());

            if (currentTrx.IsEmpty())
                return BadRequest(string.Format("No matching Position transaction found to update, for {0}  ", editedTransaction.TransactionId));

            currentTrx.First().TransactionPositionId = editedTransaction.PositionId;
            currentTrx.First().TransactionId = editedTransaction.TransactionId;
            currentTrx.First().Units = editedTransaction.Units;
            currentTrx.First().MktPrice = editedTransaction.MktPrice;
            currentTrx.First().Fees = editedTransaction.Fees;
            currentTrx.First().UnitCost = editedTransaction.UnitCost;
            currentTrx.First().CostBasis = editedTransaction.CostBasis;
            currentTrx.First().Valuation = editedTransaction.Valuation;
            currentTrx.First().Action = editedTransaction.TransactionEvent;
            currentTrx.First().Date = Convert.ToDateTime(DateTime.Now.ToString(CultureInfo.InvariantCulture));


            var isUpdated = await Task.FromResult(_repository.Update(currentTrx.First(), new Guid(currentTrx.First().TransactionId.ToString())));
            if (!isUpdated)
                return BadRequest(string.Format("Unable to update Position transaction : {0} ", currentTrx.First().TransactionId));

            return Ok();
        }


        [HttpPut]
        [HttpPatch]
        [Route("")]
        public async Task<IHttpActionResult> UpdatePositionTransactions([FromBody] TransactionVm[] editedTransactions) {

            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid Position transaction received for updating."
                });
            }

            var currentInvestor = _identityService.CurrentUser;
            var mappedTransactions = new Transaction[editedTransactions.Length]; 

            // Allow for Fiddler debugging
            if (currentInvestor == null)
                currentInvestor = "joeblow@yahoo.com";

            for (var i = 0; i < editedTransactions.Length; i++)
                mappedTransactions[i] = MapVmToTransaction(editedTransactions[i]);
            

            var isUpdated = await Task.FromResult(_repositoryTransaction.UpdateTransactions(mappedTransactions));
            if (!isUpdated)
                return BadRequest(string.Format("Unable to update one or more transactions for Position : {0} ", mappedTransactions[0].TransactionPositionId));

            return Ok();
        }


        [HttpPost]
        [Route("")]
        // e.g. http://localhost/Pims.Web.Api/api/PositionTransactions
        public async Task<IHttpActionResult> CreateNewTransaction([FromBody]TransactionVm transactionData)
        {
           if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid data received for Transaction creation."
                });
            }

            // Allow for Fiddler debugging
            var currentInvestor = _identityService.CurrentUser ?? "joeblow@yahoo.com";

            var transactionToCreate = MapVmToTransaction(transactionData);
            var isCreated = await Task.FromResult(_repository.Create(transactionToCreate));
            

            if(!isCreated)
                return BadRequest(string.Format("Error adding transaction for Position: {0} ", transactionData.PositionId));

            return Created("http://localhost/PIMS.Web.Api/api/PositionTransactions", transactionToCreate);
        }




        #region Helpers

        private static Transaction MapVmToTransaction(TransactionVm sourceData )
        {
            return new Transaction
                   {
                       Action = sourceData.TransactionEvent,
                       TransactionId = sourceData.TransactionId,
                       TransactionPositionId = sourceData.PositionId,
                       Units = sourceData.Units,
                       MktPrice = sourceData.MktPrice,
                       Valuation = sourceData.Valuation,
                       Fees = sourceData.Fees,
                       CostBasis = sourceData.CostBasis,
                       UnitCost = sourceData.UnitCost,
                       Date = DateTime.Now
                   };

        }


        #endregion

    }

}