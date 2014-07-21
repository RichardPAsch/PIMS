using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using NUnit.Framework;
using PIMS.Core.Models;


namespace PIMS.IntegrationTest
{
    [TestFixture]
    public class VerifyProfileController
    {
       
        private const string UrlBase = "http://localhost/PIMS.Web.API/api/Asset";


        public void Init() {
           
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_a_Profile_based_on_a_ticker_symbol() {

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/ETV/Profile");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                // Act
                var resp = await client.GetAsync(client.BaseAddress);
                var assetProfile = await resp.Content.ReadAsAsync<Profile>();


                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNull(assetProfile);
                Assert.IsTrue(assetProfile.TickerSymbol.ToUpper().Trim() == "ETV");
            }
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Cannot_GET_a_Profile_based_on_an_invalid_ticker_symbol() {

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/ETVX/Profile");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                // Act
                var resp = await client.GetAsync(client.BaseAddress);
                await resp.Content.ReadAsAsync<Profile>();


                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.NotFound);
            }
        }
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_POST_a_new_Profile_as_part_of_Asset_creation() {

            using (var client = new HttpClient()) {

                // TODO: Why are we retreiving a profile from our own db and posting it back again? Retreive from yahoo?
                // Arrange
                const string testTicker = "IBM";
                client.BaseAddress = new Uri(UrlBase + "/" + testTicker + "/Profile");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                var resp = await client.GetAsync(client.BaseAddress);
                var assetProfile = await resp.Content.ReadAsAsync<Profile>();


                // Act
                var response = client.PostAsJsonAsync(client.BaseAddress.ToString(), assetProfile).Result;
                var jsonResult = response.Content.ReadAsStringAsync().Result;
                var profile = JsonConvert.DeserializeObject<Profile>(jsonResult);


                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Created); //ok
                Assert.IsTrue(profile.TickerSymbol.Trim().ToUpper() == testTicker);
                Assert.IsTrue(response.Headers.Location.AbsoluteUri.Contains(testTicker));
            }

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Cannot_POST_a_duplicate_Profile_as_part_of_Asset_creation() {

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/ETV/Profile");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                var resp = await client.GetAsync(client.BaseAddress);
                var assetProfile = await resp.Content.ReadAsAsync<Profile>();


                // Act
                var response = client.PostAsJsonAsync(client.BaseAddress.ToString(), assetProfile).Result;
                var jsonResult = response.Content.ReadAsStringAsync().Result;
                JsonConvert.DeserializeObject<Profile>(jsonResult);


                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Conflict); // 409

            }

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_a_Profile_based_on_a_ProfileId() {

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/ETV/Profile/93feabc4-7b36-44d0-a66f-a32c00e72bd4");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                // Act
                var resp = await client.GetAsync(client.BaseAddress);
                var assetProfile = await resp.Content.ReadAsAsync<Profile>();


                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNull(assetProfile);
                Assert.IsTrue(assetProfile.TickerSymbol.ToUpper().Trim() == "ETV");
            }
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_DELETE_a_Profile_based_on_a_Id() {

            using (var client = new HttpClient()) {

                // Arrange - change id with each new test.
                client.BaseAddress = new Uri(UrlBase + "/IBM/Profile/422fafdb-0b9c-4be8-9c13-a33a00f44f09");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                

                // Act
                var resp = await client.DeleteAsync(client.BaseAddress);
                await resp.Content.ReadAsAsync<Profile>();


                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);

            }
        }


    }


}

