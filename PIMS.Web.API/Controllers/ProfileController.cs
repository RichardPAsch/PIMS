﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIMS.Core.Models;
using PIMS.Data;
using PIMS.Data.Repositories;
using PIMS.Core.Models.ViewModels;


namespace PIMS.Web.Api.Controllers
{

    /* Business Rules:
     * GET    - All currently available persisted Profiles will be available for grid viewing to allow for comparisons. Additionally,
     *          database searches will be utilized before accessing any specific ticker Profile data via 3rd party financial services.
     * POST   - Data to be persisted will either be 1) currently existing & current (via GET), or 2) non-existent and created (via service).  
     * PUT    - N/A; read-only data refreshed during GET.
     * DELETE - N/A; No business need.
    */

    [RoutePrefix("api/Profile")]
    public class ProfileController : ApiController
    {

        private static IGenericRepository<Profile> _repository;
        private const string BaseTiingoUrl = "https://api.tiingo.com/tiingo/daily/";
        private const string TiingoAccountToken = "95cff258ce493ec51fd10798b3e7f0657ee37740";


        public ProfileController(IGenericRepository<Profile> repository)
        {
            _repository = repository;
        }


        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAllPersistedProfiles()
        {
            // Includes ALL Profiles, even those missing some info.
            var savedProfiles = await Task.FromResult(_repository.RetreiveAll()
                                                                 .OrderBy(p => p.TickerSymbol)
                                                                 .AsQueryable());

            if (savedProfiles == null) return BadRequest("No saved Profiles found, or unable to retreive data.");

            // Use of Vm mandated by received Http status:500 Error - "An error has occurred.","exceptionMessage":"Error getting value
            // from 'DefaultValue' on 'NHibernate.Type.DateTimeOffsetType'.","exceptionType":"Newtonsoft.Json.JsonSerializationException"
            // Proxy setup by NH results in serialization error, although no DateTime-related types exist in projects.
            IList<ProfileVm> profileListing = savedProfiles.Select(p => new ProfileVm
                                                                        {
                                                                            ProfileId = p.ProfileId,
                                                                            TickerSymbol = p.TickerSymbol,
                                                                            TickerDescription = p.TickerDescription,
                                                                            DividendFreq = p.DividendFreq,
                                                                            DividendRate = p.DividendRate,
                                                                            DividendYield = p.DividendYield,
                                                                            EarningsPerShare = p.EarningsPerShare,
                                                                            PE_Ratio = p.PE_Ratio,
                                                                            LastUpdate = p.LastUpdate,
                                                                            ExDividendDate = p.ExDividendDate,
                                                                            DividendPayDate = p.DividendPayDate,
                                                                            Price = p.Price
                                                                        }).ToList();

            if(profileListing.Count >= 1)
                return Ok(profileListing);

            return BadRequest(string.Format("Error fetching saved Profile data"));
        }



        /*
           ** NOTE **
            11.9.2017 -  GetProfileByTicker() & GetProfiles() are now OBSOLETE due to the shutdown of Yahoo Finance API.
                         New ticker profile data now supplied via Tiingo.com API.
            11.10.2017 - Seperate API calls will be necessary for gathering meta & price data, as mandated by Tiingo.
        */
        //[HttpGet]
        //[Route("{tickerForProfile?}")]
        //// e.g. http://localhost/Pims.Web.Api/api/Profile/IBM
        //public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        //{
        //    Profile updatedOrNewProfile;
        //    var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == tickerForProfile.Trim()).AsQueryable());

        //    // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=VNR&f=nsb2dyreqr1
        //    if (existingProfile.Any())
        //    {
        //        // Update Profile table only IF existing Profile was last updated > 24hrs ago; return updated Profile.
        //        if (Convert.ToDateTime(existingProfile.First().LastUpdate) < DateTime.UtcNow.AddHours(-24))
        //        {
        //            updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), existingProfile.First()));
        //            if (updatedOrNewProfile != null)
        //                return Ok(updatedOrNewProfile);
        //        }
        //        else
        //        {
        //            // Return existing table Profile
        //            return Ok(existingProfile.First());
        //        }
        //    }
        //    else
        //    {
        //        // Return new Profile.
        //        updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), new Profile()));
        //        if (updatedOrNewProfile != null)
        //            return Ok(updatedOrNewProfile);
        //    }

        //    return BadRequest(string.Format("Error creating or updating Profile for {0}, check ticker symbol.", tickerForProfile));

        //}


        //[HttpGet]
        //[Route("~/api/Profiles/{t1}/{t2?}/{t3?}/{t4?}/{t5?}")]
        //// e.g. http://localhost/Pims.Web.Api/api/Profiles/{t1}/{t2?}/{t3?}/{t4?}/{t5?}/ 
        //// t = ticker symbol
        //public async Task<IHttpActionResult> GetProfiles(string t1, string t2 = "", string t3 = "", string t4 = "", string t5 = "") {

        //    if (string.IsNullOrEmpty(t1))
        //        return BadRequest(string.Format("Error fetching Profile, minimum of 1 ticker symbol required."));

        //    var tickersTemp = string.Empty;
        //    tickersTemp += t1;
        //    if (!string.IsNullOrEmpty(t2)) tickersTemp += "," + t2;
        //    if (!string.IsNullOrEmpty(t3)) tickersTemp += "," + t3;
        //    if (!string.IsNullOrEmpty(t4)) tickersTemp += "," + t4;
        //    if (!string.IsNullOrEmpty(t5)) tickersTemp += "," + t5;

        //    var tickers = tickersTemp.Split(',');
            

        //    // Yahoo url sample:  http://finance.yahoo.com/d/quotes.csv?s=<tickers>&f=sodyrr1
        //    //if (existingProfile.Any()) {
        //    //    // Update existing Profile only IF Profile last updated > 24hrs ago.
        //    //    if (Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.UtcNow.AddHours(-24))
        //    //        return Ok(existingProfile.First());

        //    //    updatedOrNewProfile = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfile(tickerForProfile.Trim(), existingProfile.First()));
        //    //    if (updatedOrNewProfile != null)
        //    //        return Ok(MapProfileToVm(updatedOrNewProfile));

        //    //    return BadRequest("Error updating Profile for ticker: " + tickerForProfile);
        //    //}

        //    var initializedProfiles = await Task.FromResult(YahooFinanceSvc.ProcessYahooProfiles(tickers));
        //    if (initializedProfiles != null)
        //        return Ok(initializedProfiles);

        //    return BadRequest(string.Format("Error obtaining Profile data due to connectivity, invalid submitted ticker symbols, or no data available. \nCheck ticker accuracy."));
        //}


        [HttpGet]
        [Route("{tickerForProfile?}")]
        // e.g. http://localhost/Pims.Web.Api/api/Profile/IBM
        public async Task<IHttpActionResult> GetProfileByTicker(string tickerForProfile)
        {
            var updatedOrNewProfile = new Profile();
            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == tickerForProfile.Trim()).AsQueryable());

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseTiingoUrl + tickerForProfile); // https://api.tiingo.com/tiingo/daily/<ticker>
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TiingoAccountToken);

                HttpResponseMessage response;
                JArray jsonTickerPriceData;
                Task<string> responsePriceData;

                if (existingProfile.Any())
                {
                    // Update Profile table only IF existing Profile was last updated > 48hrs ago.
                    if (Convert.ToDateTime(existingProfile.First().LastUpdate) < DateTime.Now.AddHours(-48)) {
                        // Only 'divCash" (aka div rate) needed at this time via: https://api.tiingo.com/tiingo/daily/<ticker>/prices
                        response = await client.GetAsync(client.BaseAddress + "/prices?token=" + TiingoAccountToken);
                        if (response == null)
                            return BadRequest("Unable to update Profile price data for: " + tickerForProfile);

                        responsePriceData = response.Content.ReadAsStringAsync();
                        jsonTickerPriceData = JArray.Parse(responsePriceData.Result);

                        foreach (var objChild in jsonTickerPriceData.Children<JObject>()) {
                            foreach (var property in objChild.Properties()) {
                                if(property.Name == "divCash")
                                    updatedOrNewProfile.DividendRate = decimal.Parse(property.Value.ToString());
                            }
                        }
                    } else {
                        return Ok(existingProfile);
                    }
                }
                else
                {
                    // 11.14.17 - tested Ok.
                    // 11.13.17 - New Profile required; both 'meta' & 'price' data fetches are mandated as seperate API calls at this time.
                    response = await client.GetAsync(client.BaseAddress + "?token=" + TiingoAccountToken);
                    if (response == null)
                        return BadRequest("Unable to create Profile meta data for: " + tickerForProfile);

                    var responseMetaData = response.Content.ReadAsStringAsync();        // Json string returned
                    var jsonTickerMetaData = JObject.Parse(await responseMetaData);
                    updatedOrNewProfile.TickerDescription = jsonTickerMetaData["name"].ToString().Trim();
                    updatedOrNewProfile.TickerSymbol = jsonTickerMetaData["ticker"].ToString().Trim();
                    response.Dispose();

                    response = await client.GetAsync(client.BaseAddress + "/prices?token=" + TiingoAccountToken);
                    if (response == null)
                        return BadRequest("Unable to update Profile price data for: " + tickerForProfile);

                    responsePriceData = response.Content.ReadAsStringAsync();
                    jsonTickerPriceData = JArray.Parse(responsePriceData.Result);

                    foreach (var objChild in jsonTickerPriceData.Children<JObject>()) {
                        foreach (var property in objChild.Properties())
                        {
                            switch (property.Name)
                            {
                                case "divCash":
                                    updatedOrNewProfile.DividendRate = decimal.Parse(property.Value.ToString());
                                    break;
                                case "date":
                                    var dateInfo = (DateTime) property.Value;
                                    updatedOrNewProfile.ExDividendDate = new DateTime(dateInfo.Year, dateInfo.Month, dateInfo.Day);
                                    break;
                                case "close":
                                    updatedOrNewProfile.Price = decimal.Parse(property.Value.ToString());
                                    break;
                            }
                        }
                    }

                    updatedOrNewProfile.ProfileId = Guid.NewGuid();
                    updatedOrNewProfile.AssetId = Guid.NewGuid();
                    updatedOrNewProfile.DividendYield = 0; // TODO: to be calculated ?
                    updatedOrNewProfile.EarningsPerShare = 0;
                    updatedOrNewProfile.PE_Ratio = 0;
                    updatedOrNewProfile.DividendPayDate = null;

                }

                updatedOrNewProfile.LastUpdate = DateTime.Now;
                return Ok(updatedOrNewProfile);
            }
        }




        [HttpPost]
        [Route("", Name = "CreateNewProfile")]
        public async Task<IHttpActionResult> CreateNewProfile([FromBody] ProfileVm submittedProfile)
        {
  
            if (!ModelState.IsValid) return ResponseMessage(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Invalid data received for new Profile creation."
            });

            var existingProfile = await Task.FromResult(_repository.Retreive(p => p.TickerSymbol.Trim() == submittedProfile.TickerSymbol.Trim()));

            //TODO: Use configurable hours for: DateTime.Now.AddHours(-24)
            if (existingProfile.Any() && Convert.ToDateTime(existingProfile.First().LastUpdate) > DateTime.Now.AddHours(-24))
                return ResponseMessage(new HttpResponseMessage {
                                                StatusCode = HttpStatusCode.NotModified, // Status:304
                                                ReasonPhrase = "No Profile created, existing data is less than 24 hours old."
                                     });

            var profileToCreate = MapVmToProfile(submittedProfile);
            var isCreated = await Task.FromResult(_repository.Create(profileToCreate));
            if (!isCreated) return BadRequest("Unable to create new Profile for :  " + submittedProfile.TickerSymbol.Trim());

            //TODO: Should use RouteData for determining newLocation Url. 7-8-15: Reeval, RouteData avail via RPC?
           // return Created("http://localhost/Pims.Web.Api/api/Asset/", profileToCreate);
            return Created(submittedProfile.Url + "/" + profileToCreate.ProfileId, profileToCreate);
        }


        [HttpPut]
        [Route("~/api/Asset/{ticker}/Profile")]
        public async Task<IHttpActionResult> UpdateProfile([FromBody]ProfileVm editedProfile)
        {
            if (!ModelState.IsValid) {
                return ResponseMessage(new HttpResponseMessage {
                                            StatusCode = HttpStatusCode.BadRequest,
                                            ReasonPhrase = "Invalid/Incomplete data received for Profile update."
                });
            }

            var profileToUpdate = await GetProfileByTicker(editedProfile.TickerSymbol.Trim()) as OkNegotiatedContentResult<Profile>;
            if (profileToUpdate == null)
                return BadRequest("Unable to retreive Profile data for update.");

            var isUpdated = await Task.FromResult(_repository.Update(profileToUpdate.Content, profileToUpdate.Content.ProfileId));
            if (!isUpdated)
                return BadRequest(string.Format("Unable to update Profile for: {0}", editedProfile.TickerSymbol.Trim()));

            return Ok("Profile successfully updated for " + editedProfile.TickerSymbol.Trim());
        }




        #region Helpers

           

            private static Profile MapVmToProfile(ProfileVm sourceData)
            {
                return new Profile
                       {
                           TickerSymbol = sourceData.TickerSymbol.ToUpper().Trim(),
                           TickerDescription = sourceData.TickerDescription.Trim(),
                           DividendRate = sourceData.DividendRate,
                           DividendYield = sourceData.DividendYield,
                           DividendFreq = sourceData.DividendFreq,
                           EarningsPerShare = sourceData.EarningsPerShare,
                           PE_Ratio = sourceData.PE_Ratio,
                           LastUpdate = DateTime.Now,
                           ExDividendDate = sourceData.ExDividendDate,
                           DividendPayDate = sourceData.DividendPayDate,
                           Price = sourceData.Price,
                           Url = sourceData.Url.Trim()
                       };
            }

            // Deferred
            //private static ProfileVm MapProfileToVm(Profile sourceData)
            //{
            //    return new ProfileVm
            //    {
            //        ProfileId = sourceData.ProfileId,
            //        TickerSymbol = sourceData.TickerSymbol,
            //        TickerDescription = sourceData.TickerDescription,
            //        DividendFreq = sourceData.DividendFreq,
            //        DividendRate = sourceData.DividendRate,
            //        DividendYield = sourceData.DividendYield,
            //        EarningsPerShare = sourceData.EarningsPerShare,
            //        PE_Ratio = sourceData.PE_Ratio,
            //        LastUpdate = sourceData.LastUpdate,
            //        ExDividendDate = sourceData.ExDividendDate,
            //        DividendPayDate = sourceData.DividendPayDate,
            //        Price = sourceData.Price
            //    };
            //}


        #endregion


    }

}
