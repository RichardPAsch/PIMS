using System;
using System.Collections.Generic;
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


namespace PIMS.Web.Api.Controllers
{
   [RoutePrefix("api/AccountType")]
    public class AccountTypeController : ApiController
    {
        private static IGenericRepository<AccountType> _repository;
        private static IGenericRepository<Asset> _repositoryAssets;
        private readonly IPimsIdentityService _identityService;


        public AccountTypeController(IGenericRepository<AccountType> repository, IGenericRepository<Asset> repositoryAssets, IPimsIdentityService identityService)
        {
            _repository = repository;
            _repositoryAssets = repositoryAssets;
            _identityService = identityService;
        }


        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAllAccounts() {

            var availableAccountTypes = await Task.FromResult(_repository.RetreiveAll()
                                                            .OrderBy(at => at.AccountTypeDesc)
                                                            .AsQueryable());

            if (availableAccountTypes == null) return BadRequest("Unable to retreive AccountType data.");
            
            // Use of Vm mandated by received Http status:500 Error - "An error has occurred.","exceptionMessage":"Error getting value
            // from 'DefaultValue' on 'NHibernate.Type.DateTimeOffsetType'.","exceptionType":"Newtonsoft.Json.JsonSerializationException"
            // Proxy setup by NH results in serialization error, although no DateTime-related types exist in projects.
            IList<AccountTypeVm> accountTypeListing = availableAccountTypes.Select(at => new AccountTypeVm {
                                                            AccountTypeDesc = at.AccountTypeDesc,
                                                            KeyId = at.KeyId,
                                                            Url = string.Empty
                                                        }).ToList();

            return Ok(accountTypeListing);
        }


        //TODO: 4-20-15:  Reevaluate need for this! Fiddler testing needed.
        [HttpGet]
        [Route("~/api/AccountType/{forTicker}")]
        public async Task<IHttpActionResult> GetAllAccountsForInvestor(string forTicker = "") {
            var currentInvestor = _identityService.CurrentUser;
            IQueryable<Position> matchingPositions;

            if (forTicker.Trim().ToUpper() != "NONE") {
                matchingPositions = await Task.FromResult(_repositoryAssets.Retreive(a => a.Investor.LastName == currentInvestor.Trim() &&
                                                                                          a.Url.Contains(forTicker.Trim().ToUpper()))
                                                                 .AsQueryable()
                                                                 .SelectMany(a => a.Positions.Where(p => p.PositionId != default(Guid))));
            } else {
                matchingPositions = await Task.FromResult(_repositoryAssets.Retreive(a => a.Investor.LastName == currentInvestor.Trim())
                                                                .AsQueryable()
                                                                .SelectMany(a => a.Positions.Where(p => p.PositionId != default(Guid))));
            }


            if (!matchingPositions.Any())
                return BadRequest("No matching Position data found, or unable to retreive for: " + currentInvestor);

            var matchingAccounts = await Task.FromResult(matchingPositions.Select(p => p.Account.AccountTypeDesc).Distinct().AsQueryable());
            if (!matchingAccounts.Any())
                return BadRequest("Unable to retreive matching Account type data for: " + currentInvestor);

            return Ok(matchingAccounts);
        }



        [HttpPost]
        [Route("", Name = "CreateNewAccountType")]
        public async Task<IHttpActionResult> CreateNewAccountType([FromBody] AccountType newAcctType )
       {
           // New AccountType will not contain a valid PositionRefId if added via 'Admin' for lookup purposes.
           if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for new AccountType creation."
           });

           var existingAccountType = await Task
               .FromResult(_repository.Retreive(at => at.AccountTypeDesc.Trim() == newAcctType.AccountTypeDesc.Trim()));

           if (existingAccountType.Any())
                return ResponseMessage(new HttpResponseMessage {
                                        StatusCode = HttpStatusCode.Conflict,
                                        ReasonPhrase = "Duplicate AccountType found."
           });

           var requestUri = ControllerContext.RequestContext.Url.Request.RequestUri.AbsoluteUri;
           newAcctType.Url = requestUri + "/" + newAcctType.AccountTypeDesc.Trim();

           var isCreated = await Task.FromResult(_repository.Create(newAcctType));
           if (!isCreated) return BadRequest("Unable to create AccountType :  " + newAcctType.AccountTypeDesc.Trim());
          

           var newLocation = Url.Link("CreateNewAccountType", new {});
           return Created(newLocation, newAcctType);
       }


        [HttpPut]
        [HttpPatch]
        [Route("{preEditAcctTypeCode}")]
        public async Task<IHttpActionResult> UpdateAccountType([FromBody] AccountType updatedAcctType, string preEditAcctTypeCode)
       {
           var isUpdated = false;
           if (!ModelState.IsValid || preEditAcctTypeCode.IsEmpty()) return ResponseMessage(new HttpResponseMessage {
               StatusCode = HttpStatusCode.BadRequest,
               ReasonPhrase = "Invalid AccountType data received for update."
           });

           // Confirm received AccountType matches correct AccountType to be updated.
           var fetchedAccountType = _repository.RetreiveById(updatedAcctType.KeyId);
           var isCorrectAccountType = fetchedAccountType.AccountTypeDesc.Trim() == preEditAcctTypeCode.Trim();

           if (isCorrectAccountType) {
               isUpdated = await Task.FromResult(_repository.Update(updatedAcctType, updatedAcctType.KeyId));
           }


           if (isUpdated)
               return Ok(updatedAcctType);

           return BadRequest("Unable to update AccountType for: " + updatedAcctType.AccountTypeDesc.Trim());

       }


        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(Guid id)
       {
           var isDeleted = await Task.FromResult(_repository.Delete(id));

           if (isDeleted)
               return Ok("Delete successful");

           return BadRequest(string.Format("Unable to delete AccountType with id:  {0} , not found", id));

        }




        #region Helpers

           public AccountType MapVmToAccountType(AccountTypeVm sourceData)
           {
               return new AccountType
                      {
                          PositionRefId = new Guid(), 
                          AccountTypeDesc = sourceData.AccountTypeDesc,
                          Url = sourceData.Url
                      };
           }
           
           
       
       
           
   


        #endregion





        // TODO: Ignore! Uses in-memory data! 
        //[HttpGet]
        //[Route("~/api/AccountType")]
        //public async Task<IHttpActionResult> GetLookUpAccounts([FromUri] bool lookUps = true)
        //{
        //    // use of extension method.
        //    var availableAccts = await Task.FromResult(_repository.RetreiveLookUpAccounts().AsQueryable());

        //    if (availableAccts != null)
        //        return Ok(availableAccts);

        //    return BadRequest("Unable to retreive available lookup data.");
        //}



    }
    
}
