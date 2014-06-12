using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using NUnit.Framework;
using PIMS.Core.Security;


namespace PIMS.IntegrationTest
{
    [TestFixture]
    public class VerifyAssetClassController
    {
        

        /*
            API tests WebApi endpoints, mimicking client-based calls, as in sending resource-based URL requests; this 
            tests from the client's point of view, and most closely resembles real world scenarios.
            All response information is derived via a test database.
            These tests also verify IoC/DI is working correctly.
        */

        //private AssetClassController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api";
      

        public void Init()
        {
           
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_All_Asset_Classifications() {

            using (var client = new HttpClient())
                {
                    // Arrange
                    client.BaseAddress = new Uri(UrlBase + "/AssetClass");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Act
                    var resp = await client.GetAsync(client.BaseAddress);
                    var classes = await resp.Content.ReadAsAsync<IEnumerable<AssetClass>>();


                    // Assert
                    Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                    Assert.IsNotNull(classes);
                    // Avoid posssible multiple enumerations.
                    var assetClasses = classes as AssetClass[] ?? classes.ToArray();
                    Assert.IsTrue(assetClasses.First().Code != string.Empty);
                    Assert.IsTrue(assetClasses.First().Description != string.Empty);
                    Assert.GreaterOrEqual(assetClasses.Count(), 4);
               }
        }
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_an_Asset_Classification_By_Asset_Class() {

            using (var client = new HttpClient()) {
                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/ETF");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Act
                HttpResponseMessage resp = await client.GetAsync(client.BaseAddress);
                var assetClass = await resp.Content.ReadAsAsync<AssetClass>();

                

                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNull(assetClass);
                Assert.IsTrue(assetClass.Code.ToUpper().Trim() == "ETF");
                Assert.IsFalse(assetClass.Description == string.Empty);
            }
        }


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Can_PUT_Update_an_Asset_Classification_By_Asset_Class() {

        //    using (var client = new HttpClient()) {

        //        // Arrange
        //        client.BaseAddress = new Uri(UrlBase + "/AssetClass/ETF");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        //        var respGet = await client.GetAsync(client.BaseAddress); // async call to server for data
                
        //        var updatedAssetClass = new AssetClass
        //                                {
        //                                    KeyId = Guid.NewGuid(),
        //                                    Description = "Exchange-Traded Fund",
        //                                    Code = "ETF"
        //                                };


        //        //var newContent = new ObjectContent<AssetClass>();
        //        //newContent.
        //        //respGet.Content = myObjectContent(updatedAssetClass, new JsonMediaTypeFormatter());
                

        //        var content = respGet.Content; // write content to a string
        //        var z = JsonConvert.SerializeObject(content);
        //        var modifiedAssetClass = JsonConvert.DeserializeObject<AssetClass>(z); // deserialize Json to .Net type
        //        modifiedAssetClass.Description = "Exchange-Traded Fund"; // made modification
        //        var jsonModifiedAssetClass = JsonConvert.SerializeObject(modifiedAssetClass); // .Net type to Json string
        //        var stringContent = new StringContent(jsonModifiedAssetClass);
              
                

                
        //        // Act
        //        //var respPut = await client.PutAsJsonAsync(client.BaseAddress.ToString(), jsonModifiedAssetClass);
        //        var respPut = await client.PutAsync(client.BaseAddress.ToString(), stringContent); //respGet.Content);
        //        var assetClass = await respPut.Content.ReadAsAsync<AssetClass>();

        //        var a = 2;
        //        // Assert
        //        Assert.IsTrue(respPut.StatusCode == HttpStatusCode.OK);
        //        //Assert.IsNotNull(assetClass);
        //        //Assert.IsTrue(assetClass.Code.ToUpper().Trim() == "ETF");
        //        Assert.IsTrue(assetClass.Description == "Exchange-Traded Fund");
        //    }
        //}


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Cannot_GET_an_Asset_Classification_By_An_Invalid_Asset_Class() {

            using (var client = new HttpClient()) {
                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/RPA");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Act
                var resp = await client.GetAsync(client.BaseAddress);
                var assetClass = await resp.Content.ReadAsAsync<AssetClass>();

                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.NotFound);
                Assert.IsNull(assetClass);
            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_an_Asset_Classification_By_Asset_Id() {

            using (var client = new HttpClient()) {
                // Arrange                                       // b9c7d922-a4d2-49b6-aeb0-a30600f02cc3
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/b9c7d922-a4d2-49b6-aeb0-a30600f02cc3");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                // Act
                var resp = await client.GetAsync(client.BaseAddress);
                var assetClass = await resp.Content.ReadAsAsync<AssetClass>();

                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNull(assetClass);
                Assert.IsTrue(assetClass.KeyId == new Guid("b9c7d922-a4d2-49b6-aeb0-a30600f02cc3"));
                Assert.IsFalse(assetClass.Description != "Preferred Stock");
            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Can_POST_a_New_Asset_Classification() {

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var random = new Random();
                var randomNumber = random.Next(0, 100);

                var newClassification = new AssetClass {
                                                            Code = "t" + randomNumber.ToString(CultureInfo.InvariantCulture),
                                                            Description = DateTime.Now.ToLocalTime().ToString(CultureInfo.InvariantCulture)
                                                        };
                
                // Act
                var response = client.PostAsJsonAsync(client.BaseAddress.ToString(), newClassification).Result;
                var jsonResult = response.Content.ReadAsStringAsync().Result;
                var classification = JsonConvert.DeserializeObject<AssetClass>(jsonResult);
              

                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Created);
                Assert.IsTrue(classification.Code.Contains("t"));
                Assert.IsTrue(response.Headers.Location.AbsoluteUri.Contains(classification.Code));
            }
            
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Cannot_POST_a_duplicate_Asset_Classification() {

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var newClassification = new AssetClass {
                    Code = "CEF",
                    Description = "Closed-End Fund"
                };

                // Act
                var response = client.PostAsJsonAsync(client.BaseAddress.ToString(), newClassification).Result;
              
                // Assert
                Assert.IsTrue(response.StatusCode == HttpStatusCode.Conflict);


            }

        }

    }


}
