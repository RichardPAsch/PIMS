using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Data.Repositories;
using StructureMap.Pipeline;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/AccountType")]
    public class AccountTypeController : ApiController
    {
        private static AccountTypeRepository _repository;
        public AccountTypeController(AccountTypeRepository repository)
        {
            _repository = repository;
        }


        // GET api/accounttype
        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<AccountType>> Get()
        {
            return await Task<IEnumerable<AccountType>>.Factory.StartNew(() => _repository.RetreiveAll());
        }
        
        
        // GET api/accounttype/IRA
        [HttpGet]
        [Route("{acctTypeDesc?}")]
        public async Task<IHttpActionResult> Get(string acctTypeDesc)
        {
            var acctType = await Task<AccountType>.Factory
                                                  .StartNew(() => _repository.RetreiveAll()
                                                            .First(at => String.Equals(at.AccountTypeDesc.Trim(),
                                                                         acctTypeDesc.Trim(),
                                                                         StringComparison.CurrentCultureIgnoreCase)));
            if (acctType == null)
                return NotFound();

            return Ok(acctType);
        }



        // POST api/AccounTtype
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody]AccountType newAcctType)
        {
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage
                                                                {
                                                                    StatusCode = HttpStatusCode.BadRequest,
                                                                    ReasonPhrase = "Invalid data received for Account Type."
                                                                });

            var existingAcctType = await Task<AccountType>.Factory
                                                          .StartNew(() => _repository.RetreiveAll()
                                                                                     .FirstOrDefault(at => String.Equals(at.AccountTypeDesc.Trim(),
                                                                                         newAcctType.AccountTypeDesc.Trim(),
                                                                                         StringComparison.CurrentCultureIgnoreCase)));

            if (existingAcctType != null)
                return ResponseMessage(new HttpResponseMessage
                                            {
                                                StatusCode = HttpStatusCode.Conflict,
                                                ReasonPhrase = "Duplicate Account Type found."
                                            });

            var isCreated = await Task<bool>.Factory.StartNew(() => _repository.Create(newAcctType));

            return ResponseMessage(isCreated
                                    ? new HttpResponseMessage { StatusCode = HttpStatusCode.Created } 
                                    : new HttpResponseMessage { StatusCode = HttpStatusCode.Conflict, 
                                                                ReasonPhrase = "Error: Unable to save account type - " + newAcctType.AccountTypeDesc});
        }
        

        // PUT ...Pims.Web.Api/api/AccountType  
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Put([FromBody]AccountType updatedAcctType)
        {
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for Account Type update."
            });

            // Only account type descriptions may be modified.
            var isModified = await Task<bool>.Factory.StartNew(() => _repository.Update(updatedAcctType, updatedAcctType.KeyId));


            return ResponseMessage(isModified
                                    ? new HttpResponseMessage {
                                        StatusCode = HttpStatusCode.NoContent,  // HttpStatus code: 204 
                                        ReasonPhrase = "Account type updated."
                                    }
                                    : new HttpResponseMessage {
                                        StatusCode = HttpStatusCode.NotModified,
                                        ReasonPhrase = "Error: Unable to update account type to - " + updatedAcctType.AccountTypeDesc
                                    });

        }



        // DELETE api/AccountType/IRA
        [HttpDelete]
        [Route("{acctType}")]
        // ReSharper disable once InconsistentNaming
        public async Task<IHttpActionResult> Delete(string acctType)
        {
            var isDeleted = await Task<bool>.Factory.StartNew(() => _repository.DeleteByType(acctType));

            return ResponseMessage(isDeleted
                                    ? new HttpResponseMessage {
                                        StatusCode = HttpStatusCode.NoContent,  // HttpStatus code: 204 
                                        ReasonPhrase = "Account type deleted."}
                                    : new HttpResponseMessage {
                                        StatusCode = HttpStatusCode.NotModified,
                                        ReasonPhrase = "Error: Unable to delete account type id:  " + acctType
                                    });
        }


      




    }
}
