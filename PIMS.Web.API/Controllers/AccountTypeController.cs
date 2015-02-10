using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.FakeRepositories;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
   [RoutePrefix("api")]
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
            var accounts = await Task.FromResult(_repository.RetreiveAll().AsQueryable());
            if (accounts != null)
                return Ok(accounts);

            return BadRequest("Unable to retreive AccountType data.");
        }


        [HttpGet]
        [Route("~/api/AccountType")]
        public async Task<IHttpActionResult> GetLookUpAccounts([FromUri] bool lookUps = true)
        {
            // use of extension method.
            var availableAccts = await Task.FromResult(_repository.RetreiveLookUpAccounts().AsQueryable());

            if (availableAccts != null)
                return Ok(availableAccts);

            return BadRequest("Unable to retreive available lookup data.");
        }


        [HttpGet]
        [Route("~/api/AccountType/{forTicker}")]
        public async Task<IHttpActionResult> GetAllAccountsForInvestor(string forTicker = "")
        {
            var currentInvestor = _identityService.CurrentUser;
            IQueryable<Position> matchingPositions;

            if (forTicker.Trim().ToUpper() != "NONE")
            {
                matchingPositions = await Task.FromResult(_repositoryAssets.Retreive(a => a.Investor.LastName == currentInvestor.Trim() &&
                                                                                          a.Url.Contains(forTicker.Trim().ToUpper()) )
                                                                 .AsQueryable()
                                                                 .SelectMany(a => a.Positions.Where(p => p.PositionId != default(Guid))));
            }
            else
            {
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



        [HttpPut]
        [Route("~/api/AccountType/{acctTypeId}")]
        public async Task<IHttpActionResult> UpdateAccountType([FromBody]AccountType editedAcctType, Guid acctTypeId)
        {
            var currentInvestor = _identityService.CurrentUser;
            var existingPosition = await Task.FromResult(_repositoryAssets.Retreive(a => a.Investor.LastName == currentInvestor.Trim() )
                                                                  .AsQueryable()
                                                                  .SelectMany(a => a.Positions.Where(p => p.Account.PositionRefId == editedAcctType.PositionRefId )));

            if (existingPosition == null)
                return BadRequest("No matching Account Type found for :" + editedAcctType.AccountTypeDesc.Trim());


            var isUpdated = await Task<bool>.Factory.StartNew(() => _repository.Update(existingPosition.First().Account, editedAcctType.PositionRefId));

            if (isUpdated)
                return Ok();

            return BadRequest("Unable to update Account Type for: " + editedAcctType.AccountTypeDesc.Trim());
        }
        




    }
}
