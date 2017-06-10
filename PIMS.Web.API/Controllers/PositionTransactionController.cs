using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using PIMS.Core.Models.ViewModels;
using PIMS.Core.Security;
using PIMS.Data.Repositories;
using PIMS.Web.Api.Common;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/PositionTransaction")]
    public class PositionTransactionController : ApiController
    {
        private readonly PositionTransactionRepository _repository;
        private readonly IPimsIdentityService _identityService;

        /* Satisfies specialized need for handling Position-Transaction 'rollover' processing. */


        public PositionTransactionController(PositionTransactionRepository repository, IPimsIdentityService identityService) {
            _repository = repository;
            _identityService = identityService;
        }



        [HttpPost]
        [Route("Rollover")]
        // e.g. http://localhost/Pims.Web.Api/api/PositionTransaction/Rollover
        public async Task<IHttpActionResult> CreateUpdatePositionsAndTransactions([FromBody]object[] rolloverInfo) {

            /*  Tested expected Json parameter model for rollover persistence ; 
             *  FYI: rolloverInfo indices: [0]sourceTrxVm/[1]sourcePosVm/[2]targetTrxVm/[3]targetPosVm
             *  ---------------------------------------------------------------
             *  [
                    { // source Trx.
                        PositionId : "64f24181-0fdc-4804-9b37-a741011178de",
                        TransactionId : "95e68b3a-2849-46f2-9c74-b47ffa7b5f35",
                        TransactionEvent : "R",
                        Units : 10,
                        MktPrice : 31.050,
                        Fees : 32.56, 
                        UnitCost : 34.306, 
                        CostBasis: 343.06, 
                        Valuation: 310.50,
                        DateCreated : "6/09/2017"
                    },
                    {  // source Pos.
                        Qty : 263,
                        UnitCost: 32.454,
                        TransactionFees: 369.27,
                        LastUpdate : "6/9/2017",
                        CreatedPositionId: "64f24181-0fdc-4804-9b37-a741011178de",
                        Status : "A",
                        DateOfPurchase: "2/21/2017",
                        DatePositionAdded: "3/27/2017",
                        PostEditPositionAccount: "01bacffb-3727-4383-a0ec-f63e4cbdd1b1",
                        ReferencedAssetId: "4ba3b760-e38c-4957-bf1f-a74101114904"
                    }...[2],[3].
            */

            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Invalid data received for Transaction-Position rollover processing."
                });
            }

            // Allow for Fiddler debugging
            var currentInvestor = _identityService.CurrentUser ?? "joeblow@yahoo.com";


            object[] sourceAccount = new object[2];
            object[] targetAccount = new object[2];
            // TODO: Eliminate duplicate MapVmToTransaction() in TransactionController.cs ?
            sourceAccount[0] = Utilities.MapVmToTransaction(JsonConvert.DeserializeObject<TransactionVm>(rolloverInfo[0].ToString())); 
            sourceAccount[1] = Utilities.MapVmToPosition(JsonConvert.DeserializeObject<PositionVm>(rolloverInfo[1].ToString()));
            targetAccount[0] = Utilities.MapVmToTransaction(JsonConvert.DeserializeObject<TransactionVm>(rolloverInfo[2].ToString()));
            targetAccount[1] = Utilities.MapVmToPosition(JsonConvert.DeserializeObject<PositionVm>(rolloverInfo[3].ToString()));
            
            var transactionsAndPositionsPersisted = await Task.FromResult(_repository.UpdateTransactionAndPosition(sourceAccount, targetAccount));


            if (!transactionsAndPositionsPersisted)
                return BadRequest(string.Format("Error adding/updating Position-Transaction data for: {0} ", sourceAccount[0]));

            return Created("http://localhost/Pims.Web.Api/api/PositionTransaction/Rollover/sourceInfo/targetInfo",targetAccount[1]);

        }

     

    }

}