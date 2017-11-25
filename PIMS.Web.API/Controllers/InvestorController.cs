using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using NHibernate.Transform;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Controllers
{
	[RoutePrefix("api/Investor")]
	public class InvestorController : ApiController
	{
		private static IGenericRepository<Investor> _repository;


		public InvestorController(IGenericRepository<Investor> repository)
		{
			_repository = repository;
		}


		[HttpGet]
		[Route("")]
		public async Task<IHttpActionResult> GetInvestors() {

			var registeredInvestors = await Task.FromResult(_repository.RetreiveAll()
															.OrderBy(i => i.LastName)
															.AsQueryable());

			if (registeredInvestors == null) return BadRequest("Unable to retreive Investor data.");

			return Ok(registeredInvestors);

		}


		[HttpPost]
		[Route("", Name = "CreateNewInvestor")]
		public async Task<IHttpActionResult> CreateNewInvestor([FromBody] Investor newInvestor)
		{
			/* Fiddler test data
			   {
					AspNetUsersId: "7e3ea538-93d1-42f2-ab48-3a228c7b2866",
					LastName: "Asch",
					FirstName: "Richard",
					MiddleInitial: "P.",
					Birthday: "10/30/1950",
					Address1: "544 Shorebird Circle",
					Address2: "Unit 25102",
					City: "Redwood Shores",
					State: "CA",
					ZipCode: "94065",
					Phone: "5927996",
					Mobile: "3654609",
					EMailAddr: "rpasch@rpclassics.net",
					DateAdded: "4/21/2015"
				}
			*/

			if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
				ReasonPhrase = "Invalid data received for Investor creation/registration."
			});

			var existingInvestor = await Task
				.FromResult(_repository.Retreive(i => i.EMailAddr.Trim() == newInvestor.EMailAddr.Trim()));

			if (existingInvestor.Any())
				return ResponseMessage(new HttpResponseMessage {
					StatusCode = HttpStatusCode.Conflict,
					ReasonPhrase = "Investor registration already exists."
				});

			// TODO: Re-evaluate need for URL link.
			// URL - location at which content (Investor info) has been created, will be available via Pims client.
			newInvestor.Url = "http://localhost/Pims.Client/App/Layout/#/";
			
			var isCreated = await Task.FromResult(_repository.Create(newInvestor));
			if (!isCreated) return BadRequest("Unable to create/register Investor :  " + newInvestor.FirstName.Trim()
																					   + " " + newInvestor.MiddleInitial 
																					   + " " + newInvestor.LastName.Trim()
											 );
			
			return Created(newInvestor.Url, newInvestor);
		}


		[HttpPut]
		[HttpPatch]
		[Route("")]
		public async Task<IHttpActionResult> UpdateInvestor([FromBody] Investor updatedInvestor)
		{
			// dc20c248-d731-4540-92e8-a48100bcc6d9 : InvestorId
			var isUpdated = false;
			if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
				StatusCode = HttpStatusCode.BadRequest,
				ReasonPhrase = "Invalid Investor data received for update."
			});

			var fetchedInvestor = _repository.RetreiveById(updatedInvestor.InvestorId);

			if(fetchedInvestor == null)
				return BadRequest("Unable to retreive Investor data for: " + updatedInvestor.FirstName.Trim()
															   + " " + updatedInvestor.MiddleInitial
															   + " " + updatedInvestor.LastName.Trim());

			isUpdated = await Task.FromResult(_repository.Update(updatedInvestor, updatedInvestor.InvestorId));
			
			if (isUpdated)
				return Ok(updatedInvestor);

			return BadRequest("Unable to update Investor data for: " + updatedInvestor.FirstName.Trim()
															   + " " + updatedInvestor.MiddleInitial 
															   + " " + updatedInvestor.LastName.Trim());

		}


		[HttpDelete]
		[Route("{id}")]
		public async Task<IHttpActionResult> Delete(Guid id)
		{
			var isDeleted = await Task.FromResult(_repository.Delete(id));

			if (isDeleted)
				return Ok("Delete successful");

			return BadRequest(string.Format("Unable to delete Investor with id:  {0} , not found", id));

		}

	}
}