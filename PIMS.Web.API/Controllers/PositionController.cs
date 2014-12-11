using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
    [RoutePrefix("api/Asset")]
    public class PositionController : ApiController
    {
        private static IGenericRepository<Position> _repository;
        private static IGenericRepository<Asset> _repositoryAsset;
        private readonly IPimsIdentityService _identityService;


        public PositionController(IGenericRepository<Position> repositoryPosition, IPimsIdentityService identitySvc, IGenericRepository<Asset> repositoryAsset)
        {
            _repositoryAsset = repositoryAsset;
            _repository = repositoryPosition;
            _identityService = identitySvc;
        }



        [HttpGet]
        [Route("{tickerSymbol}/Position/{account?}")]
         public async Task<IHttpActionResult> GetPositionByAccount(string tickerSymbol, string account)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;
            
            var currentAsset = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == tickerSymbol.ToUpper().Trim() 
                                                                                 && a.Investor.LastName.Trim() == currentInvestor.Trim()).AsQueryable() );


           var currentPosition = await Task.FromResult(_repository.Retreive(p => String.Equals(p.Account.AccountTypeDesc.Trim(), account.Trim(),
                                                            StringComparison.CurrentCultureIgnoreCase), currentAsset).AsQueryable());
            
            if (currentPosition.Any())
                return Ok(currentPosition);


            return BadRequest(string.Format("No Position found matching {0} for investor {1} under account {2}", 
                                                                                                tickerSymbol.ToUpper(), 
                                                                                                currentInvestor.ToUpper(),
                                                                                                account));
                                                                                     
        }


        [HttpGet]
        [Route("{tickerSymbol}/Position")]
        public async Task<IHttpActionResult> GetPositionsByAsset(string tickerSymbol)
        {
            _repositoryAsset.UrlAddress = ControllerContext.Request.RequestUri.ToString();
            var currentInvestor = _identityService.CurrentUser;

            var currentAsset = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == tickerSymbol.ToUpper().Trim()
                                                                                 && a.Investor.LastName.Trim() == currentInvestor.Trim()).AsQueryable());


            var currentPositions = await Task.FromResult(currentAsset.First().Positions.OrderBy(p => p.Account.AccountTypeDesc).ToList().AsQueryable());

            if (currentPositions.Any())
                return Ok(currentPositions);


            return BadRequest(string.Format("No Positions found matching {0} for investor {1} ", tickerSymbol.ToUpper(), currentInvestor.ToUpper()));

        }


        [HttpPost]
        [Route("{tickerSymbol}/Position")]
        public async Task<IHttpActionResult> CreateNewPosition([FromBody]Position positionData, [FromUri] bool newAsset=false)
        {
            var currentInvestor = _identityService.CurrentUser;
            string newLocation;
            
            if (!ModelState.IsValid)
            {
                return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid data received for Position creation."
                                        });
            }

            // Check for duplicate entry via: ticker, user, and account, ONLY if not part of the Asset/Create menu process.
            var ticker = ParseUrlForTicker(ControllerContext.Request.RequestUri.ToString());
            if (!newAsset)
            {
                newLocation = ControllerContext.Request.RequestUri + "/" + positionData.Account.AccountTypeDesc.Trim();
                var currentAsset = await Task.FromResult(_repositoryAsset.Retreive(a => a.Profile.TickerSymbol.Trim().ToUpper() == ticker.ToUpper().Trim()
                                                                                && a.Investor.LastName.Trim() == currentInvestor.Trim()).AsQueryable());
                
                var currentPosition = await Task.FromResult(_repository.Retreive(p => String.Equals(p.Account.AccountTypeDesc.Trim(), positionData.Account.AccountTypeDesc.Trim(),
                                                               StringComparison.CurrentCultureIgnoreCase), currentAsset).AsQueryable());

                if (currentPosition.Any())
                    return BadRequest(string.Format("No additional Position has been added, due to Position {0} already existing for {1} ",
                                                                                                    positionData.Account.AccountTypeDesc.Trim(),
                                                                                                    ticker.ToUpper()));

                var isCreated = await Task<bool>.Factory.StartNew(() => _repository.Create(positionData));

                if (!isCreated) return BadRequest(string.Format("Unable to add Position to: {0} ", ticker.ToUpper()));
                return Created(newLocation, positionData);
            }

            // TODO: When adding Positions during new Asset creation, we'll have to make sure no duplicate Positions have been added!
            // TODO: This should be enforced during the Asset Save() of AssetRepository (or AssetController)?
            var isNewlyCreated = await Task<bool>.Factory.StartNew(() => _repository.Create(positionData));

            // TODO: Investigate using route data for building links instead.
            newLocation = ParseForNewPositionUrl(ControllerContext.Request.RequestUri.AbsoluteUri, positionData.Account.AccountTypeDesc.Trim());
            return !isNewlyCreated ? (IHttpActionResult) BadRequest(string.Format("Unable to add Position to: {0} ", ticker.ToUpper()))
                                   : Created(newLocation, positionData );
        }



        // GET api/position/5
        public string Get(int id)
        {
            return "value";
        }


        // PUT api/position/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/position/5
        public void Delete(int id)
        {
        }



        private static string ParseUrlForTicker(string urlToParse)
        {
            var pos1 = urlToParse.IndexOf("Asset/", System.StringComparison.Ordinal) + 6;
            var pos2 = urlToParse.IndexOf("/Position", System.StringComparison.Ordinal);
            return urlToParse.Substring(pos1, pos2 - pos1);
        }

        private static string ParseForNewPositionUrl(string urlForPosition, string accountType)
        {
            var pos1 = urlForPosition.IndexOf("Position?", System.StringComparison.Ordinal);
            return urlForPosition.Substring(0, pos1 + 8) + "/" + accountType.Trim();
        }
    }
}
